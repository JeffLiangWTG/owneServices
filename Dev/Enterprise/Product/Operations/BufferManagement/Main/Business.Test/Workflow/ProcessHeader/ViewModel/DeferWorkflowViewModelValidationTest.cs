using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	class DeferWorkflowViewModelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDeferralReason_RegistryValuesPopulated()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Removal of fur");
			var componentPKs = new List<ZGuid>();
			var viewModel = new DeferWorkflowViewModel(workflow, componentPKs);

			viewModel.DeferralReason = "";
			AssertHasError(viewModel.DeferralReasonInfo, "Please enter a Reason.");

			viewModel.DeferralReason = "PRV";
			AssertHasError(viewModel.DeferralReasonInfo, "Enter a valid Reason.");

			viewModel.DeferralReason = WorkflowDeferralReasonsList.Codes.PrioritiesChanged;
			AssertNoErrors(viewModel.DeferralReasonInfo);
		}

		public void TestValidateDeferralReason_RegistryValuesEmpty()
		{
			BMSRegistry.Instance.DeferralReasons.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZArchitecture.Core.ReadOnlyCodeDescriptionPairList());

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Removal of fur");
			var componentPKs = new List<ZGuid>();
			var viewModel = new DeferWorkflowViewModel(workflow, componentPKs);

			viewModel.DeferralReason = "";
			AssertNoErrors(viewModel.DeferralReasonInfo);

			viewModel.DeferralReason = "PRV";
			AssertNoErrors(viewModel.DeferralReasonInfo);

			viewModel.DeferralReason = WorkflowDeferralReasonsList.Codes.PrioritiesChanged;
			AssertNoErrors(viewModel.DeferralReasonInfo);
		}
	}
}
