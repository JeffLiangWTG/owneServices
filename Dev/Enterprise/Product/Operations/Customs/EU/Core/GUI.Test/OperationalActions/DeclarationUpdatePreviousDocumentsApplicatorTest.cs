using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.OperationalActions;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing.OperationalActions;

[TestedType(typeof(DeclarationUpdatePreviousDocumentsApplicator))]
public class DeclarationUpdatePreviousDocumentsApplicatorTest : OperationalActionMethodApplicatorTest
{
	public void TestGetByDataGroupingCode()
	{
		CombineAssertions("Applicator type depending on country", () =>
		{
			TestCountryApplicator("DeclarationUpdatePreviousDocumentsApplicator should be defaulted to EU type for non EU countries.", Core.Constants.CountryCodes.Afghanistan, "Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator");
			TestCountryApplicator("DeclarationUpdatePreviousDocumentsApplicator should be defaulted to EU type.", Core.Constants.CountryCodes.Latvia, "Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator");
			TestCountryApplicator("DeclarationUpdatePreviousDocumentsApplicator should be defaulted to DE type for DE.", Core.Constants.CountryCodes.Germany, "Enterprise.Customs.DE.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator");
			TestCountryApplicator("DeclarationUpdatePreviousDocumentsApplicator should be defaulted to ES type for non ES.", Core.Constants.CountryCodes.Spain, "Enterprise.Customs.ES.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator");
			TestCountryApplicator("DeclarationUpdatePreviousDocumentsApplicator should be defaulted to FR type for FR.", Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator");
			TestCountryApplicator("DeclarationUpdatePreviousDocumentsApplicator should be defaulted to GB type for GB.", Core.Constants.CountryCodes.UnitedKingdom, "Enterprise.Customs.GB.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator");
			TestCountryApplicator("DeclarationUpdatePreviousDocumentsApplicator should be defaulted to IT type for IT.", Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator");
		});
	}

	void TestCountryApplicator(ZString comment, ZString country, ZString expectedType)
	{
		var applicator = DeclarationUpdatePreviousDocumentsApplicator.GetByDataGroupingCode(Factory, country);
		AssertEquals(comment, expectedType, applicator.GetType().ToString());
	}

	public void TestDefaults()
	{
		CombineAssertions("Default values", () =>
		{
			AssertEquals("Document Code", ZString.Empty, applicator.DocumentCode);
			AssertEquals("Reference Number", ZString.Empty, applicator.ReferenceNumber);
			AssertEquals("Class", ZString.Empty, applicator.Class);
			AssertEquals("LineNo", 0, applicator.LineNo);
		});
	}

	public void TestDocumentCodeList()
	{
		var factory = new BusinessObjectFactory();
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "DC40I", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "DC40E", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "TST1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "TST2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();

		var documentCodeList = applicator.DocumentCodeList;
		AssertContainsExactElementsInAnyOrder("Document code list should be populated from refDB with previous docs of any direction.", new ZString[] { "TST1", "TST2" }, documentCodeList.GetAllCodes());
	}

	public void TestClassCodeList()
	{
		var classCodeList = applicator.ClassCodeList;
		AssertContainsExactElementsInAnyOrder("The list should be built from existing PreviousDocumentClassList.", new PreviousDocumentClassList().GetAllCodes(), classCodeList.GetAllCodes());
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		applicator = new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
	}
	DeclarationUpdatePreviousDocumentsApplicator applicator;
}
