using NUnit.Framework;
namespace Enterprise.Customs.Common.US.Testing
{
	class CusEntryHeaderStatusTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetFirstClearStatusFor()
		{
			var list = new ImportMessageStatusList();
			var messageType = ImportMessageStatusList.MessageType.EntrySummary;
			var firstClearStatus = list.GetFirstClearStatusFor(messageType);
			NUnit.Framework.Assert.That(firstClearStatus.Count, Is.EqualTo(6), "Length of First Clear Status.");
			NUnit.Framework.Assert.That(firstClearStatus[0], Is.EqualTo(ImportMessageStatusList.Codes.ClearEntrySummaryOriginal));
			NUnit.Framework.Assert.That(firstClearStatus[1], Is.EqualTo(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings));
			NUnit.Framework.Assert.That(firstClearStatus[2], Is.EqualTo(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings));
			NUnit.Framework.Assert.That(firstClearStatus[3], Is.EqualTo(ImportMessageStatusList.Codes.ClearEntrySummaryReplace));
			NUnit.Framework.Assert.That(firstClearStatus[4], Is.EqualTo(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings));
			NUnit.Framework.Assert.That(firstClearStatus[5], Is.EqualTo(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings));
		}

		[ExpectNoExceptions]
		public void TestGetErrorStatusForEntrySummary()
		{
			var errorStatusForEntrySummary = ImportMessageStatusList.GetErrorStatusForEntrySummary();
			NUnit.Framework.Assert.That(errorStatusForEntrySummary.Length, Is.EqualTo(5), "Length of Error Status for Entry Summary.");
			NUnit.Framework.Assert.That(errorStatusForEntrySummary[0], Is.EqualTo(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings));
			NUnit.Framework.Assert.That(errorStatusForEntrySummary[1], Is.EqualTo(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings));
			NUnit.Framework.Assert.That(errorStatusForEntrySummary[2], Is.EqualTo(ImportMessageStatusList.Codes.ErrorEntrySummaryDelete));
			NUnit.Framework.Assert.That(errorStatusForEntrySummary[3], Is.EqualTo(ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal));
			NUnit.Framework.Assert.That(errorStatusForEntrySummary[4], Is.EqualTo(ImportMessageStatusList.Codes.ErrorEntrySummaryReplace));

			var	rejectStatusInterested = new ImportMessageStatusList().RejectStatusInterested;
			NUnit.Framework.Assert.That(rejectStatusInterested.Count, Is.EqualTo(10), "Length of Reject Status Interested.");
			NUnit.Framework.Assert.That(rejectStatusInterested[0], Is.EqualTo(ImportMessageStatusList.Codes.ErrorEntrySummaryDelete));
			NUnit.Framework.Assert.That(rejectStatusInterested[1], Is.EqualTo(ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal));
			NUnit.Framework.Assert.That(rejectStatusInterested[2], Is.EqualTo(ImportMessageStatusList.Codes.ErrorEntrySummaryReplace));
			NUnit.Framework.Assert.That(rejectStatusInterested[3], Is.EqualTo(ImportMessageStatusList.Codes.ErrorElectronicInvoiceDelete));
			NUnit.Framework.Assert.That(rejectStatusInterested[4], Is.EqualTo(ImportMessageStatusList.Codes.ErrorElectronicInvoiceOriginal));
			NUnit.Framework.Assert.That(rejectStatusInterested[5], Is.EqualTo(ImportMessageStatusList.Codes.ErrorElectronicInvoiceReplace));
			NUnit.Framework.Assert.That(rejectStatusInterested[6], Is.EqualTo(ImportMessageStatusList.Codes.ErrorACECargoReleaseDelete));
			NUnit.Framework.Assert.That(rejectStatusInterested[7], Is.EqualTo(ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd));
			NUnit.Framework.Assert.That(rejectStatusInterested[8], Is.EqualTo(ImportMessageStatusList.Codes.ErrorACECargoReleaseReplace));
			NUnit.Framework.Assert.That(rejectStatusInterested[9], Is.EqualTo(ImportMessageStatusList.Codes.ErrorACECargoReleaseUpdate));

			var acceptedStatusToCancelRejectStatusInterested = new ImportMessageStatusList().AcceptedStatusToCancelRejectStatusInterested;
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested.Count, Is.EqualTo(11), "Length of Accepted Status to Cancel Reject Status Interested.");
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[0], Is.EqualTo(ImportMessageStatusList.Codes.ClearEntrySummaryDelete));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[1], Is.EqualTo(ImportMessageStatusList.Codes.ClearEntrySummaryOriginal));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[2], Is.EqualTo(ImportMessageStatusList.Codes.ClearEntrySummaryReplace));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[3], Is.EqualTo(ImportMessageStatusList.Codes.ClearElectronicInvoiceDelete));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[4], Is.EqualTo(ImportMessageStatusList.Codes.ClearElectronicInvoiceOriginal));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[5], Is.EqualTo(ImportMessageStatusList.Codes.ClearElectronicInvoiceReplace));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[6], Is.EqualTo(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[7], Is.EqualTo(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[8], Is.EqualTo(ImportMessageStatusList.Codes.ClearACECargoReleaseDelete));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[9], Is.EqualTo(ImportMessageStatusList.Codes.ClearACECargoReleaseReplace));
			NUnit.Framework.Assert.That(acceptedStatusToCancelRejectStatusInterested[10], Is.EqualTo(ImportMessageStatusList.Codes.ClearACECargoReleaseUpdate));
		}

		[ExpectNoExceptions]
		public void TestStatusClear()
		{
			var list = new ImportMessageStatusList();
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearEntrySummaryOriginal));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearEntrySummaryReplace));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearEntrySummaryDelete));

			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings));

			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings));

			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearElectronicInvoiceOriginal));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearElectronicInvoiceReplace));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearElectronicInvoiceDelete));

			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearDepartureAmendment));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearDepartureOriginal));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearArrival));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearExportation));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearDepartureWithdraw));

			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearFDATransmission));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearTransferOfLiability));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearDeparturePartialOriginal));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearDeparturePartialWithdraw));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearCargoReleaseOriginal));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearCargoReleaseReplace));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearCargoReleaseDelete));

			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearBorderCargoReleaseReplace));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearBorderCargoReleaseDelete));

			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearConsigneeNameAddressAdd));

			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearACECargoReleaseAdd));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearACECargoReleaseDelete));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearACECargoReleaseReplace));
			NUnit.Framework.Assert.That(list.IsStatusClear(ImportMessageStatusList.Codes.ClearACECargoReleaseUpdate));

			NUnit.Framework.Assert.That(!list.IsStatusClear(ImportMessageStatusList.Codes.AwaitingDepartureAmendment));
			NUnit.Framework.Assert.That(!list.IsStatusClear(ImportMessageStatusList.Codes.AwaitingArrival));
			NUnit.Framework.Assert.That(!list.IsStatusClear(ImportMessageStatusList.Codes.AwaitingDepartureOriginal));
			NUnit.Framework.Assert.That(!list.IsStatusClear(ImportMessageStatusList.Codes.ErrorDepartureAmendment));
			NUnit.Framework.Assert.That(!list.IsStatusClear(ImportMessageStatusList.Codes.ErrorDepartureWithdraw));
		}

		[ExpectNoExceptions]
		public void TestStatusWaitingForResponse()
		{
			var list = new ImportMessageStatusList();
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace));

			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingElectronicInvoiceOriginal));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingElectronicInvoiceDelete));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingElectronicInvoiceReplace));

			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingArrival));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingExportation));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingFDATransmission));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingTransferOfLiability));

			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingDepartureAmendment));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingDepartureOriginal));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw));

			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingCargoReleaseOriginal));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingCargoReleaseReplace));

			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseOriginal));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseDelete));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseReplace));

			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingConsigneeNameAddressAdd));

			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingACECargoReleaseDelete));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingACECargoReleaseReplace));
			NUnit.Framework.Assert.That(list.IsWaitingForResponse(ImportMessageStatusList.Codes.AwaitingACECargoReleaseUpdate));

			NUnit.Framework.Assert.That(!list.IsWaitingForResponse(ImportMessageStatusList.Codes.ClearDepartureAmendment));
			NUnit.Framework.Assert.That(!list.IsWaitingForResponse(ImportMessageStatusList.Codes.ClearArrival));
			NUnit.Framework.Assert.That(!list.IsWaitingForResponse(ImportMessageStatusList.Codes.ClearDepartureOriginal));
			NUnit.Framework.Assert.That(!list.IsWaitingForResponse(ImportMessageStatusList.Codes.ClearDepartureWithdraw));
		}

		[ExpectNoExceptions]
		public void TestIsWithdrawnStatusStatus()
		{
			var list = new ImportMessageStatusList();
			NUnit.Framework.Assert.That(list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearDepartureWithdraw));
			NUnit.Framework.Assert.That(list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearDeparturePartialWithdraw));
			NUnit.Framework.Assert.That(list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearEntrySummaryDelete));
			NUnit.Framework.Assert.That(list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearElectronicInvoiceDelete));
			NUnit.Framework.Assert.That(list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearCargoReleaseDelete));
			NUnit.Framework.Assert.That(list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearBorderCargoReleaseDelete));
			NUnit.Framework.Assert.That(list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearACECargoReleaseDelete));

			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.AwaitingDepartureOriginal));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.AwaitingDepartureAmendment));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.AwaitingArrival));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearDepartureAmendment));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearArrival));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearDepartureOriginal));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearDeparturePartialAmendment));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ClearDeparturePartialOriginal));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ErrorDepartureAmendment));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ErrorArrival));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ErrorDepartureOriginal));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.ErrorDepartureWithdraw));
			NUnit.Framework.Assert.That(!list.IsWithdrawnStatus(ImportMessageStatusList.Codes.NotSent));
		}

		[ExpectNoExceptions]
		public void TestIsPartialStatus()
		{
			var list = new ImportMessageStatusList();
			NUnit.Framework.Assert.That(list.IsPartialStatus(ImportMessageStatusList.Codes.ClearDeparturePartialAmendment));
			NUnit.Framework.Assert.That(list.IsPartialStatus(ImportMessageStatusList.Codes.ClearDeparturePartialOriginal));
			NUnit.Framework.Assert.That(list.IsPartialStatus(ImportMessageStatusList.Codes.ClearDeparturePartialWithdraw));

			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.AwaitingDepartureOriginal));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.AwaitingDepartureAmendment));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.AwaitingArrival));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.ClearDepartureAmendment));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.ClearArrival));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.ClearDepartureOriginal));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.ClearDepartureWithdraw));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.ErrorDepartureAmendment));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.ErrorArrival));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.ErrorDepartureOriginal));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.ErrorDepartureWithdraw));
			NUnit.Framework.Assert.That(!list.IsPartialStatus(ImportMessageStatusList.Codes.NotSent));
		}

		[ExpectNoExceptions]
		public void TestIsArrivalExportBTATransmissionStatus()
		{
			var list = new ImportMessageStatusList();
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.AwaitingArrival));
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.AwaitingExportation));
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.AwaitingFDATransmission));
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.AwaitingTransferOfLiability));

			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.ClearArrival));
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.ClearExportation));
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.ClearFDATransmission));
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.ClearTransferOfLiability));

			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.ErrorArrival));
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.ErrorExportation));
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.ErrorFDATransmission));
			NUnit.Framework.Assert.That(list.IsArrivalExportBTATransmissionStatus(ImportMessageStatusList.Codes.ErrorTransferOfLiability));
		}
	}
}
