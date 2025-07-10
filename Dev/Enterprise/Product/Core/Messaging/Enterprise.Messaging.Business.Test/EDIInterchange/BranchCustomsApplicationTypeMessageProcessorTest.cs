using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class BranchCustomsApplicationTypeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			var branch2 = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch2.GB_Code = "BR2";
			branch2.GB_RL_NKHomePort = "AUSYD";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var interchange1 = CreateInterchange("MT1", GlbBranch.CurrentBranch.PK);
			var interchange2 = CreateInterchange("MT1", branch2.PK);
			var interchange3 = CreateInterchange("MT2", GlbBranch.CurrentBranch.PK);
			var interchange4 = CreateInterchange("MT1", GlbBranch.CurrentBranch.PK);
			interchange4.EI_ApplicationCode = EDIInterchange.ApplicationCodes.INConsolManifest;
			var interchange5 = CreateInterchange("MT3", GlbBranch.CurrentBranch.PK);
			var interchange6 = CreateInterchange("MT1", GlbBranch.CurrentBranch.PK);
			interchange6.EI_SystemCreateTimeUtc = ZDateTime.UtcToday.AddYears(-1);
			Factory.Save();
			ErrorReporter.Clear();

			var processor = new GMDInboundInterchangeProcessorTestHelper(new ZString[] { "MT1", "MT3" });
			processor.ExecuteBatch();
			AssertInterchange(interchange1, EDIInterchange.Status.Received);
			AssertInterchange(interchange2, EDIInterchange.Status.Queued);
			AssertInterchange(interchange3, EDIInterchange.Status.Queued);
			AssertInterchange(interchange4, EDIInterchange.Status.Queued);
			AssertInterchange(interchange5, EDIInterchange.Status.Received);
			AssertInterchange(interchange6, EDIInterchange.Status.Received);
		}

		void AssertInterchange(EDIInterchange interchange, ZString status)
		{
			interchange.Reload();
			AssertEquals(status, interchange.EI_Status);
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZGuid branchPK)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = "ABC";
			interchange.EI_To = "DEF";
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_GB = branchPK;
			return interchange;
		}
	}
}
