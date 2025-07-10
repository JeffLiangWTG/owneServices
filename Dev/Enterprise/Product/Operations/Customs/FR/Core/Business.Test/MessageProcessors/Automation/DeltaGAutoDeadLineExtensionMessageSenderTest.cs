using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaGAutoDeadLineExtensionMessageSenderTest : FRAutoDeadLineExtensionMessageSenderTest
	{
		protected override ZString MessageType => DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G1 ? EntryActionCodeList.Codes.MAP : EntryActionCodeList.Codes.MDA;

		protected override ZString CandidateEntriesRequiredStatus => EntryStatusDescriptionCodeList.Codes.ES050;

		protected override ZString UnsuitableEntryStatus => EntryStatusDescriptionCodeList.Codes.ES100;

		protected override bool IsUCC6 => false;

		[TestDate(2023, 1, 15, 11, 52, 00)]
		public void TestMAPDeltaAutomationProcessorWorkingCorrectly()
		{
			DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			DeltaActionCode = 3;
			base.AssertAutomationProcessorWorkingCorrectly();
		}

		[TestDate(2023, 1, 15, 11, 52, 00)]
		public void TestMDADeltaAutomationProcessorWorkingCorrectly()
		{
			DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			DeltaActionCode = 6;
			base.AssertAutomationProcessorWorkingCorrectly();
		}
	}
}
