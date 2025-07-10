using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Documents.DocDataObjects;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.DE.Business.WarehouseIntegration;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Declaration
{
	[SystemDefinedValues]
	[VisualizableDocumentsSupportable(nameof(JobDeclarationDEVisualizableDocumentSupporter))]
	public partial class JobDeclaration : AutoDEJobDeclaration,
		Integration.Customs.DE.IJobDeclaration
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override Properties

		[ReadOnlyMember(nameof(JE_RL_NKFinalDestination_ReadOnly))]
		public override ZString JE_RL_NKFinalDestination
		{
			get => base.JE_RL_NKFinalDestination;
			set
			{
				var oldValue = JE_RL_NKFinalDestination;
				base.JE_RL_NKFinalDestination = value;
				if (!IsCopying && oldValue != JE_RL_NKFinalDestination)
				{
					InvoiceLines.ForEach(x => x.MarkAsNeedingValidation());
				}
			}
		}
		ZBool JE_RL_NKFinalDestination_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		public override ZString JE_MessageType
		{
			get { return base.JE_MessageType; }
			set
			{
				var oldValue = JE_MessageType;
				base.JE_MessageType = value;
				if (!IsCopying && oldValue != JE_MessageType)
				{
					CustomsEntryInstructions.ForEach(x =>
					{
						x.ReimportCountryCodes.MarkAsNeedingValidation();
						x.IdentificationMeanCodes.MarkAsNeedingValidation();
						x.GoodsLocation.CGL_Qualifier = ZString.Empty;
					});
					RefreshIncotermAndChargeFactory();
					Packages.MarkAsNeedingValidation();
					if (value.In(new ZString[] { Common.DE.DEJobMessageTypeList.Codes.Import, Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment }))
					{
						JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					}
					ClearFieldsIfNeeded();
				}
			}
		}

		[ReadOnlyMember(nameof(JE_TransportModeInland_ReadOnly))]
		public override ZString JE_TransportModeInland
		{
			get => base.JE_TransportModeInland;
			set
			{
				var oldValue = JE_TransportModeInland;
				base.JE_TransportModeInland = value;
				if (!IsCopying && oldValue != JE_TransportModeInland)
				{
					SetDefaultTransportMeansForInlandTransportMode();
				}
			}
		}

		ZBool JE_TransportModeInland_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[MaxLength(35)]
		[ReadOnlyMember(nameof(JE_LocationOfGoods_ReadOnly))]
		public override ZString JE_LocationOfGoods
		{
			get => base.JE_LocationOfGoods;
			set => base.JE_LocationOfGoods = value;
		}
		ZBool JE_LocationOfGoods_ReadOnly => IsWarehouseAdjustment;

		[ReadOnly(true)]
		public override ZDateTime JE_EntryAuthorisationDate
		{
			get { return base.JE_EntryAuthorisationDate; }
			set { base.JE_EntryAuthorisationDate = value; }
		}

		[ReadOnly(true)]
		public override ZString JE_EntryStatus
		{
			get { return base.JE_EntryStatus; }
			set { base.JE_EntryStatus = value; }
		}

		[ReadOnly(true)]
		public override ZString JE_MessageStatus
		{
			get { return base.JE_MessageStatus; }
			set { base.JE_MessageStatus = value; }
		}

		[MaxLength(35)]
		public override ZString JE_OwnerRef
		{
			get { return base.JE_OwnerRef; }
			set { base.JE_OwnerRef = value; }
		}

		[ReadOnlyMember(nameof(JE_VesselName_ReadOnly))]
		public override ZString JE_VesselName
		{
			get => base.JE_VesselName;
			set
			{
				base.JE_VesselName = value;
				if (!IsCopying && !IsSea && !value.IsEmpty)
				{
					JE_MasterBill = value;
				}
			}
		}
		ZBool JE_VesselName_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_ContainerMode_ReadOnly))]
		public override ZString JE_ContainerMode
		{
			get => base.JE_ContainerMode;
			set
			{
				var oldValue = JE_ContainerMode;
				base.JE_ContainerMode = value;
				if (!IsCopying && oldValue != JE_ContainerMode && IsContainerised)
				{
					foreach (var package in Packages.Cast<BasePackage>())
					{
						if (!package.CW_Seal.IsEmpty)
						{
							package.CW_Seal = ZString.Empty;
						}
					}
				}
			}
		}
		ZBool JE_ContainerMode_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		public new ICusContainerCollection<CusContainer> CusContainers => (ICusContainerCollection<CusContainer>)base.CusContainers;

		public new InlandTransportCollection InlandTransports => (InlandTransportCollection)base.InlandTransports;

		protected override EU.Business.Declaration.InlandTransportCollection GetNewInlandTransportCollection() => new InlandTransportCollection(this);

		public new ItineraryCountryCollection ItineraryCountries => (ItineraryCountryCollection)base.ItineraryCountries;

		protected override EU.Business.ItineraryCountryCollection GetNewItineraryCountriesCollection() => new ItineraryCountryCollection(this);

		protected sealed override bool ShouldSetDefaultBorderTransportToIDForTransportMode() => true;

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new CusContainerCollection(this, Factory);

		protected override ZAddress GetNewJE_OA_Representative_ZAddress()
		{
			var result = base.GetNewJE_OA_Representative_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;
		}

		protected override ZAddress GetNewJE_OA_BuyingAgentAddress_ZAddress()
		{
			var result = base.GetNewJE_OA_BuyingAgentAddress_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;
		}

		protected override ZAddress GetNewJE_OA_SellerAddress_ZAddress()
		{
			var result = base.GetNewJE_OA_SellerAddress_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;
		}

		protected override ZAddress GetNewJE_OA_ConsigneeAddress_ZAddress()
		{
			var result = base.GetNewJE_OA_ConsigneeAddress_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;
		}

		protected override ZAddress GetNewJE_OA_DeclarantAddress_ZAddress()
		{
			var result = base.GetNewJE_OA_DeclarantAddress_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;
		}

		[ReadOnlyMember(nameof(ZG_MethodOfPayment_ReadOnly))]
		public override ZString ZG_MethodOfPayment
		{
			get => base.ZG_MethodOfPayment;
			set
			{
				var oldValue = ZG_MethodOfPayment;
				base.ZG_MethodOfPayment = value;
				if (oldValue != ZG_MethodOfPayment)
				{
					DefermentPartyDocAddress.MarkAsNeedingValidation();
				}
			}
		}
		#endregion

		#region New Properties

		[MaxLength(Schema.JE_PaymentMethodMaxLength)]
		[ResourceStringData("76CDDC6C-5D50-4757-B933-8E4D0EC1A68B", Caption = "Payment Party 1", MediumCaption = "Pmt. Party 1", ShortCaption = "Party 1")]
		public override ZString JE_PaymentMethod
		{
			get => base.JE_PaymentMethod;
			set => base.JE_PaymentMethod = value;
		}

		[MaxLength(10)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DefermentAccountNumberList))]
		[ResourceStringData("F6791DFA-0136-4441-B1F8-3419F6986D9B", Caption = "Account No. 1", MediumCaption = "Acc. No. 1", ShortCaption = "Acc. 1")]
		public override ZString JE_DefermentAccountNumber
		{
			get => base.JE_DefermentAccountNumber;
			set => base.JE_DefermentAccountNumber = value;
		}

		[ResourceStringData("88F42753-36F6-4A55-A895-5857A7A3EF90", Caption = "Payment Party 2", MediumCaption = "Pmt. Party 2", ShortCaption = "Party 2")]
		public override ZString ZG_VATDeferType
		{
			get => base.ZG_VATDeferType;
			set => base.ZG_VATDeferType = value;
		}

		[ResourceStringData("6DA37D31-94A0-444F-B02B-80CF01038EB8", Caption = "Account No. 2", MediumCaption = "Acc. No. 2", ShortCaption = "Acc. 2")]
		public override ZString ZG_VATDeferNumber
		{
			get => base.ZG_VATDeferNumber;
			set => base.ZG_VATDeferNumber = value;
		}

		void ClearDeferralFieldsIfNeeded()
		{
			if (!IsImport)
			{
				JE_PaymentMethod = ZString.Empty;
				JE_DefermentAccountNumber = ZString.Empty;
				ZG_VATDeferType = ZString.Empty;
				ZG_VATDeferNumber = ZString.Empty;
			}
			else if (!IsExport)
			{
				ContractualPartnerDocAddress.Delete();
			}
		}

		[ReadOnlyMember(nameof(ZG_IsHighValueOvrd_ReadOnly))]
		public override ZBool ZG_IsHighValueOvrd
		{
			get => base.ZG_IsHighValueOvrd;
			set
			{
				if (base.ZG_IsHighValueOvrd != value)
				{
					base.ZG_IsHighValueOvrd = value;
					if (value && IsImport)
					{
						TopGroupInvoice.Charges.Cast<GroupInvoiceCharge>().ForEach(x => x.SetValuesIfNeeded());
						Invoices.ForEach(x => x.Charges.Cast<InvoiceCharge>().ForEach(y => y.SetValuesIfNeeded()));
						InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Charges.Cast<InvoiceLineCharge>().ForEach(y => y.SetValuesIfNeeded()));
						if (!DV1Details.Any())
						{
							DV1Details.AddNew();
						}
					}
				}
			}
		}

		public override ZGuid JE_OA_DeclarantAddress
		{
			get => base.JE_OA_DeclarantAddress;
			set
			{
				var oldValue = JE_OA_DeclarantAddress;
				base.JE_OA_DeclarantAddress = value;
				if (oldValue != JE_OA_DeclarantAddress)
				{
					UpdateAuthorizationNumberOnEntryInstructionsPreviousDocumentMasterIfNecessaryAndIsImport();
					InvoiceLines.MarkAsNeedingValidation();
					UpdateCusReconEntriesPropertyIfNecessary(x => x.CRE_OA_DeclarantAddressInfo, JE_OA_DeclarantAddress);
					UpdateVATClaimBackIfNecessary();
				}
			}
		}

		[ReadOnlyMember(nameof(JE_OA_Representative_ReadOnly))]
		public override ZGuid JE_OA_Representative
		{
			get => base.JE_OA_Representative;
			set
			{
				var oldValue = JE_OA_Representative;
				base.JE_OA_Representative = value;
				if (oldValue != JE_OA_Representative)
				{
					UpdateAuthorizationNumberOnEntryInstructionsPreviousDocumentMasterIfNecessaryAndIsImport();
					UpdateCusReconEntriesPropertyIfNecessary(x => x.CRE_OA_RepresentativeAddressInfo, JE_OA_Representative);
				}
			}
		}

		public override ZGuid JE_OA_BuyingAgentAddress
		{
			get => base.JE_OA_BuyingAgentAddress;
			set
			{
				var oldValue = JE_OA_BuyingAgentAddress;
				base.JE_OA_BuyingAgentAddress = value;
				if (oldValue != JE_OA_BuyingAgentAddress)
				{
					UpdateCusReconEntriesPropertyIfNecessary(x => x.CRE_OA_BuyingAgentAddressInfo, JE_OA_BuyingAgentAddress);
				}
			}
		}

		protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.ImporterDocumentaryAddressChanged(sender, e);

			DeclarationValueSetStrategy.UpdateAddressesDependingOnDeclarantType(JE_DeclarantType);
			UpdateCusReconEntriesPropertyIfNecessary(x => x.CRE_OA_ImporterAddressInfo, ImporterDocumentaryAddress.E2_OA_Address);
			UpdateInvoiceHeadersConsigneeAddress();
		}

		protected override void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.SupplierDocumentaryAddressChanged(sender, e);
			UpdateDeclarantAddressIfEmpty();
		}

		[ResourceStringData("67CE7106-DA7B-4D3C-BA5D-E1E384C05AA5", Caption = "Start")]
		public override ZDateTime ZG_PresentationStartDate
		{
			get { return base.ZG_PresentationStartDate; }
			set { base.ZG_PresentationStartDate = value; }
		}

		[ResourceStringData("156494FD-DE68-4E50-82F7-C6D9F9F354B4", Caption = "End")]
		public override ZDateTime ZG_PresentationEndDate
		{
			get { return base.ZG_PresentationEndDate; }
			set { base.ZG_PresentationEndDate = value; }
		}

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.StatisticStatusCodeList))]
		[UniversalCopyAddInfoPropertyMapping(DEJobDeclarationSchema.Constants.JE_StatisticsGoodsStatus)]
		[ResourceStringData("E08C43CD-5A4C-4F40-B250-E7626E3D725E", Caption = "Statistic Status")]
		public ZString JE_StatisticStatus
		{
			get => base.JE_StatisticsGoodsStatus;
			set => base.JE_StatisticsGoodsStatus = value;
		}

		public ZPropertyInfo JE_StatisticStatusInfo => GetWrappedZPropertyInfo(nameof(JE_StatisticStatus), c => JE_StatisticsGoodsStatusInfo);

		[ResourceStringData("Enterprise.Customs.DE.Business.Declaration|PrematureInputFlag", Caption = "Premature Input Flag")]
		public override ZBool JE_PrematureInputFlag
		{
			get => base.JE_PrematureInputFlag;
			set => base.JE_PrematureInputFlag = value;
		}

		[ResourceStringData("Enterprise.Customs.DE.Business.Declaration|VATClaimBack", Caption = "VAT Claim Back")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.VATClaimBackList))]
		public override ZString JE_VATClaimBack
		{
			get => base.JE_VATClaimBack;
			set => base.JE_VATClaimBack = value;
		}

		ZString DeclarantVATClaimBackFlag => ((DEOrgImpAddInfo)Declarant?.Header?.GetCountryData(Core.Constants.CountryCodes.Germany).ImpAddInfo)?.ZO_VATClaimBack ?? ZString.Empty;

		public event CancelEventHandler OnPreviousDocumentMasterCSI_ProcedureAboutToChange;

		public void PreviousDocumentMasterCSI_ProcedureAboutToChange(object sender, CancelEventArgs args) => OnPreviousDocumentMasterCSI_ProcedureAboutToChange?.Invoke(sender, args);

		public event EventHandler OnInvHeaderZG_AgreedPlaceCodeValueChanged;

		public void InvHeaderZG_AgreedPlaceCodeValueChanged(object sender, EventArgs args) => OnInvHeaderZG_AgreedPlaceCodeValueChanged?.Invoke(sender, args);

		public ZBool IsDeclarantEntitledToClaimBackVAT => YesNoList.IsYes(DeclarantVATClaimBackFlag);

		void UpdateVATClaimBackIfNecessary()
		{
			if (IsImport)
			{
				JE_VATClaimBack = DeclarantVATClaimBackFlag;
			}
		}

		public ZString ControlMessageUnreadEDocStatus
		{
			get
			{
				return Factory.GetValue(ref controlMessageUnreadEDocStatus, () => this.GetSystemDefinedValue<ZString>(nameof(ControlMessageUnreadEDocStatus)));
			}
			set
			{
				if (value != ControlMessageUnreadEDocStatus)
				{
					this.SetSystemDefinedValue(nameof(ControlMessageUnreadEDocStatus), value);
				}
			}
		}
		CachedProperty<ZString> controlMessageUnreadEDocStatus;

		public ZBool IsStockMovement
		{
			get
			{
				var customsEntryInstructions = CustomsEntryInstructions;
				return IsImport && customsEntryInstructions.Any() && customsEntryInstructions.All(x => ImportDeclarationTypeList.IsLUZ(x.CEI_Style));
			}
		}

		public RefVessel InlandVessel
		{
			get
			{
				if (inlandVessel == null || inlandVessel.RV_Code != JE_TransportIDInland)
				{
					var vesselQuery = new ZQuery(RefVesselSchema.RV_Code, JE_TransportIDInland);
					vesselQuery.IgnoreActiveFilter = true;
					inlandVessel = Factory.LoadTop1<RefVessel>(vesselQuery);
				}

				return inlandVessel;
			}
		}
		RefVessel inlandVessel;

		public ZBool IsImportAndNotStockMovement => IsImport && !IsStockMovement;

		public ZBool SkipSnapshotUpdate = false;

		#endregion

		#region Override

		protected override bool IsInventorySelectionEnabledCore => !IsImport && base.IsInventorySelectionEnabledCore;

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new DeclarationJobDocAddressValidation(addressToValidate, this);

		public override void OnSaving()
		{
			UpdateOrganizationAddressesIfNecessary();
			DefaultReferralDataIfNecessary();

			base.OnSaving();

			Factory.Saved += PublishBondedWarehouseWhenFieldsChangedAndShouldUpdate;
		}

		public new JobDeclarationValidation Validation => (JobDeclarationValidation)GetNewValidation();

		public override ZString WarehouseTransactionStatus
		{
			get => JE_WarehouseTransactionStatus;
			set
			{
				JE_WarehouseTransactionStatus = value;
				foreach (CusEntryHeader entryHeader in ActiveEntryHeaders)
				{
					entryHeader.CH_WarehouseTransactionStatus = value;
				}
			}
		}

		public ZBool IsOutwardOrderImported
		{
			get
			{
				return Factory.GetValue(ref isOutwardOrderImported, () => this.GetSystemDefinedValue<ZBool>(nameof(IsOutwardOrderImported)));
			}
			set
			{
				if (value != IsOutwardOrderImported)
				{
					this.SetSystemDefinedValue(nameof(IsOutwardOrderImported), value);
				}
			}
		}
		CachedProperty<ZBool> isOutwardOrderImported;

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			JobDeclarationValidation result;
			if (IsStockMovement)
			{
				result = new StockMovementJobDeclarationValidation(this);
			}
			else if (IsImport)
			{
				result = new ImportJobDeclarationValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobDeclarationValidation(this);
			}
			else if (IsWarehouseAdjustment)
			{
				result = new WarehouseAdjustmentJobDeclarationValidation(this);
			}
			else
			{
				result = new JobDeclarationValidation(this);
			}

			return result;
		}

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		protected override Customs.Business.JobDeclarationLookups GetNewLookups()
		{
			JobDeclarationLookups result;

			if (IsImport)
			{
				result = new ImportJobDeclarationLookups(this);
			}
			else if (IsExport)
			{
				result = new ExportJobDeclarationLookups(this);
			}
			else
			{
				result = new JobDeclarationLookups(this);
			}

			return result;
		}

		public override ZString GetNumericIncoTermModeCodeFromWtgCode(string pfIncotermMode)
		{
			ZString result = "";
			switch (pfIncotermMode)
			{
				case OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS:
					result = "3";
					break;
				case OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OTH:
					result = "2";
					break;
				case OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OUT:
					result = "1";
					break;
				default:
					break;
			}
			return result;
		}

		protected override bool IsLookupsCachedInBase => false;

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>(this);

		public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

		public override EU.Business.Declaration.EntryCreationStrategy CreateEntryCreationStrategy() => new EntryCreationStrategy(this);

		protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

		protected override DocumentSupporter CreateNewDocumentSupporter() => new JobDeclarationDocumentSupporter(this);

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection() => new BaseDeclarationLevelPackageCollection<Package>(this);

		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this, new CusEntryInstructionComparer());

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
		public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

		#region CustomsOffices
		[ChildEditable(true)]
		public new DEOfficeCodeCollection CustomsOffices => (DEOfficeCodeCollection)base.CustomsOffices;

		protected override EuOfficeCodeCollection GetCustomsOffices() => new DEOfficeCodeCollection(this);

		#endregion

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			return result;
		}

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			return new Dictionary<ZString, Type>()
			{
				{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, typeof(DEOfficeCode) },
				{ EU.Business.CusCodeDataTypeList.Codes.TransportInland, typeof(InlandTransport) },
				{ CusCodeDataTypeList.Codes.CompletionCustomsOffice, typeof(CompletionCustomsOffice) },
				{ EU.Business.CusCodeDataTypeList.Codes.CountryOfRoutingCode, typeof(ItineraryCountry) },
			};
		}

		protected override EU.Business.JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

		protected override string GetIApportionInvoiceHolderCountryContextCore()
		{
			return CountryCode + this.GetIncoTermChargeFactoryCacheKey();
		}

		public override bool IsDeclarantAddressRequired => IsImport || base.IsDeclarantAddressRequired;

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			base.JE_MessageTypeChanged(oldValue, newValue);

			if (newValue == MessageTypeList.Codes.Import)
			{
				if (JE_StatisticStatus.IsEmpty)
				{
					JE_StatisticStatus = StatisticStatusCodeList.Codes.C04;
				}
			}
			else if (oldValue == MessageTypeList.Codes.Import)
			{
				JE_StatisticStatusInfo.ClearValue();
				ZG_IsHighValueOvrdInfo.ClearValue();
				DV1Details.RemoveAndDeleteAll();

				if (fAcquirerDocAddress != null)
				{
					DocAddresses.RemoveAndDelete(fAcquirerDocAddress);
				}

				foreach (var entryInstruction in CustomsEntryInstructions)
				{
					entryInstruction.FiscalReferences.RemoveAndDeleteAll();
				}

				foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					line.JI_CustomsFourthQuantity = ZDecimal.Zero;
					line.JI_CustomsFourthUnitQty = ZString.Empty;
				}
			}

			if (oldValue == MessageTypeList.Codes.Export)
			{
				foreach (var entryInstruction in CustomsEntryInstructions)
				{
					entryInstruction.CusSupplyChainActorReferences.RemoveAndDeleteAll();
				}
			}

			if (newValue == Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment)
			{
				foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					line.ClearFieldsForWarehouseAdjustment();
				}
			}

			foreach (var header in Invoices.Cast<JobComInvoiceHeader>())
			{
				header.DefaultValuesBasedOnDeclarationMessageType();
			}

			ClearDeferralFieldsIfNeeded();
		}

		protected override ZBool IsReciprocalRatesCore => IsReciprocalRatesConstant;

		protected override ZString LocalCurrencyCodeCore => LocalCurrencyConstantCode;

		protected override ZBool ExistsDefermentAccount(OrgHeader defermentParty)
		{
			return defermentParty.DefermentAccountNumberCollection.Any();
		}

		protected override CusAuthorizationUsageUpdater GetNewCusAuthorisationUsageUpdaterCore()
		{
			if (IsImport)
			{
				return new CusAuthorizationUsageImportUpdater(this);
			}
			else if (IsExport)
			{
				return new CusAuthorizationUsageExportUpdater(this);
			}
			else
			{
				return base.GetNewCusAuthorisationUsageUpdaterCore();
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (IsImport && !SkipSnapshotUpdate && HasChanges)
			{
				CustomsEntryInstructions.Where(ei => ImportDeclarationTypeList.GetSimplifiedDeclarationTypeList(Factory).ContainsCode(ei.CEI_Style)).ForEach(ei =>
				{
					if (ei.EntryHeader is CusEntryHeader header && !header.EntryNumber.IsEmpty)
					{
						SimplifiedDeclarationReconEntryBuilder entryBuilder = null;
						if (ImportDeclarationTypeList.IsSimplifiedFreeCirculation(ei.CEI_Style))
						{
							entryBuilder = new CFCRECReconEntryBuilder(header, new CFCRECHeaderProvider(header));
						}
						else if (ImportDeclarationTypeList.IsSimplifiedWarehouse(ei.CEI_Style))
						{
							entryBuilder = new SCWRECReconEntryBuilder(header, new SCWRECHeaderProvider(header));
						}
						else if (ImportDeclarationTypeList.IsSimplifiedInwardProcessing(ei.CEI_Style))
						{
							entryBuilder = new SCIRECReconEntryBuilder(header, new SCIRECHeaderProvider(header));
						}
						entryBuilder.CreateCurrentSnapshot();
					}
				});
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			SkipSnapshotUpdate = false;
		}

		protected override void DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource source)
		{
			if (!IsImport || CustomsEntryInstructions.Count == 0)
			{
				base.DefaultMessageTypeFromSupplierOrImporter(source);
			}
		}

		protected override bool SupportEquipmentsCore => false;

		ZGuid GetDefaultAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			if (orgHeader is OrgHeader organisation)
			{
				result = organisation.MainAddress.PK;
			}
			return result;
		}

		protected override void SetRepresentationTypeIfMatchingEORICodes()
		{
			if (!IsImport)
			{
				base.SetRepresentationTypeIfMatchingEORICodes();
			}
		}

		protected override IValueSetStrategy GetValueSetStrategy() => DeclarationValueSetStrategy;

		internal JobDeclarationValueSetStrategy DeclarationValueSetStrategy => declarationValueSetStrategy ?? (declarationValueSetStrategy = new JobDeclarationValueSetStrategy(this));
		JobDeclarationValueSetStrategy declarationValueSetStrategy;

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser() => new JobDeclarationSynchroniser(this);

		public override bool ShouldCreateDummyInvoiceLinesForMerge => false;

		protected override EU.Business.Declaration.EntryFeePaymentPartyUnderstander GetEntryFeePaymentPartyUnderstanderCore(EU.Business.Declaration.CusEntryHeader header) => new EntryFeePaymentPartyUnderstander(this);

		protected override bool EUD_AgreedPlaceCodeValidationSupportCore => false;

		protected override bool ZG_AgreedPlaceCodeValidationSupportCore => false;

		protected override bool SupportsCalculateInsuranceCore => IsImport;

		public override ZString GetNumericIncoTermModeCodeIncoTermAndFlux(string pfIncoterm)
		{
			if (IsImport || IsExport)
			{
				switch (pfIncoterm)
				{
					case Core.Constants.IncoTerms.CostAndFreight:
					case Core.Constants.IncoTerms.CostInsuranceAndFreight:
					case Core.Constants.IncoTerms.CarriageAndInsurancePaidTo:
					case Core.Constants.IncoTerms.CarriagePaidTo:
					case Core.Constants.IncoTerms.DeliveredAtPlace:
					case Core.Constants.IncoTerms.DeliveredAtTerminal:
					case Core.Constants.IncoTerms.DeliveredDutyPaid:
					case Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded:
						return UniversalReferenceConstants.AgreedPlaceCodes._3;
					case Core.Constants.IncoTerms.ExWorks:
					case Core.Constants.IncoTerms.FreeAlongsideShip:
					case Core.Constants.IncoTerms.FreeCarrier:
					case Core.Constants.IncoTerms.FreeOnBoard:
						return UniversalReferenceConstants.AgreedPlaceCodes._1;
				}
			}

			return ZString.Empty;
		}

		protected override void SetDefaultValuesForDeclarantAndRepresentativeWithOrgProxy(OrgHeader orgHeaderOfOrgProxy)
		{
			var proxyMainAddress = orgHeaderOfOrgProxy.MainAddress?.PK ?? ZGuid.Empty;
			if (proxyMainAddress.IsValid)
			{
				if (IsExport && (orgHeaderOfOrgProxy.OH_IsShippingProvider || orgHeaderOfOrgProxy.OH_IsForwarder || orgHeaderOfOrgProxy.OH_IsBroker))
				{
					JE_OA_Representative = proxyMainAddress;
				}
				else
				{
					JE_OA_DeclarantAddress = proxyMainAddress;
				}
			}
		}

		protected override void OnSuccessfulMerge()
		{
			base.OnSuccessfulMerge();
			CreatePreviousProceduresFromInventoryForEntryInstruction();

			if (IsExport && IsExWarehouse || IsWarehouseAdjustment && AtLeastOneEntryInstructionHasFromWarehouse)
			{
				Factory.Saved += PublishBondedWarehouseAutomationOnMerge;
			}
		}

		public override ZBool ExitControlTabVisible => !IsWarehouseAdjustment && base.ExitControlTabVisible;

		#endregion

		#region ContractualPartnerDocAddress

		protected override void ContractualPartnerDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			var message = Res.GetString("3DAFD241-01DB-4BEE-83B8-F8DAC1D673D6", "The chosen Party Constellation requires a Contractual Partner to be entered.");
			ValidateOrganisationPK(validation, (IEnumerable<CusEntryInstruction> instructions) => instructions.Any(i => i.Constellation1stDigitIs1()), message);
		}

		#endregion

		#region AcquirerDocAddress

		public JobDocAddress AcquirerDocAddress
		{
			get
			{
				if (fAcquirerDocAddress == null || fAcquirerDocAddress.IsDeleted)
				{
					fAcquirerDocAddress = DocAddresses.FindOrCreateWithRequirement(AcquirerDocAddressRequirement);
				}
				return fAcquirerDocAddress;
			}
		}
		JobDocAddress fAcquirerDocAddress;

		public JobDocAddressRequirement AcquirerDocAddressRequirement
		{
			get
			{
				if (fAcquirerDocAddressRequirement == null)
				{
					fAcquirerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Acquirer, ContactType.Administration);
					DocAddressManager.AddRequirement(fAcquirerDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fAcquirerDocAddressRequirement, DocAddressType.Acquirer);
				return fAcquirerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fAcquirerDocAddressRequirement;

		#endregion

		#region ExporterDocAddress

		protected override void ExporterDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			var message = Res.GetString("3FC148CB-A96D-426F-BD65-3C08581C1909", "The chosen Party Constellation requires an Exporter to be entered.");
			ValidateOrganisationPK(validation, (IEnumerable<CusEntryInstruction> instructions) => instructions.Any(i => i.Constellation2ndDigitIs1()), message);
		}

		#endregion

		#region DefermentPartyDocAddress

		protected override void DefermentPartyDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			if (IsImport && !IsDefermentAllowed && !IsInwardProcessingAVABR)
			{
				MandatoryValidation.MessageErrorIfNotEntered(validation.Parent.OrganisationPKInfo);
			}
			else
			{
				base.DefermentPartyDocAddressRequirement_ValidateOrganisationPK(validation);
			}
		}

		#endregion

		public IEnumerable<OrgAddress> GetAuthorizationOrgAddress()
		{
			if (IsImport)
			{
				if (JE_DeclarantType == RepresentationTypeList.Codes._2Direct)
				{
					yield return DeclarantOrgAddress;
					yield return Representative;
				}
				else
				{
					yield return DeclarantOrgAddress;
				}
			}
			else if (IsExport)
			{
				yield return DeclarantOrgAddress;
				yield return Representative;
				yield return SellerAddress;
				yield return SupplierDocumentaryAddress.Address;
			}
		}

		public static readonly ImmutableHashSet<string> PreviousEntryIsATLASSet = ImmutableHashSet.Create(
			"ATC51", "ATD51", "ATE51", "ATP51",
			"ATC71", "ATD71", "ATE71", "ATH71", "ATT71");

		internal static bool IsReciprocalRatesConstant => false;

		internal static ZString LocalCurrencyConstantCode => Enterprise.Core.Constants.CurrencyCodes.Germany;

		internal static RefCurrency GetLocalCurrency() => RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, LocalCurrencyConstantCode);

		void ValidateOrganisationPK(JobDocAddressValidation validation, Func<IEnumerable<CusEntryInstruction>, ZBool> checkPartyConstellationExists, string message)
		{
			if (IsExport)
			{
				var parent = validation.Parent;
				if (parent.OrganisationPK.IsEmpty)
				{
					if (checkPartyConstellationExists(CustomsEntryInstructions))
					{
						parent.OrganisationPKInfo.AddMessageError(message);
					}
				}
			}
		}

		void UpdateDeclarantAddressIfEmpty()
		{
			if (IsExport && SupplierDocumentaryAddress != null && JE_OA_DeclarantAddress.IsEmpty)
			{
				var orgHeader = SupplierDocumentaryAddress.Organisation;
				if (orgHeader != null && (orgHeader.OH_IsConsignor || orgHeader.OH_IsConsignee))
				{
					JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
				}
			}
		}

		void UpdateAuthorizationNumberOnEntryInstructionsPreviousDocumentMasterIfNecessaryAndIsImport()
		{
			if (IsImport)
			{
				this.UpdateAuthorizationNumberOnEntryInstructionsIfNecessary();
			}
		}

		void UpdateCusReconEntriesPropertyIfNecessary(Func<CusReconEntry, ZPropertyInfo> propertyInfoGetter, ZGuid propertyValue)
		{
			ActiveEntryHeaders.Cast<CusEntryHeader>().ForEach(header =>
			{
				var cusReconEntry = header.GetCusReconEntry();
				if (cusReconEntry != null)
				{
					propertyInfoGetter(cusReconEntry).Value = propertyValue;
				}
			});
		}

		ZGuid previousImporterDocumentaryAddress;

		protected override void SetupImporterDocumentaryAddress(JobDocAddress importerDocumentaryAddress)
		{
			base.SetupImporterDocumentaryAddress(importerDocumentaryAddress);

			importerDocumentaryAddress.OrgAddressBeforeChange += (sender, args) => previousImporterDocumentaryAddress = ImporterDocumentaryAddress.E2_OA_Address;
		}

		void UpdateInvoiceHeadersConsigneeAddress() => Invoices
			.Where(i => i.JZ_OA_ConsigneeAddress == previousImporterDocumentaryAddress)
			.Cast<JobComInvoiceHeader>()
			.ForEach(i => i.CopyConsigneeAddressFromDeclarationImporter());

		void UpdateOrganizationAddressesIfNecessary()
		{
			if (ZG_IsHighValueOvrd)
			{
				UpdateAddressIfNecessary(JE_OA_ConsigneeAddressInfo, ImporterDocumentaryAddress);
				UpdateAddressIfNecessary(JE_OA_SellerAddressInfo, SupplierDocumentaryAddress);
			}

			void UpdateAddressIfNecessary(ZPropertyInfo propertyInfo, JobDocAddress docAddress)
			{
				if (propertyInfo.Value.IsEmpty)
				{
					var address = docAddress.Address;
					if (address != null)
					{
						propertyInfo.Value = address.PK;
					}
				}
			}
		}

		void DefaultReferralDataIfNecessary()
		{
			if (IsImport && IsDefermentAllowed && PaymentMethodIsEmpty() && VATDeferTypeIsEmpty())
			{
				var selectedDeferralParty1AsDefAndAccount1As10 = false;
				var defermentParty = DefermentPartyDocAddress.Organisation;
				var declarantHeader = Declarant.Header;
				var representativeHeader = RepresentativeOrgAddress?.Header;

				// Determine Deferral Party 1 and Account 1

				UpdateDeferralFromOrgWithFallBack(
					needUpdate: true,
					paymentParty: DeferralPaymentPartyList.Codes.DefermentParty,
					organisation: defermentParty,
					setValues: SetPaymentMethodAndDefermentAccountNumber,
					OrgCusAccountCodeList.Codes.ImportDutiesOneMonth,
					OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT);

				UpdateDeferralFromOrgWithFallBack(
					needUpdate: PaymentMethodIsEmpty(),
					paymentParty: DeferralPaymentPartyList.Codes.Declarant,
					organisation: declarantHeader,
					setValues: SetPaymentMethodAndDefermentAccountNumber,
					OrgCusAccountCodeList.Codes.ImportDutiesOneMonth,
					OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT);

				UpdateDeferralFromOrgWithFallBack(
					needUpdate: PaymentMethodIsEmpty(),
					paymentParty: DeferralPaymentPartyList.Codes.Representative,
					organisation: representativeHeader,
					setValues: SetPaymentMethodAndDefermentAccountNumber,
					OrgCusAccountCodeList.Codes.ImportDutiesOneMonth,
					OrgCusAccountCodeList.Codes.ImportDutiesOneMonthWithoutImportVAT);

				// Determine Deferral Party 2 and Account 2

				UpdateDeferralFromOrgWithFallBack(
					needUpdate: true,
					paymentParty: DeferralPaymentPartyList.Codes.DefermentParty,
					organisation: defermentParty,
					setValues: SetVATDeferTypeAndVATDeferNumber,
					OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity);

				UpdateDeferralFromOrgWithFallBack(
					needUpdate: VATDeferTypeIsEmpty() && !selectedDeferralParty1AsDefAndAccount1As10,
					paymentParty: DeferralPaymentPartyList.Codes.Declarant,
					organisation: declarantHeader,
					setValues: SetVATDeferTypeAndVATDeferNumber,
					OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity);

				UpdateDeferralFromOrgWithFallBack(
					needUpdate: VATDeferTypeIsEmpty() && !selectedDeferralParty1AsDefAndAccount1As10,
					paymentParty: DeferralPaymentPartyList.Codes.Representative,
					organisation: representativeHeader,
					setValues: SetVATDeferTypeAndVATDeferNumber,
					OrgCusAccountCodeList.Codes.ImportVATWithoutSecurity);

				// validate updated fields again
				Validation.ValidateJE_PaymentMethod();
				Validation.ValidateJE_DefermentAccountNumber();
				AddInfoValidation.ValidateZG_VATDeferType();
				AddInfoValidation.ValidateZG_VATDeferNumber();

				void SetPaymentMethodAndDefermentAccountNumber(ZString paymentMethod, OrgCusAccount defermentAccount)
				{
					JE_PaymentMethod = paymentMethod;
					JE_DefermentAccountNumber = defermentAccount.CZ_Account.Left(JE_DefermentAccountNumberInfo.MaxLength);
					selectedDeferralParty1AsDefAndAccount1As10 =
						JE_PaymentMethod == DeferralPaymentPartyList.Codes.DefermentParty
						&& defermentAccount.CZ_Code == OrgCusAccountCodeList.Codes.ImportDutiesOneMonth;
				}

				void SetVATDeferTypeAndVATDeferNumber(ZString deferType, OrgCusAccount vatAccount)
				{
					ZG_VATDeferType = deferType;
					ZG_VATDeferNumber = vatAccount.CZ_Account.Left(ZG_VATDeferNumberInfo.MaxLength);
				}
			}

			static void UpdateDeferralFromOrgWithFallBack(ZBool needUpdate, ZString paymentParty, OrgHeader organisation, Action<ZString, OrgCusAccount> setValues, params ZString[] accountCodes)
			{
				if (needUpdate && organisation != null && accountCodes?.Length > 0)
				{
					foreach (var accountCode in accountCodes)
					{
						var cusAccount = GetAccount(organisation, accountCode);
						if (cusAccount != null)
						{
							setValues(paymentParty, cusAccount);
							break;
						}
					}
				}
			}

			static OrgCusAccount GetAccount(OrgHeader organisation, ZString accountCode) => organisation?.GetDefermentAccounts().FirstOrDefault(a => a.CZ_Code == accountCode);

			ZBool PaymentMethodIsEmpty() => JE_PaymentMethod.IsEmpty;

			ZBool VATDeferTypeIsEmpty() => ZG_VATDeferType.IsEmpty;
		}

		public ZBool IsDefermentAllowed => Factory.GetValue(ref isDefermentAllowed, () => MethodOfPaymentHelper.RequireDeferralPaymentParty(ZG_MethodOfPayment));
		CachedProperty<ZBool> isDefermentAllowed;

		protected void SetDefaultTransportMeansForInlandTransportMode()
		{
			if (IsUCC6)
			{
				if (!Lookups.TransportMeansList.ContainsCode(JE_TransportMeans))
				{
					JE_TransportMeans = Lookups.TransportMeansList.DefaultCode;
				}
				else if (Lookups.TransportMeansList.DefaultCode == null)
				{
					JE_TransportMeans = ZString.Empty;
				}
			}
		}

		protected override Customs.Business.BondedWarehousingHelper GetNewBondedWarehousingHelper()
		{
			return new BondedWarehousingHelper(this);
		}

		protected override DeclarationInventorySelectionHeader GetNewInventorySelectionHeader()
		{
			DeclarationInventorySelectionHeader result = null;
			if (IsExport)
			{
				result = new ExportInventorySelectionHeader(this);
			}
			else if (IsImport)
			{
				result = new ImportInventorySelectionHeader(this);
			}
			else if (IsWarehouseAdjustment)
			{
				result = new WarehouseAdjustmentInventorySelectionHeader(this);
			}
			return result;
		}

		void CreatePreviousProceduresFromInventoryForEntryInstruction()
		{
			foreach (CusEntryHeader entryHeader in ActiveEntryHeaders)
			{
				var instruction = entryHeader.EntryInstruction;
				if (instruction != null)
				{
					var provider = GetPreviousProceduresProvidersForInstruction(instruction).FirstOrDefault(p => p.IsApplicable);

					provider?.CreatePreviousProcedures();
				}
			}
		}

		protected internal virtual IEnumerable<PreviousProceduresFromInventoryForEntryInstructionProvider> GetPreviousProceduresProvidersForInstruction(
			CusEntryInstruction instruction)
		{
			yield return new BondedWarehousePreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
			yield return new InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(instruction);
		}

		protected override bool SupportMultipleWarehouseEntryCore
		{
			get
			{
				if (IsImport && InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.IsOutOfWarehouseWarehousing))
				{
					return false;
				}
				return base.SupportMultipleWarehouseEntryCore;
			}
		}

		protected override bool SupportsBondedWarehousingCore => true;

		protected override bool IsIntegrationWithAccountingSupported
		{
			get
			{
				var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(Branch?.GB_GC.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty);
				return options.EnableAccountingIntegration;
			}
		}

		public override ZBool IsExWarehouse => InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.IsOutOfWarehouseWarehousing || l.IsOutOfInwardProcessing);

		protected override OrgHeader GetWarehouseClientCore()
		{
			if (IsWarehouseAdjustment)
			{
				return Supplier;
			}
			return base.GetWarehouseClientCore();
		}

		public bool AtLeastOneEntryInstructionHasFromWarehouse => CustomsEntryInstructions.Any(cei => !cei.CEI_OA_Warehouse.IsEmpty);

		void PublishBondedWarehouseAutomationOnMerge(BusinessObjectFactory businessObjectFactory, bool savedSuccessfully)
		{
			businessObjectFactory.Saved -= PublishBondedWarehouseAutomationOnMerge;
			if (savedSuccessfully && (IsExport || IsWarehouseAdjustment))
			{
				var newFactory = new BusinessObjectFactory();
				var declaration = newFactory.Load<JobDeclaration>(PK);
				foreach (var instruction in declaration.CustomsEntryInstructions)
				{
					var entryHeader = instruction.EntryHeader;
					if ((entryHeader != null) && (entryHeader.IsOutOfWarehouseWarehousing || declaration.IsWarehouseAdjustment) && entryHeader.GetMessageErrorOfRequiredFieldsForBondedWarehousing().IsEmpty)
					{
						var result = entryHeader.PublishShipmentForWHSOutward(true);
						MessageInitiator.IsPublishToUniversalTransactionOK(result);
					}
				}
			}
		}

		void PublishBondedWarehouseWhenFieldsChangedAndShouldUpdate(BusinessObjectFactory factory,
			bool savedSuccessfully)
		{
			Factory.Saved -= PublishBondedWarehouseWhenFieldsChangedAndShouldUpdate;
			if (savedSuccessfully)
			{
				foreach (var entryInstruction in CustomsEntryInstructions)
				{
					if (entryInstruction.BondedWhsFieldsChangedAndShouldUpdate())
					{
						entryInstruction.EntryHeader.PublishShipmentForWHSInwardFromLastHoldOrLatestData();
					}
				}
			}
		}

		protected override bool IsAutoUpdateBondedWarehouseEnabledCore => IsExport;
	}
}
