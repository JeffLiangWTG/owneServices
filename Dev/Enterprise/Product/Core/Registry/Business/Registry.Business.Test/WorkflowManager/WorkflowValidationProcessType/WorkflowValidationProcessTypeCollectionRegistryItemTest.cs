using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowValidationProcessTypeCollectionRegistryItem))]
	sealed class WorkflowValidationProcessTypeCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<WorkflowValidationProcessTypeCollection>
	{
		protected override StronglyTypedRegistryItem<WorkflowValidationProcessTypeCollection, WorkflowValidationProcessTypeCollection> GetNewRegistryItem()
		{
			return new WorkflowValidationProcessTypeCollectionRegistryItem("1", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, WorkflowValidationProcessTypeCollection.DefaultValue);
		}
	}
}
