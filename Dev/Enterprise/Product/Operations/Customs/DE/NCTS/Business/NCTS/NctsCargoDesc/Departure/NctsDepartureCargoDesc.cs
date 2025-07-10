using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsDepartureCargoDesc : EU.NCTS.Business.NctsDepartureCargoDesc
		, Integration.Customs.DE.IDepartureCargoDesc
		, INctsPreviousProcedureParentProvider
	{
		public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[MaxLength(nameof(BY_Description_MaxLength))]
		public override ZString BY_Description
		{
			get => base.BY_Description;
			set => base.BY_Description = value;
		}

		public new int BY_Description_MaxLength => IsInPhase5TransitionPeriod ? 280 : 512;

		public new NctsHeader Header => (NctsHeader)base.Header;

		public new NctsPackageCollection Packages => (NctsPackageCollection)base.Packages;

		public NctsPreviousProcedureMaster PreviousProcedureMaster => previousProcedureMaster ??= GetNewPreviousProcedureMaster();

		NctsPreviousProcedureMaster GetNewPreviousProcedureMaster()
		{
			var master = new NctsPreviousProcedureMaster(Factory, this);
			RegisterEditableChildObject(master);
			return master;
		}

		NctsPreviousProcedureMaster previousProcedureMaster;

		[ChildEditable]
		public INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousProcedures
		{
			get
			{
				if (previousProcedures == null)
				{
					previousProcedures = new NctsPreviousDocumentCollection<NctsPreviousDocument>(
						parent: this,
						additionalFilter: () => new ZQuery(CusSupportingInfoSchema.CSI_Procedure, SQLComparisonOperator.NotEqual, string.Empty),
						setDefaultsForNewChild: newChild => newChild.CSI_Procedure = PreviousProcedureMaster.CSI_Procedure
					);
					previousProcedures.Load();
					RegisterEditableChildObject(previousProcedures);
				}

				return previousProcedures;
			}
		}
		INctsPreviousDocumentCollection<NctsPreviousDocument> previousProcedures;

		public new INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => (INctsPreviousDocumentCollection<NctsPreviousDocument>)base.PreviousDocuments;

		protected override INctsPreviousDocumentCollection<EU.NCTS.Business.NctsPreviousDocument> GetPreviousDocuments()
		{
			return new NctsPreviousDocumentCollection<NctsPreviousDocument>(
				parent: this,
				additionalFilter: () => new ZQuery(CusSupportingInfoSchema.CSI_Procedure, SQLComparisonOperator.Equal, string.Empty)
			);
		}

		public string EffectiveCountryOfDispatch => Factory.GetValue(ref effectiveCountryOfDispatch, () =>
			BY_RN_NKCountryOfDispatch.ValueOrNullIfEmpty() ??
			Bill?.B0_RN_NKCountryOfExport.ValueOrNullIfEmpty() ??
			Header.MovementHeader.BM_RN_NKCountryOfDispatch.ValueOrNullIfEmpty());
		CachedProperty<string> effectiveCountryOfDispatch;

		public string EffectiveCountryOfDestination => Factory.GetValue(ref effectiveCountryOfDestination, () =>
			BY_RN_NKCountryOfDestination.ValueOrNullIfEmpty() ??
			Bill?.B0_RN_NKCountryOfDestination.ValueOrNullIfEmpty() ??
			Header.MovementHeader.BM_RL_NKDestinationPort.ValueOrNullIfEmpty());
		CachedProperty<string> effectiveCountryOfDestination;

		public ZString DeclarationTypeEffective
		{
			get
			{
				var declarationType = BY_Type;
				return !declarationType.IsEmpty || Header == null ? declarationType : Header.MovementHeader?.BM_InBondEntryType ?? ZString.Empty;
			}
		}

		[DecimalPrecision(11)]
		[DecimalPlaces(3)]
		public override ZDecimal BY_GrossWeight
		{
			get => base.BY_GrossWeight;
			set => base.BY_GrossWeight = value;
		}

		[DecimalPrecision(11)]
		[DecimalPlaces(3)]
		public override ZDecimal BY_NetWeight
		{
			get => base.BY_NetWeight;
			set => base.BY_NetWeight = value;
		}

		public string EffectiveMethodOfPayment => Factory.GetValue(ref effectiveMethodOfPayment, () =>
			BY_TransportChargesMethodOfPayment.ValueOrNullIfEmpty() ??
			Bill?.B0_TransportPaymentMethod.ValueOrNullIfEmpty() ??
			Header.MovementHeader.BM_MethodOfPayment.ValueOrNullIfEmpty());
		CachedProperty<string> effectiveMethodOfPayment;

		public string EffectiveReferenceNumberUCR => Factory.GetValue(ref effectiveReferenceNumberUCR, () =>
			BY_CommercialReferenceNumber.ValueOrNullIfEmpty() ??
			Bill?.B0_ReferenceID.ValueOrNullIfEmpty() ??
			Header.MovementHeader.BM_UniqueConsignmentReference.ValueOrNullIfEmpty());
		CachedProperty<string> effectiveReferenceNumberUCR;

		public JobDocAddress EffectiveConsignee => Factory.GetValue(ref effectiveConsignee, () =>
			Consignee.GetDocAddressOrNullIfInvalid() ??
			Bill?.Consignee.GetDocAddressOrNullIfInvalid() ??
			Header.Consignee.GetDocAddressOrNullIfInvalid());
		CachedProperty<JobDocAddress> effectiveConsignee;

		protected override NctsDepartureCargoDescPhase5Validation GetNewPhase5Validation() => new NctsDepartureCargoDescValidation(this);

		protected override INctsPackageCollection<EU.NCTS.Business.NctsPackage, NctsCommonCargoDesc> GetNctsPackageCollection() => new NctsPackageCollection(this);

		public new INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
		protected override INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		public new INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;
		protected override INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

		protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
		protected override Type PreviousDocumentType => typeof(NctsPreviousDocument);
		protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (Header != null && !BY_WarehouseEntryNumber.IsEmpty
				&& (HasChanges || Header.Consignor.HasChanges || Header.Principal.HasChanges))
			{
				CreatePreviousProcedure();
			}
		}

		protected override ZDecimal VATRateForEmptyTaxType => ZDecimal.Zero;

		void CreatePreviousProcedure()
		{
			var authorizationNumberToReuse = PreviousProcedureMaster.CSI_Procedure == NctsPreviousProcedureList.Codes._9DEZ ? PreviousProcedureMaster.AuthorizationNumber : ZString.Empty;
			var tariffToReuse = previousProcedures.FirstOrDefault()?.CSI_Tariff ?? ZString.Empty;

			PreviousProcedures.RemoveAndDeleteAll();
			PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			PreviousProcedureMaster.AuthorizationNumber = GetAuthorizationNumber(authorizationNumberToReuse);
			var previousProcedure = PreviousProcedures[0];
			previousProcedure.CSI_ReferenceNumber = BY_WarehouseEntryNumber;
			previousProcedure.CSI_ReferenceNumber2 = BY_BondedWHSOrderNumber;
			previousProcedure.CSI_LineNo = BY_WarehouseEntryLineNo;
			previousProcedure.CSI_Quantity2 = BY_BondedWhsQuantity;
			previousProcedure.CSI_UnitOfQuantity2 = BY_BondedWhsUnitQty;
			previousProcedure.CSI_Tariff = GetTariff();
			previousProcedure.Status = previousProcedure.CSI_ReferenceNumber.IsValidAtlasReferenceForBondedWarehouse();

			ZString GetAuthorizationNumber(ZString authorizationNumber)
			{
				if (authorizationNumber.IsEmpty)
				{
					var validAuthorizationNumbers = PreviousProcedureMaster.Lookups.AuthorizationNumberList;
					if (validAuthorizationNumbers.Count == 1)
					{
						authorizationNumber = validAuthorizationNumbers.CodesAsString;
					}
				}
				return authorizationNumber;
			}

			ZString GetTariff()
			{
				var tariffFromProduct = GetTariffFromProduct();
				return tariffFromProduct.IsEmpty ? tariffToReuse : tariffFromProduct;
			}

			ZString GetTariffFromProduct()
			{
				var result = ZString.Empty;
				var productPK = BY_OP_Part;
				if (productPK.IsValid)
				{
					var product = Factory.Load<DE.Business.OrgSupplierPart>(productPK);
					if (product != null)
					{
						result = product.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Germany).SingleOrDefault(x => x.CI_ChildType == Common.Shared.ClassificationTypeList.Codes.Import)?.CI_FormattedTariffNum ?? ZString.Empty;
					}
				}
				return result;
			}
		}

		public override void Delete()
		{
			var bill = Bill;
			base.Delete();
			bill?.Header?.ApportionedAmountToGuaranteesLiabilityAmount();
		}

		NctsHeader INctsPreviousProcedureParentProvider.NctsHeader => Header;
	}
}
