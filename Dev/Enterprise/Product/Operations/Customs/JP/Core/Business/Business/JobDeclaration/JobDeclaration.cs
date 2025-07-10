using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Utils;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.JobDocAddressRequirement;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Business
{
	public partial class JobDeclaration : AutoJPJobDeclaration, IInvoicesProvider, IAdditionalReferenceNumberSupporter, INACCSMessageImportSupporter
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJobDeclaration.Schema
		{
			public new const int JE_PaymentMethodMaxLength = 1;
			public new const int JE_DefermentAccountNumberMaxLength = 9;
		}

		public GlbExternalPasswordCUS Credential => Factory.Load<GlbExternalPasswordCUS>(JE_NACCSCredential);

		public const string BasketRadioCallSign = "9999";

		#region Implementation

		#region protected override

		public new JobDeclarationDocumentSupporter DocumentSupporter => (JobDeclarationDocumentSupporter)base.DocumentSupporter;

		protected override DocumentSupporter CreateNewDocumentSupporter() => new JobDeclarationDocumentSupporter(this);

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser() => new JobDeclarationSynchroniser(this);

		protected override bool ShowSubmitMenuItemCore() => false;

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		public override bool IsInvoicesRequiredToBeInSameCurrency => true;

		public override bool IsInvoicesRequiredToBeInSameIncoTerm => true;

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return new EntryInstructionProvider(this);
		}

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Japan;

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("JP"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JE_ValuationDate = ZDate.Today;

			var defaultBrokerAndCredential = JPRegistry.Instance.DefaultBrokerAndCredential.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			JE_GS_NKCusAgent = defaultBrokerAndCredential.DefaultBrokerCode;
			JE_NACCSCredential = GetNACCSCredentialValue();
			JE_PaymentMethod = ZString.Empty;
		}

		ZGuid GetNACCSCredentialValue()
		{
			var defaultBrokerAndCredential = JPRegistry.Instance.DefaultBrokerAndCredential.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			var descriptionString = IsSea ? defaultBrokerAndCredential.DefaultCredentialSEA : defaultBrokerAndCredential.DefaultCredentialAIR;

			var matchedCredential = descriptionString.IsEmpty
				? null
				: Lookups.NACCSCredentialsList.FirstOrDefault(x => x.GP_MailBoxID == descriptionString.SubstringSafe(0, 5) && x.GP_UserID == descriptionString.SubstringSafe(5, 3))?.PK;

			return matchedCredential ?? Lookups.NACCSCredentialsList.FirstOrDefault()?.PK ?? ZGuid.Empty;
		}

		protected override void DefaultAdditionalReferenceNumbersFromShipmentCore()
		{
			base.DefaultAdditionalReferenceNumbersFromShipmentCore();
			var shipment = Shipment;
			var referenceNumbers = shipment.Numbers.GetAllReferenceNumbersByTypeAndCountry(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, CountryCodes.Japan);
			if (referenceNumbers.Length == 0)
			{
				var consols = shipment.Consols.Cast<ForwardingConsol>();
				referenceNumbers = consols.Where(c => !c.JK_BookingReference.IsEmpty).Select(x => x.JK_BookingReference).ToArray();

				if (referenceNumbers.Length == 0)
				{
					referenceNumbers = consols.SelectMany(x => x.Numbers.GetAllReferenceNumbersByTypeAndCountry(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, CountryCodes.Japan)).ToArray();
				}
			}

			var additionalReferenceNumbers = AdditionalReferenceNumbers;
			referenceNumbers.ForEach(x => additionalReferenceNumbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, x));
		}

		protected override string GetIApportionInvoiceHolderCountryContextCore() => this.GetCountryContext();

		protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

		#endregion

		#region Properties

		[ResourceStringData("JPJobDeclaration|JE_ValuationDate", ShortCaption = "Val. Date", MediumCaption = "Valuation Date", Caption = "Valuation Date", FullDescription = "This value is used by the system to determine exchange rates.")]
		public override ZDate JE_ValuationDate
		{
			get => base.JE_ValuationDate;
			set => base.JE_ValuationDate = value;
		}

		[ResourceStringData("JPJobDeclaration|JE_MessageStatus", ShortCaption = "Status", MediumCaption = "Msg. Status", Caption = "Message Status")]
		public override ZString JE_MessageStatus
		{
			get => base.JE_MessageStatus;
			set => base.JE_MessageStatus = value;
		}

		[ResourceStringData("JPJobDeclaration|JE_MessageStatusDescription", ShortCaption = "Desc.", MediumCaption = "Msg. Status Desc.", Caption = "Message Status Description")]
		public override ZString JE_MessageStatusDescription => Lookups.MessageStatusList.GetDescriptionFromCode(JE_MessageStatus) ?? string.Empty;

		[ResourceStringData("JPJobDeclaration|JE_MessageType", ShortCaption = "Msg. Type", MediumCaption = "Msg. Type", Caption = "Message Type", FullDescription = "Indicates the type of customs declaration to be created. When IMP is selected, IDA import customs declaration will be created. When EXP is selected, EDA export customs declaration will be created.")]
		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				var oldValue = JE_MessageType;
				base.JE_MessageType = value;

				if (oldValue != JE_MessageType)
				{
					NeedToGetNewIncoTermAndChargeFactory = true;

					foreach (var invoice in Invoices)
					{
						invoice.NeedToGetNewIncoTermAndChargeFactory = true;
					}

					Invoices.MarkAsNeedingValidation();

					if (!IsCopying)
					{
						var customsEntryInstructions = CustomsEntryInstructions.Cast<CusEntryInstruction>();
						customsEntryInstructions.ForEach(c =>
						{
							c.UpdateInvoiceLinesStorageTypeIfNeeded();
							c.UpdateInvoiceLineJPNACCSCodeIfNeeded();
							c.UpdateECRCargoTypeIfNeeded();
						});
						if (IsExport)
						{
							customsEntryInstructions.ForEach(x => x.Guarantees.RemoveAll());
						}
					}
				}
			}
		}

		[MaxLength(2)]
		[ResourceStringData("JobDeclaration|JE_CustomsOffice", Caption = "Customs Office", MediumCaption = "Customs Off.", ShortCaption = "Cus. Off.")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
		public override ZString JE_CustomsOffice
		{
			get => base.JE_CustomsOffice;
			set
			{
				if (value != JE_CustomsOffice)
				{
					base.JE_CustomsOffice = value;

					if (!IsCopying)
					{
						var previousCustomsOfficeDepValue = JE_CustomsOfficeDepartment;
						JE_CustomsOfficeDepartment = ZString.Empty;
						if (!previousCustomsOfficeDepValue.IsEmpty)
						{
							JE_CustomsOfficeDepartment = previousCustomsOfficeDepValue;
						}
						else
						{
							if (Lookups.CustomsOfficeDepartmentsList.Count == 1)
							{
								JE_CustomsOfficeDepartment = Lookups.CustomsOfficeDepartmentsList[0].Code;
							}
						}
					}
				}
			}
		}

		[MaxLength(2)]
		[ResourceStringData("JobDeclaration|JE_CustomsOfficeDepartment", FullDescription = "Customs Office Department", Caption = "Customs Office Dept.", MediumCaption = "Customs Off. Dept.", ShortCaption = "Cus. Off. Dept.")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeDepartmentsList))]
		[ReadOnlyMember(nameof(JE_CustomsOfficeDepartment_ReadOnly))]
		public override ZString JE_CustomsOfficeDepartment
		{
			get => base.JE_CustomsOfficeDepartment;
			set
			{
				base.JE_CustomsOfficeDepartment = value;
				if (!IsCopying)
				{
					JE_CustomsOfficeDepartmentInfo.RefreshBinding();
				}
			}
		}

		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set
			{
				var hasChanged = base.JE_TransportMode != value;
				base.JE_TransportMode = value;
				if (hasChanged && !IsCopying)
				{
					JE_NACCSCredential = GetNACCSCredentialValue();
					CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(c =>
					{
						c.UpdateInvoiceLinesStorageTypeIfNeeded();
						c.UpdateECRCargoTypeIfNeeded();
					});
				}
			}
		}

		[MaxLength(Schema.JE_PaymentMethodMaxLength)]
		[ResourceStringData("JPJobDeclaration|JE_PaymentMethod", Caption = "Payment Method")]
		public override ZString JE_PaymentMethod { get => base.JE_PaymentMethod; set => base.JE_PaymentMethod = value; }

		[MaxLength(Schema.JE_DefermentAccountNumberMaxLength)]
		[ResourceStringData("JPJobDeclaration|JE_DefermentAccountNumber", ShortCaption = "Bank Acc. No.", Caption = "Bank Account Number")]
		public override ZString JE_DefermentAccountNumber { get => base.JE_DefermentAccountNumber; set => base.JE_DefermentAccountNumber = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.PaymentDeadlineExtensionCodeList))]
		[ResourceStringData("JPJobDeclaration|JE_PaymentDeadlineExtension", ShortCaption = "Pymt. Deadline Ext.", Caption = "Payment Deadline Extension")]
		public override ZString JE_PaymentDeadlineExtension { get => base.JE_PaymentDeadlineExtension; set => base.JE_PaymentDeadlineExtension = value; }

		bool JE_CustomsOfficeDepartment_ReadOnly => JE_CustomsOffice.IsEmpty;

		public override ZString JE_GS_NKCusAgent
		{
			get => base.JE_GS_NKCusAgent;
			set
			{
				if (JE_GS_NKCusAgent != value)
				{
					base.JE_GS_NKCusAgent = value;

					if (!IsCopying && !string.IsNullOrWhiteSpace(value) && JE_NACCSCredential.IsEmpty)
					{
						JE_NACCSCredential = Lookups.NACCSCredentialsList.FirstOrDefault()?.PK ?? ZGuid.Empty;
					}
				}
			}
		}

		public override ZString JE_GoodsDescription
		{
			get => base.JE_GoodsDescription;
			set
			{
				var oldValue = JE_GoodsDescription;
				base.JE_GoodsDescription = value;
				if (!IsCopying && oldValue != value)
				{
					CustomsEntryInstructions.Cast<CusEntryInstruction>().Where(x => x.CEI_GoodsDescription.IsEmpty).ForEach(c =>
					{
						c.CEI_GoodsDescription = value.Left(AutoJPCusEntryInstruction.Schema.CEI_GoodsDescriptionMaxLength);
					});
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.NACCSCredentialsList))]
		[ReadOnlyMember(nameof(JE_NACCSCredential_ReadOnly))]
		public override ZGuid JE_NACCSCredential { get => base.JE_NACCSCredential; set => base.JE_NACCSCredential = value; }

		bool JE_NACCSCredential_ReadOnly => JE_GS_NKCusAgent.IsEmpty;

		[ResourceStringData("JPJobDeclaration|JE_ArrivalAtLoadingDate", ShortCaption = "Ent.", Caption = "Date Of Entry", FullDescription = "Entry date at Port of Loading")]
		public override ZDate JE_ArrivalAtLoadingDate { get => base.JE_ArrivalAtLoadingDate; set => base.JE_ArrivalAtLoadingDate = value; }

		public override ZGuid JE_OH_ShippingLine
		{
			get => base.JE_OH_ShippingLine;
			set
			{
				var oldValue = JE_OH_ShippingLine;
				if (oldValue != value)
				{
					base.JE_OH_ShippingLine = value;
					if (!IsCopying && ShippingLine is OrgHeader shippingLine)
					{
						if (IsAir)
						{
							JE_CarrierCode = shippingLine.MiscServ?.Airline?.RM_TwoCharacterCode ?? ZString.Empty;
						}
						else if (IsSea)
						{
							var customsCarrierRegNo = shippingLine.CustomsCodes.GetCustomsRegNoMatching(CodeTypes.CarrierCode);
							if (IsValidCarrierCode(customsCarrierRegNo))
							{
								JE_CarrierCode = customsCarrierRegNo;
							}
							else if (IsValidCarrierCode(shippingLine.ShippingLineSCAC))
							{
								JE_CarrierCode = shippingLine.ShippingLineSCAC;
							}
						}
					}
				}
			}
		}

		bool IsValidCarrierCode(ZString code)
		{
			var collection = Lookups.CarrierCodeCollection;
			var query = new ZQuery(ZZRefCarrierCombinedSchema.ZZ4_Code, code)
				.AddToFilter(collection.CompleteFilter, JoinCondition.And);
			return collection.Factory.LoadTop1<ZZRefCarrierCombined>(query) != null;
		}

		public override ZGuid JE_OA_Representative
		{
			get => base.JE_OA_Representative;
			set
			{
				var oldValue = base.JE_OA_Representative;
				if (oldValue != value)
				{
					base.JE_OA_Representative = value;
					if (!IsCopying)
					{
						OnRepresentativeChanged();
					}
				}
			}
		}

		[MaxLength(10)]
		[ResourceStringData("37284D3B-DF5D-422F-8A4B-CD5881E660E1", ShortCaption = "ACP POA", Caption = "ACP Power of Attorney")]
		public override ZString JE_ACP_POA
		{
			get => base.JE_ACP_POA;
			set
			{
				var oldValue = base.JE_ACP_POA;
				if (oldValue != value)
				{
					base.JE_ACP_POA = value;
					if (!IsCopying && !IsValidationSuspended)
					{
						Validation.ValidateJE_OA_Representative();
					}
				}
			}
		}

		[ResourceStringData("JPJobDeclaration|JE_CarrierCode", Caption = "Carrier Code")]
		public override ZString JE_CarrierCode
		{
			get => base.JE_CarrierCode;
			set
			{
				var oldValue = JE_CarrierCode;
				if (oldValue != value)
				{
					base.JE_CarrierCode = value;
					if (!IsCopying)
					{
						SetShippingLineIfNeeded(JE_CarrierCode);
					}
				}
			}
		}

		[ResourceStringData("JPJobDeclaration|Air|JE_MasterBill", Caption = "MAWB", MultipleKey = AirCaptionKey)]
		[ResourceStringData("JPJobDeclaration|Sea|JE_MasterBill", Caption = "Master Bill", MultipleKey = SeaCaptionKey)]
		public override ZString JE_MasterBill { get => base.JE_MasterBill; set => base.JE_MasterBill = value; }

		void SetShippingLineIfNeeded(ZString carrierCode)
		{
			if (IsSea && !carrierCode.IsEmpty && JE_OH_ShippingLine.IsEmpty)
			{
				if (TryGetShippingLineByCCCRegistrationNumber(carrierCode, out var cccPK))
				{
					JE_OH_ShippingLine = cccPK;
				}
				else if (TryGetShippingLineBySCAC(carrierCode, out var scacPK))
				{
					JE_OH_ShippingLine = scacPK;
				}
			}
			else if (IsAir && !carrierCode.IsEmpty && JE_OH_ShippingLine.IsEmpty)
			{
				if (TryGetShippingLineByAirLine(carrierCode, out var airPK))
				{
					JE_OH_ShippingLine = airPK;
				}
			}
		}

		bool TryGetShippingLineByCCCRegistrationNumber(ZString regNum, out ZGuid cccPK)
		{
			var result = false;
			cccPK = ZGuid.Empty;
			var collection = Lookups.ShippingLineList;
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			subQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, regNum);
			subQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, CodeTypes.CarrierCode);
			subQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, CountryCodes.Japan);
			query.AddSubQuery(subQuery, JoinCondition.And);

			query.AddToFilter(collection.CompleteFilter, JoinCondition.And);
			var org = collection.Factory.LoadTop1<OrgHeader>(query);
			if (org != null)
			{
				cccPK = org.PK;
				result = true;
			}
			return result;
		}

		bool TryGetShippingLineBySCAC(ZString shippingLineSCAC, out ZGuid scacPK)
		{
			var result = false;
			scacPK = ZGuid.Empty;
			var collection = Lookups.ShippingLineList;
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var shippingLineFilter = new ZDBOnlySubQuery(typeof(RefShippingLine), RefShippingLineSchema.PK);
			shippingLineFilter.AddToFilter(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, shippingLineSCAC);
			query.AddSubQuery(OrgHeaderSchema.OH_RSL_ShippingLine, shippingLineFilter, JoinCondition.And);

			query.AddToFilter(collection.CompleteFilter, JoinCondition.And);
			var org = collection.Factory.LoadTop1<OrgHeader>(query);
			if (org != null)
			{
				scacPK = org.PK;
				result = true;
			}
			return result;
		}

		bool TryGetShippingLineByAirLine(ZString airLineTwoCharacterCode, out ZGuid airPK)
		{
			var result = false;
			airPK = ZGuid.Empty;
			var collection = Lookups.ShippingLineList;
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var airLineQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
			airLineQuery.AddToFilter(RefAirlineSchema.RM_TwoCharacterCode, airLineTwoCharacterCode);

			var orgMiscServQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			orgMiscServQuery.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, airLineQuery, JoinCondition.And);
			query.AddSubQuery(OrgHeaderSchema.PK, orgMiscServQuery, JoinCondition.And);
			query.AddToFilter(collection.CompleteFilter, JoinCondition.And);
			var org = collection.Factory.LoadTop1<OrgHeader>(query);
			if (org != null)
			{
				airPK = org.PK;
				result = true;
			}
			return result;
		}

		public override ZBool IsImport => JE_MessageType == JobMessageTypeList.Codes.Import;

		public ZBool IsExportAndSea => Factory.GetValue(ref isExportAndSeaCached, () => IsExport && IsSea);
		CachedProperty<ZBool> isExportAndSeaCached;

		public ZBool IsExportAndAir => Factory.GetValue(ref isExportAndAirCached, () => IsExport && IsAir);
		CachedProperty<ZBool> isExportAndAirCached;

		public ZBool IsDirectMessaging => JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin;

		public ZBool IsAttachmentRegMessaging => Factory.GetValue(ref isAttachmentRegMessagingProperty, () => ActiveEntryHeaders.Cast<CusEntryHeader>().Any(c => c.IsCustomsDeclarationPhaseIDCOrEDC));
		CachedProperty<ZBool> isAttachmentRegMessagingProperty;

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer() => new DeclarationValueChangedAnnouncer(this);

		public ZBool IsMailedCargo => CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(c => c.IsMailedCargo);

		public ZPropertyInfo IsMailedCargoInfo => GetZPropertyInfo(nameof(IsMailedCargo));

		public ZBool ShouldShowDescriptionForUnknownPortName => Factory.GetValue(ref shouldShowDescriptionForUnknownPortName, () => IsForMarineProductsExport || IsExport && (IsSea || IsMailedCargo) && JE_RL_NKPortOfLoading == Common.Constants.PortNames.UnknownPortName);
		CachedProperty<ZBool> shouldShowDescriptionForUnknownPortName;

		public ZString PortOfLoadingDescription
		{
			get
			{
				ZString descriptionText;
				if (JE_RL_NKPortOfLoading.IsEmpty)
				{
					descriptionText = FindBoxMessages.NoneSelected;
				}
				else if (ShouldShowDescriptionForUnknownPortName)
				{
					descriptionText = Common.Constants.PortNames.UnknownPortName;
				}
				else if (PortOfLoading is RefUNLOCO portOfLoading && Lookups.Origins.Contains(portOfLoading))
				{
					descriptionText = portOfLoading.Description;
				}
				else
				{
					descriptionText = FindBoxMessages.InvalidSelection;
				}
				return descriptionText;
			}
		}

		[ResourceStringData("JPJobDeclaration|JE_FinalDestinationName", FullDescription = "Final Destination Name", Caption = "Final Destination")]
		public override ZString JE_FinalDestinationName
		{
			get => base.JE_FinalDestinationName;
			set => base.JE_FinalDestinationName = value;
		}

		[ResourceStringData("528F6ADD-43AC-4E11-A6EC-4B4FD6EA5489", ShortCaption = "RM", Caption = "Receipt Mode")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ReceiptModeList))]
		public override ZString JE_ReceiptMode
		{
			get => base.JE_ReceiptMode;
			set => base.JE_ReceiptMode = value;
		}

		[ResourceStringData("3D98724B-C153-4F27-8B3A-852D6E06597B", ShortCaption = "DM", Caption = "Delivery Mode")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeliveryModeList))]
		public override ZString JE_DeliveryMode
		{
			get => base.JE_DeliveryMode;
			set => base.JE_DeliveryMode = value;
		}

		public override ZString JE_VesselName
		{
			get { return base.JE_VesselName; }
			set
			{
				var oldValue = JE_VesselName;
				base.JE_VesselName = value;
				if (!IsCopying && oldValue != value)
				{
					UpdateRadioCallSignFromVessel();
				}
			}
		}

		[ResourceStringData("F6326A0C-7844-4005-B9E2-CDB2FE4122B5", Caption = "Radio Call Sign")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.RadioCallSignVessels))]
		public override ZString JE_RadioCallSign
		{
			get { return base.JE_RadioCallSign; }
			set
			{
				var oldValue = JE_RadioCallSign;
				base.JE_RadioCallSign = value;
				if (!IsCopying && oldValue != value)
				{
					UpdateVesselFromRadioCallSign();
				}
			}
		}

		[ResourceStringData("JPJobDeclaration|JE_OwnerSectionCode", Caption = "Owner Section Code", ShortCaption = "Owner Sec. Co.")]
		public override ZString JE_OwnerSectionCode
		{
			get => base.JE_OwnerSectionCode;
			set => base.JE_OwnerSectionCode = value.KeepAlphanumericCharacters();
		}

		void UpdateRadioCallSignFromVessel()
		{
			var vessel = Vessel;
			if (vessel != null)
			{
				var vesselZZ = RefVesselZZ.LookupVesselByCode(vessel.RV_Code, CountryCodes.Japan, Factory);
				if (vesselZZ != null)
				{
					JE_RadioCallSign = vesselZZ.ZZO_RadioCallSign;
				}
			}
		}

		void UpdateVesselFromRadioCallSign()
		{
			var query = new ZQuery(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Japan);
			query.AddToFilter(RefVesselZZSchema.ZZO_RadioCallSign, JE_RadioCallSign);
			var vesselZZ = Factory.LoadTop1<RefVesselZZ>(query);
			if (vesselZZ != null)
			{
				var refVesselLoader = new RefVessel.Loader(Factory);
				var vessel = refVesselLoader.LoadUnique(vesselZZ.ZZO_Code, ZString.Empty, ZString.Empty, ZString.Empty);
				if (vessel != null)
				{
					JE_VesselName = vessel.RV_Code;
				}
			}
		}

		[ResourceStringData("8B550A71-3E2E-4767-9ECC-F404E6FC3B27", Caption = "Booking Number")]
		public ZString JE_BookingNumber
		{
			get => BookingNumber?.CE_EntryNum ?? string.Empty;
			set
			{
				if (BookingNumber != null)
				{
					BookingNumber.CE_EntryNum = value;
				}
				else
				{
					AdditionalReferenceNumbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, value);
				}
			}
		}

		CusEntryNumber BookingNumber => bookingNumber ??= AdditionalReferenceNumbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG);
		CusEntryNumber bookingNumber;

		#endregion

		#endregion

		protected override IReadOnlyList<string> MultipleKeysToUseCore => IsAir ? [AirCaptionKey] : [SeaCaptionKey];

		public const string AirCaptionKey = "2823FB8E-F52E-4FF8-9264-0524D33CC67F";
		public const string SeaCaptionKey = "508FEA9E-FA91-447A-A3EC-3B72AA633810";

		#region DocAddress

		#region DeclarationConsigneeAddress

		public JPJobDocAddress DeclarationConsigneeAddress
		{
			get
			{
				if (declarationConsigneeAddress == null || declarationConsigneeAddress.IsDeleted)
				{
					if (declarationConsigneeAddress != null)
					{
						declarationConsigneeAddress.DocAddressChanged -= DeclarationConsigneeAddressChanged;
						foreach (ZPropertyInfo propertyInfo in declarationConsigneeAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= DeclarationConsigneeAddressChanged;
						}
					}

					declarationConsigneeAddress = (JPJobDocAddress)DocAddresses.FindOrCreateWithRequirement(DeclarationConsigneeAddressRequirement);

					declarationConsigneeAddress.DocAddressChanged += DeclarationConsigneeAddressChanged;
					foreach (ZPropertyInfo propertyInfo in declarationConsigneeAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += DeclarationConsigneeAddressChanged;
					}
				}

				return declarationConsigneeAddress;
			}
		}

		JPJobDocAddress declarationConsigneeAddress;

		JobDocAddressRequirement DeclarationConsigneeAddressRequirement
		{
			get
			{
				if (declarationConsigneeAddressRequirement == null)
				{
					declarationConsigneeAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneeAddress, ContactType.Administration);
					declarationConsigneeAddressRequirement.GetRegistrationNumberResult = getRegistrationNumberResult;
					DocAddressManager.AddRequirement(declarationConsigneeAddressRequirement);
				}
				return declarationConsigneeAddressRequirement;
			}
		}

		JobDocAddressRequirement declarationConsigneeAddressRequirement;

		protected void DeclarationConsigneeAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
			DefaultOrganisationFromDocumentaryAddress(DeclarationConsigneeAddress, JE_OH_ConsigneeInfo, JE_OA_ConsigneeAddressInfo);
		}

		#endregion

		#region Consignor Address

		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|ConsignorAddress", Caption = "Consignor")]
		public JPJobDocAddress DeclarationConsignorAddress
		{
			get
			{
				if (declarationConsignorAddress == null || declarationConsignorAddress.IsDeleted)
				{
					declarationConsignorAddress = (JPJobDocAddress)DocAddresses.FindOrCreateWithRequirement(DeclarationConsignorAddressRequirement);
				}
				return declarationConsignorAddress;
			}
		}
		JPJobDocAddress declarationConsignorAddress;

		JobDocAddressRequirement DeclarationConsignorAddressRequirement
		{
			get
			{
				if (declarationConsignorAddressRequirement == null)
				{
					declarationConsignorAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorAddress, ContactType.Administration);
					declarationConsignorAddressRequirement.GetRegistrationNumberResult = getRegistrationNumberResult;
					DocAddressManager.AddRequirement(declarationConsignorAddressRequirement);
				}
				return declarationConsignorAddressRequirement;
			}
		}

		JobDocAddressRequirement declarationConsignorAddressRequirement;

		#endregion

		#region Attorney For Customs Procedures Address

		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|AttorneyForCustomsProceduresAddress", Caption = "Attorney for Customs Procedure (ACP)", ShortCaption = "ACP")]
		public JPJobDocAddress AttorneyForCustomsProceduresAddress
		{
			get
			{
				if (attorneyForCustomsProceduresAddress == null || attorneyForCustomsProceduresAddress.IsDeleted)
				{
					if (attorneyForCustomsProceduresAddress != null)
					{
						attorneyForCustomsProceduresAddress.DocAddressChanged -= AttorneyForCustomsProceduresAddressChanged;
						foreach (ZPropertyInfo propertyInfo in attorneyForCustomsProceduresAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= AttorneyForCustomsProceduresAddressChanged;
						}
					}

					attorneyForCustomsProceduresAddress = (JPJobDocAddress)DocAddresses.FindOrCreateWithRequirement(AttorneyForCustomsProceduresAddressRequirement);

					attorneyForCustomsProceduresAddress.DocAddressChanged += AttorneyForCustomsProceduresAddressChanged;
					foreach (ZPropertyInfo propertyInfo in attorneyForCustomsProceduresAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += AttorneyForCustomsProceduresAddressChanged;
					}
				}
				return attorneyForCustomsProceduresAddress;
			}
		}
		JPJobDocAddress attorneyForCustomsProceduresAddress;

		JobDocAddressRequirement AttorneyForCustomsProceduresAddressRequirement
		{
			get
			{
				if (attorneyForCustomsProceduresAddressRequirement == null)
				{
					attorneyForCustomsProceduresAddressRequirement = new JobDocAddressRequirement(DocAddressType.AttorneyForCustomsProceduresAddress, ContactType.Administration);
					attorneyForCustomsProceduresAddressRequirement.GetRegistrationNumberResult = getRegistrationNumberResult;
					DocAddressManager.AddRequirement(attorneyForCustomsProceduresAddressRequirement);
				}
				return attorneyForCustomsProceduresAddressRequirement;
			}
		}
		JobDocAddressRequirement attorneyForCustomsProceduresAddressRequirement;

		protected void AttorneyForCustomsProceduresAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
			DefaultOrganisationFromDocumentaryAddress(AttorneyForCustomsProceduresAddress, null, JE_OA_RepresentativeInfo);
		}

		#endregion

		#region InspectionWitness

		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|InspectionWitness", Caption = "Inspection Witness")]
		public JPJobDocAddress InspectionWitness
		{
			get
			{
				if (inspectionWitness == null || inspectionWitness.IsDeleted)
				{
					if (inspectionWitness != null)
					{
						inspectionWitness.DocAddressChanged -= InspectionWitnessAddressChanged;
						foreach (ZPropertyInfo propertyInfo in inspectionWitness.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= InspectionWitnessAddressChanged;
						}
					}

					inspectionWitness = (JPJobDocAddress)DocAddresses.FindOrCreateWithRequirement(InspectionWitnessAddressRequirement);

					inspectionWitness.DocAddressChanged += InspectionWitnessAddressChanged;
					foreach (ZPropertyInfo propertyInfo in inspectionWitness.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += InspectionWitnessAddressChanged;
					}
				}
				return inspectionWitness;
			}
		}
		JPJobDocAddress inspectionWitness;

		JobDocAddressRequirement InspectionWitnessAddressRequirement
		{
			get
			{
				if (inspectionWitnessAddressRequirement == null)
				{
					inspectionWitnessAddressRequirement = new JobDocAddressRequirement(DocAddressType.InspectionWitness, ContactType.Administration);
					inspectionWitnessAddressRequirement.GetRegistrationNumberResult = getRegistrationNumberResult;
					DocAddressManager.AddRequirement(inspectionWitnessAddressRequirement);
				}
				return inspectionWitnessAddressRequirement;
			}
		}
		JobDocAddressRequirement inspectionWitnessAddressRequirement;

		[MaxLength(5)]
		[ReadOnlyMember(nameof(InspectionWitnessCodeReadOnly))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|InspectionWitnessCode", Caption = "NACCS User Code", FullDescription = "Inspection Witness NACCS User Code")]
		public ZString InspectionWitnessCode
		{
			get => InspectionWitness.E2_GovRegNum;
			set
			{
				if (value.IsEmpty && InspectionWitness.E2_CompanyName.IsEmpty && InspectionWitness.E2_AddressOverride == true)
				{
					InspectionWitness.E2_AddressOverride = false;
				}
				else
				{
					InspectionWitness.E2_AddressOverride = true;
					InspectionWitness.E2_GovRegNumType = JapanCodeTypes.NUC;
					CheckMaximumLength(InspectionWitnessCodeInfo, value);
					InspectionWitness.E2_GovRegNum = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateInspectionWitnessCode();
					}
					if (!IsCopying)
					{
						InspectionWitnessCodeInfo.RefreshBinding();
					}
				}
			}
		}

		public ZPropertyInfo InspectionWitnessCodeInfo => GetZPropertyInfo(nameof(InspectionWitnessCode));

		ZString InspectionWitnessCusCode => InspectionWitness.Address?.CustomsCodes.GetCustomsRegNo(JapanCodeTypes.NUC, CountryCodes.Japan) ?? ZString.Empty;

		ZBool InspectionWitnessCodeReadOnly => InspectionWitness.E2_OA_Address.IsEmpty || (!InspectionWitnessCusCode.IsEmpty && !InspectionWitness.E2_AddressOverride);

		protected void InspectionWitnessAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
			if (e is ValueChangedEventArgs args)
			{
				if (args.Info == InspectionWitness.E2_OA_AddressInfo)
				{
					Validation.ValidateInspectionWitnessCode();
				}
				else if (InspectionWitness.E2_AddressOverride && args.Info == InspectionWitness.E2_CompanyNameInfo && args.NewValue.IsEmpty)
				{
					InspectionWitness.E2_AddressOverride = false;
				}
			}
		}

		#endregion

		#region ExternalBroker

		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|ExternalBroker", Caption = "External Broker")]
		public JPJobDocAddress ExternalBrokerAddress
		{
			get
			{
				if (externalBroker == null || externalBroker.IsDeleted)
				{
					if (externalBroker != null)
					{
						externalBroker.DocAddressChanged -= ExternalBrokerAddressChanged;
						foreach (ZPropertyInfo propertyInfo in externalBroker.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= ExternalBrokerAddressChanged;
						}
					}

					externalBroker = (JPJobDocAddress)DocAddresses.FindOrCreateWithRequirement(ExternalBrokerAddressRequirement);

					externalBroker.DocAddressChanged += ExternalBrokerAddressChanged;
					foreach (ZPropertyInfo propertyInfo in externalBroker.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += ExternalBrokerAddressChanged;
					}
				}
				return externalBroker;
			}
		}
		JPJobDocAddress externalBroker;

		JobDocAddressRequirement ExternalBrokerAddressRequirement
		{
			get
			{
				if (externalBrokerAddressRequirement == null)
				{
					externalBrokerAddressRequirement = new JobDocAddressRequirement(DocAddressType.ExternalBroker, ContactType.Administration);
					externalBrokerAddressRequirement.GetRegistrationNumberResult = getRegistrationNumberResult;
					DocAddressManager.AddRequirement(externalBrokerAddressRequirement);
				}
				return externalBrokerAddressRequirement;
			}
		}
		JobDocAddressRequirement externalBrokerAddressRequirement;

		[MaxLength(5)]
		[ReadOnlyMember(nameof(ExternalBrokerCodeReadOnly))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|ExternalBrokerCode", Caption = "NACCS User Code", FullDescription = "External Broker NACCS User Code")]
		public ZString ExternalBrokerCode
		{
			get => ExternalBrokerAddress.E2_GovRegNum;
			set
			{
				if (value.IsEmpty && ExternalBrokerAddress.E2_CompanyName.IsEmpty && ExternalBrokerAddress.E2_AddressOverride)
				{
					ExternalBrokerAddress.E2_AddressOverride = false;
				}
				else
				{
					ExternalBrokerAddress.E2_AddressOverride = true;
					ExternalBrokerAddress.E2_GovRegNumType = JapanCodeTypes.NUC;
					CheckMaximumLength(ExternalBrokerCodeInfo, value);
					ExternalBrokerAddress.E2_GovRegNum = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateExternalBrokerCode();
					}
					if (!IsCopying)
					{
						ExternalBrokerCodeInfo.RefreshBinding();
					}
				}
			}
		}

		public ZPropertyInfo ExternalBrokerCodeInfo => GetZPropertyInfo(nameof(ExternalBrokerCode));

		ZString ExternalBrokerCusCode => ExternalBrokerAddress.Address?.CustomsCodes.GetCustomsRegNo(JapanCodeTypes.NUC, CountryCodes.Japan) ?? ZString.Empty;

		ZBool ExternalBrokerCodeReadOnly => ExternalBrokerAddress.E2_OA_Address.IsEmpty || (!ExternalBrokerCusCode.IsEmpty && !ExternalBrokerAddress.E2_AddressOverride);

		protected void ExternalBrokerAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
			if (e is ValueChangedEventArgs args)
			{
				if (args.Info == ExternalBrokerAddress.E2_OA_AddressInfo)
				{
					Validation.ValidateExternalBrokerCode();
				}
				else if (ExternalBrokerAddress.E2_AddressOverride && args.Info == ExternalBrokerAddress.E2_CompanyNameInfo && args.NewValue.IsEmpty)
				{
					ExternalBrokerAddress.E2_AddressOverride = false;
				}
				DefaultOrganisationFromDocumentaryAddress(ExternalBrokerAddress, JE_OH_ExternalBrokerInfo, null);
			}
		}

		#endregion

		#region Forwarder

		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|ForwarderAddress", Caption = "Forwarder")]
		public JPJobDocAddress ForwarderAddress
		{
			get
			{
				if (forwarderAddress == null || forwarderAddress.IsDeleted)
				{
					if (forwarderAddress != null)
					{
						forwarderAddress.DocAddressChanged -= ForwarderAddressChanged;
						foreach (ZPropertyInfo propertyInfo in forwarderAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= ForwarderAddressChanged;
						}
					}

					forwarderAddress = (JPJobDocAddress)DocAddresses.FindOrCreateWithRequirement(ForwarderAddressRequirement);

					forwarderAddress.DocAddressChanged += ForwarderAddressChanged;
					foreach (ZPropertyInfo propertyInfo in forwarderAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += ForwarderAddressChanged;
					}
				}
				return forwarderAddress;
			}
		}
		JPJobDocAddress forwarderAddress;

		JobDocAddressRequirement ForwarderAddressRequirement
		{
			get
			{
				if (forwarderAddressRequirement == null)
				{
					forwarderAddressRequirement = new JobDocAddressRequirement(DocAddressType.Forwarder, ContactType.Administration);
					forwarderAddressRequirement.GetRegistrationNumberResult =
						(JobDocAddress docAddress) =>
						{
							return new RegistrationNumberResult(docAddress.Factory, true, () => new RegistrationNumber() { Number = ForwarderCusCode, NumberType = JapanCodeTypes.NUC });
						};
					DocAddressManager.AddRequirement(forwarderAddressRequirement);
				}
				return forwarderAddressRequirement;
			}
		}
		JobDocAddressRequirement forwarderAddressRequirement;

		[MaxLength(5)]
		[ReadOnlyMember(nameof(ForwarderCodeReadOnly))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|ForwarderCode", Caption = "NACCS User Code", FullDescription = "Forwarder NACCS User Code")]
		public ZString ForwarderCode
		{
			get => ForwarderAddress.E2_GovRegNum;
			set
			{
				if (value.IsEmpty && ForwarderAddress.E2_CompanyName.IsEmpty && ForwarderAddress.E2_AddressOverride)
				{
					ForwarderAddress.E2_AddressOverride = false;
				}
				else
				{
					ForwarderAddress.E2_AddressOverride = true;
					ForwarderAddress.E2_GovRegNumType = JapanCodeTypes.NUC;
					CheckMaximumLength(ForwarderCodeInfo, value);
					ForwarderAddress.E2_GovRegNum = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateForwarderCode();
					}
					if (!IsCopying)
					{
						ForwarderCodeInfo.RefreshBinding();
					}
				}
			}
		}

		public ZPropertyInfo ForwarderCodeInfo => GetZPropertyInfo(nameof(ForwarderCode));

		ZString ForwarderCusCode => ForwarderAddress.Address?.CustomsCodes.GetCustomsRegNo(JapanCodeTypes.NUC, CountryCodes.Japan) ?? ZString.Empty;

		ZBool ForwarderCodeReadOnly => ForwarderAddress.E2_OA_Address.IsEmpty || (!ForwarderCusCode.IsEmpty && !ForwarderAddress.E2_AddressOverride);

		protected void ForwarderAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
			if (e is ValueChangedEventArgs args)
			{
				if (args.Info == ForwarderAddress.E2_OA_AddressInfo)
				{
					Validation.ValidateForwarderCode();
				}
				else if (ForwarderAddress.E2_AddressOverride && args.Info == ForwarderAddress.E2_CompanyNameInfo && args.NewValue.IsEmpty)
				{
					ForwarderAddress.E2_AddressOverride = false;
				}
			}
		}

		#endregion

		#region AirCargoAgent

		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|AirCargoAgent", Caption = "Air Cargo Agent")]
		public JPJobDocAddress AirCargoAgent
		{
			get
			{
				if (airCargoAgent == null || airCargoAgent.IsDeleted)
				{
					if (airCargoAgent != null)
					{
						airCargoAgent.DocAddressChanged -= AirCargoAgentAddressChanged;
						foreach (ZPropertyInfo propertyInfo in airCargoAgent.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= AirCargoAgentAddressChanged;
						}
					}

					airCargoAgent = (JPJobDocAddress)DocAddresses.FindOrCreateWithRequirement(AirCargoAgentAddressRequirement);

					airCargoAgent.DocAddressChanged += AirCargoAgentAddressChanged;
					foreach (ZPropertyInfo propertyInfo in airCargoAgent.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += AirCargoAgentAddressChanged;
					}
				}
				return airCargoAgent;
			}
		}
		JPJobDocAddress airCargoAgent;

		JobDocAddressRequirement AirCargoAgentAddressRequirement
		{
			get
			{
				if (airCargoAgentAddressRequirement == null)
				{
					airCargoAgentAddressRequirement = new JobDocAddressRequirement(DocAddressType.AirCargoAgent, ContactType.Administration);
					airCargoAgentAddressRequirement.GetRegistrationNumberResult = getRegistrationNumberResult;
					DocAddressManager.AddRequirement(airCargoAgentAddressRequirement);
				}
				return airCargoAgentAddressRequirement;
			}
		}
		JobDocAddressRequirement airCargoAgentAddressRequirement;

		[MaxLength(5)]
		[ReadOnlyMember(nameof(AirCargoAgentCodeReadOnly))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|AirCargoAgentCode", Caption = "NACCS User Code", FullDescription = "Air Cargo Agent NACCS User Code")]
		public ZString AirCargoAgentNACCSCode
		{
			get => AirCargoAgent.E2_GovRegNum;
			set
			{
				if (value.IsEmpty && AirCargoAgent.E2_CompanyName.IsEmpty && AirCargoAgent.E2_AddressOverride == true)
				{
					AirCargoAgent.E2_AddressOverride = false;
				}
				else
				{
					AirCargoAgent.E2_AddressOverride = true;
					AirCargoAgent.E2_GovRegNumType = JapanCodeTypes.NUC;
					CheckMaximumLength(AirCargoAgentNACCSCodeInfo, value);
					AirCargoAgent.E2_GovRegNum = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAirCargoAgentNACCSCode();
					}
					if (!IsCopying)
					{
						AirCargoAgentNACCSCodeInfo.RefreshBinding();
					}
				}
			}
		}

		public ZPropertyInfo AirCargoAgentNACCSCodeInfo => GetZPropertyInfo(nameof(AirCargoAgentNACCSCode));

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.JP.Business.JPJobDeclaration|AirCargoAgentLocationCode", Caption = "Location Code", FullDescription = "Air Cargo Agent Location Code")]
		[ReadOnlyMember(nameof(AirCargoAgentCodeReadOnly))]
		public ZString AirCargoAgentLocationCode
		{
			get => AirCargoAgent.LocationCode;
			set
			{
				if (value.IsEmpty && AirCargoAgent.E2_CompanyName.IsEmpty && AirCargoAgent.E2_AddressOverride == true)
				{
					AirCargoAgent.E2_AddressOverride = false;
				}
				else
				{
					AirCargoAgent.E2_AddressOverride = true;
					CheckMaximumLength(AirCargoAgentLocationCodeInfo, value);
					AirCargoAgent.LocationCode = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAirCargoAgentLocationCode();
					}
					if (!IsCopying)
					{
						AirCargoAgentLocationCodeInfo.RefreshBinding();
					}
				}
			}
		}

		public ZPropertyInfo AirCargoAgentLocationCodeInfo => GetZPropertyInfo(nameof(AirCargoAgentLocationCode));

		ZString AirCargoAgentNACCSCusCode => AirCargoAgent.Address?.CustomsCodes.GetCustomsRegNo(JapanCodeTypes.NUC, CountryCodes.Japan) ?? ZString.Empty;

		ZString AirCargoAgentLocationCusCode => AirCargoAgent.Address?.CustomsCodes.GetCustomsRegNo(JapanCodeTypes.AAL, CountryCodes.Japan) ?? ZString.Empty;

		ZBool AirCargoAgentCodeReadOnly => AirCargoAgent.E2_OA_Address.IsEmpty || ((!AirCargoAgentNACCSCusCode.IsEmpty || !AirCargoAgentLocationCusCode.IsEmpty) && !AirCargoAgent.E2_AddressOverride);

		protected void AirCargoAgentAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
			if (e is ValueChangedEventArgs args)
			{
				if (args.Info == AirCargoAgent.E2_OA_AddressInfo)
				{
					Validation.ValidateAirCargoAgentNACCSCode();
				}
				else if (AirCargoAgent.E2_AddressOverride && args.Info == AirCargoAgent.E2_CompanyNameInfo && args.NewValue.IsEmpty)
				{
					AirCargoAgent.E2_AddressOverride = false;
				}
			}
		}

		#endregion

		public override JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JPJobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}
				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		#region ImporterDocumentaryAddress

		public new JPJobDocAddress ImporterDocumentaryAddress => (JPJobDocAddress)base.ImporterDocumentaryAddress;

		protected override void SetupImporterDocumentaryAddress(JobDocAddress importerDocumentaryAddress)
		{
			base.SetupImporterDocumentaryAddress(importerDocumentaryAddress);
			importerDocumentaryAddress.Requirement.GetRegistrationNumberResult = getRegistrationNumberResult;

			if (importerDocumentaryAddress.E2_OA_Address.IsValid && JE_OA_ImporterAddress.IsEmpty)
			{
				DefaultOrganisationFromDocumentaryAddress(importerDocumentaryAddress, JE_OH_ImporterInfo, JE_OA_ImporterAddressInfo);
			}
		}

		protected override JobDocAddressRequirement GetImporterDocumentaryAddressRequirement()
		{
			return new ImporterAddressRequirement(DocAddressType.ImporterDocumentaryAddress, ContactType.Consignee);
		}

		protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.ImporterDocumentaryAddressChanged(sender, e);
			MarkAsNeedingValidation();
			DefaultOrganisationFromDocumentaryAddress(ImporterDocumentaryAddress, JE_OH_ImporterInfo, JE_OA_ImporterAddressInfo);
		}

		protected override void FlushImporterDocumentaryAddressIfBlank(ZGuid je_oh_importer) { }

		public override bool UseImporterAddress => true;

		#endregion

		#region SupplierDocumentaryAddress

		public new JPJobDocAddress SupplierDocumentaryAddress => (JPJobDocAddress)base.SupplierDocumentaryAddress;

		protected override void SetupSupplierDocumentaryAddress(JobDocAddress supplierDocumentaryAddress)
		{
			base.SetupSupplierDocumentaryAddress(supplierDocumentaryAddress);
			supplierDocumentaryAddress.Requirement.GetRegistrationNumberResult = getRegistrationNumberResult;

			if (supplierDocumentaryAddress.E2_OA_Address.IsValid && JE_OA_SupplierAddress.IsEmpty)
			{
				DefaultOrganisationFromDocumentaryAddress(supplierDocumentaryAddress, JE_OH_SupplierInfo, JE_OA_SupplierAddressInfo);
			}
		}

		protected override JobDocAddressRequirement GetSupplierDocAddressRequirement()
		{
			return new SupplierAddressRequirement(DocAddressType.SupplierDocumentaryAddress, ContactType.Consignor);
		}

		protected override void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.SupplierDocumentaryAddressChanged(sender, e);
			DefaultOrganisationFromDocumentaryAddress(SupplierDocumentaryAddress, JE_OH_SupplierInfo, JE_OA_SupplierAddressInfo);
		}

		protected override void FlushSupplierDocumentaryAddressIfBlank(ZGuid je_oh_supplier) { }

		public override bool UseSupplierAddress => true;

		#endregion

		readonly RegistrationNumberResultDelegate getRegistrationNumberResult = (JobDocAddress docAddress) =>
		{
			var requiredCusCodeTypes = docAddress.Lookups.GovRegNumTypes.GetAllCodesZString();
			var code = docAddress.Address?.CustomsCodes?.GetOrgCusCodeObjectMatchingCountryAndCodes(CountryCodes.Japan, requiredCusCodeTypes);
			return new RegistrationNumberResult(docAddress.Factory, true, () => new RegistrationNumber() { Number = code?.OK_CustomsRegNo ?? string.Empty, NumberType = code?.OK_CodeType ?? requiredCusCodeTypes.FirstOrDefault() });
		};

		public ZString GetDepotCode()
		{
			var orgHeader = DepotDocAddress.Organisation;
			return orgHeader?.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(CodeTypes.ControlledPremisesID, CountryCodes.Japan, DepotDocAddress.E2_OA_Address) ?? ZString.Empty;
		}

		public ZString GetBondedLocationCode()
		{
			var orgHeader = WarehouseDocAddress.Organisation;
			return orgHeader?.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(CodeTypes.ControlledPremisesID, CountryCodes.Japan, WarehouseDocAddress.E2_OA_Address) ?? ZString.Empty;
		}

		public ZString GetBondedLocationName()
		{
			return WarehouseDocAddress.Organisation?.OH_FullName ?? ZString.Empty;
		}

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			return addressType switch
			{
				DocAddressType.ConsigneeAddress => DeclarationConsigneeAddressRequirement,
				DocAddressType.ConsignorAddress => DeclarationConsignorAddressRequirement,
				DocAddressType.AttorneyForCustomsProceduresAddress => AttorneyForCustomsProceduresAddressRequirement,
				DocAddressType.InspectionWitness => InspectionWitnessAddressRequirement,
				DocAddressType.ExternalBroker => ExternalBrokerAddressRequirement,
				_ => base.GetDocAddressRequirement(addressType),
			};
		}

		protected override DocAddressType[] SupportedAddressTypesCore
		{
			get
			{
				return base.SupportedAddressTypesCore.Concat(new[]
				{
					DocAddressType.ConsigneeAddress,
					DocAddressType.InspectionWitness,
					DocAddressType.ExportBroker,
				}).ToArray();
			}
		}

		protected override void WarehouseDocAddress_ValueChanged(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs args && args.Info == WarehouseDocAddress.E2_OA_AddressInfo)
			{
				var bonedLocationCode = GetBondedLocationCode();
				var bonedLocationName = GetBondedLocationName();

				foreach (var instruction in CustomsEntryInstructions)
				{
					instruction.RefreshBondedLocationIfNeeded(bonedLocationCode, bonedLocationName);
					instruction.Validation.ValidateCEI_BondedLocationCode();
				}
			}
		}

		public bool HasDesignatedCustomsCodes(OrgAddress address, List<ZString> customsCodeTypes)
		{
			return address?.CustomsCodes?.Any(x => customsCodeTypes.Contains(x.OK_CodeType)) ?? false;
		}

		public JobRequiredDocument GetDocumentTrackingRecord(OrgHeader org)
		{
			bool IsDocumentMatched(JobRequiredDocument doc)
			{
				return doc.EQ_DocCategory == Core.Constants.ReferenceTypes.ClientSupplierRelationship
					&& doc.EQ_DocUsage == JobRequiredDocument.DocUsage.AttorneyForCustomsProcedures
					&& doc.EQ_ValidToDate >= ZDateTime.Now
					&& doc.EQ_DateReceived <= ZDateTimeOffset.Now;
			}

			return org?.RequiredDocuments?.GetDocByType(Core.Constants.RefDocTypes.PowerOfAttorney, CountryCodes.Japan, IsDocumentMatched);
		}

		void UpdatePowerOfAttorneyIfNeeded(JobRequiredDocument doc)
		{
			if (doc != null && Representative != null && doc.EQ_OH_DocumentOwner == Representative.OA_OH && JE_ACP_POA.IsEmpty)
			{
				JE_ACP_POA = doc.EQ_DocNumber;
			}
		}

		protected override void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_SupplierChanged(oldValue, newValue);

			OnRepresentativeChanged();

			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_OA_Representative();
			}
		}

		void OnRepresentativeChanged()
		{
			var trackingRecord = GetDocumentTrackingRecord(Supplier);
			UpdatePowerOfAttorneyIfNeeded(trackingRecord);

			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_ACP_POA();
			}
		}

		protected override void DepotDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			base.DepotDocAddress_DocAddressChanged(sender, e);
			if (IsForMarineProductsExport)
			{
				foreach (var instruction in CustomsEntryInstructions)
				{
					if (instruction.CEI_Style.IsEmpty)
					{
						instruction.CEI_Style = JPExportDeclarationTypeList.Codes.E;
					}
				}

				if (JE_RL_NKPortOfLoading.IsEmpty)
				{
					JE_RL_NKPortOfLoading = Common.Constants.PortNames.UnknownPortName;
				}
			}
		}

		void DefaultOrganisationFromDocumentaryAddress(JobDocAddress docAddress, ZPropertyInfo orgInfo, ZPropertyInfo addressInfo)
		{
			if (docAddress == null || docAddress.E2_AddressOverride)
			{
				if (orgInfo != null)
				{
					orgInfo.Value = ZGuid.Empty;
				}
				if (addressInfo != null)
				{
					addressInfo.Value = ZGuid.Empty;
				}
			}
			else
			{
				if (orgInfo != null)
				{
					orgInfo.Value = docAddress.OrganisationPK;
				}
				if (addressInfo != null)
				{
					addressInfo.Value = docAddress.E2_OA_Address;
				}
			}
		}

		#endregion

		#region Events

		public void LogCustomsCommenced(string reference)
		{
			var mostRecentCommencedLog = LogsOfDeclarationOrShipment.MostRecentLogByEventTimeExcludingEstimated(CustomsCommencedEventType, reference, GetBranchQuery());
			LogsOfDeclarationOrShipment.AddNew(CustomsCommencedEventType, reference);

			if (ShouldLogCustomsCommencedDatail)
			{
				JE_CustomsCommencedDate = ZDateTime.Now;
				JE_GS_NKCustomsCommencedUser = GlbStaff.CurrentUser.GS_Code;
			}

			PopulateBrokerWhenLogCustomsCommenced(mostRecentCommencedLog);
		}

		#endregion

		#region MessageSending

		public IDisposable SetCurrentMessageSendingContext(IMessageSendingContext context)
		{
			return new DisposableAction(() => currentMessageSendingContext = context, () => currentMessageSendingContext = null);
		}
		IMessageSendingContext currentMessageSendingContext;

		public IMessageSendingContext MessageSendingContext => currentMessageSendingContext;

		public bool IsEDASendingInProgress => IsMessageSendingInProgress(JPProcedureCodeList.Codes.EDA);

		public bool IsEDA01SendingInProgress => IsMessageSendingInProgress(JPProcedureCodeList.Codes.EDA01);

		public bool IsIDASendingInProgress => IsMessageSendingInProgress(JPProcedureCodeList.Codes.IDA);

		public bool IsIDA01SendingInProgress => IsMessageSendingInProgress(JPProcedureCodeList.Codes.IDA01);

		public bool IsECRSendingInProgress => IsMessageSendingInProgress(JPProcedureCodeList.Codes.ECR);

		bool IsMessageSendingInProgress(string procedureCode) => currentMessageSendingContext?.ProcedureCode.Equals(procedureCode) ?? false;

		public bool IsReadyForSending => !JPRegistry.Instance.IsMailboxAndRemoteWebPrintClientCredentialsEmpty && JPNACCSMailboxCredentialChecker.HasSetupNACCSMailbox(Company);

		#endregion

		#region IAdditionalReferenceNumberSupporter

		void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
		{
		}

		void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
		{
		}

		void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
		{
			Validation.ValidateAdditionalEntryNumber(info, type, number);
		}

		bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems => false;

		#endregion

		public bool IsForMarineProductsExport
		{
			get
			{
				var depotCode = GetDepotCode();
				var today = ZDateTime.Today;
				return Factory.GetCachedValue($"IsForMarineProductsExport {IsExport}-{depotCode}-{today}", () =>
				{
					if (IsExport)
					{
						var refCusCodeList = ZZRefCusCodeListCombined.Loader.LoadByCode(Factory, Core.Constants.CountryCodes.Japan, [Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode], depotCode, today).FirstOrDefault();

						return refCusCodeList?.IsMarineProductsExportLocation() ?? false;
					}

					return false;
				});
			}
		}

		public bool IsECR => Factory.GetValue(ref isECR, () => CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.IsECR));
		CachedProperty<bool> isECR;

		public bool IsBasketRadioCallSign => JE_RadioCallSign == BasketRadioCallSign;

		#region INACCSMessageImportSupporter

		bool INACCSMessageImportSupporter.IsValidParent(IBusiness parent)
		{
			return parent.TableName == CusEntryHeader.Schema.TableName && ActiveEntryHeaders.Any(x => x.PK == parent.Identifier);
		}

		#endregion

		[ResourceStringData("JPJobDeclaration|PortOfLoadingIATACode", Caption = "IATA Code")]
		public ZString PortOfLoadingIATACode
		{
			get => isPortOfLoadingIATACodeOverridden ? portOfLoadingIATACode : PortOfLoading?.RL_IATA ?? portOfLoadingIATACode;
			set
			{
				if (value != portOfLoadingIATACode)
				{
					SetNonPersistentPropertyValue(PortOfLoadingIATACodeInfo, ref portOfLoadingIATACode, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePortOfLoadingIATACode();
					}
					if (!portOfLoadingIATACode.IsEmpty)
					{
						var refUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_IATA, value));
						if (refUNLOCO != null)
						{
							JE_RL_NKPortOfLoading = refUNLOCO.RL_Code;
							isPortOfLoadingIATACodeOverridden = false;
						}
						else
						{
							isPortOfLoadingIATACodeOverridden = true;
						}
					}
				}
			}
		}

		ZString portOfLoadingIATACode;
		bool isPortOfLoadingIATACodeOverridden;

		public ZPropertyInfo PortOfLoadingIATACodeInfo => GetZPropertyInfo(nameof(PortOfLoadingIATACode));

		[ResourceStringData("JPJobDeclaration|FinalDestinationIATACode", Caption = "IATA Code")]
		public ZString FinalDestinationIATACode
		{
			get => isFinalDestinationIATACodeOverridden ? finalDestinationIATACode : FinalDestination?.RL_IATA ?? finalDestinationIATACode;
			set
			{
				if (value != finalDestinationIATACode)
				{
					SetNonPersistentPropertyValue(FinalDestinationIATACodeInfo, ref finalDestinationIATACode, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateFinalDestinationIATACode();
					}
					if (!finalDestinationIATACode.IsEmpty)
					{
						var refUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_IATA, value));
						if (refUNLOCO != null)
						{
							JE_RL_NKFinalDestination = refUNLOCO.RL_Code;
							isFinalDestinationIATACodeOverridden = false;
						}
						else
						{
							isFinalDestinationIATACodeOverridden = true;
						}
					}
				}
			}
		}

		ZString finalDestinationIATACode;
		bool isFinalDestinationIATACodeOverridden;

		public ZPropertyInfo FinalDestinationIATACodeInfo => GetZPropertyInfo(nameof(FinalDestinationIATACode));
	}
}
