using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(B2AdjustmentsOperationalActionMethod))]
	sealed class B2AdjustmentsOperationalActionMethodTest : Enterprise.Services.OperationalActions.Support.Testing.OperationalActionMethodTest<B2AdjustmentsOperationalActionMethod>
	{
		protected override B2AdjustmentsOperationalActionMethod NewMethod() => new B2AdjustmentsOperationalActionMethod();
	}
}
