using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JCDServiceTaskControllerRegistryItem))]
	class JCDServiceTaskControllerRegistryItemTest : CodePairRegistryItemTest
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem() => new JCDServiceTaskControllerRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);

		//More State Transition validation related unit tests are available at 
		//...\Enterprise\Product\Operations\Accounting\Enterprise.Accounting.ServiceTasks.Testing\JobCostingReport\Registry\JCDServiceTaskControllerStateTransitionTest.cs
		//As these unit tests require references to classes that belong to Enterprise.Accounting.ServiceTasks project, I couldn't place those tests here. 
	}
}
