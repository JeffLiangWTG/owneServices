using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccComplianceDocumentProcessTaskCollection))]
	public class AccComplianceDocumentProcessTaskCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			header.ADH_Ledger = LedgerTypes.AccountsReceivable;
			return (AccComplianceDocumentProcessTaskCollection)header.WorkflowItems;
		}
	}
}
