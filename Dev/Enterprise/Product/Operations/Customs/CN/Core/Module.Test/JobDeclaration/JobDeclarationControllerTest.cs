using Enterprise.Customs.Module.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	sealed class JobDeclarationControllerTest : JobDeclarationControllerTestCase
	{
		protected override bool CountryHasExWarehouse => false;
	}
}
