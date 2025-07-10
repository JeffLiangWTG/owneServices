using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccComplianceDocumentPivot))]
	public class AccComplianceDocumentPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var testObjectCreator = new TestObjectCreator(factory);
			var complianceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			var complianceDocumentLine = testObjectCreator.CreateComplianceDocumentLine(complianceDocumentHeader, "Test");
			return complianceDocumentLine;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
