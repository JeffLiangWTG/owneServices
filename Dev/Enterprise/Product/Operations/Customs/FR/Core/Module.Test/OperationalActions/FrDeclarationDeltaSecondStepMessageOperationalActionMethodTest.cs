using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(FrDeclarationDeltaSecondStepMessageOperationalActionMethod))]
	class FrDeclarationDeltaSecondStepMessageOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<FrDeclarationDeltaSecondStepMessageOperationalActionMethod>
	{
		protected override FrDeclarationDeltaSecondStepMessageOperationalActionMethod NewMethod()
		{
			return new FrDeclarationDeltaSecondStepMessageOperationalActionMethod();
		}
	}
}
