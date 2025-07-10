using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	sealed class RequestWrapperTest : Customs.Business.Testing.DataProviderTestCase<RequestWrapper>
	{
		#region OperatorRequestReference

		[TestDate(2024, 09, 02, 10, 20, 30)]
		public void TestOperatorRequestReference()
		{
			AssertEquals("OperatorRequestReference should be equal to entry.CorrelationID - YYYYMMDDHHMMSS", "0123456789" + "-" + new DateTime(2024, 09, 02, 10, 20, 30).ToString("yyyyMMddhhmmss"), Provider.OperatorRequestReference);
		}

		#endregion

		#region AmendmentRequestDateAndTime

		[TestDate(2024, 08, 12, 08, 50, 30)]
		public void TestAmendmentRequestDateAndTime()
		{
			AssertEquals("AmendmentRequestDateAndTime should be equal to current date.", "2024-08-12T08:50:30", Provider.AmendmentRequestDateAndTime);
		}

		#endregion

		#region AmendmentReason

		public void TestAmendmentReason()
		{
			AssertEquals("AmendmentReason should be equal to sendingObject.VOCReason", "VOC Reason", Provider.AmendmentReason);
		}

		#endregion

		#region AmendmentMotivation

		public void TestAmendmentMotivation()
		{
			AssertEquals(
				"AmendmentMotivation should be equal to sendingObject.ChangeAcknowledgementIndicator", "MOTIV", Provider.AmendmentMotivation
			);
		}

		#endregion

		protected override RequestWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntrySubmittedDate = new ZDateTime(2020, 01, 01, 12, 30, 00);
			entry.CorrelationID = "0123456789";
			var sendingObject = new DeltaIEJobDeclarationMessageSendingObject(entry);
			sendingObject.VOCReason = "VOC Reason";
			sendingObject.ChangeAcknowledgementIndicator = "MOTIV";
			return RequestWrapper.New(sendingObject);
		}
	}
}
