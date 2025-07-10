using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.OperationalActions;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing.OperationalActions;

[TestedType(typeof(DeclarationUpdatePreviousDocumentsApplicator))]
class DeclarationUpdatePreviousDocumentsApplicatorTest : OperationalActionMethodApplicatorTest
{
	public void TestDocumentCodeList()
	{
		var factory = new BusinessObjectFactory();
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "DC40I", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "DC40E", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.DC40A, "DC40A", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "TST1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "TST2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListTypes.DC40A, "TST3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();

		var applicator = new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		var documentCodeList = applicator.DocumentCodeList;

		AssertContainsExactElementsInAnyOrder("Document code list should be populated from refDB with previous docs of any direction + DC40A docs.", new ZString[] { "TST1", "TST2", "TST3" }, documentCodeList.GetAllCodes());
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
	}
}
