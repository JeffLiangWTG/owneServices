using System;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.ServiceTasks.RraProcessors.RRA12;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA12
{
	/// <summary>
	/// Takes one EdiMEssage string and chops it up into RRA12Message objects. ApplicationTypeMessageProcessor.
	/// </summary>
	public class RRA12ApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		ILogger ServiceLogger { get; set; }

		protected readonly IEDocsDelayedSaver EDocsSaver;

		public RRA12ApplicationTypeMessageProcessor(ILogger serviceLogger, IEDocsDelayedSaver eDocsSaver)
			: base(new LoggingInformation())
		{
			this.ServiceLogger = serviceLogger;
			EDocsSaver = eDocsSaver;
		}

		protected override void ProcessMessageCore(EDIMessage ediMessage)
		{
			FormattedRRA12Text = ediMessage.EM_MessageText;
			RRA12MessageBase rra12Message = null;
			if (ediMessage.EM_MessageSubType == "ISL")
			{
				rra12Message = new RRA12ISLMessage().CreateFromString<RRA12ISLMessage>(FormattedRRA12Text);
				DoAllUpdatesAndProcessing(rra12Message, ediMessage);
			}
			else
			{
				SendWarningToPostMasterAboutDuffRRA12_DoesNotSendToUserOnlyPostmaster(FormattedRRA12Text, ediMessage.Factory);
				ediMessage.EM_Status = EDIMessage.Status.Failed;
			}
			if (ediMessage.Interchange != null)
			{
				ediMessage.Interchange.EI_Status = EDIInterchange.Status.Received;
			}
		}

		void DoAllUpdatesAndProcessing(RRA12MessageBase rra12Message, EDIMessage ediMessage)
		{
			CusEntryHeader entryHeader = GetCusEntryHeaderFromCusEntryNumber(rra12Message.Header, ediMessage);
			if (entryHeader != null)
			{
				UpdateEntryHeaderToClearedOrHeld(entryHeader, rra12Message, ediMessage);
				RecordIndividualUcnsThatAreHeld(entryHeader, rra12Message, ediMessage);
				SendSuccessEmailShowingPrettyRRA12(
					rra12Message,
					ediMessage,
					entryHeader);
				SaveRra12ToEDocs(entryHeader, rra12Message);
				ServiceLogger.Log(LogType.Information, delegate
				{ return "Successfully processed RRA12 for entry " + entryHeader.EntryNumber; });
			}
			else
			{
				ServiceLogger.Log(LogType.Warning, delegate
				{ return "Could not update dbo.CusEntryHeader since no matching entry header was found. Entry number: " + rra12Message.Header.EntryNumber; });
				ediMessage.EM_Status = EDIMessage.Status.Failed;
				SendWarningAboutNotFindingTheEntryNumber(rra12Message, ediMessage);
			}
		}

		void RecordIndividualUcnsThatAreHeld(CusEntryHeader entryHeader, RRA12MessageBase rra12Message, EDIMessage ediMessage)
		{
			foreach (var oldUcn in entryHeader.MaritimeUcnsThatAreHeld.ToArray())
			{
				entryHeader.MaritimeUcnsThatAreHeld.RemoveAndDelete(oldUcn);
			}
			foreach (var individualUcnLine in rra12Message.Lines)
			{
				if (!individualUcnLine.RemovalNote)
				{
					var ucn = entryHeader.MaritimeUcnsThatAreHeld.AddNew();
					ucn.Data.NW_UCN = individualUcnLine.UCN;
					ucn.Data.NW_HoldTypeComments = new ZStringBuilder(individualUcnLine.Comments).ToStringWithDelimiterBetweenAppends(";");
				}
			}
		}

		void SendWarningAboutNotFindingTheEntryNumber(RRA12MessageBase rra12Message, EDIMessage ediMessage)
		{
			string subject = string.Format("Customs amalgamation clearance advice for  {0} - entry and job not found", rra12Message.Header.EntryNumber);
			string body = "An amalgamation clearance advice email was received, but the job could not be found. The entry number in the message did not match any entry in your system.  The advice message is attached.";
			SendEmailToOriginatingUserShared(subject, body, ediMessage, CreateMailAttachmentFromRRA12Text(), null, ServiceLogger, Logger);
		}

		void SendSuccessEmailShowingPrettyRRA12(RRA12MessageBase rra12Message, EDIMessage ediMessage, CusEntryHeader entryHeader)
		{
			ZString declarationNumber = entryHeader.Declaration.JE_DeclarationReference;
			ZString entryNumber = entryHeader.EntryNumber;
			string trimmedString = rra12Message.PrettyText;

			string preformattedHtml = string.Format("<pre>{0}</pre>", trimmedString);
			ZString body = string.Format("<h2>Customs clearance advice for {0} / {1}</h1> {2}", declarationNumber, entryNumber, preformattedHtml);
			ZString subject = string.Format("Customs amalgamation clearance advice for  {0} / {1}", declarationNumber, entryNumber);
			ediMessage.EM_MessageInterpretation = body;
			SendEmailToOriginatingUserShared(subject, body, ediMessage, null, entryHeader, ServiceLogger, Logger);
		}

		internal static void SendEmailToOriginatingUserShared(ZString subject, ZString body, EDIMessage ediMessage, AttachmentDef attachment, CusEntryHeader cusEntryHeader, ILogger serviceLogger, LoggingInformation logger)
		{
			HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
			GlbStaff user;
			RRA01AndRRA11.McpRRA01RRA11AndRRA06EmailResponseProcessor.SetUserForThisJob(cusEntryHeader, logger, out user);
			EmailDef emailDef = cusEntryHeader != null ? emailSender.CreateEmail(subject, body, branchPK: cusEntryHeader.RegistryBranchPK, compPK: cusEntryHeader.RegistryCompanyPK)
														: emailSender.CreateEmail(subject, body);
			if (user == null || user.GS_EmailAddress.IsEmpty)
			{
				serviceLogger.Log(LogType.Warning, delegate
				{ return "No originating sender was found, unable to return the message directly to them. Selecting a postmaster instead."; });
				ZQuery gs = new ZQuery(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
				user = ediMessage.Factory.LoadTop1<GlbStaff>(gs);
				emailDef.Subject += " (fallback to postmaster)";
			}

			if (attachment != null)
			{
				emailDef.Attachments.Add(attachment);
			}

			var cusEntryHeaderRegistryBranchPK = Guid.Empty;
			if (cusEntryHeader != null)
			{
				cusEntryHeaderRegistryBranchPK = cusEntryHeader.RegistryBranchPK;
			}

			new Customs.Business.EmailSender(logger).SendNotification(
				emailDef,
				user,
				GBCustomsDataRegistry.Instance.CustomsResponseNotifications,
				GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationMcpRra12, "", Guid.Empty, cusEntryHeaderRegistryBranchPK, Guid.Empty),
				GBCustomsDataRegistry.Instance.NotificationMcpRra12,
				ediMessage.Factory);
			serviceLogger.Log(LogType.Information, delegate
			{ return "Sent email [" + subject + "] to " + emailDef.Recipients.RecipientsAsDelimitedString(); });
		}

		CusEntryHeader GetCusEntryHeaderFromCusEntryNumber(RRA12MessageHeader rra12Header, EDIMessage ediMessage)
		{
			var entries = GB.Business.GbExtensionHelpers.GetCusEntryHeaderFromCusEntryNumberAndDate(rra12Header.EntryNumber, ediMessage.Factory, rra12Header.EntryDate);
			if (entries.Length == 0)
			{
				return null;
			}
			else if (entries.Length == 1)
			{
				return entries[0];
			}
			else
			{
				var message = string.Format("Found too many entries, expected only one entry with a given number and date. Number={0}, Date={1}.", rra12Header.EntryNumber, rra12Header.EntryDate);
				ErrorReporter.ReportOnce("BGB-MCP-RRA12-TooManyEntries", message);
				return entries.FirstOrDefault();
			}
		}

		void UpdateEntryHeaderToClearedOrHeld(CusEntryHeader entryHeader, RRA12MessageBase oneRra12, EDIMessage ediMessage)
		{
			ediMessage.EM_Status = EDIMessage.Status.Received;
			ediMessage.EM_LinkedObject = entryHeader;
			ediMessage.EM_GB = entryHeader.Branch.PK;
			entryHeader.ApplyOrRemoveHoldOrClear(oneRra12);
			AddClearedEventIfNeeded(oneRra12, entryHeader);
			ServiceLogger.Log(LogType.Information, delegate
			{ return string.Format("Updated CusEntryHeader to CLR: {0}; PK {1}", entryHeader.EntryNumber, entryHeader.PK.ToString()); });
		}

		void AddClearedEventIfNeeded(RRA12MessageBase oneRra12, CusEntryHeader entryHeader)
		{
			if (entryHeader.CH_EntryStatus != EntryStatusList.Codes.Clear)
			{
				entryHeader.Logs.AddNew(Events.CustomsCleared, "Cleared via RRA12", oneRra12.Date.ToOffset());
			}
		}

		void SendWarningToPostMasterAboutDuffRRA12_DoesNotSendToUserOnlyPostmaster(string formattedRRA12Text, BusinessObjectFactory factory)
		{
			EmailDef email = new EmailDef();
			email.Subject = "Could not parse RRA12";
			email.Body = Core.Constants.ProductName + " could not parse an RRA12 due to it being in an unsupported format. Text RRA12s are not supported, only ISL.";
			email.Attachments.Add(CreateMailAttachmentFromRRA12Text());
			Enterprise.Environment.Env.OutgoingCustomsMailManager.CreateAndSaveToPostmasterGroup(email, factory);
			ServiceLogger.Log(LogType.Error, delegate
			{ return "Unparsable RRA12. " + System.Environment.NewLine + formattedRRA12Text; });
		}

		AttachmentDef CreateMailAttachmentFromRRA12Text()
		{
			AttachmentDef attachment = new AttachmentDef("Invalid RRA12 " + ZDateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".txt", StringToBytes(FormattedRRA12Text));
			return attachment;
		}

		byte[] StringToBytes(string input)
		{
			ASCIIEncoding encoding = new ASCIIEncoding();
			return encoding.GetBytes(input);
		}

		void SaveRra12ToEDocs(CusEntryHeader cusEntryHeader, RRA12MessageBase rra12Message)
		{
			try
			{
				var docManagerInfo = ((IDocManagerSupport)cusEntryHeader.Declaration).DocManagerInfo;
				var fileName = $"Amalgamation clearance advice {CargoWise.IO.MakeFilenameSafe.MakeSafe(cusEntryHeader.EntryNumber)}.rra12.txt";
				var printFile = docManagerInfo.AddFileOrDocument(ZBlob.FromAscii(rra12Message.PrettyText), fileName, Core.Constants.RefDocTypes.ReleaseRemovalAdvice, overwriteExistingFileIfNotImageFile: false);
				printFile.Description = "Amalgamation clearance advice (RRA12) from Destin8";
				_ = cusEntryHeader.Logs.AddNew(Events.DocumentAllocated, StmALogEventSourceExtensions.GenerateEventReference(Core.Constants.RefDocTypes.ReleaseRemovalAdvice, printFile.UniqueKey));
				docManagerInfo.MasterFactory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Information($"The entry document failed to be generated due to the following error: {ex.Message}");
			}
		}

		public string FormattedRRA12Text { get; private set; }

		protected override string ApplicationCodeCore
		{
			get { return RRA12ServiceTask.Code; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return RRA12ServiceTask.FriendlyName; }
		}
	}
}
