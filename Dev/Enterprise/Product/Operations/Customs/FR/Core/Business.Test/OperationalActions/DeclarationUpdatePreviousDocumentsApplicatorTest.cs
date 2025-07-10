using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(DeclarationUpdatePreviousDocumentsApplicator))]
	class DeclarationUpdatePreviousDocumentsApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestDocumentCodeList()
		{
			AssertContainsExactElementsInAnyOrder(new PreviousDocumentCodeList(), new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).DocumentCodeList);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}
	}
}
