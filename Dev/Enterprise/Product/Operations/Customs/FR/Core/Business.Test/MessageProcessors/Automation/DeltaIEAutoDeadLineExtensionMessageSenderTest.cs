using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaIEAutoDeadLineExtensionMessageSenderTest : FRAutoDeadLineExtensionMessageSenderTest
	{
		protected override ZString MessageType => DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;

		protected override ZString CandidateEntriesRequiredStatus => DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;

		protected override ZString UnsuitableEntryStatus => DeltaIEImportCusEntryStatusList.Codes.Amending;

		protected override bool IsUCC6 => true;

		[TestDate(2023, 1, 15, 11, 52, 00)]
		public void TestAmendmentRequestAutomationProcessorWorkingCorrectly() => base.AssertAutomationProcessorWorkingCorrectly();
	}
}
