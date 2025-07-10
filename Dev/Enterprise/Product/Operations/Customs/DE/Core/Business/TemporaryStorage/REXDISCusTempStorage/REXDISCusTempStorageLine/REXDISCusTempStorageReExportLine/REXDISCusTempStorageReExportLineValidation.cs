using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageReExportLineValidation : CusTempStorageLineValidation
	{
		public REXDISCusTempStorageReExportLineValidation(REXDISCusTempStorageReExportLine parent) : base(parent)
		{
		}

		public new REXDISCusTempStorageReExportLine Parent => (REXDISCusTempStorageReExportLine)base.Parent;

		protected override void CheckTSL_CustodianIdentifierMandatory()
		{
		}

		protected override void CheckTSL_PackageQty()
		{
			base.CheckTSL_PackageQty();
			CheckPackageQtyIsBetween1And99999();
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
