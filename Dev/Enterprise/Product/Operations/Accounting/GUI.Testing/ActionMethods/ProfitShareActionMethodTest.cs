using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ProfitShareActionMethod))]
	internal sealed class ProfitShareActionMethodTest : OperationalActionMethodTest<ProfitShareActionMethod>
	{
		public void TestApplicatorIsOfTheCorrectType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, null);
			AssertNotNull(applicator);
			AssertEquals(typeof(ProfitShareActionMethodApplicator), applicator.GetType());
		}

		protected override ProfitShareActionMethod NewMethod()
		{
			return new ProfitShareActionMethod(typeof(OperationalActionSupporter));
		}
	}
}
