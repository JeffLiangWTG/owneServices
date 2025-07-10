using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowValidationProcessTypeCollection))]
	sealed class WorkflowValidationProcessTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<WorkflowValidationProcessTypeCollection>
	{
		#region Implementation

		protected override WorkflowValidationProcessTypeCollection GetCollectionToTest()
		{
			return new WorkflowValidationProcessTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WorkflowValidationProcessType();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
