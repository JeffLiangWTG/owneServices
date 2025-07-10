using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AutoRateRevenueActionMethod))]
	internal sealed class AutoRateRevenueActionMethodTest : OperationalActionMethodTest<AutoRateRevenueActionMethod>
	{
		public void TestRequiredCheckpoints()
		{
			var helper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);

			var method = new AutoRateRevenueActionMethod(helper);

			AssertContainsExactElementsInAnyOrder(
				new[] { Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AutoRateRevenue) },
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

		protected override AutoRateRevenueActionMethod NewMethod()
		{
			return new AutoRateRevenueActionMethod(new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing));
		}

		#endregion
	}
}
