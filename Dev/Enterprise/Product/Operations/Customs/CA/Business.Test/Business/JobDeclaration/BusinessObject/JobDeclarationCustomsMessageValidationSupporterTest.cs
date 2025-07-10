using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationCustomsMessageValidationSupporterTest : CustomsMessageValidationSupporterTest<JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get { yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging; }
		}
	}
}
