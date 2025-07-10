using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBCustomsMessageValidationSupporterTest : CustomsMessageValidationSupporterTest<CusMAWB>
	{
		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
				yield return WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn;
			}
		}
	}
}
