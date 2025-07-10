using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Common.US.Testing
{
	class AESDirectCustomsEntryStatusTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestIsStatusClear()
		{
			var status = new AESDirectCustomsEntryStatus();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear), Is.EqualTo(true), "ReplacementSEDClear");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.Error), Is.EqualTo(false), "Error");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.OriginalSEDClear), Is.EqualTo(true), "OriginalSEDClear");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.Warning), Is.EqualTo(true), "Warning");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.WarningCorrectRetransmit), Is.EqualTo(true), "WarningCorrectRetransmit");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.DeleteSEDClear), Is.EqualTo(true), "DeleteSEDClear");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse), Is.EqualTo(false), "AwaitingDeleteResponse");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.Verification), Is.EqualTo(true), "Verification");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.Compliance), Is.EqualTo(true), "Compliance");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.Hold), Is.EqualTo(false), "Hold");
				NUnit.Framework.Assert.That(status.IsStatusClear(AESDirectCustomsEntryStatus.Codes.Released), Is.EqualTo(false), "Released");
			});
		}

		[ExpectNoExceptions]
		public void TestIsWaitingForResponse()
		{
			var status = new AESDirectCustomsEntryStatus();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(status.IsWaitingForResponse(AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse), Is.EqualTo(true), "AwaitingOriginalResponse");
				NUnit.Framework.Assert.That(status.IsWaitingForResponse(AESDirectCustomsEntryStatus.Codes.Error), Is.EqualTo(false), "Error");
				NUnit.Framework.Assert.That(status.IsWaitingForResponse(AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse), Is.EqualTo(true), "AwaitingReplacementResponse");
				NUnit.Framework.Assert.That(status.IsWaitingForResponse(AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear), Is.EqualTo(false), "ReplacementSEDClear");
				NUnit.Framework.Assert.That(status.IsWaitingForResponse(AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse), Is.EqualTo(true), "AwaitingDeleteResponse");
				NUnit.Framework.Assert.That(status.IsWaitingForResponse(AESDirectCustomsEntryStatus.Codes.Hold), Is.EqualTo(false), "Hold");
				NUnit.Framework.Assert.That(status.IsWaitingForResponse(AESDirectCustomsEntryStatus.Codes.Released), Is.EqualTo(false), "Released");
			});
		}

		[ExpectNoExceptions]
		public void TestMiscStatus()
		{
			var status = new AESDirectCustomsEntryStatus();

			foreach (CodeDescriptionPair code in status)
			{
				NUnit.Framework.Assert.That(status.IsPartialStatus(code.Code), Is.EqualTo(false), "IsPartialStatus");
			}

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(status.IsWithdrawnStatus(AESDirectCustomsEntryStatus.Codes.OriginalSEDClear), Is.EqualTo(false), "OriginalSEDClear");
				NUnit.Framework.Assert.That(status.IsWithdrawnStatus(AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear), Is.EqualTo(false), "ReplacementSEDClear");
				NUnit.Framework.Assert.That(status.IsWithdrawnStatus(AESDirectCustomsEntryStatus.Codes.WarningCorrectRetransmit), Is.EqualTo(false), "WarningCorrectRetransmit");
				NUnit.Framework.Assert.That(status.IsWithdrawnStatus(AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse), Is.EqualTo(false), "AwaitingDeleteResponse");
				NUnit.Framework.Assert.That(status.IsWithdrawnStatus(AESDirectCustomsEntryStatus.Codes.Verification), Is.EqualTo(false), "Verification");
				NUnit.Framework.Assert.That(status.IsWithdrawnStatus(AESDirectCustomsEntryStatus.Codes.Compliance), Is.EqualTo(false), "Compliance");
				NUnit.Framework.Assert.That(status.IsWithdrawnStatus(AESDirectCustomsEntryStatus.Codes.DeleteSEDClear), Is.EqualTo(true), "DeleteSEDClear");
				NUnit.Framework.Assert.That(status.IsWithdrawnStatus(AESDirectCustomsEntryStatus.Codes.Hold), Is.EqualTo(false), "IsWithdrawalStatus");
				NUnit.Framework.Assert.That(status.IsWithdrawnStatus(AESDirectCustomsEntryStatus.Codes.Released), Is.EqualTo(false), "IsWithdrawalStatus");
			});
		}

		[ExpectNoExceptions]
		public void TestGetFirstClearStatusCatersForReplacementOfAnotherSystemsEntry()
		{
			var aesStatus = new AESDirectCustomsEntryStatus();
			var firstClearStatusList = aesStatus.GetFirstClearStatusFor(ImportMessageStatusList.MessageType.Export);
			NUnit.Framework.Assert.That(firstClearStatusList.Count, Is.EqualTo(6), "6 Potential Cleared Status values for Export");
			NUnit.Framework.Assert.That(firstClearStatusList[1], Is.EqualTo(AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear), "Replace Clear code needs to be in this list for functionality that allows user to send Replacement entry for another systems original SED entry.");
		}
	}
}
