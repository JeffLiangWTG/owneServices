using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.HK.Business;
using Enterprise.Edifact;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.HK.ServiceTasks
{
	public class TraxonMessageProcessor : CustomsMessageProcessor
	{
		#region Constants

		const string MessageResponseUCM = "UCM";
		public const string OriginalMessageNotFoundError = "Segment indicating what the original message number was didn't exist.";
		public const string UnableToFindOriginalMessageForResponseError = "The Original Message was not found for Message Number : ";

		#endregion

		public TraxonMessageProcessor(LoggingInformation logger)
			: base(logger, "TRX", "ISAC Response")
		{
		}

		#region Email Settings

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return HKDataRegistry.Instance.GroupToCopyTraxonResponseEmailsTo.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return Core.Constants.EmailTo.StaffMemberAndNominatedGroup; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return HKDataRegistry.Instance.GroupToCopyTraxonResponseEmailsTo.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return Core.Constants.EmailTo.StaffMemberAndNominatedGroup; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return HKDataRegistry.Instance.GroupToCopyTraxonResponseEmailsTo.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return Core.Constants.EmailTo.NominatedGroup; }
		}

		protected override GlbStaff GetUserToNotify(IBusiness originalMessage)
		{
			GlbStaff result = null;

			if (originalMessage != null)
			{
				var ediMessage = originalMessage as EDIMessage;
				if (ediMessage != null && ediMessage.EM_SystemCreateUser != User.ServiceUserCode)
				{
					result = originalMessage.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ediMessage.EM_SystemCreateUser);
				}
			}

			return result;
		}

		#endregion

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			var messagePart =
				MessageProcessingUtils.ReadMessage(new UNOACharacterSet(), message.EM_MessageText);
			var originalMessageNumber = ZString.Empty;
			if (messagePart.Length > 0)
			{
				if (SafeGet(messagePart[0], 2, 0).Left(3) == "CIM")
				{
					originalMessageNumber = SafeGet(messagePart[0], 3, 0); //Common Access Reference
				}
			}

			foreach (string[][] segment in messagePart)
			{
				if (SafeGet(segment, 0, 0) == MessageResponseUCM) //Original UNH Content
				{
					originalMessageNumber = SafeGet(segment, 1, 1); //Common access reference
					break;
				}
			}

			if (string.IsNullOrEmpty(originalMessageNumber))
			{
				var innerMessageParts = ExtractInnerMessageParts(messagePart);

				if (innerMessageParts.Length >= 0)
				{
					originalMessageNumber = SafeGet(innerMessageParts[1], 0, 0);

					try
					{
						var messageNumber = int.Parse(originalMessageNumber);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						originalMessageNumber = string.Empty;
					}
				}
			}

			if (originalMessageNumber.IsEmpty)
			{
				return ProcessError(OriginalMessageNotFoundError, message);
			}
			else
			{
				var originalMessage = GetOriginalMessage(message.Factory, originalMessageNumber);
				if (originalMessage != null)
				{
					message.EM_LinkTable = originalMessage.EM_LinkTable;
					message.EM_LinkUniqueID = originalMessage.EM_LinkUniqueID;

					var email = new EmailDef();
					email.Subject = MessageFriendlyName + " Consol Number: " + OriginalJobNumber(message);
					email.Body = message.EM_MessageText;
					SendAcknowledgementReport(originalMessage, email);
					return EDIMessage.Status.Received;
				}
				else
				{
					return ProcessError(UnableToFindOriginalMessageForResponseError + originalMessageNumber,
						message);
				}
			}
		}

		string ProcessError(string errorMessage, EDIMessage message)
		{
			var email = new EmailDef();
			Logger.Log(errorMessage);
			email.Subject = MessageFriendlyName + " Error";
			email.Body = errorMessage + "\n" + message.EM_MessageText;
			SendErrorReport(null, email);
			return EDIMessage.Status.Error;
		}

		string OriginalJobNumber(EDIMessage message)
		{
			var consol = message.Factory.Load<CommonConsol>(message.EM_LinkUniqueID);
			return consol != null ? consol.JK_UniqueConsignRef : ZString.Empty;
		}

		string[][][] ExtractInnerMessageParts(string[][][] messageParts)
		{
			var start = -1;
			var finish = messageParts.Length;
			var result = new ArrayList();

			for (int i = 0; i < messageParts.Length; i++)
			{
				string left = SafeGet(messageParts[i], 0, 0);
				if (left == "UNH")
				{
					start = i;
				}

				if (left == "UNT")
				{
					finish = i;
				}

				if ((start >= 0) && (i <= finish))
				{
					result.Add(messageParts[i]);
				}
			}

			return (string[][][])result.ToArray(typeof(string[][]));
		}

		protected EDIMessage GetOriginalMessage(BusinessObjectFactory factory, ZString originalMessageNumber)
		{
			var filter = new ZDBOnlyQuery(typeof(EDIMessage));
			filter.AddToFilter(EDIMessageSchema.EM_MessageNum, originalMessageNumber);
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.Traxon);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);

			var interchangeFilter = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_From, HKDataRegistry.Instance.HKTraxonSenderID.Value);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIMessage.ApplicationCodes.Traxon);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIMessage.Direction.Transmit);

			filter.AddSubQuery(EDIMessageSchema.EM_EI, interchangeFilter, JoinCondition.And);

			return factory.LoadTop1<EDIMessage>(filter);
		}
	}
}
