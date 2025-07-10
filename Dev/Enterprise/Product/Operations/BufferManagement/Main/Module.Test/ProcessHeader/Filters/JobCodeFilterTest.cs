using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Testing
{
	[TestedType(typeof(JobCodeFilter))]
	class JobCodeFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobCodeFilter(() => ObjectFactory.Get<IWorkflowDescriptorList>());
		}
	}
}
