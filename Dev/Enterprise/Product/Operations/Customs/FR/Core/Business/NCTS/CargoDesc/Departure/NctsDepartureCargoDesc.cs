using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsDepartureCargoDesc : EU.NCTS.Business.NctsDepartureCargoDesc
		, Integration.Customs.FR.IDepartureCargoDesc
		, ICusInBondCargoDescTypeProvider
	{
		public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public override ZGuid BY_ParentID
		{
			get => base.BY_ParentID;
			set
			{
				var oldValue = BY_ParentID;
				base.BY_ParentID = value;
				if (!IsCopying && oldValue != BY_ParentID)
				{
					Fees.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString BY_ParentTableCode
		{
			get => base.BY_ParentTableCode;
			set
			{
				var oldValue = BY_ParentTableCode;
				base.BY_ParentTableCode = value;
				if (!IsCopying && oldValue != BY_ParentTableCode)
				{
					Fees.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString BY_Description
		{
			get => base.BY_Description;
			set
			{
				base.BY_Description = value.SubstringSafe(0, BY_Description_MaxLength);
			}
		}

		public new NctsDepartureMovementHeader MoveHeader => (NctsDepartureMovementHeader)base.MoveHeader;

		protected override EU.NCTS.Business.NctsDepartureCargoDescPhase4Validation GetNewPhase4Validation() => new NctsDepartureCargoDescPhase4Validation(this);

		protected override EU.NCTS.Business.NctsDepartureCargoDescPhase5Validation GetNewPhase5Validation() => new NctsDepartureCargoDescPhase5Validation(this);

		[ChildEditable(true)]
		public new EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => (EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument>)base.PreviousDocuments;
		protected override EU.NCTS.Business.INctsPreviousDocumentCollection<EU.NCTS.Business.NctsPreviousDocument> GetPreviousDocuments() => new EU.NCTS.Business.NctsPreviousDocumentCollection<NctsPreviousDocument>(this);

		[ChildEditable(true)]
		public new EU.NCTS.Business.INctsPackageCollection<NctsPackage, NctsDepartureCargoDesc> Packages => (EU.NCTS.Business.NctsPackageCollection<NctsPackage, NctsDepartureCargoDesc>)base.Packages;
		protected override EU.NCTS.Business.INctsPackageCollection<EU.NCTS.Business.NctsPackage, EU.NCTS.Business.NctsCommonCargoDesc> GetNctsPackageCollection() => new EU.NCTS.Business.NctsPackageCollection<NctsPackage, NctsDepartureCargoDesc>(this);

		[ChildEditable(true)]
		public new CusInBondFeeCollection<NctsCargoDescFee> Fees => (CusInBondFeeCollection<NctsCargoDescFee>)base.Fees;

		protected override ICusInBondFeeCollection<EU.NCTS.Business.NctsCargoDescFee> GetNctsCargoDescFeeCollection() => new CusInBondFeeCollection<NctsCargoDescFee>(this);

		public Type CusInBondCargoDescType => typeof(NctsDepartureCargoDesc);

		public ZDecimal HarbourTaxAmountInLocalCurrency
		{
			get
			{
				var harbourFeeCodes = new HarbourFeeCodes();
				return Fees.Where(fee => harbourFeeCodes.ContainsCode(fee.BFE_ChargeType)).Sum(x => x.BFE_ChargeAmount);
			}
		}

		protected override Type PreviousDocumentType => typeof(NctsPreviousDocument);

		protected override ITemporaryStorageRegisterTransactionDataProvider GetNewTemporaryStorageRegisterTransactionDataProvider()
			=> IsPhase5 ? new NctsDepartureCargoDescPhase5TemporaryStorageRegisterTransactionDataProvider(this) : new NctsDepartureCargoDescPhase4TemporaryStorageRegisterTransactionDataProvider(this);

		protected override IEntryNumberFormatterForNctsAndDeclarationIntegration GetEntryNumberFormatterCore() => new EntryNumberFormatterForNctsAndDeclarationIntegration(Factory);

		protected override void ValidateConsignor(JobDocAddressValidation validation)
		{
			base.ValidateConsignor(validation);

			if (Header.IsDepartureMovement)
			{
				var isConsignorFilled = !Consignor.OrganisationPK.IsEmpty;
				var isHeaderConsignorFilled = !Header.Consignor.OrganisationPK.IsEmpty;

				if (isConsignorFilled && isHeaderConsignorFilled)
				{
					Consignor.OrganisationPKInfo.AddWarning(Res.GetString("a487d9a1-a1d0-46e4-bedb-f45a2782b7e2", "You can't fill in the Consignor in both Departure Declaration and Goods Items tab. Please note that only the Consignor in Departure Declaration tab will be sent to customs."));
				}
				else if (!isConsignorFilled && !isHeaderConsignorFilled)
				{
					Consignor.OrganisationPKInfo.AddWarning(Res.GetString("6961b660-71f5-4bab-af28-4c9e688a01ac", "Please specify the Consignor, either in Departure Declaration or Goods Items tab."));
				}
			}
		}

		protected override void ValidateConsignee(JobDocAddressValidation validation)
		{
			base.ValidateConsignee(validation);

			if (Header.IsDepartureMovement)
			{
				var isConsigneeFilled = !Consignee.OrganisationPK.IsEmpty;
				var isHeaderConsigneeFilled = !Header.Consignee.OrganisationPK.IsEmpty;

				if (isConsigneeFilled && isHeaderConsigneeFilled)
				{
					Consignee.OrganisationPKInfo.AddWarning(Res.GetString("9d83e255-4ea0-45b6-8a45-c363f7aea206", "You can't fill in the Consignee in both Departure Declaration and Goods Items tab. Please note that only the Consignee in Departure Declaration tab will be sent to customs."));
				}
				else if (!isConsigneeFilled && !isHeaderConsigneeFilled)
				{
					Consignee.OrganisationPKInfo.AddWarning(Res.GetString("a6c55d9a-b0e1-468a-a56a-fe6b90194251", "Please specify the Consignee, either in Departure Declaration or Goods Items tab."));
				}
			}
		}

		public new NonPersistentDepartureContainerPivotCollection ContainersPivots => (NonPersistentDepartureContainerPivotCollection)base.ContainersPivots;

		protected override EU.NCTS.Business.NonPersistentDepartureContainerPivotCollection GetContainersPivots() => new NonPersistentDepartureContainerPivotCollection(this);

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = new NctsDepartureCargoDescValueSetStrategy(this));
		IValueSetStrategy valueSetStrategy;
	}
}
