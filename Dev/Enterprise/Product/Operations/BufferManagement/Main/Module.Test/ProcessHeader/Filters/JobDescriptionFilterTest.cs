using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Testing
{
	[TestedType(typeof(JobDescriptionFilter))]
	class JobDescriptionFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobDescriptionFilter(() => ObjectFactory.Get<IWorkflowDescriptorList>());
		}
	}
}
