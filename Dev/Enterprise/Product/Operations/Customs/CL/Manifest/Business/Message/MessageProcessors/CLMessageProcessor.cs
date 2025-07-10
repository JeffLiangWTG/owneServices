using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public abstract class CLMessageProcessor : BranchCustomsApplicationTypeMessageProcessor
	{
		public CLMessageProcessor(LoggingInformation logger) : base(logger) { }

		internal string errorColumn = Res.GetString("480FDA68-19BF-42C9-B3DC-06B6B7CC7D0C", "Error");
		internal string detailColumn = Res.GetString("27816EE0-7FAB-41A6-8443-2808E888425B", "Detail");
		internal HtmlTableCreator tableCreator;
		internal bool isFailure;

		protected override string MessageFriendlyNameCore => Res.GetString("9B47FFFD-E795-45BF-AB19-BFF5569CA603", "CL Message");

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.CLCustoms;

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem() => CLCustomsDataRegistry.Instance.CLMANGroupNotification;

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			bool success = false;

			var linkedObject = message.EM_LinkedObject;
			if (linkedObject == null)
			{
				Logger.LogWarning(Res.GetString("954166AE-C93C-40C1-80A9-CED870652E92", "Unable to find business object for message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to ERROR.", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference));
			}
			else
			{
				if (linkedObject is IMessageAttachee messageAttachee)
				{
					message.EM_GB = messageAttachee.GlobalBranchPK;
				}
				success = true;
			}

			message.EM_Status = success ? EDIMessage.Status.PreProcessedOK : EDIMessage.Status.Discarded;
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			tableCreator = new HtmlTableCreator(new string[] { Res.GetString("47D07904-47E3-4526-9E1B-FAC8A5450933", "Column"), Res.GetString("08C2CF3D-1AD4-4851-8F8F-5EF4664BA2AF", "Value") });
			isFailure = true;

			var status = EDIMessageStatusList.Codes.ProcessedOK;
			var billAttachee = message.EM_LinkedObject as IMessageAttachee;

			var clMessage = message as CLMessage;
			var bodyText = clMessage.EM_MessageText;
			if (bodyText.IsEmpty || !XmlUtils.IsValidXml(bodyText))
			{
				status = EDIMessageStatusList.Codes.Discarded;

				var error = Res.GetString("A8B88121-34BA-4AA4-94BF-8DB518C04B21", "Message {0} failed to parse the message as {1} message.", clMessage.EM_MessageNum, clMessage.EM_MessageType);
				Logger.LogError(error);
				tableCreator.WriteRow(errorColumn, error);
			}
			else
			{
				status = ProcessCustomsManifestMessage(clMessage, status);
			}

			clMessage.EM_Status = status;

			SendNotificationEmailIfNeeded(billAttachee, message);
		}

		internal abstract string ProcessCustomsManifestMessage(CLMessage message, string status);

		internal void PopulateBillCustomsEntryNumber(ASYCUDA.Business.AsycudaBill bill, ZString number)
		{
			bill.CustomsEntryNumber = number;
			bill.CustomsEntryNumberType = CusEntryNumberTypes.Chile.IDS;
		}

		internal ZString DiscardedCase()
		{
			var error = CLMessageConstants.BillNotFound;
			Logger.LogError(error);
			tableCreator.WriteRow(errorColumn, error);
			return EDIMessageStatusList.Codes.Discarded;
		}

		internal ZString ErrorCase(CLMessage message)
		{
			var error = CLMessageConstants.WrongXML;
			message.Notes.AddNew(true, CLMessageConstants.Processing, error);
			tableCreator.WriteRow(errorColumn, error);
			return EDIMessageStatusList.Codes.Error;
		}

		#region Send Notification Email

		void SendNotificationEmailIfNeeded(IMessageAttachee bill, EDIMessage message)
		{
			if (bill != null)
			{
				var branchPK = bill.GlobalBranchPK;
				if (branchPK.IsValid)
				{
					messageBranch = message.Factory.Load<IGlbBranch>(branchPK);
				}
				else
				{
					messageBranch = MasterFiles.Business.GlbBranch.CurrentBranch;
				}

				var emailGroupRegistryItem = GetEmailGroupRegistryItem();
				var supportMessageSuppressRegistry = emailGroupRegistryItem as ISupportMessageSuppressRegistry;
				var shouldSendErrorEmailsOnly = supportMessageSuppressRegistry?.ShouldSEndErrorsOnly(MessageBranch.GB_GC, MessageBranch.PK, ZGuid.Empty) ?? false;
				var uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, bill.PK.ToGuid());

				if (!shouldSendErrorEmailsOnly || (shouldSendErrorEmailsOnly && isFailure))
				{
					GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, uri, bill.Header.AMA_JobReference, MailSubject(),
						GetEmailBody(bill, tableCreator, isFailure),
						isFailure, message.Branch, bill as BusinessObject,
						() => GetEmailAddressToSendTo(bill, message));
				}
			}
		}

		string MailSubject() => Res.GetString("67EF13E7-46F7-4298-996A-BF1CC5405F1A", "Manifest Status Message");

		ZString GetEmailBody(IMessageAttachee bill, HtmlTableCreator tableCreator, bool isFailure)
		{
			var htmlBody = new StringBuilder();
			var jobReference = bill.Header.AMA_JobReference;
			var blNumber = bill.Number;

			if (!isFailure)
			{
				htmlBody.Append(Res.GetString("689FB901-B751-4013-A5DB-2C1ABD7174DF", "Manifest Message for job {0}, bill {1} has been cleared. For details please follow the Link to the Manifest", jobReference, blNumber));
			}
			else
			{
				htmlBody.Append(Res.GetString("EC4E67C0-DC40-4011-8BA0-520BDF39D4AD", "Manifest Message for job {0}, bill {1} has been rejected. For details please follow the Link to the Manifest", jobReference, blNumber));
			}
			htmlBody.Append("<br /><br />");
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}

		internal void BuildEmailBody(List<Tuple<string, string>> nodes, ZString messageInterpretation, ZString column)
		{
			foreach (var item in nodes)
			{
				tableCreator.WriteRow(new string[] { item.Item1, item.Item2 });
			}
			tableCreator.WriteRow(column, messageInterpretation);
		}

		ZString GetEmailAddressToSendTo(IMessageAttachee bill, EDIMessage message)
		{
			var result = ZString.Empty;
			if (bill != null)
			{
				var sender = (IUser)message?.UserWhoQueuedThisRecord;
				if (sender == null || sender.IsBatchProcessor)
				{
					message = GetLastTransmitMessage(bill);
					sender = message?.UserWhoQueuedThisRecord;
				}
				result = sender?.EmailAddress ?? string.Empty;
			}
			return result;
		}

		EDIMessage GetLastTransmitMessage(IMessageAttachee bill)
		{
			EDIMessage result = null;
			if (bill != null)
			{
				if (lastTransmitMessageCached == null || lastTransmitMessageCached.EM_LinkUniqueID != bill.PK)
				{
					lastTransmitMessageCached = bill.Messages.OfType<EDIMessage>().Where(x => x.IsTransmitMessage && x.EM_SystemCreateUser != User.ServiceUserCode)
						.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
				}
				result = lastTransmitMessageCached;
			}
			return result;
		}
		EDIMessage lastTransmitMessageCached;

		IGlbBranch MessageBranch
		{
			get { return messageBranch; }
			set { messageBranch = value; }
		}
		IGlbBranch messageBranch;

		#endregion
	}
}
