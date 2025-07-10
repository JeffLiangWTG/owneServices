using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AutoRateCostsRevenueActionMethod))]
	internal sealed class AutoRateCostsRevenueActionMethodTest : OperationalActionMethodTest<AutoRateCostsRevenueActionMethod>
	{
		public void TestRequiredCheckpoints()
		{
			var helper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			var method = new AutoRateCostsRevenueActionMethod(helper);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AutoRateCost),
					Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AutoRateRevenue),
				},
				method.GetRequiredSecurityCheckpoints()
			);
		}

		public void TestApplicatorIsOfTheCorrectType()
		{
			var applicator = Method.NewApplicator(Factory, null);
			AssertNotNull(applicator);
			AssertEquals(typeof(AutoRatingActionMethodApplicator), applicator.GetType());
		}

		#region Implementation

		protected override AutoRateCostsRevenueActionMethod NewMethod()
		{
			return new AutoRateCostsRevenueActionMethod(new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing));
		}

		#endregion
	}
}
