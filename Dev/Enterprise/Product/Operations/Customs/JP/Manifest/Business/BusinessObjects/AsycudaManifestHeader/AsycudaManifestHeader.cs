using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.JP.Manifest.Business.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader
		, Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader
		, IBranchProvider
		, ISupportMultipleResourceStringData
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const int AMA_SubConsolMAWBLength = 16;
			public const int AMA_InputReferenceLength = 10;
			public const int AMA_BookingNumberLength = 16;
		}

		public override ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One;

		#region ISupportMultipleResourceStringData

		public const string CaptionKeyAir = Core.Constants.TransportModes.Air;

		public const string CaptionKeySea = Core.Constants.TransportModes.Sea;

		public const string CaptionKeyImp = JPJobMessageTypeList.Codes.Import;

		public const string CaptionKeyExp = JPJobMessageTypeList.Codes.Export;

		public IReadOnlyList<string> MultipleKeysToUse => new string[] { $"{AMA_TransportMode}{AMA_Nature}" };
		#endregion

		public ZString ConsolidatorNACCSUserCode => Consolidator?.CustomsCodes.GetCustomsRegNo(OrgCusCode.JapanCodeTypes.NUC, Core.Constants.CountryCodes.Japan) ?? ZString.Empty;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.PortOfFinalDepartureCollection))]
		[ResourceStringData("JPAsycudaManifestHeader.AMA_RL_NKPortOfFinalDeparture", Caption = "Move-In Destination", ShortCaption = "Move-In Dest.", FullDescription = "Move-In Destination Code")]
		public override ZString AMA_RL_NKPortOfFinalDeparture { get => base.AMA_RL_NKPortOfFinalDeparture; set => base.AMA_RL_NKPortOfFinalDeparture = value; }

		public override ZString AMA_RL_NKPortOfLoading
		{
			get => base.AMA_RL_NKPortOfLoading;
			set
			{
				var previousValue = base.AMA_RL_NKPortOfLoading;
				if (previousValue != value)
				{
					base.AMA_RL_NKPortOfLoading = value;
					PortOfLoadingIATACode = PortOfLoading?.RL_IATA ?? ZString.Empty;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAMA_RL_NKPortOfLoading();
						Validation.ValidatePortOfLoadingIATACode();
					}
				}
			}
		}

		[ResourceStringData("9E456C92-01EE-4FDF-987E-2C0F23118C33", Caption = "Procedure")]
		[ReadOnlyMember(nameof(AMA_ManifestType_ReadOnly))]
		public override ZString AMA_ManifestType
		{
			get => base.AMA_ManifestType;
			set
			{
				var oldValue = base.AMA_ManifestType;
				base.AMA_ManifestType = value;
				if (!IsCopying && oldValue != AMA_ManifestType)
				{
					DefaultTransportModeFromManifestType();
				}
			}
		}
		ZBool AMA_ManifestType_ReadOnly => !IsStandAlone;

		void DefaultTransportModeFromManifestType()
		{
			if (ManifestType != null)
			{
				AMA_TransportMode = ManifestType.Code switch
				{
					JPManifestTypeCodeList.Codes.HCH or JPManifestTypeCodeList.Codes.HDF => TransportTypeList.Codes.Air,
					JPManifestTypeCodeList.Codes.NVC or JPManifestTypeCodeList.Codes.VAN => TransportTypeList.Codes.Sea,
					_ => AMA_TransportMode
				};
			}
		}

		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsol))]
		[ResourceStringData("JPAsycudaManifestHeader.PortOfLoadingIATACode", Caption = "IATA Code")]
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
							AMA_RL_NKPortOfLoading = refUNLOCO.RL_Code;
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

		public override ZString AMA_RL_NKPortOfDischarge
		{
			get => base.AMA_RL_NKPortOfDischarge;
			set
			{
				var previousValue = base.AMA_RL_NKPortOfDischarge;
				if (previousValue != value)
				{
					base.AMA_RL_NKPortOfDischarge = value;
					PortOfDischargeIATACode = PortOfDischarge?.RL_IATA ?? ZString.Empty;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAMA_RL_NKPortOfDischarge();
						Validation.ValidatePortOfDischargeIATACode();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsol))]
		[ResourceStringData("JPAsycudaManifestHeader.PortOfDischargeIATACode", Caption = "IATA Code")]
		public ZString PortOfDischargeIATACode
		{
			get => isPortOfDischargeIATACodeOverridden ? portOfDischargeIATACode : PortOfDischarge?.RL_IATA ?? portOfDischargeIATACode;
			set
			{
				if (value != portOfDischargeIATACode)
				{
					SetNonPersistentPropertyValue(PortOfDischargeIATACodeInfo, ref portOfDischargeIATACode, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePortOfDischargeIATACode();
					}
					if (!portOfDischargeIATACode.IsEmpty)
					{
						var refUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_IATA, value));
						if (refUNLOCO != null)
						{
							AMA_RL_NKPortOfDischarge = refUNLOCO.RL_Code;
							isPortOfDischargeIATACodeOverridden = false;
						}
						else
						{
							isPortOfDischargeIATACodeOverridden = true;
						}
					}
				}
			}
		}

		ZString portOfDischargeIATACode;
		bool isPortOfDischargeIATACodeOverridden;

		public ZPropertyInfo PortOfDischargeIATACodeInfo => GetZPropertyInfo(nameof(PortOfDischargeIATACode));

		[ReadOnly(true)]
		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				var previousValue = base.AMA_TransportMode;
				if (previousValue != value)
				{
					base.AMA_TransportMode = value;

					if (string.IsNullOrWhiteSpace(AMA_GS_NKCustomsAgent) && CustomsAgentCredential == null)
					{
						var defaultBrokerAndCredential = JPRegistry.Instance.DefaultBrokerAndCredential.Value;
						var defaultBroker = defaultBrokerAndCredential?.DefaultBroker;
						if (defaultBroker != null)
						{
							AMA_GS_NKCustomsAgent = defaultBroker.GS_Code;
							var descriptionString = IsAir ? defaultBrokerAndCredential.ForwarderManifestAIR : IsSea ? defaultBrokerAndCredential.ForwarderManifestSEA : string.Empty;
							AMA_CustomsAgentCredentialPK = Lookups.NaccsCredentialList.FirstOrDefault(x => x.GP_MailBoxID == descriptionString.SubstringSafe(0, 5) && x.GP_UserID == descriptionString.SubstringSafe(5, 3))?.PK ?? ZGuid.Empty;
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.NaccsCredentialList))]
		[ResourceStringData("JPAsycudaManifestHeader.AMA_CustomsAgentCredentialPK", Caption = "NACCS Credential")]
		public ZGuid AMA_CustomsAgentCredentialPK
		{
			get => this.GetSystemDefinedValue<ZGuid>(GenAddOnColumnFieldName.AMA_CustomsAgentCredentialPK);
			set
			{
				var oldValue = AMA_CustomsAgentCredentialPK;
				this.SetSystemDefinedValue(GenAddOnColumnFieldName.AMA_CustomsAgentCredentialPK, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAMA_CustomsAgentCredentialPK();
				}
				AMA_CustomsAgentCredentialPKInfo.RefreshBinding(oldValue);
			}
		}

		public GlbExternalPasswordCUS CustomsAgentCredential => Factory.Load<GlbExternalPasswordCUS>(AMA_CustomsAgentCredentialPK);

		public ZPropertyInfo AMA_CustomsAgentCredentialPKInfo => GetZPropertyInfo(nameof(AMA_CustomsAgentCredentialPK));

		[ResourceStringData("JPAsycudaManifestHeader.AMA_GS_NKCustomsAgent", Caption = "Customs Agent")]
		public override ZString AMA_GS_NKCustomsAgent
		{
			get => base.AMA_GS_NKCustomsAgent;
			set
			{
				var oldValue = AMA_GS_NKCustomsAgent;
				if (oldValue != value)
				{
					base.AMA_GS_NKCustomsAgent = value;
					if (string.IsNullOrWhiteSpace(AMA_GS_NKCustomsAgent))
					{
						AMA_CustomsAgentCredentialPK = Guid.Empty;
					}
					else
					{
						AMA_CustomsAgentCredentialPK = Lookups.NaccsCredentialList.FirstOrDefault()?.PK ?? Guid.Empty;
					}
				}
			}
		}

		public override ZString AMA_CarrierCode
		{
			get => base.AMA_CarrierCode;
			set
			{
				var oldValue = AMA_CarrierCode;
				if (oldValue != value)
				{
					base.AMA_CarrierCode = value;
					if (AMA_OA_Carrier.IsEmpty)
					{
						if (IsSea)
						{
							UpdateOrgAddressFromSCACCode(AMA_CarrierCode);
						}
						else if (IsAir && AMA_CarrierCode.Length == 2)
						{
							UpdateOrgAddressFromAirlineTwoLetterCode(AMA_CarrierCode);
						}
					}
				}
			}
		}

		internal bool NeedToUpdateRegistrationStatus { get; set; }

		internal bool NeedToUpdateMessageStatus { get; set; }

		[List(nameof(Lookups) + "." + nameof(ASYCUDA.Business.AsycudaManifestHeaderLookups.CustomsOffices))]
		[MaxLength(2)]
		public override ZString AMA_CustomsOffice { get => base.AMA_CustomsOffice; set => base.AMA_CustomsOffice = value; }

		[MaxLength(20)]
		public override ZString AMA_MasterBill { get => base.AMA_MasterBill; set => base.AMA_MasterBill = value; }

		public override ZGuid AMA_OA_Carrier
		{
			get => base.AMA_OA_Carrier;
			set
			{
				var oldValue = AMA_OA_Carrier;
				if (oldValue != value)
				{
					base.AMA_OA_Carrier = value;
					if (AMA_CarrierCode.IsEmpty)
					{
						if (IsAir)
						{
							AMA_CarrierCode = Carrier?.Header?.MiscServ?.Airline?.RM_TwoCharacterCode ?? ZString.Empty;
						}
						else if (IsSea)
						{
							AMA_CarrierCode = Carrier?.Header?.ShippingLineSCAC ?? ZString.Empty;
						}
					}
				}
			}
		}

		[ReadOnly(true)]
		public override ZString AMA_Nature
		{
			get => base.AMA_Nature;
			set
			{
				base.AMA_Nature = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAMA_MasterBill();
				}
			}
		}

		[ResourceStringData("JPAsycudaManifestHeader.IsSubConsolidation", Caption = "Is Sub-Consolidation?", ShortCaption = "Is Sub-Consol?")]
		public ZBool IsSubConsolidation
		{
			get => IsAir && IsImport && AMA_AgentType == AgentTypeList.Codes.H;
			set
			{
				isSubConsolidation = value;
				AMA_AgentType = isSubConsolidation ? AgentTypeList.Codes.H : AgentTypeList.Codes.M;
				IsSubConsolidationInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateAMA_MasterBill();
				}
			}
		}

		ZBool isSubConsolidation;

		public ZPropertyInfo IsSubConsolidationInfo => GetZPropertyInfo(nameof(IsSubConsolidation));

		[ReadOnly(true)]
		[MaxLength(Schema.AMA_InputReferenceLength)]
		[ResourceStringData("JPAsycudaManifestHeader.AMA_InputReference", Caption = "Input Reference")]
		public ZString AMA_InputReference
		{
			get => InputReferenceEntryNumber?.CE_EntryNum ?? string.Empty;
			set
			{
				var entryNumber = InputReferenceEntryNumber;
				var number = entryNumber?.CE_EntryNum ?? ZString.Empty;

				if (!value.IsEmpty)
				{
					if (entryNumber == null)
					{
						inputReferenceEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.JP.InputReference, AMA_RN_NKCountry);
						RegisterEditableChildObject(inputReferenceEntryNumber);
					}

					CheckMaximumLength(AMA_InputReferenceInfo, value);
					inputReferenceEntryNumber.CE_EntryNum = value;
					inputReferenceEntryNumber.CE_EntryIsSystemGenerated = false;
				}
				else if (entryNumber != null)
				{
					entryNumber.CE_EntryNum = value;
				}

				AMA_InputReferenceInfo.RefreshBinding(number);
			}
		}

		public ZPropertyInfo AMA_InputReferenceInfo => GetZPropertyInfo(nameof(AMA_InputReference));

		CusEntryNumber InputReferenceEntryNumber
		{
			get
			{
				if (inputReferenceEntryNumber == null || inputReferenceEntryNumber.IsDeleted || inputReferenceEntryNumber.CE_EntryType != CusEntryNumberTypes.JP.InputReference)
				{
					inputReferenceEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.JP.InputReference, AMA_RN_NKCountry);

					if (inputReferenceEntryNumber != null)
					{
						RegisterEditableChildObject(inputReferenceEntryNumber);
					}
				}

				return inputReferenceEntryNumber;
			}
		}
		CusEntryNumber inputReferenceEntryNumber;

		[MaxLength(Schema.AMA_BookingNumberLength)]
		[ResourceStringData("JPAsycudaManifestHeader.AMA_BookingNumber", Caption = "Booking Number", ShortCaption = "Booking#", FullDescription = "VAN/VAE Booking Number")]
		public ZString AMA_BookingNumber
		{
			get => BookingNumberEntryNumber?.CE_EntryNum ?? string.Empty;
			set
			{
				var entryNumber = BookingNumberEntryNumber;
				var number = entryNumber?.CE_EntryNum ?? ZString.Empty;

				if (!value.IsEmpty)
				{
					if (entryNumber == null)
					{
						bookingNumberEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.JP.BookingNumber, AMA_RN_NKCountry);
						RegisterEditableChildObject(bookingNumberEntryNumber);
					}

					CheckMaximumLength(AMA_BookingNumberInfo, value);
					bookingNumberEntryNumber.CE_EntryNum = value;
					bookingNumberEntryNumber.CE_EntryIsSystemGenerated = false;
				}
				else
				{
					entryNumber?.Delete();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAMA_BookingNumber();
				}
				AMA_BookingNumberInfo.RefreshBinding(number);
			}
		}

		public ZPropertyInfo AMA_BookingNumberInfo => GetZPropertyInfo(nameof(AMA_BookingNumber));

		CusEntryNumber BookingNumberEntryNumber
		{
			get
			{
				if (bookingNumberEntryNumber == null || bookingNumberEntryNumber.IsDeleted || bookingNumberEntryNumber.CE_EntryType != CusEntryNumberTypes.JP.BookingNumber)
				{
					bookingNumberEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.JP.BookingNumber, AMA_RN_NKCountry);

					if (bookingNumberEntryNumber != null)
					{
						RegisterEditableChildObject(bookingNumberEntryNumber);
					}
				}

				return bookingNumberEntryNumber;
			}
		}
		CusEntryNumber bookingNumberEntryNumber;

		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MasterBillStatusList))]
		[ResourceStringData("D66B8096-AA7B-4D4C-8541-D3C60D2C6165", Caption = "Master Bill Customs Status")]
		public ZString MasterBillCustomsStatus => MasterBill.ABL_BillStatus;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MessageStatusList))]
		[ResourceStringData("F151234E-74F8-416D-9990-EEFD8F9EC382", Caption = "Master Bill Message Status")]
		public ZString MasterBillMessageStatus => MasterBill.ABL_MessageStatus;

		[ResourceStringData("51B790C8-07FA-F933-76ED-EC8AB08BB4EE", Caption = "Is Co-Loaded?", FullDescription = "Is master bill Co-Loaded?")]
		public ZBool IsCoLoaded
		{
			get => MasterBill.ABL_IsCoLoaded;
			set => MasterBill.ABL_IsCoLoaded = value;
		}

		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.ViaLocationCodeList))]
		[ResourceStringData("2E23F2E1-CAF9-4159-9F21-EE8A8FFD54BB", Caption = "Via Location", ShortCaption = "Via", FullDescription = "Via Location Code")]
		public ZString ViaLocationCode
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnFieldName.AMA_ViaLocationCode);
			set
			{
				var oldValue = ViaLocationCode;
				if (value != oldValue)
				{
					CheckMaximumLength(ViaLocationCodeInfo, value);
					this.SetSystemDefinedValue(GenAddOnColumnFieldName.AMA_ViaLocationCode, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateViaLocationCode();
					}
					ViaLocationCodeInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo ViaLocationCodeInfo => GetZPropertyInfo(nameof(ViaLocationCode));

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		public new AsycudaManifestHeaderDocumentSupporter DocumentSupporter => (AsycudaManifestHeaderDocumentSupporter)base.DocumentSupporter;

		protected override DocumentSupporter CreateNewDocumentSupporter() => new AsycudaManifestHeaderDocumentSupporter(this);

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);

		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Japan;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
		}

		public override ZBool IsImport => AMA_Nature == JPJobMessageTypeList.Codes.Import;

		public override ZBool IsExport => AMA_Nature == JPJobMessageTypeList.Codes.Export;

		public new EDIMessageCollection Messages => (EDIMessageCollection)base.Messages;

		protected override EDIMessageCollectionNonDependent CreateNewEDIMessageCollection() => new EDIMessageCollection(Factory, this) { IsManagedForDataRefresh = true };

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;

		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;

		void UpdateOrgAddressFromAirlineTwoLetterCode(ZString carrierCode)
		{
			var airlineSubQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
			airlineSubQuery.AddToFilter(RefAirlineSchema.RM_TwoCharacterCode, carrierCode);
			var miscServQuery = new ZDBOnlyQuery(typeof(OrgMiscServ));
			miscServQuery.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, airlineSubQuery, JoinCondition.And);
			var miscServ = Factory.LoadTop1<OrgMiscServ>(miscServQuery);
			AMA_OA_Carrier = miscServ?.Header?.MainAddress?.PK ?? ZGuid.Empty;
		}

		void UpdateOrgAddressFromSCACCode(ZString carrierCode)
		{
			var shippingLineSubQuery = new ZDBOnlySubQuery(typeof(RefShippingLine), RefShippingLineSchema.PK);
			shippingLineSubQuery.AddToFilter(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, carrierCode);
			shippingLineSubQuery.AddToFilter(RefShippingLineSchema.RSL_IsActive, true);
			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			orgHeaderQuery.AddSubQuery(OrgHeaderSchema.OH_RSL_ShippingLine, shippingLineSubQuery, JoinCondition.And);
			var orgHeader = Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
			AMA_OA_Carrier = orgHeader?.MainAddress?.PK ?? ZGuid.Empty;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			UpdateRegistrationStatusIfNeeded();
			UpdateMessageStatusIfNeeded();
			PopulateNumberPropertyIfRequired<ZString>(AMA_InputReferenceInfo, factory => Env.NumberFountains.NACCSInputReference.GetNextFormatted(factory), ignoreInDatabaseCheck: true);
		}

		void UpdateRegistrationStatusIfNeeded()
		{
			ZString GetCurrentRegistrationStatus()
			{
				string firstBillStatus = null;

				foreach(var bill in Bills.Cast<AsycudaBill>())
				{
					var status = bill.ABL_BillStatus;
					firstBillStatus ??= status;

					if (status == JPCustomsStatusList.Codes.Mismatch || status == JPCustomsStatusList.Codes.AMD)
					{
						return status;
					}
					else if (status != firstBillStatus)
					{
						return JPCustomsStatusList.Codes.MultipleStatus;
					}
				}

				return firstBillStatus;
			}

			if (NeedToUpdateRegistrationStatus)
			{
				NeedToUpdateRegistrationStatus = false;
				RegistrationStatus = GetCurrentRegistrationStatus();
			}
		}

		void UpdateMessageStatusIfNeeded()
		{
			if (NeedToUpdateMessageStatus)
			{
				NeedToUpdateMessageStatus = false;

				var bills = Bills.Cast<AsycudaBill>().ToArray();
				if (bills.Length != 0)
				{
					var firstBillStatus = bills[0].ABL_MessageStatus;
					var newStatus = bills.All(x => x.ABL_MessageStatus == firstBillStatus) ? firstBillStatus : new ZString(JPMessageStatusList.Codes.MultipleStatus);
					AMA_MessageStatus = newStatus;
				}
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ApplicationCode = RefCusCodeListTypes.Codes.NVC;
			AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
		}

#endif
	}
}
