using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSSplitCusTempStorageLineValidation : CusTempStorageLineValidation
	{
		public CUSPCSSplitCusTempStorageLineValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public new CUSPCSSplitCusTempStorageLine Parent => (CUSPCSSplitCusTempStorageLine)base.Parent;

		protected override void CheckTSL_PackageQty()
		{
			base.CheckTSL_PackageQty();
			CheckPackageQtyIsBetween1And99999();
			CheckSinglePackageRequirements();
		}

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

		protected override void CheckTSL_RN_NKDepartureCountry()
		{
			base.CheckTSL_RN_NKDepartureCountry();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_RN_NKDepartureCountryInfo);
		}

		protected override void CheckTSL_OwnerReferenceNumber()
		{
			base.CheckTSL_OwnerReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceNumberInfo);
		}

		protected override void CheckTSL_OwnerReferenceType()
		{
			base.CheckTSL_OwnerReferenceType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceTypeInfo);
		}

		protected override void CheckTSL_PackageType()
		{
			base.CheckTSL_PackageType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_PackageTypeInfo);
		}

		protected override void CheckTSL_UnionStatus()
		{
			base.CheckTSL_UnionStatus();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_UnionStatusInfo);
		}
	}
}
