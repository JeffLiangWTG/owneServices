using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkflowTransferDiagnosisViewModel))]
	class WorkflowTransferDiagnosisViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkflowTransferDiagnosisViewModel(Factory.NewWithValidTestData<ProcessHeader>());
		}
	}
}
