using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CreditControlledDocumentsApprovalDocument))]
	public class CreditControlledDocumentsApprovalDocumentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreditControlledDocumentsApprovalDocument("123");
		}

		public void TestConstruct()
		{
			var doc = (CreditControlledDocumentsApprovalDocument)GetNewBusinessObject();
			AssertEquals("123", doc.DocumentName);
		}
	}
}
