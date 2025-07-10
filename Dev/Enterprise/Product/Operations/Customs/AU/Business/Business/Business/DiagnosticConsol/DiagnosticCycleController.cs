using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DiagnosticCycleController
	{
		public DiagnosticCycleController(ZGuid testEdiMessagePk, ZDateTime messageCreateTime, ZString consolKey)
		{
			this.testEdiMessagePk = testEdiMessagePk;
			this.messageCreateTime = messageCreateTime;
			this.consolKey = consolKey;
		}

		public string CheckNextDiagnosticCycleStep(string currentStatus)
		{
			var result = currentStatus;
			var newFactory = GetNewBusinessObjectFactory();

			switch (currentStatus)
			{
				case DiagnosticStatusList.Codes.TestMessageAtQUEStatus:
					if (CheckIfEdiMessageStatusChangedFromQUE(newFactory))
					{
						result = DiagnosticStatusList.Codes.LookingForOutgoingItem;
					}
					break;

				case DiagnosticStatusList.Codes.LookingForOutMailItem:
					if (CheckOutInterchangeSent(newFactory))
					{
						result = DiagnosticStatusList.Codes.OutgoingItemAtQUEStatus;
					}
					break;

				case DiagnosticStatusList.Codes.OutgoingItemAtQUEStatus:
					result = DiagnosticStatusList.Codes.WaitingForTestMessageResponse;
					break;

				case DiagnosticStatusList.Codes.WaitingForTestMessageResponse:
					if (CheckInboundResponseReceived(newFactory))
					{
						result = DiagnosticStatusList.Codes.InComingResponseReceived;
					}
					break;

				case DiagnosticStatusList.Codes.InComingResponseReceived:
					if (CheckInterchangeHasGeneratedMessages(newFactory))
					{
						result = DiagnosticStatusList.Codes.InInterchangeProcessed;
					}
					break;

				case DiagnosticStatusList.Codes.InInterchangeProcessed:
					if (CheckResponseEdiMessageProcessed(newFactory))
					{
						result = DiagnosticStatusList.Codes.OKSuccess;
					}
					break;
			}

			return result;
		}

		bool CheckIfEdiMessageStatusChangedFromQUE(BusinessObjectFactory factory)
		{
			bool result = false;

			var messageToTrack = factory.Load<EDIMessage>(new ZGuid(testEdiMessagePk));

			if (messageToTrack == null)
			{
				throw new InvalidOperationException("Test Message to track cannot be found");
			}
			else if (messageToTrack.EM_Status == EDIMessage.Status.Sent)
			{
				if (messageToTrack.Interchange == null)
				{
					throw new DiagnosticMessageException(DiagnosticStatusList.Codes.NoOutInterchange, "");
				}
				else
				{
					outInterchangePk = messageToTrack.Interchange.PK;
					result = true;
				}
			}
			else if (messageToTrack.EM_Status != EDIMessage.Status.Queued)
			{
				throw new DiagnosticMessageException(DiagnosticStatusList.Codes.TestMessageStatusInvalid, string.Format(CultureInfo.InvariantCulture, "Test Message status not set to expected SNT, but is {0}", messageToTrack.EM_Status));
			}

			return result;
		}

		bool CheckOutInterchangeSent(BusinessObjectFactory factory)
		{
			bool result = false;

			var outInterchange = factory.Load<EDIInterchange>(new ZGuid(outInterchangePk));

			if (outInterchange == null)
			{
				throw new DiagnosticMessageException("FAL", "Out Interchange cannot be found");
			}
			else if (outInterchange.EI_Status == EDIInterchange.Status.Sent)
			{
				result = true;
			}
			else if (outInterchange.EI_Status == EDIInterchange.Status.SyntaxRejected)
			{
				throw new DiagnosticMessageException(DiagnosticStatusList.Codes.OutInterchangeRejected, "");
			}
			else if (outInterchange.EI_Status != EDIInterchange.Status.Queued && outInterchange.EI_Status != EDIInterchange.Status.eHubQueued && outInterchange.EI_Status != EDIInterchange.Status.eHubPending)
			{
				throw new DiagnosticMessageException(DiagnosticStatusList.Codes.OutInterchangeStatusInvalid, string.Format(CultureInfo.CurrentCulture, "Out Interchange status not set to expected QUE or SNT, but is {0}", outInterchange.EI_Status));
			}

			return result;
		}

		bool CheckInboundResponseReceived(BusinessObjectFactory factory)
		{
			bool result = false;
			var replyInterchange = LoadReplyEdiInterchange(factory);
			if (replyInterchange != null)
			{
				result = true;
			}

			return result;
		}

		bool CheckInterchangeHasGeneratedMessages(BusinessObjectFactory factory)
		{
			bool result = false;
			var interchangeMessages = LoadReplyEdiInterchange(factory)?.ContainedMessages;
			if (interchangeMessages != null)
			{
				if (interchangeMessages.Count >= 1)
				{
					result = interchangeMessages[0] != null;
				}
			}
			else
			{
				throw new DiagnosticMessageException("FAL", "The interchange timed out, or doesn't exist.");
			}

			return result;
		}

		bool CheckResponseEdiMessageProcessed(BusinessObjectFactory factory)
		{
			bool result = false;
			var interchange = LoadReplyEdiInterchange(factory);
			EDIMessage message = null;

			if (interchange != null)
			{
				if (interchange.ContainedMessages.Count >= 1)
				{
					message = interchange.ContainedMessages[0];
				}

				if (message == null)
				{
					throw new DiagnosticMessageException(DiagnosticStatusList.Codes.WaitingForTestMessageResponse, "");
				}

				var replyEdiMessage = factory.Load<EDIMessage>(message.PK);

				if (replyEdiMessage == null)
				{
					throw new InvalidOperationException("Response EDI message cannot be found");
				}
				else if (replyEdiMessage.EM_Status == EDIMessage.Status.Error)
				{
					result = true;
				}
				else if (replyEdiMessage.EM_Status != EDIMessage.Status.Queued)
				{
					throw new DiagnosticMessageException(DiagnosticStatusList.Codes.InInterchangeStatusInvalid, string.Format(CultureInfo.CurrentCulture, "Response EDI message status set to unexpected value {0}", replyEdiMessage.EM_Status));
				}
			}
			else
			{
				throw new DiagnosticMessageException("FAL", "The interchange timed out, or doesn't exist.");
			}

			return result;
		}

		EDIInterchange LoadReplyEdiInterchange(BusinessObjectFactory factory)
		{
			var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.CMR);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, messageCreateTime);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_BodyText, SQLComparisonOperator.Contains, "RFF+ABO:" + consolKey.Replace("-", ""));

			return factory.LoadTop1<EDIInterchange>(interchangeQuery);
		}

		/// <summary>
		/// Virtual for testing only
		/// </summary>
		protected virtual BusinessObjectFactory GetNewBusinessObjectFactory()
		{
			return new BusinessObjectFactory();
		}

		ZGuid outInterchangePk;
		readonly ZGuid testEdiMessagePk;
		readonly ZString consolKey;
		readonly ZDateTime messageCreateTime;
	}
}
