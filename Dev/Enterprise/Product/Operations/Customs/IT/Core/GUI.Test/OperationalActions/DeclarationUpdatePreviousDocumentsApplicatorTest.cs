using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.OperationalActions;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing.OperationalActions;

[TestedType(typeof(DeclarationUpdatePreviousDocumentsApplicator))]
sealed class DeclarationUpdatePreviousDocumentsApplicatorTest : OperationalActionMethodApplicatorTest
{
	public void TestDocumentCodeList()
	{
		AssertContainsExactElementsInExactOrder(new PreviousDocumentCodeList(), new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).DocumentCodeList);
	}

	public void TestClassCodeList()
	{
		AssertContainsExactElementsInExactOrder(new PreviousDocumentClassList(), new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).ClassCodeList);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
	}
}
