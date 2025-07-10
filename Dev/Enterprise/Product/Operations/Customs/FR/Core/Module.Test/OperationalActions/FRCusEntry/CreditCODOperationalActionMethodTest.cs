using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(CreditCODOperationalActionMethod))]
	public class CreditCODOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<CreditCODOperationalActionMethod>
	{
		protected override CreditCODOperationalActionMethod NewMethod()
		{
			return new CreditCODOperationalActionMethod();
		}
	}
}
