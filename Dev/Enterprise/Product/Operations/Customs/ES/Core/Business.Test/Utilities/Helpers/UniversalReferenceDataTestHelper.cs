using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class UniversalReferenceDataTestHelper
	{
		public static void SetBulkTypeHelper(this BusinessObjectFactory factory, string code = "VG")
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UnitedNationsPackageTypes");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
									  Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
									  code,
									  "Bulk",
									  ZDateTime.MinSmallDateTimeValue,
									  ZDateTime.MaxSmallDateTimeValue,
									  Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk,
									  "");
			factory.Save();
		}

		public static void AddDocumentsToRefDataForTest(this BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var esCode = CountryCodes.Spain;
			var supportingDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			helper.CreateNewOrGetExistingCusCodeType(supportingDocumentType, "Supporting Documents for Import");
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "N851", "CERTIFICADO FITOSANITARIO", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "C085", "DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "N853", "DOC.SANIT.COMUN ENTRADA PRODUCTOS(B)", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "C657", "CERTIFICADO DE SANIDAD", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "C678", "DOC.SANIT.COMUN PIENSOS+ALIMENTOS", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "1405", "INSPECCION SANIDAD EXTERIOR. NO PROCEDE", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "1413", "INSPECCION SANIDAD EXTERIOR-NO AFECTADOS", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			helper.CreateNewOrGetExistingDataGrouping(esCode, "Spain", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			factory.Save();
		}
	}
}
