using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WorkflowItemWrapperCollection))]
	sealed class WorkflowItemWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<WorkflowItemWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return WorkflowItemWrapper.New(Factory.New<ProcessTask>(), Factory);
		}

		protected override WorkflowItemWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new WorkflowItemWrapperCollection(null, Factory);
		}
	}
}
