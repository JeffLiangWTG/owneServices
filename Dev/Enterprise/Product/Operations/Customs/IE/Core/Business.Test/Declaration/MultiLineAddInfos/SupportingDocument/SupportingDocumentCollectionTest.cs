using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	public class SupportingDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewType()
		{
			AssertType<SupportingDocument>(supportingDocuments.AddNew());
		}

		protected override BusinessObjectCollection GetCollectionToTest() => supportingDocuments;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			supportingDocuments = declaration.SupportingDocuments;
		}
		SupportingDocumentCollection supportingDocuments;
	}
}
