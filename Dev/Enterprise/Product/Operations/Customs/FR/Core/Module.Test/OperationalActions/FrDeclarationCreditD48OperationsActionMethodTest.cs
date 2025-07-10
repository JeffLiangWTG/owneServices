using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(FrDeclarationCreditD48OperationalActionMethod))]
	class FrDeclarationCreditD48OperationsActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<FrDeclarationCreditD48OperationalActionMethod>
	{
		protected override FrDeclarationCreditD48OperationalActionMethod NewMethod()
		{
			return new FrDeclarationCreditD48OperationalActionMethod();
		}
	}
}
