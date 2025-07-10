using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBCustomsMessageValidationSupporterTest : CustomsMessageValidationSupporterTest<CusHAWB>
	{
		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get { yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging; }
		}

		protected override CusHAWB GetValidateForCustomsMessagingSupporter()
		{
			var mawb = Factory.New<CusMAWB>();
			return mawb.ChildBills.AddNew();
		}
	}
}
