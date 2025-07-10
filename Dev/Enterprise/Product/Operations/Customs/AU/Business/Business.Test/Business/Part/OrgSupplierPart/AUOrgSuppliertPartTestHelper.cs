using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public static class AUOrgSuppliertPartTestHelper
	{
		#region  TestHelpers
		public static CusClassPartPivot AddNewImportPivotWithClassification(this AUOrgSupplierPart part, ZGuid classificationPK)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classificationPK;
			return pivot;
		}

		public static CusClassPartPivot AddNewExportPivotWithClassification(this AUOrgSupplierPart part, ZGuid classificationPK)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_CC = classificationPK;
			return pivot;
		}
		#endregion
	}
}
