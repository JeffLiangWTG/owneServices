using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ConsolEqualizeAndAutorateActionMethod))]
	internal sealed class ConsolEqualizeAndAutorateActionMethodTest : OperationalActionMethodTest<ConsolEqualizeAndAutorateActionMethod>
	{
		public void TestRequiredCheckpoints()
		{
			var helper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);

			var method = new ConsolEqualizeAndAutorateActionMethod(helper);

			AssertContainsExactElementsInAnyOrder(
				new SecurityCheckpoint[]
				{
					Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AutoRateCost),
				},
				method.GetRequiredSecurityCheckpoints()
			);
		}

		public void TestApplicatorIsOfTheCorrectType()
		{
			var applicator = Method.NewApplicator(Factory, null);
			AssertNotNull(applicator);
			AssertEquals(typeof(ConsolEqualizeAndAutorateActionMethodApplicator), applicator.GetType());
		}

		public void TestMethodNameAndDescription()
		{
			AssertEquals("Should be Auto-Cost", "Auto-Cost With Volume Discount", Method.Name);
			AssertEquals("Should be Auto-Cost", "Auto-Cost selected Consols with Volume Discount.", Method.Description);
		}

		#region Implementation

		protected override ConsolEqualizeAndAutorateActionMethod NewMethod()
		{
			return new ConsolEqualizeAndAutorateActionMethod(new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing));
		}

		#endregion
	}
}
