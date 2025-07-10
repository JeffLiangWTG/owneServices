using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.OperationalActions.Testing
{
	[TestedType(typeof(CADeclarationOperationalActionMethodProvider))]
	sealed class CADeclarationOperationalActionMethodProviderTest : Enterprise.Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override Enterprise.Services.OperationalActions.Support.ActionMethodProviderID ID => Enterprise.Services.OperationalActions.Support.ActionMethodProviderIDs.CAJobDeclaration;
	}
}
