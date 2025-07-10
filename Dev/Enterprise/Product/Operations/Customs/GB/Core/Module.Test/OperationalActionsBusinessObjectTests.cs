using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.OperationalActions.Testing
{
	[TestedType(typeof(GbDeclarationOperationalActionMethod))]
	class GbDeclarationOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<GbDeclarationOperationalActionMethod>
	{
		protected override GbDeclarationOperationalActionMethod NewMethod()
		{
			return new GbDeclarationOperationalActionMethod();
		}
	}

	[TestedType(typeof(GbDeclarationOperationalActionMethodProvider))]
	class GbDeclarationOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override Services.OperationalActions.Support.ActionMethodProviderID ID
		{
			get { return Enterprise.Services.OperationalActions.Support.ActionMethodProviderIDs.GbJobDeclaration; }
		}
	}
}
