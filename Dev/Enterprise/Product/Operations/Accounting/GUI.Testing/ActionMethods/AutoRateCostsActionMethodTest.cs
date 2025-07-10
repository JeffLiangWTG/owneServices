using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AutoRateCostsActionMethod))]
	internal sealed class AutoRateCostsActionMethodTest : OperationalActionMethodTest<AutoRateCostsActionMethod>
	{
		public void TestRequiredCheckpoints()
		{
			var helper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);

			var method = new AutoRateCostsActionMethod(helper);

			AssertContainsExactElementsInAnyOrder(
				new[] { Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AutoRateCost) },
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

		protected override AutoRateCostsActionMethod NewMethod()
		{
			return new AutoRateCostsActionMethod(new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing));
		}

		#endregion
	}
}
