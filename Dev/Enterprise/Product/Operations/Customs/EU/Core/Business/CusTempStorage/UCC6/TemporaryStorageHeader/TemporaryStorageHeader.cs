using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[SystemDefinedValues]
	[CodeProperty(TemporaryStorageHeader.Schema.AMA_JobReference), DescriptionProperty(nameof(TemporaryStorageHeader.HumanReadableName))]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class TemporaryStorageHeader : AsycudaManifestHeader, Integration.Customs.EU.ITemporaryStorageHeader
		, IWorkflowProvider, IEuOfficeCodeProvider, ICusCodeDataTypeSupporter, ILRNGenerator, ICusGoodsLocationProvider, ICusSupportingInfoTypeSupporter, IAdditionalBusinessObjectFetchStrategyProvider, ITemplateCopyable, IJobInvoicingPlugIn, ICusGoodsLocationTypeSupporter, IDocManagerSupport, IDocumentSupportable, IEDocsProvider
	{
		public TemporaryStorageHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new static readonly TemporaryStorageHeaderTypeDecider TypeDecider = new TemporaryStorageHeaderTypeDecider();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : AsycudaManifestHeader.Schema
		{
			public const string CustomsStatus = nameof(TemporaryStorageHeader.CustomsStatus);
			public const string ENSReuse = "ENSReuse";
		}

		[ResourceStringData("BC3853B5-CED2-482D-994E-40CB21DCF787", Caption = "Job #")]
		public override ZString AMA_JobReference
		{
			get => base.AMA_JobReference;
			set => base.AMA_JobReference = value;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return AMA_JobReference;
			}
		}

		[ResourceStringData("9871F94D-C824-4D97-956C-4CECCAF4B2E6", Caption = "Goods Presentation Date")]
		public override ZDateTime AMA_DateAtCustomsOffice
		{
			get => base.AMA_DateAtCustomsOffice;
			set
			{
				var oldValue = base.AMA_DateAtCustomsOffice;
				base.AMA_DateAtCustomsOffice = value;
				if (oldValue != value && !IsValidationSuspended)
				{
					Validation.ValidateAMA_MessageType();
					LRNEntryNumber.Validation.ValidateCE_IssueDate();
					DeclarationDateInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("78555FC7-7858-481B-AA52-00BD43D458D3", Caption = "Supervising Customs Office")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.CustomsOfficeCodeList))]
		public override ZString AMA_CustomsOffice
		{
			get => base.AMA_CustomsOffice;
			set => base.AMA_CustomsOffice = value;
		}

		[ResourceStringData("19D9628F-28BB-411D-8DDF-BCDB6E9F99FB", Caption = "Branch")]
		public override ZGuid AMA_GB
		{
			get => base.AMA_GB;
			set => base.AMA_GB = value;
		}

		[ResourceStringData("0b60cd3c-d34b-4cb6-a791-20b2ca258756", Caption = "Message Mode")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.MessageTypeList))]
		public override ZString AMA_MessageType
		{
			get => base.AMA_MessageType;
			set
			{
				var oldValue = AMA_MessageType;
				if (oldValue != value)
				{
					base.AMA_MessageType = value;
					ClearUnusedValues_Transfer();
					ClearUnusedValues_Deconsolidation();
					this.HouseBills.ForEach(x => x.MarkAsNeedingValidation());
					this.Containers.ForEach(x => x.MarkAsNeedingValidation());
				}
			}
		}

		void ClearUnusedValues_Transfer()
		{
			if (IsTransfer)
			{
				ENSReuse = 0;
				AMA_TransportMode = ZString.Empty;
				TransportType = ZString.Empty;
				ArrivalTransportMeansCode = ZString.Empty;
				PresentationCustomsOffice = ZString.Empty;
				AMA_DateAtCustomsOffice = ZDateTime.Empty;
				EstimatedDateOfArrival = ZDateTime.Empty;
				AMA_OA_Carrier = ZGuid.Empty;
				PlaceOfUnloading = ZString.Empty;
				Containers.RemoveAndDeleteAll();
				foreach (var bill in Bills)
				{
					bill.ABL_UCRNumber = ZString.Empty;
					bill.ABL_OA_Shipper = ZGuid.Empty;
					bill.ConsigneeOrgPK = ZGuid.Empty;
					bill.ConsignorOrgPK = ZGuid.Empty;
					bill.NotifyPartyOrgPK = ZGuid.Empty;
					bill.ABL_ShipperName = ZString.Empty;
					bill.ABL_ShipperStreet1 = ZString.Empty;
					bill.ABL_ShipperStreet2 = ZString.Empty;
					bill.ABL_ShipperCity = ZString.Empty;
					bill.ABL_RN_NKShipperCountry = ZString.Empty;
					bill.ABL_ShipperState = ZString.Empty;
					bill.ABL_ShipperPostcode = ZString.Empty;
					bill.ABL_ShipperPhone = ZString.Empty;
					bill.ABL_ShipperRegNoType = ZString.Empty;
					bill.ABL_ShipperRegNo = ZString.Empty;
					bill.ABL_OA_Consignee = ZGuid.Empty;
					bill.ABL_ConsigneeName = ZString.Empty;
					bill.ABL_ConsigneeStreet1 = ZString.Empty;
					bill.ABL_ConsigneeStreet2 = ZString.Empty;
					bill.ABL_ConsigneeCity = ZString.Empty;
					bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
					bill.ABL_ConsigneeState = ZString.Empty;
					bill.ABL_ConsigneePostcode = ZString.Empty;
					bill.ABL_ConsigneePhone = ZString.Empty;
					bill.ABL_ConsigneeRegNoType = ZString.Empty;
					bill.ABL_ConsigneeRegNo = ZString.Empty;
					bill.ABL_OA_NotifyParty = ZGuid.Empty;
					bill.ABL_NotifyPartyName = ZString.Empty;
					bill.ABL_NotifyPartyStreet1 = ZString.Empty;
					bill.ABL_NotifyPartyStreet2 = ZString.Empty;
					bill.ABL_NotifyPartyCity = ZString.Empty;
					bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
					bill.ABL_NotifyPartyState = ZString.Empty;
					bill.ABL_NotifyPartyPostcode = ZString.Empty;
					bill.ABL_NotifyPartyPhone = ZString.Empty;
					bill.ABL_NotifyPartyRegNoType = ZString.Empty;
					bill.ABL_NotifyPartyRegNo = ZString.Empty;
					bill.PackedItems.RemoveAndDeleteAll();
					bill.SupportingDocuments.RemoveAndDeleteAll();
					bill.PreviousDocuments.RemoveAndDeleteAll();
					bill.AdditionalInfos.RemoveAndDeleteAll();
					bill.SupplyChainActors.RemoveAndDeleteAll();
					foreach (TemporaryStoragePack pack in bill.Packs)
					{
						pack.ContainerPK = ZGuid.Empty;
						pack.APA_MarksAndNumbers = ZString.Empty;
					}
				}
			}
		}

		void ClearUnusedValues_Deconsolidation()
		{
			if (IsDeconsolidation)
			{
				AMA_Calc_HasHouseConsignment = true;
				ENSReuse = 0;
				AMA_TransportMode = ZString.Empty;
				TransportType = ZString.Empty;
				ArrivalTransportMeansCode = ZString.Empty;
				PresentationCustomsOffice = ZString.Empty;
				AMA_DateAtCustomsOffice = ZDateTime.Empty;
				EstimatedDateOfArrival = ZDateTime.Empty;
				AMA_OA_Presenter = ZGuid.Empty;
				GoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				GoodsLocation.CGL_Qualifier = ZString.Empty;
				GoodsLocation.CGL_Type = ZString.Empty;
				GoodsLocation.Address.E2_GovRegNum = ZString.Empty;
				GoodsLocation.Address.E2_Address1 = ZString.Empty;
				GoodsLocation.Address.E2_Address2 = ZString.Empty;
				GoodsLocation.Address.E2_Postcode = ZString.Empty;
				GoodsLocation.Address.E2_City = ZString.Empty;
				GoodsLocation.Address.E2_RN_NKCountryCode = ZString.Empty;
				GoodsLocation.Address.E2_Contact = ZString.Empty;
				GoodsLocation.Address.E2_Phone = ZString.Empty;
				GoodsLocation.Address.E2_Email = ZString.Empty;
				GoodsLocation.Address.E2_Longitude = ZDecimal.Zero;
				GoodsLocation.Address.E2_Latitude = ZDecimal.Zero;
				AuthorizationType = ZString.Empty;
				AuthorizationOwner = ZGuid.Empty;
				AuthorizationNumber = ZString.Empty;
				AMA_OA_Carrier = ZGuid.Empty;
				PlaceOfUnloading = ZString.Empty;
				foreach (var bill in Bills.Where(x => x.ABL_Calc_IsMaster))
				{
					bill.ABL_UCRNumber = ZString.Empty;
					bill.ABL_GrossWeight = ZDecimal.Zero;
					bill.ABL_GrossWeightUQ = ZString.Empty;
					bill.ABL_OA_Shipper = ZGuid.Empty;
					bill.ConsigneeOrgPK = ZGuid.Empty;
					bill.ConsignorOrgPK = ZGuid.Empty;
					bill.NotifyPartyOrgPK = ZGuid.Empty;
					bill.ABL_ShipperName = ZString.Empty;
					bill.ABL_ShipperStreet1 = ZString.Empty;
					bill.ABL_ShipperStreet2 = ZString.Empty;
					bill.ABL_ShipperCity = ZString.Empty;
					bill.ABL_RN_NKShipperCountry = ZString.Empty;
					bill.ABL_ShipperState = ZString.Empty;
					bill.ABL_ShipperPostcode = ZString.Empty;
					bill.ABL_ShipperPhone = ZString.Empty;
					bill.ABL_ShipperRegNoType = ZString.Empty;
					bill.ABL_ShipperRegNo = ZString.Empty;
					bill.ABL_OA_Consignee = ZGuid.Empty;
					bill.ABL_ConsigneeName = ZString.Empty;
					bill.ABL_ConsigneeStreet1 = ZString.Empty;
					bill.ABL_ConsigneeStreet2 = ZString.Empty;
					bill.ABL_ConsigneeCity = ZString.Empty;
					bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
					bill.ABL_ConsigneeState = ZString.Empty;
					bill.ABL_ConsigneePostcode = ZString.Empty;
					bill.ABL_ConsigneePhone = ZString.Empty;
					bill.ABL_ConsigneeRegNoType = ZString.Empty;
					bill.ABL_ConsigneeRegNo = ZString.Empty;
					bill.ABL_OA_NotifyParty = ZGuid.Empty;
					bill.ABL_NotifyPartyName = ZString.Empty;
					bill.ABL_NotifyPartyStreet1 = ZString.Empty;
					bill.ABL_NotifyPartyStreet2 = ZString.Empty;
					bill.ABL_NotifyPartyCity = ZString.Empty;
					bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
					bill.ABL_NotifyPartyState = ZString.Empty;
					bill.ABL_NotifyPartyPostcode = ZString.Empty;
					bill.ABL_NotifyPartyPhone = ZString.Empty;
					bill.ABL_NotifyPartyRegNoType = ZString.Empty;
					bill.ABL_NotifyPartyRegNo = ZString.Empty;
					bill.PackedItems.RemoveAndDeleteAll();
					bill.SupportingDocuments.RemoveAndDeleteAll();
					bill.PreviousDocuments.RemoveAndDeleteAll();
					bill.AdditionalInfos.RemoveAndDeleteAll();
					bill.SupplyChainActors.RemoveAndDeleteAll();
					bill.Packs.RemoveAndDeleteAll();
				}
			}
		}

		[ResourceStringData("e023b8e8-948d-43f5-9ce8-183018c6abea", Caption = "Carrier")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.CarrierList))]
		public override ZGuid AMA_OA_Carrier
		{
			get => base.AMA_OA_Carrier;
			set => base.AMA_OA_Carrier = value;
		}

		[ResourceStringData("1D69598B-AE6D-44F4-8DF7-2FA3714A690B", Caption = "Declarant")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.DeclarantList))]
		public override ZGuid AMA_OA_Declarant
		{
			get => base.AMA_OA_Declarant;
			set
			{
				base.AMA_OA_Declarant = value;
			}
		}

		[ResourceStringData("C5E618CF-B2B8-4D21-9BA6-1211D6E81F61", Caption = "Person Presenting the Goods")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.PresenterList))]
		public override ZGuid AMA_OA_Presenter
		{
			get => base.AMA_OA_Presenter;
			set => base.AMA_OA_Presenter = value;
		}

		[ResourceStringData("5444617B-4C3C-4464-952B-DBD074024880", Caption = "Representative")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.RepresentativeList))]
		public override ZGuid AMA_OA_Representative
		{
			get => base.AMA_OA_Representative;
			set => base.AMA_OA_Representative = value;
		}

		[ResourceStringData("0eec09e8-cf91-4a28-aeaa-292515a8930a", Caption = "Transport Mode")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.TransportModeList))]
		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set => base.AMA_TransportMode = value;
		}

		[ResourceStringData("3A1AC854-A272-4883-A46D-4D032C55B8B3", Caption = "Audit Details")]
		public override ZDateTime AMA_SystemCreateTimeUtc
		{
			get => base.AMA_SystemCreateTimeUtc;
			set => base.AMA_SystemCreateTimeUtc = value;
		}

		public override ZString AMA_RN_NKCountry
		{
			get => base.AMA_RN_NKCountry;
			set
			{
				var oldValue = AMA_RN_NKCountry;
				base.AMA_RN_NKCountry = value;
				if (oldValue != value)
				{
					if (!IsCopying)
					{
						MasterBill.RefreshBinding();
						Bills.MarkAsNeedingValidation();
						Containers.ForEach(x => x.MarkAsNeedingValidation());
					}
				}
			}
		}

		[ResourceStringData("415e4f7d-1fdc-41de-b7c7-31c8409c08de", Caption = "Broker")]
		public override ZString AMA_GS_NKCustomsAgent
		{
			get => base.AMA_GS_NKCustomsAgent;
			set => base.AMA_GS_NKCustomsAgent = value;
		}

		void DefaultCustomsAgentIfApplicable()
		{
			if (Configuration.SupportAgentDefaulting && !IsInDatabase && AMA_GS_NKCustomsAgent.IsEmpty && CurrentUserIsBroker)
			{
				AMA_GS_NKCustomsAgent = GlbStaff.CurrentUser.GS_Code;
			}
		}

		protected bool CurrentUserIsBroker => CurrentUserIsBrokerCore;

		protected virtual bool CurrentUserIsBrokerCore => !GlbStaff.CurrentUser.GS_IsSystemAccount;

		[ResourceStringData("7b7e04b3-0d8c-46a8-99bb-47fdf17ff604", Caption = "Declaration Date")]
		public ZDateTime DeclarationDate
		{
			get => LRNEntryNumber.CE_IssueDate;
			set
			{
				if (DeclarationDate != value)
				{
					RegisterEditableChildObject(lrnEntryNumber);
					LRNEntryNumber.CE_IssueDate = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateDeclarationDate();
					}
					DeclarationDateInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo DeclarationDateInfo => GetZPropertyInfo(nameof(DeclarationDate));

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.TransportTypeList))]
		[ResourceStringData("6a82ad65-44c6-4f89-8b10-58b5aa204fa8", Caption = "Transport Type", FullDescription = "[19 06 061 000] Arrival Transport Means > Type of Identification")]
		[MaxLength(2)]
		public virtual ZString TransportType
		{
			get => ArrivalTransportMeans.TPM_TypeOfIdentification;
			set
			{
				var oldValue = TransportType;
				ArrivalTransportMeans.TPM_TypeOfIdentification = value;
				if (oldValue != value && !IsValidationSuspended)
				{
					ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
				}
				TransportTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TransportTypeInfo => GetZPropertyInfo(nameof(TransportType));

		public virtual ZString ArrivalTransportMeansCode
		{
			get => ArrivalTransportMeans.TPM_IdentificationNumber;
			set
			{
				var oldValue = ArrivalTransportMeans.TPM_IdentificationNumber;
				ArrivalTransportMeans.TPM_IdentificationNumber = value;
				if (oldValue != value && !IsValidationSuspended)
				{
					ArrivalTransportMeansCodeInfo.RefreshBinding();
				}
			}
		}

		protected virtual Type ArrivalTransportMeansTypeCore => typeof(ArrivalTransportMeans);

		public ArrivalTransportMeans ArrivalTransportMeans
		{
			get
			{
				if (arrivalTransportMeans == null || arrivalTransportMeans.IsDeleted)
				{
					var query = new ZQuery(CusTransportMeansSchema.TPM_ParentID, this.PK);
					query.AddToFilter(CusTransportMeansSchema.TPM_ParentTableCode, AsycudaManifestHeaderSchema.Constants.Prefix);
					arrivalTransportMeans = (ArrivalTransportMeans)Factory.LoadTop1(ArrivalTransportMeansTypeCore, query);

					if (arrivalTransportMeans == null)
					{
						arrivalTransportMeans = (ArrivalTransportMeans)Factory.New(ArrivalTransportMeansTypeCore);
						using (SuspendMarkingAsNeedingValidation())
						using (arrivalTransportMeans.SuspendSettingHasChanges())
						{
							arrivalTransportMeans.TPM_ParentID = this.PK;
							arrivalTransportMeans.TPM_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
						}
					}

					RegisterEditableChildObject(arrivalTransportMeans);
				}
				return arrivalTransportMeans;
			}
		}

		ArrivalTransportMeans arrivalTransportMeans;

		public ZPropertyInfo ArrivalTransportMeansCodeInfo => GetWrappedZPropertyInfo(nameof(ArrivalTransportMeansCode), x => ArrivalTransportMeans.TPM_IdentificationNumberInfo);

		[ResourceStringData("3eab21de-9135-4bde-a521-64527fd11959", Caption = "Place of Unloading")]
		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.RefUNLOCOCollection))]
		public ZString PlaceOfUnloading
		{
			get
			{
				return MasterBill.ABL_RL_NKPortOfDischarge;
			}
			set
			{
				fMasterBill = MasterBill;
				var oldValue = fMasterBill.ABL_RL_NKPortOfDischarge;
				fMasterBill.ABL_RL_NKPortOfDischarge = value;
				if (oldValue != value && !IsValidationSuspended)
				{
					Validation.ValidatePlaceOfUnloading();
				}
				PlaceOfUnloadingInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo PlaceOfUnloadingInfo => GetZPropertyInfo(nameof(PlaceOfUnloading));

		[ResourceStringData("D34F08D8-64CA-4705-8BD1-8D1D2A2DF7C8", Caption = "Place of Loading")]
		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.RefUNLOCOCollection))]
		public ZString PlaceOfLoading
		{
			get => MasterBill.ABL_RL_NKPortOfLoading;
			set
			{
				var oldValue = PlaceOfLoading;
				MasterBill.ABL_RL_NKPortOfLoading = value;
				PlaceOfLoadingInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo PlaceOfLoadingInfo => GetZPropertyInfo(nameof(PlaceOfLoading));

		public TemporaryStorageBill MasterBill
		{
			get
			{
				if (!IsDeleting && !IsDeleted && (fMasterBill == null || fMasterBill.IsDeleted))
				{
					fMasterBill = Factory.Load<TemporaryStorageBill>(GetMasterBillQuery()).OrderBy(x => x.ABL_SystemCreateTimeUtc).FirstOrDefault();
					if (fMasterBill == null)
					{
						fMasterBill = (TemporaryStorageBill)Factory.New(GetBillType());
						fMasterBill.ABL_AMA = PK;
						fMasterBill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
					}
					RegisterEditableChildObject(fMasterBill);
				}
				return fMasterBill;
			}
		}
		TemporaryStorageBill fMasterBill;

		[ResourceStringData("{9F6C15C6-7170-47FE-8448-0AEBE432E1B1}", Caption = "Estimated Date of Arrival", ShortCaption = "ETA")]
		public ZDateTime EstimatedDateOfArrival
		{
			get
			{
				return MasterBill.ABL_A_ARV;
			}
			set
			{
				var oldValue = EstimatedDateOfArrival;
				MasterBill.ABL_A_ARV = value;
				EstimatedDateOfArrivalInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo EstimatedDateOfArrivalInfo => GetZPropertyInfo(nameof(EstimatedDateOfArrival));

		protected override Type GetBillTypeCore() => typeof(TemporaryStorageBill);

		internal ZQuery GetMasterBillQuery()
		{
			var query = DataHelper.GenerateClusterKeyQuery(AMA_ClusterKey, PK, AsycudaBillSchema.ABL_ClusterKey, AsycudaBillSchema.ABL_AMA, !IsInDatabase);
			query.AddToFilter(new ZQuery(AsycudaBillSchema.ABL_BolType, TemporaryStorageBill.ChildBolCode));
			return query;
		}

		[ReadOnly(true)]
		[ResourceStringData("4fdcdd93-a335-4c62-9816-68d394ab92f8", Caption = "Message Status")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.PNTSMessageStatusList))]
		public override ZString AMA_MessageStatus
		{
			get => base.AMA_MessageStatus;
			set => base.AMA_MessageStatus = value;
		}

		public ZString MessageStatusDescription => Factory.GetValue(ref messageStatusDescriptionCached, () =>
		{
			var message = AMA_MessageStatus;
			return message.IsEmpty
				? ZString.Empty
				: Lookups.PNTSMessageStatusList.GetDescriptionFromCode(message);
		});

		CachedProperty<ZString> messageStatusDescriptionCached;

		[ReadOnly(true)]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("F1D04072-6F43-411D-B349-2B55F23602D9", Caption = "LRN #")]
		public ZString LRN
		{
			get => LRNEntryNumber.CE_EntryNum;
			set
			{
				if (LRN != value)
				{
					LRNEntryNumber.CE_EntryNum = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateLRN();
					}
					RegisterEditableChildObject(lrnEntryNumber);
					LRNInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo LRNInfo => GetZPropertyInfo(nameof(LRN));

		CusEntryNumber LRNEntryNumber
		{
			get
			{
				if (lrnEntryNumber == null || lrnEntryNumber.IsDeleted)
				{
					lrnEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.EU.LocalReferenceNumber, AMA_RN_NKCountry);
					if (lrnEntryNumber == null)
					{
						lrnEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.EU.LocalReferenceNumber, AMA_RN_NKCountry);
					}
					lrnEntryNumber.CE_EntryNumInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					lrnEntryNumber.CE_IssueDateInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
				}
				return lrnEntryNumber;
			}
		}
		CusEntryNumber lrnEntryNumber;

		protected virtual bool IsMrnReadOnly => true;

		[ReadOnlyMember(nameof(IsMrnReadOnly))]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("1e09a14d-168e-48ce-9a9f-2b5dd7a8bb6e", Caption = "MRN #")]
		public ZString MRN
		{
			get => MRNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (MRN != value)
				{
					if (MRNEntryNumber == null)
					{
						mrnEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, AMA_RN_NKCountry);
					}
					MRNEntryNumber.CE_EntryNum = value;
					RegisterEditableChildObject(mrnEntryNumber);
					MRNInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo MRNInfo => GetZPropertyInfo(nameof(MRN));

		CusEntryNumber MRNEntryNumber
		{
			get
			{
				if (mrnEntryNumber == null || mrnEntryNumber.IsDeleted)
				{
					mrnEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, AMA_RN_NKCountry);
				}
				return mrnEntryNumber;
			}
		}
		CusEntryNumber mrnEntryNumber;

		[ReadOnly(true)]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("3d761a77-efb1-440f-b62c-cba1a8caecbe", Caption = "CRN #")]
		public ZString CRN
		{
			get => CRNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (CRN != value)
				{
					if (CRNEntryNumber == null)
					{
						crnEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.EU.CustomsRegistrationNumber, AMA_RN_NKCountry);
					}
					CRNEntryNumber.CE_EntryNum = value;
					RegisterEditableChildObject(crnEntryNumber);
					CRNInfo.RefreshBinding();
				}
			}
		}

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ??= new(this, Core.Constants.DocManagerCodes.TempStorageHeaderUCC6);
		DocManagerInfo docManagerInfo;

		#endregion

		public ZPropertyInfo CRNInfo => GetZPropertyInfo(nameof(CRN));

		public ZDateTime PreLodgedDate
		{
			get => CRNEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				if (PreLodgedDate != value)
				{
					if (CRNEntryNumber == null)
					{
						crnEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.EU.CustomsRegistrationNumber, AMA_RN_NKCountry);
					}
					CRNEntryNumber.CE_IssueDate = value;
				}
			}
		}

		CusEntryNumber CRNEntryNumber
		{
			get
			{
				if (crnEntryNumber == null || crnEntryNumber.IsDeleted)
				{
					crnEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.EU.CustomsRegistrationNumber, AMA_RN_NKCountry);
				}
				return crnEntryNumber;
			}
		}
		CusEntryNumber crnEntryNumber;

		[ReadOnly(true)]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("688cb4c7-bc30-4722-a8d0-ab407fc6c3c0", Caption = "FRN #")]
		public ZString FRN
		{
			get => FRNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (FRN != value)
				{
					if (FRNEntryNumber == null)
					{
						frnEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.EU.FunctionalReferenceNumber, AMA_RN_NKCountry);
					}
					FRNEntryNumber.CE_EntryNum = value;
					RegisterEditableChildObject(frnEntryNumber);
					FRNInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo FRNInfo => GetZPropertyInfo(nameof(FRN));

		CusEntryNumber FRNEntryNumber
		{
			get
			{
				if (frnEntryNumber == null || frnEntryNumber.IsDeleted)
				{
					frnEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.EU.FunctionalReferenceNumber, AMA_RN_NKCountry);
				}
				return frnEntryNumber;
			}
		}
		CusEntryNumber frnEntryNumber;

		public CusEntryNumber RegistrationEntryNumber
		{
			get
			{
				if (registrationEntryNumber == null || registrationEntryNumber.IsDeleted)
				{
					registrationEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
				}

				if (registrationEntryNumber != null)
				{
					registrationEntryNumber.MarkParentAsNeedingValidation = true;
				}

				return registrationEntryNumber;
			}
		}
		CusEntryNumber registrationEntryNumber;

		[ReadOnly(true)]
		[MaxLength(CusEntryNumber.Schema.CE_EntryStatusMaxLength)]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.CustomsStatusList))]
		[ResourceStringData("7fe6acf0-7081-4c9d-9bf3-6e6aeeb959c2", Caption = "Customs Status")]
		public ZString CustomsStatus
		{
			get => RegistrationEntryNumber?.CE_EntryStatus ?? ZString.Empty;
			set
			{
				var oldValue = CustomsStatus;
				if (oldValue != value)
				{
					if (RegistrationEntryNumber == null)
					{
						registrationEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
					}
					RegistrationEntryNumber.CE_EntryStatus = value;
					RegisterEditableChildObject(registrationEntryNumber);
					CustomsStatusInfo.RefreshBinding();

					if (ShouldConfirmTemporaryStorageGoodsConsumption)
					{
						ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
							GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty,
							CountryCode,
							CustomsStatus,
							TemporaryStorageTransactionInternalReferenceNumber,
							TemporaryStorageTransactionInternalReferenceType,
							LRN,
							PreviousDocumentCodeForDataToReserveTemporaryStorageGoods,
							GetEntryLineDataDeclaredToReserveTSGoods,
							MRN,
							TemporaryStorageTransactionCommentPrefix,
							AMA_JobReference,
							GetIssueDateForTemporaryStorageGoodsConsumptionConfirmation(),
							GetIssueDateForTemporaryStorageGoodsConsumptionConfirmation(),
							TemporaryStorageWriteOffTransactionCommentReferenceNumber,
							Logs,
							CustomsStatusToCancelTemporaryStoragePendingTransactions,
							CustomsStatusToNotCreateTemporaryStorageTransactions,
							CustomsStatusToConfirmTemporaryStoragePendingTransactions,
							formatDocRef: ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation ? GetDocumentNumberFormat : null);
					}
					OnCustomsStatusChangedCore(oldValue, value);
				}
			}
		}

		protected virtual void OnCustomsStatusChangedCore(ZString oldValue, ZString newValue)
		{
			// Override this method to handle any additional logic when CustomsStatus changes.
		}

		protected virtual ZDateTime GetIssueDateForTemporaryStorageGoodsConsumptionConfirmation() => MRNEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;

		public ZPropertyInfo CustomsStatusInfo => GetZPropertyInfo(nameof(CustomsStatus));

		public ZString CustomsStatusDescription => Factory.GetValue(ref customsStatusDescriptionCached, () => GetCustomsStatusDescriptionCore());
		CachedProperty<ZString> customsStatusDescriptionCached;

		protected virtual ZString GetCustomsStatusDescriptionCore()
		{
			var customsStatus = CustomsStatus;
			return customsStatus.IsEmpty
				? ZString.Empty
				: Lookups.CustomsStatusList.OfType<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == customsStatus)?.ZZD_Description ?? ZString.Empty;
		}

		[ReadOnly(true)]
		[ResourceStringData("6DD0EDED-A7D7-4A11-BC52-9858F07D7A12", Caption = "Customs Status Date")]
		public ZDateTime CustomsStatusDate
		{
			get => RegistrationEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				if (CustomsStatusDate != value)
				{
					if (RegistrationEntryNumber == null)
					{
						registrationEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
					}
					RegistrationEntryNumber.CE_IssueDate = value;
					RegisterEditableChildObject(registrationEntryNumber);
					CustomsStatusDateInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CustomsStatusDateInfo => GetZPropertyInfo(nameof(CustomsStatusDate));

		#region ILRNGenerator
		INumberFountainProxy ILRNGenerator.LrnNumberFountain => Env.NumberFountains.PNTSLocalReferenceNumber(Branch.Company.PK.ToGuid());
		#endregion

		[MaxLength(10)]
		[ResourceStringData("BF302D1C-34FF-474D-95B7-98B2FCCA8C53", Caption = "Presentation Customs Office")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.CustomsOfficeCodeList))]
		public ZString PresentationCustomsOffice
		{
			get => PresentationCustomsOfficeCode.CY_Data;
			set
			{
				var oldValue = PresentationCustomsOffice;
				CheckMaximumLength(PresentationCustomsOfficeInfo, value);
				PresentationCustomsOfficeCode.CY_Data = value;
				RegisterEditableChildObject(PresentationCustomsOfficeCode);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePresentationCustomsOffice();
				}
				PresentationCustomsOfficeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo PresentationCustomsOfficeInfo => GetZPropertyInfo(nameof(PresentationCustomsOffice));

		public TemporaryStorageOfficeCode PresentationCustomsOfficeCode => Factory.GetValue(ref presentationCustomsOffice,
			() => TemporaryStorageOfficeCode.LoadOrCreate<TemporaryStorageOfficeCode>(this, EuOfficeCodesTypes.Codes.OfficeOfPresentation));
		CachedProperty<TemporaryStorageOfficeCode> presentationCustomsOffice;

		#region IEuOfficeCodeProvider
		ZString IEuOfficeCodeProvider.CountryCode => AMA_RN_NKCountry;

		ZBool IEuOfficeCodeProvider.IsImport => false;

		ZBool IEuOfficeCodeProvider.IsExport => false;

		bool IEuOfficeCodeProvider.IsNCTS => false;

		bool IEuOfficeCodeProvider.IsEMCS => false;

		IEnumerable<EuOfficeCode> IEuOfficeCodeProvider.CustomsOffices => Enumerable.Empty<EuOfficeCode>();

		CustomsOfficeRequirementHelper IEuOfficeCodeProvider.CustomsOfficeRequirementHelper => customsOfficeRequirementHelper ?? (customsOfficeRequirementHelper = GetCustomsOfficeRequirementHelper());
		TemporaryStorageHeaderCustomsOfficeRequirementHelper customsOfficeRequirementHelper;

		protected virtual TemporaryStorageHeaderCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new TemporaryStorageHeaderCustomsOfficeRequirementHelper(this);
		#endregion

		public ZPropertyInfo ENSReuseInfo => GetZPropertyInfo(nameof(ENSReuse));

		public ZByte ENSReuse
		{
			get => this.GetSystemDefinedValue<ZByte>(Customs.Business.GenAddOnHelper.ENSReuse);
			set
			{
				var oldValue = ENSReuse;
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.ENSReuse, value);

				ENSReuseInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsENSReuseInfo => GetZPropertyInfo(nameof(IsENSReuse));

		[DefaultValue(false)]
		[ReadOnlyMember(nameof(IsENSReuseReadOnly))]
		[ResourceStringData("47e03b6d-0581-4589-b432-767cbcc9c3e6", Caption = "ENS Re-use")]
		public ZBool IsENSReuse
		{
			get => new ZBool(ENSReuse == new ZByte(1));
			set
			{
				ENSReuse = value ? new ZByte(1) : new ZByte(0);

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsENSReuse();
				}

				IsENSReuseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AMA_Calc_HasHouseConsignmentInfo => GetZPropertyInfo(nameof(AMA_Calc_HasHouseConsignment));

		public bool AMA_Calc_HasHouseConsignment_ReadOnly => IsDeconsolidation;

		[DefaultValue(false)]
		[ResourceStringData("D9FC394F-BEE0-4932-88D2-0D01574BC2D8", Caption = "Has House Consignment?")]
		[ReadOnlyMember(nameof(AMA_Calc_HasHouseConsignment_ReadOnly))]
		public virtual ZBool AMA_Calc_HasHouseConsignment
		{
			get => AMA_Nature == Nature_H ? ZBool.True : ZBool.False;
			set
			{
				if (value)
				{
					AMA_Nature = Nature_H;
				}
				else
				{
					if (!HouseBills.Any())
					{
						AMA_Nature = Nature_M;
					}
					else
					{
						if (NotifyUnableToCompleteActionAsHouseBillExists == null)
						{
							throw new InvalidOperationException("NotifyUnableToCompleteActionAsHouseBillExists has not being setup");
						}
						else
						{
							NotifyUnableToCompleteActionAsHouseBillExists(Res.GetString("09AEE61C-7BF1-432C-83D4-57411CA9B881", "This action cannot be completed due to the presence of at least one house bill. Kindly remove all existing house bills and attempt the action again."));
						}
					}
				}
				AMA_Calc_HasHouseConsignmentInfo.RefreshBinding();
				Bills.RefreshBinding();
			}
		}

		public ZPropertyInfo HasNoMasterBillInfo => GetZPropertyInfo(nameof(HasNoMasterBill));

		[DefaultValue(false)]
		[ResourceStringData("1F11996B-84C5-41FF-B383-0B74DDDB3380", Caption = "Has No Master Bill?")]
		public ZBool HasNoMasterBill
		{
			get => MasterBill.HasNoMasterBill;
			set
			{
				if (MasterBill.HasNoMasterBill != value)
				{
					MasterBill.HasNoMasterBill = value;
					HasNoMasterBillInfo.RefreshBinding();
					Bills.Load();
				}
			}
		}

		public NotifyUnableToCompleteActionAsHouseBillExistsDelegate NotifyUnableToCompleteActionAsHouseBillExists;
		public delegate void NotifyUnableToCompleteActionAsHouseBillExistsDelegate(string message);

		public const string Nature_H = "H";
		public const string Nature_M = "M";

		public bool IsENSReuseReadOnly => AMA_MessageType == PNTSMessageTypeList.Codes.PresentationNotification;

		[ChildEditable]
		public new ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader> Bills => (ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>)base.Bills;

		protected sealed override IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> CreateNewAsycudaBillCollection() => CreateNewTemporaryStorageBillCollection();
		protected virtual ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader> CreateNewTemporaryStorageBillCollection()
		{
			return new TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(this);
		}

		public IEnumerable<TemporaryStorageBill> HouseBills => Bills.Where(x => x.ABL_BolType == TemporaryStorageBillKindList.Codes.HWB);

		protected override AsycudaManifestHeaderValidation GetNewValidation() => new TemporaryStorageHeaderValidation(this);

		public new TemporaryStorageHeaderValidation Validation => (TemporaryStorageHeaderValidation)base.Validation;

		protected override AsycudaManifestHeaderLookups GetNewLookups() => new TemporaryStorageHeaderLookups(this);

		public new TemporaryStorageHeaderLookups Lookups => (TemporaryStorageHeaderLookups)base.Lookups;

		[ChildEditable]
		public new IAsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader> Containers => (IAsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader>)base.Containers;

		protected override IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader>(this);

		protected override Type GetContainerTypeCore() => typeof(TemporaryStorageContainer);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TemporaryStorage;
			AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AMA_AgentType = Core.Constants.AgentType.Agent;
			AMA_OA_Declarant = GlbBranch.CurrentBranch.OrgProxy?.MainAddress.PK ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress.PK ?? ZGuid.Empty;
			using (LRNEntryNumber.SuspendSettingHasChanges())
			{
				DeclarationDate = ZDateTime.Now;
			}
		}

		#region TSD Status

		bool IsENSReuseProperty(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(IsENSReuse):
				case nameof(DeclarationDate):
				case nameof(AMA_DateAtCustomsOffice):
				case nameof(AMA_CustomsOffice):
				case nameof(PresentationCustomsOffice):
				case nameof(AMA_OA_Presenter):
				case nameof(AMA_OA_Declarant):
				case nameof(AMA_OA_Representative):
				case nameof(GoodsLocationDescription):
				case nameof(ArrivalTransportMeansCode):
				case nameof(AuthorizationType):
				case nameof(AuthorizationNumber):
				case nameof(PlaceOfUnloading):
				case nameof(AMA_OA_Carrier):
					return true;
				default:
					return false;
			}
		}

		public virtual bool IsPropertyReadOnlyAfterSent(string propertyName)
		{
			return false;
		}

		public bool IsPreLodged() => IsPreLodged(CustomsStatus);

		public static bool IsPreLodged(ZString customsStatus)
		{
			switch (customsStatus.ToUpperInvariant())
			{
				case UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged:
				case UniversalReferenceConstants.PNTS.CustomsStatus.PresentationNotificationNotLinked:
					return true;
				default:
					return false;
			}
		}

		bool IsPropertyReadOnlyUnderPreLodged(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(IsENSReuse):
				case nameof(AMA_OA_Declarant):
					return true;
				default:
					return false;
			}
		}

		public bool IsAccepted() => IsAccepted(CustomsStatus);

		public static bool IsAccepted(ZString customsStatus)
		{
			switch (customsStatus.ToUpperInvariant())
			{
				case UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated:
				case UniversalReferenceConstants.PNTS.CustomsStatus.PresentationNotificationLinked:
					return true;
				default:
					return false;
			}
		}

		bool IsPropertyReadOnlyUnderAccepted(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(AMA_MessageType):
				case nameof(IsENSReuse):
				case nameof(DeclarationDate):
				case nameof(AMA_CustomsOffice):
				case nameof(AMA_OA_Declarant):
					return true;
				default:
					return false;
			}
		}

		public bool IsNoEditAllowedCustomsStatus() => IsNoEditAllowedCustomsStatus(CustomsStatus);

		public static bool IsNoEditAllowedCustomsStatus(ZString customsStatus)
		{
			switch (customsStatus.ToUpperInvariant())
			{
				case UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated:
				case UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl:
				case UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl:
				case UniversalReferenceConstants.PNTS.CustomsStatus.IrregularityUnderInvestigation:
				case UniversalReferenceConstants.PNTS.CustomsStatus.ProofOfUnionStatusPresented:
				case UniversalReferenceConstants.PNTS.CustomsStatus.MeasuresRequired:
				case UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageEnded:
					return true;
				default:
					return false;
			}
		}

		public bool IsAmendableFieldsEditAllowedCustomsStatus() => IsAmendableFieldsEditAllowedCustomsStatus(CustomsStatus);

		public static bool IsAmendableFieldsEditAllowedCustomsStatus(ZString customsStatus)
		{
			switch (customsStatus.ToUpperInvariant())
			{
				case UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged:
				case UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated:
				case UniversalReferenceConstants.PNTS.CustomsStatus.PresentationNotificationLinked:
				case UniversalReferenceConstants.PNTS.CustomsStatus.PresentationNotificationNotLinked:
					return true;
				default:
					return false;
			}
		}

		public bool IsReadOnlyBasedOnTSDLogic()
		{
			return IsAmendableFieldsEditAllowedCustomsStatus() ? IsENSReuse : IsNoEditAllowedCustomsStatus();
		}

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			var result = ReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);

			if (!result)
			{
				if (IsAmendableFieldsEditAllowedCustomsStatus())
				{
					if (ENSReuse == 1 && IsENSReuseProperty(property.Name))
					{
						result = true;
					}
					else
					{
						if (IsPreLodged())
						{
							result = IsMessageTypeReadOnlyBasedOnTSPLogic(property.Name) || IsPropertyReadOnlyUnderPreLodged(property.Name);
						}
						else if (IsAccepted())
						{
							result = IsPropertyReadOnlyUnderAccepted(property.Name);
						}
					}
				}
				else
				{
					result = IsNoEditAllowedCustomsStatus();
				}
				if (IsSent)
				{
					if (IsPropertyReadOnlyAfterSent(property.Name))
					{
						result = true;
					}
				}
			}

			return result;
		}

		bool IsMessageTypeReadOnlyBasedOnTSPLogic(string propertyName)
		{
			return propertyName == nameof(AMA_MessageType) && ((AMA_MessageType != PNTSMessageTypeList.Codes.PreLodgedTempStorage && AMA_MessageType != PNTSMessageTypeList.Codes.PresentationNotification) || CustomsStatus != UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged);
		}

		#endregion

		#region IWorkflowProvider

		ZGuid IWorkflowProviderCore.PK => PK;

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public IColumnValueRanker GetTemplateSelectionCriteria() => new ColumnValueRanker();

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}
				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetNewProcessTaskCollection() => new ProcessTaskCollection<TemporaryStorageHeaderProcessTask, TemporaryStorageHeader>(this);

		public ZString WorkflowType => new TemporaryStorageHeaderWorkflowDescriptor().Code;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			LogCustomsStatusIfRequired();
		}

		void LogCustomsStatusIfRequired()
		{
			var shouldLogCustomsStatus = (!CustomsStatus.IsEmpty && !IsInDatabase) || CustomsStatus != ((ZString?)(RegistrationEntryNumber?.CE_EntryStatusInfo.OriginalValue) ?? ZString.Empty);

			if (shouldLogCustomsStatus)
			{
				Func<StmALog, bool> cesCriteria = (StmALog log) => { return log.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && !log.IsInDatabase && log.SL_Reference == CustomsStatus; };
				var unsavedCESEvents = Logs.Find(cesCriteria);

				if (unsavedCESEvents == null || !unsavedCESEvents.Any())
				{
					Logs.AddNew(Events.CustomsEntryStatus, CustomsStatus, ZDateTimeOffset.Now);
				}
			}
		}

		protected override void OnFactorySaving()
		{
			ApportionedAmountToGuaranteeLiabilityAmount();
			base.OnFactorySaving();

			if (Configuration.SupportLRNGeneration)
			{
				PopulateNumberPropertyIfRequired(LRNInfo, objectFactory => GenerateLocalReferenceNumber(), ignoreInDatabaseCheck: true, forceRegenerate: ForceGenerateLrn);
			}
		}

		public void ApportionedAmountToGuaranteeLiabilityAmount()
		{
			Guarantee.SetBondAmountWithLiabilityAmount();
		}

		protected virtual bool ForceGenerateLrn => false;

		protected virtual ZString GenerateLocalReferenceNumber() => LRNGeneratorHelper.GenerateLocalReferenceNumber(this);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				WorkflowItems.RemoveAndDeleteAll();
				MasterBill.Delete();
				Guarantee.Delete();
			}
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateFormattedNumberPropertyIfRequired(AMA_JobReferenceInfo, Env.NumberFountains.TSDJobReference);
			DefaultCustomsAgentIfApplicable();
		}

		public IDictionary<ZString, Type> GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.OfficeCode, typeof(TemporaryStorageOfficeCode));
			return result;
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region ICusGoodsLocationProvider

		public Business.CusGoodsLocation GoodsLocation => LoadOrCreateGoodsLocation(createNewIfNotExists: true);

		Business.CusGoodsLocation goodsLocationForLoadOrCreate;
		protected virtual Business.CusGoodsLocation LoadOrCreateGoodsLocation(bool createNewIfNotExists = false)
		{
			if (goodsLocationForLoadOrCreate == null)
			{
				var locationUse = CusGoodsLocationUseList.Codes.TemporaryStorage;
				var loadOrCreateResult = (Business.CusGoodsLocation)Customs.Business.CusGoodsLocation.Load(GoodsLocationType, this, locationUse);
				if (loadOrCreateResult == null && createNewIfNotExists)
				{
					loadOrCreateResult = (Business.CusGoodsLocation)Customs.Business.CusGoodsLocation.New(GoodsLocationType, this, locationUse);
					ToggleValueChangedHandlers(loadOrCreateResult);
				}

				if (loadOrCreateResult != null)
				{
					RegisterEditableChildObject(loadOrCreateResult);
				}
				goodsLocationForLoadOrCreate = loadOrCreateResult;
			}
			return goodsLocationForLoadOrCreate;
		}

		void ToggleValueChangedHandlers(Business.CusGoodsLocation goodsLocation)
		{
			foreach (var info in GetPropertyInfoListToMarkAsNeedingValidation(goodsLocation))
			{
				info.ValueChanged += MarkAsNeedingValidation;
			}
			goodsLocation.CGL_TypeInfo.ValueChanged += DefaultAuthorizationProperties;
		}

		protected static IEnumerable<ZPropertyInfo> GetPropertyInfoListToMarkAsNeedingValidation(Business.CusGoodsLocation goodsLocation)
		{
			yield return goodsLocation.CGL_LocationUseInfo;
			yield return goodsLocation.CGL_ParentIDInfo;
			yield return goodsLocation.CGL_ParentTableCodeInfo;
			yield return goodsLocation.CGL_AdditionalIdentifierInfo;
			yield return goodsLocation.CGL_QualifierInfo;
			yield return goodsLocation.CGL_TypeInfo;
			yield return goodsLocation.CGL_TypeInfo;
			yield return goodsLocation.Address.E2_IsResidentialInfo;
			yield return goodsLocation.Address.E2_AddressOverrideInfo;
			yield return goodsLocation.Address.E2_ScreeningStatusInfo;
			yield return goodsLocation.Address.E2_ParentIDInfo;
			yield return goodsLocation.Address.E2_ParentTableCodeInfo;
			yield return goodsLocation.Address.E2_OA_AddressInfo;
			yield return goodsLocation.Address.E2_ContactInfo;
			yield return goodsLocation.Address.E2_ValidationStatusInfo;
			yield return goodsLocation.Address.E2_RN_NKCountryCodeInfo;
			yield return goodsLocation.Address.E2_PhoneInfo;
			yield return goodsLocation.Address.E2_FaxInfo;
			yield return goodsLocation.Address.E2_MobileInfo;
			yield return goodsLocation.Address.E2_AddressMapInfo;
			yield return goodsLocation.Address.E2_CompanyNameInfo;
			yield return goodsLocation.Address.E2_GovRegNumTypeInfo;
			yield return goodsLocation.Address.E2_GovRegNumInfo;
			yield return goodsLocation.Address.E2_AdditionalAddressInformationInfo;
			yield return goodsLocation.Address.E2_GeoLocationInfo;
			yield return goodsLocation.Address.E2_AddressTypeInfo;
			yield return goodsLocation.Address.E2_EmailInfo;
			yield return goodsLocation.Address.E2_PostcodeInfo;
			yield return goodsLocation.Address.E2_CityInfo;
			yield return goodsLocation.Address.E2_Address2Info;
			yield return goodsLocation.Address.E2_Address1Info;
		}

		protected void MarkAsNeedingValidation(object sender, EventArgs e) => MarkAsNeedingValidation();

		protected virtual void DefaultAuthorizationProperties(object sender, EventArgs e)
		{
			if (sender is CusGoodsLocation goodsLocation && goodsLocation.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace)
			{
				if (AuthorizationType != AuthorizationTypeList.Codes.TST)
				{
					AuthorizationType = AuthorizationTypeList.Codes.TST;
				}
			}
			else
			{
				AuthorizationType = ZString.Empty;
				AuthorizationOwner = ZGuid.Empty;
				AuthorizationNumber = ZString.Empty;
			}
		}

		[ResourceStringData("B591A0B2-6ED6-4DA9-86FE-68084F61BDB4", Caption = "Location of Goods", ShortCaption = "Location")]
		public virtual ZString GoodsLocationDescription => LoadOrCreateGoodsLocation(createNewIfNotExists: false)?.DisplayText ?? ZString.Empty;

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		ZString ICusGoodsLocationProvider.ProviderKey => GetCusGoodsLocationProviderKeyCore();

		protected virtual ZString GetCusGoodsLocationProviderKeyCore() => ZString.Empty;

		void ICusGoodsLocationProvider.ValidateGoodsLocationDescription()
		{
			Validation.ValidateGoodsLocationDescription();
			GoodsLocationDescriptionInfo.RefreshBinding();
		}
		#endregion

		public ZString DataGrouping => GetDataGroupingCore();

		protected virtual ZString GetDataGroupingCore() => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(AMA_RN_NKCountry);

		#region IJobInvoicingPlugIn

		public IJobInvoicingSupporter InvoicingSupporter => invoicingSupporter ??= new TemporaryStorageInvoicingSupporter(this);
		TemporaryStorageInvoicingSupporter invoicingSupporter;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion => false;

		public string JobNumber => AMA_JobReference;

		#endregion

		#region Messages

		[ChildEditable]
		public EDIMessageCollection Messages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = CreateNewEDIMessageCollection();
					ediMessages.Load();
					ediMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(ediMessages);
				}

				return ediMessages;
			}
		}
		EDIMessageCollection ediMessages;

		protected virtual EDIMessageCollection CreateNewEDIMessageCollection() => new EDIMessageCollection(this);

		public TemporaryStorageMessagingProvider MessagingProvider
		{
			get
			{
				var dataGrouping = (string)DataGrouping;
				return Factory.GetCachedValue("TemporaryStorageMessagingProvider_" + dataGrouping, () =>
				{
					TemporaryStorageMessagingProvider result = null;
					var types = ObjectFactory.Get<Hashtable>("TemporaryStorageMessagingProvider");

					if (!string.IsNullOrEmpty(dataGrouping))
					{
						var objectHandle = (ObjectHandle)types[dataGrouping];
						result = (TemporaryStorageMessagingProvider)objectHandle?.GetObject();
					}

					return result;
				});
			}
		}

		#endregion

		protected override IValueSetStrategy GetValueSetStrategy() => new TemporaryStorageHeaderValueSetStrategy(this);

		CusAuthorizationUsage LoadOrCreateCusAuthorizationUsage(ZBool shouldCreateNew)
		{
			var result = CusAuthorizationUsage.Load(this);

			if (result == null && shouldCreateNew)
			{
				result = CusAuthorizationUsage.New(this);
			}

			return result;
		}

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			return GetCusSupportingInfoTypesCore();
		}

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(TemporaryStoragePreviousDocument) }
			};
		}

		[ChildEditable(true)]
		public ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> PreviousDocuments
		{
			get { return previousDocuments ?? (previousDocuments = GetPreviousDocuments()); }
		}
		ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> previousDocuments;

		ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> GetPreviousDocuments()
		{
			var result = CreateNewPreviousDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);

			return result;
		}

		protected virtual ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> CreateNewPreviousDocumentCollection() => new TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(this);

		public CusAuthorizationUsage AuthorizationUsage => LoadOrCreateCusAuthorizationUsage(false);

		public CusAuthorizationUsage AuthorizationUsageOrNew => LoadOrCreateCusAuthorizationUsage(true);

		[LightValidationTestExempt]
		public override ZInt AMA_ClusterKey
		{
			get => base.AMA_ClusterKey;
			set
			{
				if (AMA_ClusterKey != value)
				{
					base.AMA_ClusterKey = value;
					if (!IsCopying && AuthorizationUsage != null)
					{
						AuthorizationUsage.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("1DB7648B-F9FB-4373-B587-7BD8406C233C", Caption = "Authorization Type")]
		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.AuthorizationTypeList))]
		public virtual ZString AuthorizationType
		{
			get
			{
				return AuthorizationUsage?.AGC_Code ?? ZString.Empty;
			}

			set
			{
				var oldValue = AuthorizationType;
				if (oldValue != value)
				{
					var authorizationUsage = AuthorizationUsageOrNew;
					authorizationUsage.AGC_Code = value;
					RegisterEditableChildObject(authorizationUsage);

					if (value.IsEmpty)
					{
						AuthorizationNumber = ZString.Empty;
						AuthorizationOwner = ZGuid.Empty;
						authorizationUsage?.Delete();
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorizationType();
				}
				AuthorizationTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo AuthorizationTypeInfo => GetZPropertyInfo(nameof(AuthorizationType));

		[MaxLength(35)]
		[ReadOnlyMember(nameof(CusAuthorizationUsageProperty_ReadOnly))]
		[ResourceStringData("34BD0D72-B41D-4D54-9F8E-B869C9F8350D", Caption = "Authorization Number")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.AuthorizationNumberList))]
		public ZString AuthorizationNumber
		{
			get
			{
				return AuthorizationUsage?.AGC_Number ?? ZString.Empty;
			}

			set
			{
				var oldValue = AuthorizationNumber;
				if (oldValue != value)
				{
					var authorizationUsage = AuthorizationUsageOrNew;
					authorizationUsage.AGC_Number = value;
					RegisterEditableChildObject(authorizationUsage);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorizationNumber();
					Validation.ValidateAuthorizationType();
				}
				AuthorizationNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo AuthorizationNumberInfo => GetZPropertyInfo(nameof(AuthorizationNumber));

		[ResourceStringData("BCE7D795-D766-457C-B6EC-059F153234D1", Caption = "Owner")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.OwnerList))]
		[ReadOnlyMember(nameof(CusAuthorizationUsageProperty_ReadOnly))]
		public ZGuid AuthorizationOwner
		{
			get
			{
				return AuthorizationUsage?.AGC_OH_Owner ?? ZGuid.Empty;
			}

			set
			{
				var oldValue = AuthorizationOwner;
				if (oldValue != value)
				{
					var authorizationUsage = AuthorizationUsageOrNew;
					authorizationUsage.AGC_OH_Owner = value;
					RegisterEditableChildObject(authorizationUsage);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorizationOwner();
					Validation.ValidateAuthorizationType();
				}

				AuthorizationOwnerInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo AuthorizationOwnerInfo => GetZPropertyInfo(nameof(AuthorizationOwner));

		protected virtual bool CusAuthorizationUsageProperty_ReadOnly => AuthorizationType.IsEmpty;

		public override AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => AsycudaPackPackedItemPivotCollection.RelationshipType.Many;

		public TemporaryStorageMessageSendingConfiguration MessageSendingConfiguration => messageSendingConfiguration ?? (messageSendingConfiguration = GetNewMessageSendingConfiguration());
		TemporaryStorageMessageSendingConfiguration messageSendingConfiguration;
		protected virtual TemporaryStorageMessageSendingConfiguration GetNewMessageSendingConfiguration() => new TemporaryStorageMessageSendingConfiguration();

		public TemporaryStorageConfiguration Configuration => configuration ?? (configuration = TemporaryStorageConfiguration.GetConfiguration(Factory, DataGrouping));

		TemporaryStorageConfiguration configuration;

		public ZString LayoutProviderKey => LayoutProviderKeyCore;

		protected virtual ZString LayoutProviderKeyCore => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(AMA_RN_NKCountry);

		public ZBool IsTransfer => AMA_MessageType == PNTSMessageTypeList.Codes.Transfer;

		public ZBool IsDeconsolidation => AMA_MessageType == PNTSMessageTypeList.Codes.Deconsolidation;

		#region Guarantee

		public TemporaryStorageHeaderGuarantee Guarantee
		{
			get
			{
				if (guarantee == null || guarantee.IsDeleted)
				{
					guarantee = (TemporaryStorageHeaderGuarantee)Factory.LoadTop1(TemporaryStorageHeaderGuaranteeType, new ZQuery(CusBondDetailSchema.PW_ParentID, PK) { FetchOnlyFromLocalCache = !IsInDatabase });

					if (guarantee == null)
					{
						guarantee = (TemporaryStorageHeaderGuarantee)Factory.New(TemporaryStorageHeaderGuaranteeType);
						using (guarantee.SuspendSettingHasChanges())
						{
							guarantee.Parent = this;
							guarantee.PW_BondType = GuaranteeBondType;
						}
					}

					RegisterEditableChildObject(guarantee);
				}
				return guarantee;
			}
		}
		TemporaryStorageHeaderGuarantee guarantee;

		protected virtual Type TemporaryStorageHeaderGuaranteeType => typeof(TemporaryStorageHeaderGuarantee);

		public string GuaranteeBondType => GuaranteeBondTypeCore;
		protected virtual string GuaranteeBondTypeCore => EUGuaranteeTypeList.Codes.COD;

		public ZString[] GetC0009CountryCodes()
		{
			var date = ZDateTime.Today;
			return ZZRefCusCodeListCombined.GetUniqueCodes(Factory, DefaultDataGroupingCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, date, includeParentDataGroupings: true);
		}

		public ZString DefaultDataGroupingCode => DefaultDataGroupingCodeCore;

		protected virtual ZString DefaultDataGroupingCodeCore => CountryCode;

		public ZString CountryCode => Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		GlbCompany Company
		{
			get
			{
				var branch = Branch;
				return (branch == null) ? Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK) : branch.Company;
			}
		}

		#endregion

		#region Cloning and copying

		protected virtual TemporaryStorageHeaderCloneStrategy GetTemporaryStorageHeaderCloneStrategy(BusinessObject bizObjToClone) => new(bizObjToClone);

		public IBusiness TemplateCopy()
		{
			var result = (TemporaryStorageHeader)GetTemporaryStorageHeaderCloneStrategy(this).Clone();
			return result;
		}

		protected override bool SupportsCloneCore() => true;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newTemporaryStorageHeader = (TemporaryStorageHeader)base.CloneInternal(args);

			PreviousDocuments.ForEach(preDoc => newTemporaryStorageHeader.PreviousDocuments.Add(Clone(preDoc)));
			CloneBills();

			return newTemporaryStorageHeader;

			BusinessObject Clone(BusinessObject bizObjToClone)
				=> GetTemporaryStorageHeaderCloneStrategy(bizObjToClone).Clone();

			void CloneBills()
			{
				if (newTemporaryStorageHeader.MasterBill is { } masterBill)
				{
					newTemporaryStorageHeader.Bills.RemoveAndDelete(masterBill);
				}
				Bills.ForEach(bill =>
				{
					newTemporaryStorageHeader.Bills.Add(Clone(bill));
				});
			}
		}

		#endregion

		internal ITemporaryStorageHeaderValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, () => Configuration.GetValidationDecider());
		CachedValue<ITemporaryStorageHeaderValidationDecider> validationDeciderCached;

		#region ICusGoodsLocationTypeSupporter

		Type ICusGoodsLocationTypeSupporter.GoodsLocationType => GoodsLocationType;

		Type GoodsLocationType => GoodsLocationTypeCore;

		protected virtual Type GoodsLocationTypeCore => CusGoodsLocation.TypeDecider.GetTypeForCountryCode(CountryCode);

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter => documentSupporter ??= new TemporaryStorageHeaderDocumentSupporter(this);
		DocumentSupporter documentSupporter;

		string IDocumentSupportable.TableName => TableName;

		#endregion

		#region IEDocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter() => new JobInvoicingEDocsProviderSupporter(this);

		#endregion

		#region Confirm/Reserve Temporary Storage Goods

		protected virtual ZString PreviousDocumentCodeForDataToReserveTemporaryStorageGoods => ZString.Empty;

		protected virtual ZBool ShouldConfirmTemporaryStorageGoodsConsumption => false;

		public ZString TemporaryStorageTransactionInternalReferenceNumber => TemporaryStorageTransactionInternalReferenceNumberCore;
		protected virtual ZString TemporaryStorageTransactionInternalReferenceNumberCore => ZString.Empty;

		public ZString TemporaryStorageTransactionInternalReferenceType => TemporaryStorageTransactionInternalReferenceTypeCore;
		protected virtual ZString TemporaryStorageTransactionInternalReferenceTypeCore => ZString.Empty;

		protected virtual IReadOnlyList<ZString> CustomsStatusToCancelTemporaryStoragePendingTransactions => Array.Empty<ZString>();

		protected virtual IReadOnlyList<ZString> CustomsStatusToConfirmTemporaryStoragePendingTransactions => Array.Empty<ZString>();

		protected virtual IReadOnlyList<ZString> CustomsStatusToNotCreateTemporaryStorageTransactions => Array.Empty<ZString>();

		public ZString TemporaryStorageTransactionCommentPrefix => TemporaryStorageTransactionCommentPrefixCore;
		protected virtual ZString TemporaryStorageTransactionCommentPrefixCore => ZString.Empty;

		protected virtual ZString TemporaryStorageWriteOffTransactionCommentReferenceNumber => ZString.Empty;

		public (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoods() => GetEntryLineDataDeclaredToReserveTSGoodsCore();
		protected virtual (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoodsCore() => (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), ZString.Empty);

		protected virtual ZBool ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation => false;

		protected virtual ZString GetDocumentNumberFormat(ZString dsdtMRN) => dsdtMRN;

		#endregion

		public ZBool IsSent => IsSentCore;
		protected virtual ZBool IsSentCore => AMA_MessageStatus == LogicalStatusList.Codes.Sent;

		public bool ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible => MRN.IsEmpty && IsMessageTypeMatchingToTSRegisterManagementSelectInventory && TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(CountryCode) && TemporaryStorageHelper.IsLocationManagedInPremises(Factory, GoodsLocation.Address.AuthorisationNumber, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse);

		public bool IsMessageTypeMatchingToTSRegisterManagementSelectInventory => IsMessageTypeMatchingToTSRegisterManagementSelectInventoryCore;

		protected virtual bool IsMessageTypeMatchingToTSRegisterManagementSelectInventoryCore => false;
	}
}
