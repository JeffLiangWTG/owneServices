using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Test
{
	public abstract class EServicesBillingSystemTestCase : TestCaseWithFactory
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			EServicesBillingTestHelper.CreateTable();
		}

		public override void RunBare()
		{
			base.RunBare();
			EServicesBillingTestHelper.DropTable();
		}

		#endregion
	}
}
