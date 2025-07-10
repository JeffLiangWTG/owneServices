using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONConsolidatedCusTempStorageLineValidation : PRLCONCusTempStorageLineValidation
	{
		public PRLCONConsolidatedCusTempStorageLineValidation(PRLCONConsolidatedCusTempStorageLine parent) : base(parent)
		{
		}

		public new PRLCONConsolidatedCusTempStorageLine Parent => (PRLCONConsolidatedCusTempStorageLine)base.Parent;

		protected override void CheckTSL_GoodsDescription()
		{
			base.CheckTSL_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_GoodsDescriptionInfo);
		}

		protected override void CheckTSL_GrossWeight()
		{
			base.CheckTSL_GrossWeight();
			MandatoryValidation.MessageErrorIfIsZero(Parent.TSL_GrossWeightInfo);
		}

		protected override void CheckTSL_PackageType()
		{
			base.CheckTSL_PackageType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_PackageTypeInfo);
		}

		protected override void CheckTSL_PackageQty()
		{
			base.CheckTSL_PackageQty();
			CheckSinglePackageRequirements();

			var packagesToConsolidateTotal = Parent.TotalPackagesFromLinesToConsolidate;
			if (Parent.TSL_PackageQty > packagesToConsolidateTotal)
			{
				Parent.TSL_PackageQtyInfo.AddMessageError(Res.GetString("260429C9-F8F5-47C2-865E-7AC09B76C881", "Package Count must not be greater than the total number of packages to consolidate {0}", packagesToConsolidateTotal));
			}
		}

		protected override void CheckTSL_RN_NKDepartureCountry()
		{
			base.CheckTSL_RN_NKDepartureCountry();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_RN_NKDepartureCountryInfo);
		}

		protected override void CheckTSL_UnionStatus()
		{
			base.CheckTSL_UnionStatus();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TSL_UnionStatusInfo);
		}

		protected override void CheckTSL_OwnerReferenceType()
		{
			base.CheckTSL_OwnerReferenceType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceTypeInfo);
		}

		protected override void CheckTSL_OwnerReferenceNumber()
		{
			base.CheckTSL_OwnerReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceNumberInfo);
		}
	}
}
