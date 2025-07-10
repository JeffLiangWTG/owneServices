using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccComplianceDocumentTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var apHeader = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
			apHeader.ADH_Ledger = LedgerTypes.AccountsPayable;
			Factory.Save();

			var arHeader = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			arHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			AssertType(typeof(APComplianceDocumentHeader), otherFactory.Load<AccComplianceDocumentHeader>(apHeader.PK));
			AssertType(typeof(ARComplianceDocumentHeader), otherFactory.Load<AccComplianceDocumentHeader>(arHeader.PK));
		}
	}
}
