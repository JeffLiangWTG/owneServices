using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowValidationProcessType))]
	sealed class WorkflowValidationProcessTypeTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateProcessType()
		{
			var collection = new WorkflowValidationProcessTypeCollection();
			var processType = collection.AddNew();
			processType.ProcessType = "XXX";
			AssertHasErrorContaining(processType.ProcessTypeInfo, ListValidation.InvalidCodeError);

			processType.ProcessType = "ACA";
			AssertNoErrorContaining(processType.ProcessTypeInfo, ListValidation.InvalidCodeError);

			var processType2 = collection.AddNew();
			processType2.ProcessType = "ACA";
			AssertHasError(processType2.ProcessTypeInfo, WorkflowValidationProcessType.DuplicatedCodesError);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new WorkflowValidationProcessType();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
