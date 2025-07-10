using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	public abstract class DynamicTransactionCreatorDoubleLedgerTestCase : DynamicTransactionCreatorTest
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			Org3 = Factory.NewWithValidTestData<OrgHeader>();
		}

		protected OrgHeader Org1;
		protected OrgHeader Org2;
		protected OrgHeader Org3;

		protected DynamicTransactionCreatorDoubleLedger TestDoubleLedgerTransCreator;

		#endregion
	}
}