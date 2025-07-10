using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using StaffCertType = Enterprise.Core.Constants.StaffDefaultCertificateIDAndTrainingTypes;

namespace Enterprise.Customs.CN.Business
{
	public partial class JobDeclaration :
		AutoCNJobDeclaration,
		Integration.Customs.CN.IJobDeclaration,
		IInvoicesProvider, IApportionInvoiceHolder,
		Integration.Customs.ICusCodeDataTypeSupporter,
		IAdditionalReferenceNumberTypeProvider,
		ICusEntryNumberValidationDeciderOfType,
		IValidationModeProvider
	{
		public new class Schema : AutoCNJobDeclaration.Schema
		{
			public const string JE_LastPortBeforeEntry = "JE_LastPortBeforeEntry";
			public const string OfficeOfDestination = "OfficeOfDestination";
			public const string FullValidation = "FullValidation";
		}

		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobDeclarationFetchStrategy(this);

		public override void DefaultValueForFakeDeclaration()
		{
			base.DefaultValueForFakeDeclaration();
			JE_MessageSubType = DecTypeList.Codes.Both;
		}

		protected override void FlushImporterDocumentaryAddressIfBlank(ZGuid importer) { }

		protected override void FlushSupplierDocumentaryAddressIfBlank(ZGuid supplier) { }

		protected override void FlushBuyerDocAddressIfBlank(ZGuid buyer) { }

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
		public new CusEntryInstructionCollection CustomsEntryInstructions => CustomsEntryInstructionProvider.CustomsEntryInstructions;

		#region Implementation

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Ports))]
		public ZString JE_LastPortBeforeEntry
		{
			get
			{
				if (!flastPortBeforeEntry.HasValue)
				{
					flastPortBeforeEntry = GetLastPortBeforeEntry();
				}
				return flastPortBeforeEntry.Value;
			}
		}

		ZString? flastPortBeforeEntry;

		public ZPropertyInfo JE_LastPortBeforeEntryInfo => GetZPropertyInfo(Schema.JE_LastPortBeforeEntry);

		public OrgHeader Declarant => Branch?.OrgProxy ?? Branch?.Company?.OrgProxy;

		#region override

		protected override void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_SupplierChanged(oldValue, newValue);
			DefaultOriginDistrictIfNeeded();
			if (IsImport)
			{
				DefaultCountryOfTrade();
				UpdateJE_CNTransportMode();
			}
		}

		protected override void JE_OH_ImporterChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_ImporterChanged(oldValue, newValue);
			DefaultDestinationDistrictIfNeeded();
			if (IsExport)
			{
				DefaultCountryOfTrade();
				UpdateJE_CNTransportMode();
			}
		}

		public override ZGuid JE_OH_Manufacturer
		{
			get => base.JE_OH_Manufacturer;
			set
			{
				var oldValue = base.JE_OH_Manufacturer;
				base.JE_OH_Manufacturer = value;
				if (!IsCopying && oldValue != value)
				{
					DefaultOriginDistrictIfNeeded();
					DefaultManufacturerDocumentaryAddress(value);
				}
			}
		}

		public override ZGuid JE_OH_Buyer
		{
			get => base.JE_OH_Buyer;
			set
			{
				var oldValue = base.JE_OH_Buyer;
				base.JE_OH_Buyer = value;
				if (!IsCopying && oldValue != value)
				{
					DefaultDestinationDistrictIfNeeded();
				}
			}
		}

		protected override bool IsDefaultBuyerDocAddressEnabled => true;

		[ResourceStringData("C9373ACA-EB11-4F54-8594-0CB8174DC861", Caption = "Parcel Number", IsApplicableMember = nameof(IsPost))]
		[ResourceStringData("F1D8432B-7C0C-4D62-9668-4172F91BAD30", Caption = "Trans. Batch No.", IsApplicableMember = nameof(IsRoad))]
		[ResourceStringData("5D55FDF4-1A53-4885-912B-53ABFF7AA956", Caption = "Rail Waybill number", IsApplicableMember = nameof(IsRail))]
		public override ZString JE_MasterBill
		{
			get => base.JE_MasterBill;
			set
			{
				var oldValue = base.JE_MasterBill;
				base.JE_MasterBill = value;

				if (!IsCopying && value != oldValue)
				{
					TransportDataHelper.DefaultBillOfLadingOnEntryInstructions();
				}
			}
		}

		public override ZString JE_HouseBill
		{
			get => base.JE_HouseBill;
			set
			{
				var oldValue = base.JE_HouseBill;
				base.JE_HouseBill = value;

				if (!IsCopying && value != oldValue && JE_CNTransportMode == CNTransportModeList.Codes.Air)
				{
					TransportDataHelper.DefaultBillOfLadingOnEntryInstructions();
				}
			}
		}

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			base.JE_MessageTypeChanged(oldValue, newValue);

			DefaultOriginDistrictIfNeeded();
			DefaultDestinationDistrictIfNeeded();
			DefaultCountryOfTrade();

			if (JE_CNTransportMode == CNTransportModeList.Codes.Air)
			{
				TransportDataHelper.DefaultBillOfLadingOnEntryInstructions();
			}
			CustomsEntryInstructions.MarkAsNeedingValidationIncludingChildren();
			ChangeInstructionsCIQRequiredIfNeeded();
			UpdateJE_CNTransportMode();

			if (!IsImport)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.JI_PrimaryPreference = ZString.Empty);
			}

			ResetXC_ClearanceModeIfNeeded();
		}

		void ChangeInstructionsCIQRequiredIfNeeded()
		{
			foreach (var instruction in CustomsEntryInstructions.Cast<CusEntryInstruction>())
			{
				instruction.ChangeCIQRequiredIfNeeded();
			}
		}

		public void DefaultOriginDistrictIfNeeded()
		{
			if (!WillGenerateExitingEntry)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x =>
				{
					x.JI_OriginDistrict = ZString.Empty;
					x.JI_OriginRegion = ZString.Empty;
				});
			}
			else
			{
				var district = GetDefaultOriginDistrictCode();

				if (!district.IsEmpty)
				{
					InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x =>
					{
						if (x.JI_OriginDistrict.IsEmpty)
						{
							x.JI_OriginDistrict = district;
						}
					});
				}
			}
		}

		public void DefaultDestinationDistrictIfNeeded()
		{
			if (!WillGenerateEnteringEntry)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x =>
				{
					x.JI_DestinationDistrict = ZString.Empty;
					x.JI_DestinationRegion = ZString.Empty;
				});
			}
			else
			{
				var district = GetDefaultDestinationDistrictCode();
				if (!district.IsEmpty)
				{
					InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x =>
					{
						if (x.JI_DestinationDistrict.IsEmpty)
						{
							x.JI_DestinationDistrict = district;
						}
					});
				}
			}
		}

		public CNOrgImpAddInfo OrgImpAddInfo
		{
			get
			{
				CNOrgImpAddInfo result = null;

				var orgChanged = IsImport ? Importer : IsExport ? Supplier : null;
				if (orgChanged != null)
				{
					result = CNOrgImpAddInfo.Get(orgChanged);
				}

				return result;
			}
		}

		void DefaultOrgAddInfoValues()
		{
			var orgAddInfo = OrgImpAddInfo;
			if (orgAddInfo != null && !orgAddInfo.ZO_MessageSubType.IsEmpty)
			{
				JE_MessageSubType = orgAddInfo.ZO_MessageSubType;
			}
		}

		public void DefaultCountryOfTrade()
		{
			if (WillGenerateBothEntries)
			{
				JE_RN_NKCountryOfTrade = Core.Constants.CountryCodes.China;
			}
			else if (WillGenerateEnteringEntry)
			{
				JE_RN_NKCountryOfTrade = Supplier?.MainAddress?.Country?.Code ?? ZString.Empty;
			}
			else if (WillGenerateExitingEntry)
			{
				JE_RN_NKCountryOfTrade = Importer?.MainAddress?.Country?.Code ?? ZString.Empty;
			}
		}

		protected override void DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource source)
		{
			if (!AnyEntryHasBeenLodgedOrIsWaitingForResponse)
			{
				var isOrgAllInChina = Importer != null && Importer.CountryCode == Core.Constants.CountryCodes.China
					&& Supplier != null && Supplier.CountryCode == Core.Constants.CountryCodes.China;

				if (isOrgAllInChina)
				{
					if (!JE_MessageTypeInfo.ReadOnly)
					{
						if (source == MessageTypeDefaultingTriggerSource.Supplier)
						{
							JE_MessageType = OrgHeaderExtension.IsInSupervisionArea(Supplier) ? Common.Shared.SharedJobMessageTypeList.Codes.Import : Common.Shared.SharedJobMessageTypeList.Codes.Export;
						}
						else if (source == MessageTypeDefaultingTriggerSource.Importer)
						{
							JE_MessageType = OrgHeaderExtension.IsInSupervisionArea(Importer) ? Common.Shared.SharedJobMessageTypeList.Codes.Export : Common.Shared.SharedJobMessageTypeList.Codes.Import;
						}
					}
				}
				else
				{
					base.DefaultMessageTypeFromSupplierOrImporter(source);
				}

				DefaultOrgAddInfoValues();
			}
		}

		#region ContainersRequired

		public override bool ShouldDeleteContainers => !ContainersRequired;

		public override ZBool ContainersRequired =>
			base.ContainersRequired || IsAir || IsPost || (JE_ContainerMode == Core.Constants.ContainerModes.Containerised && (IsRoad || IsRail));

		#endregion

		#region JE_TransportMode

		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set
			{
				var oldValue = JE_TransportMode;
				base.JE_TransportMode = value;
				if (!IsCopying && oldValue != JE_TransportMode)
				{
					if (JE_TransportMode == Core.Constants.TransportModes.Air
						|| JE_TransportMode == Core.Constants.TransportModes.Mail
						|| JE_TransportMode == Core.Constants.TransportModes.Rail
						|| JE_TransportMode == Core.Constants.TransportModes.Road
						|| JE_TransportMode == Core.Constants.TransportModes.FixedTransportInstallations)
					{
						JE_ContainerMode = ZString.Empty;
					}

					TransportDataHelper.DefaultBillOfLadingOnEntryInstructions();
					DefaultValuesFromSupplierImporterLinkTransportMode();
				}
			}
		}

		#endregion

		ZString GetDefaultCNTransportMode(ZString economicZoneType)
		{
			ZString result = ZString.Empty;
			switch (economicZoneType)
			{
				case EconomicZoneTypeList.Codes.BondedArea:
					result = IsImport ? CNTransportModeList.Codes.BondedArea : CNTransportModeList.Codes.NonBondedArea;
					break;
				case EconomicZoneTypeList.Codes.ExportProcessingZone:
				case EconomicZoneTypeList.Codes.InternationalBorder:
					result = CNTransportModeList.Codes.ExportProcessing;
					break;
				case EconomicZoneTypeList.Codes.ComprehensiveBondedArea:
					result = CNTransportModeList.Codes.BondedPort;
					break;
				case EconomicZoneTypeList.Codes.BondedLogisticsZone:
					result = CNTransportModeList.Codes.LogisticPark;
					break;
				case EconomicZoneTypeList.Codes.ComprehensiveExperimentalZone:
					result = CNTransportModeList.Codes.Comprehensive;
					break;
				case EconomicZoneTypeList.Codes.BondedLogisticsCenter:
					result = CNTransportModeList.Codes.LogisticCenter;
					break;
			}
			return result;
		}

		public void UpdateJE_CNTransportMode()
		{
			var transportFlow = JE_CNTransportMode;
			if (TransportDataHelper.IsCrossBorder)
			{
				transportFlow = ZString.Empty;
			}
			else
			{
				if (JE_MessageSubType == DecTypeList.Codes.RecordListing)
				{
					transportFlow = CNTransportModeList.Codes.Others;
				}
				else if (JE_OfficeOfEntryExit == OfficeCodeShenZhenBay)
				{
					transportFlow = CNTransportModeList.Codes.CrossBorder;
				}
				else
				{
					var organization = IsImport ? Supplier : Importer;
					if (organization != null)
					{
						var code = organization.GetEconomicZoneType(OrgCusCode.CodeTypes.CustomsClientCode);
						transportFlow = GetDefaultCNTransportMode(code);
					}

					if (transportFlow.IsEmpty)
					{
						transportFlow = CNTransportModeList.Codes.Others;
					}
				}
			}

			if (transportFlow != JE_CNTransportMode)
			{
				JE_CNTransportMode = transportFlow;
			}
		}

		const string OfficeCodeShenZhenBay = "5345";

		#region JE_RL_NKPortOfLoading

		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|JE_RL_NKPortOfLoading", Caption = "Port Of Loading", ShortCaption = "Loading", FullDescription = "The place where shipments are loaded and secured aboard a vessel/craft. Country of this port will be submitted as Country Of Origin. It may or may not be the same as the Port of Origin.")]
		public override ZString JE_RL_NKPortOfLoading
		{
			get => base.JE_RL_NKPortOfLoading;
			set
			{
				var oldValue = JE_RL_NKPortOfLoading;
				var isCrossBorder = TransportDataHelper.IsCrossBorder;
				base.JE_RL_NKPortOfLoading = value;
				if (!IsCopying && oldValue != JE_RL_NKPortOfLoading && (Shipment == null || base.JE_OverrideFreightDefaults))
				{
					DefaultLastPortBeforeEntryIfNeeded();
					Invoices.MarkAsNeedingValidation();
					if (isCrossBorder != TransportDataHelper.IsCrossBorder)
					{
						UpdateJE_CNTransportMode();
						TransportDataHelper.DefaultBillOfLadingOnEntryInstructions();
						Validation.ValidateJE_CNTransportMode();
					}
				}
			}
		}

		public ZString CNCountryOfLoading => RefCountry.LoadFromCountryCode(Factory, JE_RL_NKPortOfLoading.Left(2))?.GetCNCountryCode() ?? ZString.Empty;

		public ZPropertyInfo CNCountryOfLoadingInfo => GetZPropertyInfo(nameof(CNCountryOfLoading));

		#endregion

		#region JE_RL_NKPortOfArrival
		public override ZString JE_RL_NKPortOfArrival
		{
			get => base.JE_RL_NKPortOfArrival;
			set
			{
				var oldValue = JE_RL_NKPortOfArrival;
				var isCrossBorder = TransportDataHelper.IsCrossBorder;
				base.JE_RL_NKPortOfArrival = value;
				if (!IsCopying && oldValue != JE_RL_NKPortOfArrival && (Shipment == null || base.JE_OverrideFreightDefaults))
				{
					if (isCrossBorder != TransportDataHelper.IsCrossBorder)
					{
						UpdateJE_CNTransportMode();
						TransportDataHelper.DefaultBillOfLadingOnEntryInstructions();
						Validation.ValidateJE_CNTransportMode();
					}
				}
			}
		}

		public ZString CNCountryOfArrival => RefCountry.LoadFromCountryCode(Factory, JE_RL_NKPortOfArrival.Left(2))?.GetCNCountryCode() ?? ZString.Empty;

		public ZPropertyInfo CNCountryOfArrivalInfo => GetZPropertyInfo(nameof(CNCountryOfArrival));

		#endregion

		#region JE_RL_NKOrigin

		public override ZString JE_RL_NKOrigin
		{
			get => base.JE_RL_NKOrigin;
			set
			{
				var oldValue = JE_RL_NKOrigin;
				base.JE_RL_NKOrigin = value;
				if (!IsCopying && oldValue != base.JE_RL_NKOrigin)
				{
					OriginDefaulter.DefaultPort();
				}
			}
		}

		#endregion

		#region JE_RL_NKFinalDestination

		public override ZString JE_RL_NKFinalDestination
		{
			get => base.JE_RL_NKFinalDestination;
			set
			{
				var oldValue = JE_RL_NKFinalDestination;
				base.JE_RL_NKFinalDestination = value;
				if (!IsCopying && oldValue != JE_RL_NKFinalDestination)
				{
					FinalDestinationDefaulter.DefaultPort();
				}
			}
		}

		#endregion

		#region Default value for JE_TotalNoOfPacksPackType

		protected override string DefaultTotalNoOfPacksPackType => string.Empty;

		#endregion

		protected override bool IsPackingInformationRelevantCore => false;

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
		public override ZString JE_CustomsOffice
		{
			get => base.JE_CustomsOffice;
			set
			{
				var oldValue = JE_CustomsOffice;
				base.JE_CustomsOffice = value;
				if (!IsCopying && oldValue != value && JE_OfficeOfEntryExit.IsEmpty)
				{
					JE_OfficeOfEntryExit = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.IncoTermList))]
		public override ZString JE_ShipmentIncoTerm
		{
			get => base.JE_ShipmentIncoTerm;
			set => base.JE_ShipmentIncoTerm = value;
		}

		protected override ZBool IsReciprocalRatesCore => IsReciprocalRatesConstant;

		internal static bool IsReciprocalRatesConstant => true;

		protected override ZString LocalCurrencyCodeCore => LocalCurrencyConstantCode;

		internal static ZString LocalCurrencyConstantCode => Core.Constants.CurrencyCodes.China;

		internal static RefCurrency GetLocalCurrency()
		{
			return RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, LocalCurrencyConstantCode);
		}

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("CN"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => true;

		protected override bool IsCustomsLineAmendmentATotalReplacement => true;

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		protected override void SetDefaultValues()
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				base.SetDefaultValues();

				JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			}
		}

		protected override Customs.Business.MergeManager GetMergeManager()
		{
			return new MergeManager(this);
		}

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		#region JobDocAddresses

		public override JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new CNJobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		CNJobDocAddressDependentCollection fDocAddresses;

		#region ImporterDocumentaryAddress

		public new CNJobDocAddress ImporterDocumentaryAddress => base.ImporterDocumentaryAddress as CNJobDocAddress;

		protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.ImporterDocumentaryAddressChanged(sender, e);
			var importer = ImporterDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
			if (JE_OH_Importer != importer)
			{
				JE_OH_Importer = importer;
			}
		}

		protected override void SetupImporterDocumentaryAddress(JobDocAddress importerDocumentaryAddress)
		{
			base.SetupImporterDocumentaryAddress(importerDocumentaryAddress);
			importerDocumentaryAddress.OrgHeaderAfterChange += ImporterDocumentaryAddress_OrgHeaderAfterChange;
			SetupJobDocAddressRequirement(importerDocumentaryAddress.Requirement);
			SetupJobDocAddressRequirement(GetDocAddressRequirement(DocAddressType.ImporterPickupDeliveryAddress));
		}

		void ImporterDocumentaryAddress_OrgHeaderAfterChange(object sender, EventArgs e)
		{
			ImporterDocumentaryAddress.ContactPK = ImporterDocumentaryAddress.Organisation?.Contacts?.GetContactForAllocation(OrgConstants.ContactAllocationType.CNCUS)?.PK ?? ZGuid.Empty;
		}

		#endregion

		public override bool SupportInvoiceLineRefs => true;

		#region SupplierDocumentaryAddress

		public new CNJobDocAddress SupplierDocumentaryAddress => base.SupplierDocumentaryAddress as CNJobDocAddress;

		protected override void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.SupplierDocumentaryAddressChanged(sender, e);
			var supplier = SupplierDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
			if (JE_OH_Supplier != supplier)
			{
				JE_OH_Supplier = supplier;
			}
		}

		protected override void SetupSupplierDocumentaryAddress(JobDocAddress supplierDocumentaryAddress)
		{
			base.SetupSupplierDocumentaryAddress(supplierDocumentaryAddress);
			supplierDocumentaryAddress.OrgHeaderAfterChange += SupplierDocumentaryAddress_OrgHeaderAfterChange;
			SetupJobDocAddressRequirement(supplierDocumentaryAddress.Requirement);
			SetupJobDocAddressRequirement(GetDocAddressRequirement(DocAddressType.SupplierPickupDeliveryAddress));
		}

		void SupplierDocumentaryAddress_OrgHeaderAfterChange(object sender, EventArgs e)
		{
			SupplierDocumentaryAddress.ContactPK = SupplierDocumentaryAddress.Organisation?.Contacts?.GetContactForAllocation(OrgConstants.ContactAllocationType.CNCUS)?.PK ?? ZGuid.Empty;
		}

		#endregion

		#region BuyerDocumentaryAddress

		public new CNJobDocAddress BuyerDocAddress => base.BuyerDocAddress as CNJobDocAddress;

		protected override void SetupBuyerDocumentaryAddress(JobDocAddress buyerDocAddress)
		{
			base.SetupBuyerDocumentaryAddress(buyerDocAddress);
			buyerDocAddress.OrgHeaderAfterChange += BuyerDocumentaryAddress_OrgHeaderAfterChange;
			SetupJobDocAddressRequirement(buyerDocAddress.Requirement);
		}

		void BuyerDocumentaryAddress_OrgHeaderAfterChange(object sender, EventArgs e)
		{
			BuyerDocAddress.ContactPK = BuyerDocAddress.Organisation?.Contacts?.GetContactForAllocation(OrgConstants.ContactAllocationType.CNCUS)?.PK ?? ZGuid.Empty;

			var buyer = BuyerDocAddress?.OrganisationPK ?? ZGuid.Empty;
			if (JE_OH_Buyer != buyer)
			{
				JE_OH_Buyer = buyer;
			}
		}

		#endregion

		#region ManufacturerDocumentaryAddress

		protected void DefaultManufacturerDocumentaryAddress(ZGuid newValue)
		{
			if (!ManufacturerDocumentaryAddress.E2_AddressOverride)
			{
				ManufacturerDocumentaryAddress.OrganisationPK = newValue;
			}
		}

		public CNJobDocAddress ManufacturerDocumentaryAddress
		{
			get
			{
				if (fManufacturerDocumentaryAddress == null || fManufacturerDocumentaryAddress.IsDeleted)
				{
					fManufacturerDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(ManufacturerRequirement) as CNJobDocAddress;
					if (fManufacturerDocumentaryAddress != null)
					{
						fManufacturerDocumentaryAddress.OrgHeaderAfterChange += ManufacturerDocumentaryAddress_OrgHeaderAfterChange;
						SetupJobDocAddressRequirement(ManufacturerRequirement);
					}
				}
				return fManufacturerDocumentaryAddress;
			}
		}
		CNJobDocAddress fManufacturerDocumentaryAddress;

		void ManufacturerDocumentaryAddress_OrgHeaderAfterChange(object sender, EventArgs e)
		{
			var manufacturer = ManufacturerDocumentaryAddress?.OrganisationPK ?? ZGuid.Empty;
			if (JE_OH_Manufacturer != manufacturer)
			{
				JE_OH_Manufacturer = manufacturer;
			}
		}

		JobDocAddressRequirement ManufacturerRequirement
		{
			get
			{
				if (fManufacturerRequirement == null)
				{
					fManufacturerRequirement = new JobDocAddressRequirement(DocAddressType.Manufacturer, ContactType.Consignor);
					DocAddressManager.AddRequirement(fManufacturerRequirement);
				}
				return fManufacturerRequirement;
			}
		}
		JobDocAddressRequirement fManufacturerRequirement;

		#endregion

		void SetupJobDocAddressRequirement(JobDocAddressRequirement requirement)
		{
			if (requirement != null && requirement.ValidateAddress1 == null)
			{
				requirement.ValidateAddress1 = CNJobDocAddressValidation.ValidateAddress1;
				requirement.ValidateAddress2 = CNJobDocAddressValidation.NoValidation;
				requirement.ValidateCountry = CNJobDocAddressValidation.ValidateCountry;
				requirement.ValidateCity = CNJobDocAddressValidation.ValidateCity;
				requirement.ValidateState = CNJobDocAddressValidation.NoValidation;
				requirement.ValidatePostCode = CNJobDocAddressValidation.NoValidation;
			}
		}

		#endregion

		void OnMessageSubTypeChanging(CancelEventArgs args)
		{
			MessageSubTypeChanging?.Invoke(this, args);
		}

		public event CancelEventHandler MessageSubTypeChanging;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MessageSubTypeList))]
		public override ZString JE_MessageSubType
		{
			get => base.JE_MessageSubType;
			set
			{
				var oldValue = JE_MessageSubType;

				CancelEventArgs args = new CancelEventArgs();
				OnMessageSubTypeChanging(args);

				if (!args.Cancel)
				{
					base.JE_MessageSubType = value;
					if (!IsCopying && oldValue != JE_MessageSubType)
					{
						DefaultOriginDistrictIfNeeded();
						DefaultDestinationDistrictIfNeeded();
						DefaultCountryOfTrade();

						if (!WillGenerateBothEntries)
						{
							CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(x => x.CEI_CEI_Parent = ZGuid.Empty);
						}
						ChangeInstructionsCIQRequiredIfNeeded();

						ResetXC_ClearanceModeIfNeeded();
					}
				}
			}
		}

		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				if (JE_MessageType != value)
				{
					base.JE_MessageType = value;
					RefreshIncotermAndChargeFactory();
				}
			}
		}

		public override bool IsMessageTypeChangeAnError => CustomsEntryHeaders.Any(x => x.HasBeenLodgedAtCustoms || x.IsWaitingForResponse);

		public bool IsInlandCarNumberApplicable => IsInlandRoadTransport || IsInlandRailTransport;

		public override ZString JE_ContainerMode
		{
			get => base.JE_ContainerMode;
			set
			{
				var oldValue = JE_ContainerMode;
				base.JE_ContainerMode = value;
				if (oldValue != value)
				{
					DefaultValuesFromSupplierImporterLinkTransportMode();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|JE_LocationOfGoods", Caption = "Location of Goods")]
		public override ZString JE_LocationOfGoods
		{
			get => base.JE_LocationOfGoods;
			set => base.JE_LocationOfGoods = value;
		}

		public override ZDateTime JE_DateOfArrival
		{
			get => base.JE_DateOfArrival;
			set
			{
				var oldValue = JE_DateOfArrival;
				base.JE_DateOfArrival = value;
				if (!IsCopying && oldValue != value)
				{
					Invoices.ForEach(invoice => invoice.MarkAsNeedingValidation());
					InvoiceLines.ForEach(line => line.MarkAsNeedingValidation());
				}
			}
		}

		public override ZDateTime DateOfValuation
		{
			get
			{
				var result = ZDateTime.Empty;

				if (CustomsEntryInstructions.Any<CusEntryInstruction>())
				{
					result = CustomsEntryInstructions.Cast<CusEntryInstruction>().Min(ins => ins.CEI_DateForDuty);
				}

				if (result.IsEmpty && IsImport && JE_DateOfArrival.IsInTheFuture())
				{
					result = JE_DateOfArrival;
				}

				if (result.IsEmpty)
				{
					result = ZDateTime.Today;
				}
				return result;
			}
		}

		public override ZGuid JE_OH_Supplier
		{
			get => base.JE_OH_Supplier;
			set
			{
				ZGuid oldValue = JE_OH_Supplier;
				base.JE_OH_Supplier = value;

				if (value != oldValue)
				{
					InvoiceLines.MarkAsNeedingValidation();
					DefaultValuesFromSupplierImporterLinkTransportMode();
				}
			}
		}

		public override ZGuid JE_OH_Importer
		{
			get => base.JE_OH_Importer;
			set
			{
				var oldValue = JE_OH_Importer;
				base.JE_OH_Importer = value;
				if (value != oldValue)
				{
					DefaultValuesFromSupplierImporterLinkTransportMode();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportModeInlandList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|JE_TransportModeInland", Caption = "Transport Mode Inland")]
		public override ZString JE_TransportModeInland { get => base.JE_TransportModeInland; set => base.JE_TransportModeInland = value; }

		#endregion

		#region AddInfo Properties

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
		public override ZString JE_OfficeOfEntryExit { get => base.JE_OfficeOfEntryExit; set => base.JE_OfficeOfEntryExit = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CNTransportModeCodes))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|XC_CNTransportMode", Caption = "Transport Flow", ShortCaption = "Trans. Flow")]
		public override ZString JE_CNTransportMode
		{
			get => base.JE_CNTransportMode;
			set
			{
				var oldValue = base.JE_CNTransportMode;
				base.JE_CNTransportMode = value;

				if (!IsCopying && value != oldValue)
				{
					TransportDataHelper.DefaultBillOfLadingOnEntryInstructions();
					MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(6)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CNPortList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|XC_CNPortOfOrigin", Caption = "Port Of Origin", ShortCaption = "Loading")]
		public override ZString JE_CNPortOfOrigin
		{
			get => base.JE_CNPortOfOrigin;
			set
			{
				var oldValue = base.JE_CNPortOfOrigin;
				base.JE_CNPortOfOrigin = value;
				if (!IsCopying && oldValue != base.JE_CNPortOfOrigin)
				{
					OriginDefaulter.DefaultUNLOCO();
					MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(6)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CNPortList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|XC_CNPortOfDestination", Caption = "Final Destination", ShortCaption = "Discharge")]
		public override ZString JE_CNPortOfDestination
		{
			get => base.JE_CNPortOfDestination;
			set
			{
				var oldValue = base.JE_CNPortOfDestination;
				base.JE_CNPortOfDestination = value;
				if (!IsCopying && oldValue != base.JE_CNPortOfDestination)
				{
					FinalDestinationDefaulter.DefaultUNLOCO();
					MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(6)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CNPortList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|XC_CNLastPortBeforeEntry", Caption = "Port of Stopover", ShortCaption = "Stopover")]
		public override ZString JE_CNLastPortBeforeEntry { get => base.JE_CNLastPortBeforeEntry; set => base.JE_CNLastPortBeforeEntry = value; }

		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|XC_DateOfUnloadComplete", Caption = "Date of Unload Complete")]
		public override ZDateTime JE_DateOfUnloadComplete
		{
			get => base.JE_DateOfUnloadComplete;
			set => base.JE_DateOfUnloadComplete = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CountryOfTrades))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|XC_RN_NKCountryOfTrade", Caption = "Country Of Trade")]
		public override ZString JE_RN_NKCountryOfTrade { get => base.JE_RN_NKCountryOfTrade; set => base.JE_RN_NKCountryOfTrade = value; }

		#region UNLOCO_CNPortsDefaulter

		public UNLOCO_CNPortsDefaulter OriginDefaulter => fOriginDefaulter ?? (fOriginDefaulter = new UNLOCO_CNPortsDefaulter(Factory, JE_CNPortOfOriginInfo, JE_RL_NKOriginInfo));
		UNLOCO_CNPortsDefaulter fOriginDefaulter;

		public UNLOCO_CNPortsDefaulter FinalDestinationDefaulter => fFinalDestination ?? (fFinalDestination = new UNLOCO_CNPortsDefaulter(Factory, JE_CNPortOfDestinationInfo, JE_RL_NKFinalDestinationInfo));
		UNLOCO_CNPortsDefaulter fFinalDestination;

		internal UNLOCO_CNPortsDefaulter LastPortBeforeEntryDefaulter => fLastPortBeforeEntryDefaulter ?? (fLastPortBeforeEntryDefaulter = new UNLOCO_CNPortsDefaulter(Factory, JE_CNLastPortBeforeEntryInfo, JE_LastPortBeforeEntryInfo, true));
		UNLOCO_CNPortsDefaulter fLastPortBeforeEntryDefaulter;

		#endregion

		public bool AnyEntryHasBeenLodgedOrIsWaitingForResponse
		{
			get
			{
				return CustomsEntryHeaders.Any(x => x.HasBeenLodgedAtCustoms) || CustomsEntryHeaders.Any(x => x.IsWaitingForResponse);
			}
		}

		public bool IsTwoStepDeclarationApplicable => CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.Value && IsImport && !WillGenerateBothEntries;

		public bool ClearanceModeReadOnly => AnyEntryHasBeenLodgedOrIsWaitingForResponse || !IsTwoStepDeclarationApplicable;

		public bool FullValidationReadOnly => !IsTwoStepDeclarationApplicable || JE_ClearanceMode != ClearanceModeList.Codes.TwoStep;

		void ResetXC_ClearanceModeIfNeeded()
		{
			if (!AnyEntryHasBeenLodgedOrIsWaitingForResponse && !IsTwoStepDeclarationApplicable)
			{
				JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			}
		}

		public bool IsTwoStepDeclaration => (JE_ClearanceMode == ClearanceModeList.Codes.TwoStep || JE_ClearanceMode == ClearanceModeList.Codes.TwoStepManual || JE_ClearanceMode == ClearanceModeList.Codes.TwoStepAuto);

		public ZString ClearanceModeDetailedDescription
		{
			get
			{
				var result = new ZStringBuilder();
				if (IsTwoStepDeclaration)
				{
					result.Append(Lookups.ClearanceModeCodes.GetDescriptionFromCode(JE_ClearanceMode));
					result.Append(GetDescriptionForBoolProperty(JE_LicenseInvolvedInfo));
					result.Append(GetDescriptionForBoolProperty(JE_InspectionInvolvedInfo));
					result.Append(GetDescriptionForBoolProperty(JE_TaxInvolvedInfo));
				}
				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		string GetDescriptionForBoolProperty(ZPropertyInfo propertyInfo)
		{
			string description = propertyInfo.HumanReadableName;
			if (propertyInfo.Value is ZBool value && !value)
			{
				description = Res.GetString("35EA5D37-2B35-4A7A-81E7-5B358C70C0FA", "non {0}", description);
			}
			return description;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CNCIQPortList))]
		public override ZString JE_CIQOfficeOfEntryExit { get => base.JE_CIQOfficeOfEntryExit; set => base.JE_CIQOfficeOfEntryExit = value; }

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ClearanceModeCodes))]
		[ReadOnlyMember(nameof(ClearanceModeReadOnly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|XC_ClearanceMode", Caption = "Clearance Mode")]
		public override ZString JE_ClearanceMode
		{
			get => base.JE_ClearanceMode;
			set
			{
				var oldValue = base.JE_ClearanceMode;
				base.JE_ClearanceMode = value;
				if (!IsCopying && oldValue != JE_ClearanceMode)
				{
					if (!IsTwoStepDeclaration)
					{
						JE_LicenseInvolved = false;
						JE_InspectionInvolved = false;
						JE_TaxInvolved = false;
					}

					DefaultValidationMode();
					InvoiceLines?.MarkAsNeedingValidation();
				}
			}
		}

		void DefaultValidationMode()
		{
			FullValidation = FullValidationReadOnly;
		}

		public ValidationModes ValidationMode { get; set; }

		[ReadOnlyMember(nameof(FullValidationReadOnly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|FullValidation", Caption = "Full Validation")]
		public ZBool FullValidation
		{
			get => ValidationMode == ValidationModes.Full;
			set
			{
				ValidationMode = value ? ValidationModes.Full : ValidationModes.Preliminary;
				FullValidationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FullValidationInfo => GetZPropertyInfo(Schema.FullValidation);

		[ReadOnlyMember(nameof(AnyEntryHasBeenLodgedOrIsWaitingForResponse))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|JE_LicenseInvolved", Caption = "Certification-Related", FullDescription = "Whether the imported goods are restricted and controlled articles.")]
		public override ZBool JE_LicenseInvolved { get => base.JE_LicenseInvolved; set => base.JE_LicenseInvolved = value; }

		[ReadOnlyMember(nameof(AnyEntryHasBeenLodgedOrIsWaitingForResponse))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|JE_InspectionInvolved", Caption = "Inspection-Related", FullDescription = "Whether the imported goods are subject to inspection or quarantine according to law.")]
		public override ZBool JE_InspectionInvolved { get => base.JE_InspectionInvolved; set => base.JE_InspectionInvolved = value; }

		[ReadOnlyMember(nameof(AnyEntryHasBeenLodgedOrIsWaitingForResponse))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|JE_TaxInvolved", Caption = "Tax-Related", FullDescription = "Whether the imported goods need to be taxed.")]
		public override ZBool JE_TaxInvolved { get => base.JE_TaxInvolved; set => base.JE_TaxInvolved = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransitModeCodes))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|JE_TransitMode", Caption = "Transit Mode")]
		public override ZString JE_TransitMode
		{
			get => base.JE_TransitMode;
			set
			{
				var oldValue = base.JE_TransitMode;
				base.JE_TransitMode = value;
				if (!IsCopying && oldValue != JE_TransitMode)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|JE_VesselInland", Caption = "Vessel Inland")]
		public override ZString JE_VesselInland { get => base.JE_VesselInland; set => base.JE_VesselInland = value; }

		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|JE_VoyageInland", Caption = "Voyage Inland")]
		public override ZString JE_VoyageInland { get => base.JE_VoyageInland; set => base.JE_VoyageInland = value; }

		#endregion

		#region Customs Offices

		[ChildEditable(true)]
		public CusCodeDataCollection<CustomsOffice> CustomsOffices
		{
			get
			{
				if (customsOffice == null)
				{
					customsOffice = new CusCodeDataCollection<CustomsOffice>(this, Constants.CusCodeDataTypes.Codes.CustomsOffice);
					customsOffice.Load();
					RegisterEditableChildObject(customsOffice);
				}
				return customsOffice;
			}
		}
		CusCodeDataCollection<CustomsOffice> customsOffice;

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobDeclaration|OfficeOfDestination", Caption = "Office of Destination")]
		public ZString OfficeOfDestination
		{
			get => officeOfDestination.CY_Data;
			set
			{
				officeOfDestination.CY_Data = value;
				OfficeOfDestinationInfo.RefreshBinding();
			}
		}

		CustomsOffice officeOfDestination => CustomsOffices.GetFirstElementHaving(CustomsOfficeTypeList.Codes.DES) ?? CustomsOffices.AddNew();

		public ZPropertyInfo OfficeOfDestinationInfo => GetWrappedZPropertyInfo(Schema.OfficeOfDestination, x => officeOfDestination.CY_DataInfo);

		#endregion

		#region Merging Rules

		[ChildEditable(true)]
		public MergingRuleOptionCollection MergingRuleOptions
		{
			get
			{
				if (fMergingRuleOptions == null)
				{
					fMergingRuleOptions = new MergingRuleOptionCollection(this);
					fMergingRuleOptions.Load();
					RegisterEditableChildObject(fMergingRuleOptions);
				}
				return fMergingRuleOptions;
			}
		}
		MergingRuleOptionCollection fMergingRuleOptions;

		[ChildEditable(true)]
		public MergingRuleCollection MergingRules
		{
			get
			{
				if (fMergingRules == null)
				{
					fMergingRules = new MergingRuleCollection(this);
					fMergingRules.Load();
					RegisterEditableChildObject(fMergingRules);
				}

				fMergingRuleOptions?.RefreshSelectionCollection();

				return fMergingRules;
			}
		}
		MergingRuleCollection fMergingRules;

		#endregion

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ Constants.CusCodeDataTypes.Codes.MergingRule, typeof(MergingRule) },
				{ Constants.CusCodeDataTypes.Codes.CustomsOffice, typeof(CustomsOffice) }
			};
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			fMergingRuleOptions?.RefreshSelectionCollection();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			DefaultValidationMode();
		}

		#region Boolean

		public ZBool WillGenerateCustomsEntry => JE_MessageSubType == DecTypeList.Codes.CustomsEntry || WillGenerateBothEntries;

		public ZBool WillGenerateRecordListing => JE_MessageSubType == DecTypeList.Codes.RecordListing || WillGenerateBothEntries;

		public ZBool WillGenerateBothEntries => JE_MessageSubType == DecTypeList.Codes.Both;

		public ZBool WillGenerateEnteringEntry => IsImport || WillGenerateBothEntries;

		public ZBool WillGenerateExitingEntry => IsExport || WillGenerateBothEntries;

		public ZBool CIQRequires => CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.CEI_CIQRequires);

		public bool IsCustomsTransit => !JE_TransitMode.IsEmpty;
		public bool IsTransshipment => JE_TransitMode == TransitModeList.Codes.Transshipment;
		public bool IsDirectTransition => JE_TransitMode == TransitModeList.Codes.DirectTransition;
		public bool IsDeclaringInAdvance => JE_TransitMode == TransitModeList.Codes.DeclaringInAdvance;

		public bool IsInlandWaterwayTransport => JE_TransportModeInland == Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
		public bool IsInlandRoadTransport => JE_TransportModeInland == Customs.Business.TransportTypeList.Codes.Road;
		public bool IsInlandRailTransport => JE_TransportModeInland == Customs.Business.TransportTypeList.Codes.Rail;

		#endregion

		public TransportDataHelper TransportDataHelper => fTransportDataHelper ?? (fTransportDataHelper = new TransportDataHelper(this));

		TransportDataHelper fTransportDataHelper;

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
		{
			return new DeclarationValueChangedAnnouncer(this);
		}

		protected override ZString GetTransportModeGeneric()
		{
			switch (JE_TransportMode)
			{
				case TransportTypeList.Codes.Air:
					return TransportTypeGenericList.Codes.Air;

				case TransportTypeList.Codes.Sea:
					return TransportTypeGenericList.Codes.Sea;

				case TransportTypeList.Codes.Mail:
					return TransportTypeGenericList.Codes.PostMail;

				case TransportTypeList.Codes.Road:
					return TransportTypeGenericList.Codes.Road;

				case TransportTypeList.Codes.Rail:
					return TransportTypeGenericList.Codes.Rail;

				case TransportTypeList.Codes.FixedTransportInstallations:
				case TransportTypeList.Codes.PassengerCarried:
					return TransportTypeGenericList.Codes.Other;
			}
			return ZString.Empty;
		}

		public void DefaultValuesFromSupplierImporterLinkTransportMode()
		{
			var mode = SupplierImporterLink?.OrgSupBuyLinkTrnModes.Find(GetTransportModeGeneric(), ContainerMode);
			if (mode != null)
			{
				var addInfoBizObj = new OrgSupBuyLinkTrnModeAddInfoBizObj((OrgSupBuyLinkTrnModeAddInfo)mode.AddInfo);

				if (!addInfoBizObj.ZO_CustomsOffice.IsEmpty)
				{
					JE_CustomsOffice = addInfoBizObj.ZO_CustomsOffice;
				}
				if (!addInfoBizObj.ZO_OfficeOfEntryExit.IsEmpty)
				{
					JE_OfficeOfEntryExit = addInfoBizObj.ZO_OfficeOfEntryExit;
				}
				if (!addInfoBizObj.ZO_CIQOfficeOfEntryExit.IsEmpty)
				{
					JE_CIQOfficeOfEntryExit = addInfoBizObj.ZO_CIQOfficeOfEntryExit;
				}
				if (!addInfoBizObj.OfficeOfDestination.IsEmpty)
				{
					OfficeOfDestination = addInfoBizObj.OfficeOfDestination.Left(OfficeOfDestinationInfo.MaxLength);
				}
			}
		}

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			CodeDescriptionPairList result = null;
			if (category == CusEntryNumber.Categories.AdditionalReferenceNumber)
			{
				result = Factory.GetCachedValue("CNAdditionalReferenceNumberTypes", () =>
				{
					var tempList = new AdditionalReferenceNumberTypes();
					tempList.AddRange(CusEntryNumLookups.GetAdditionalReferenceNumberTypes(countryCode));
					return tempList;
				}
			   );
			}
			else
			{
				result = GetAdditionalReferenceNumberTypeListCore(category, countryCode);
			}
			return result;
		}

		Type ICusEntryNumberValidationDeciderOfType.GetCusEntryNumberValidationType() => typeof(JobDeclarationEntryNumValidation);

		protected override bool EntryTypeShouldBeUniqueCore(ZString entryType, ZString category, ZString countryCode)
		{
			return !(entryType == AdditionalReferenceNumberTypes.Codes.ProposalNo || entryType == AdditionalReferenceNumberTypes.Codes.WGQWarehouseNumber);
		}

		#region New Methods

		internal ZString GetDefaultOriginDistrictCode() => Manufacturer?.LocalCustomsClientCode.Left(5) ??
				(Supplier?.LocalCustomsClientCode.Left(5) ?? ZString.Empty);

		internal ZString GetDefaultDestinationDistrictCode() => Buyer?.LocalCustomsClientCode.Left(5) ??
				(Importer?.LocalCustomsClientCode.Left(5) ?? ZString.Empty);

		#endregion

		#region New Properties

		public ZInt RemainingDaysForDeclaration
		{
			get
			{
				var result = 0;
				var array = CustomsEntryHeaders.Where(x => x.RemainingDaysForDeclaration != 0);
				if (array.Any())
				{
					result = array.Min(x => x.RemainingDaysForDeclaration);
				}
				return result;
			}
		}

		public ZString BrokerCertificateNumber => CusAgent.GetCertificationNumber(StaffCertType.BRK, ZDateTime.Today);

		public ZString NameOnBrokerCertificate => CusAgent.GetNameOnCertificate(StaffCertType.BRK, ZDateTime.Today, BrokerName);

		public ZString OperatorCardID => CusAgent.GetCertificationNumber(StaffCertType.CNO, ZDateTime.Today);

		public ZString NameOnOperatorCard => CusAgent.GetNameOnCertificate(StaffCertType.CNO, ZDateTime.Today, BrokerName);

		#endregion

		#region IApportionInvoiceHolder Members

		System.Collections.IComparer IApportionInvoiceHolder.ChargeComparer => new ChargesComparer();

		string IApportionInvoiceHolder.CountryContext => CountryCode + this.GetIncoTermChargeFactoryCacheKey();

		#endregion

		public class JobDeclarationInvoicingSupporter : BaseJobDeclarationInvoicingSupporter
		{
			public JobDeclarationInvoicingSupporter(JobDeclaration parent)
				: base(parent)
			{
				this.parent = parent;
			}
			protected readonly JobDeclaration parent;

			protected override ZGuid OverridenDepartment =>
				JobInvoicingTransportMode == TransportTypeList.Codes.PassengerCarried ?
					(IsImport ? CargoWise.Application.ObjectFactory.Get<Integration.Accounting.IAccounting>().CustomsImportOther : CargoWise.Application.ObjectFactory.Get<Integration.Accounting.IAccounting>().CustomsOther) :
					base.OverridenDepartment;
		}

		public ZBool NoMerge => JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMerge || JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription;

		public ZDateTime DeclarationDeadline => !IsImport || WillGenerateBothEntries || !JE_DateOfArrival.IsValid ? ZDateTime.Empty : JE_DateOfArrival.AddDays(14);
	}
}
