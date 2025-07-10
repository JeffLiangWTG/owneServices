using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillCustomsMessageValidationSupporterTest : CustomsMessageValidationSupporterTest<CusSCAOceanBill>
	{
		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get { yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging; }
		}
	}
}
