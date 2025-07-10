using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.CNS.ServiceTasks;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.GB.CNS
{
	/// <summary>
	/// Takes one EdiMEssage string and chops it up into CnsXmlMessage objects. ApplicationTypeMessageProcessor.
	/// </summary>
	public class CnsXmlApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		protected BusinessObjectFactory sharedFactory;
		protected ILogger ServiceLogger { get; set; }

		protected readonly IEDocsDelayedSaver EDocsSaver;

		public CnsXmlApplicationTypeMessageProcessor(ILogger serviceLogger, IEDocsDelayedSaver eDocsSaver)
			: base(new LoggingInformation())
		{
			this.ServiceLogger = serviceLogger;
			EDocsSaver = eDocsSaver;
		}

		protected override void ProcessMessageCore(EDIMessage receivedEdiMessage)
		{
			if (receivedEdiMessage == null)
			{ throw new ArgumentException("Null EDI message passed to processor."); }

			sharedFactory = receivedEdiMessage.Factory;
			ZString cnsXmlText = receivedEdiMessage.EM_MessageText;

			if (cnsXmlText.IsEmpty)
			{
				throw new ArgumentException("Empty message passed to processor. EDI message PK: " + receivedEdiMessage.PK.ToString());
			}

			CNScargoStatus updater = null;

			if (ThisIsTheFirstTimeWeHaveTriedThisMessage(receivedEdiMessage))
			{
				// if message not edited by a user it means it's the first time we've seen it.  If the editing user is not empty and not ~BP, it means a user has requeued it. 
				var invalidAgainstSchemaReason = ValidateXmlAgainstSchema(cnsXmlText);
				if (!invalidAgainstSchemaReason.IsEmpty)
				{
					receivedEdiMessage.EM_Status = EDIMessage.Status.Error;
					var reason = string.Format("CNS XML message {0} was not schema-valid. Cannot process. This is an error in the inbound data, but it may be ignored once by marking this message as queued (Maintain > System > EDIMessage > find the message > right click > actions > reset to queued). Note that reprocessing may cause a further error.  Please have CNS check that the message is schema-valid. Error reported during validation: {1}", receivedEdiMessage.EM_MessageNum, invalidAgainstSchemaReason);
					SendWarningToPostMasterAboutDuffCnsXml_DoesNotSendToUserOnlyGroup(cnsXmlText, delegate
					{ return reason; }, receivedEdiMessage,
							GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCnsErrors, "", receivedEdiMessage.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty),
							GBCustomsDataRegistry.Instance.NotificationCnsErrors);
					ServiceLogger.Log(LogType.Error, delegate
					{ return reason; });
					return;
				}
			}

			try
			{
				updater = CNScargoStatus.Parser.Read(cnsXmlText);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("BGB-CnsXml-ParserError", "Could not parse CnsXml due to it being in an unexpected format: " + ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""), ex);
				SendWarningToPostMasterAboutDuffCnsXml_DoesNotSendToUserOnlyGroup(cnsXmlText, delegate
				{ return "Unparsable CnsXml. " + ex.Message + System.Environment.NewLine + cnsXmlText; }, receivedEdiMessage,
							GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCnsErrors, "", receivedEdiMessage.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty),
							GBCustomsDataRegistry.Instance.NotificationCnsErrors);
				receivedEdiMessage.EM_Status = EDIMessage.Status.Error;
				ServiceLogger.Log(LogType.Error, delegate
				{ return string.Format("Could not parse CNS XML message. Bad format? Marking EDIMessage {0} as ERR.  Exception: {1}", receivedEdiMessage.EM_MessageNum, ex.Message); });
				return;
			}

			if (updater == null)
			{
				// Could not read for some reason
				SendWarningToPostMasterAboutDuffCnsXml_DoesNotSendToUserOnlyGroup(cnsXmlText, delegate
				{ return "Unparsable CnsXml. Unknown failure to parse XML" + System.Environment.NewLine + cnsXmlText; }, receivedEdiMessage,
							GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCnsErrors, "", receivedEdiMessage.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty),
							GBCustomsDataRegistry.Instance.NotificationCnsErrors);
				return;
			}
			DoAllUpdatesAndProcessing(updater, receivedEdiMessage);
		}

		ZString ValidateXmlAgainstSchema(ZString cnsXmlText)
		{
			var invalidReason = ZString.Empty;
			try
			{
				var settings = new XmlReaderSettings();
				var xsdName = string.Format("Enterprise.Customs.GB.CNS.Documentation.ICD.CargowiseLenient.CNSICD_v{0}.xsd", (cnsXmlText.Contains("CHIEFEntryNo") ? "2" : "1"));
				using (Stream inStream = GetType().Assembly.GetManifestResourceStream(xsdName))
				{
					if (inStream != null)
					{
						var schema = ZXmlSchema.Read(inStream, null);
						settings.Schemas.Add(schema);
						settings.ValidationType = System.Xml.ValidationType.Schema;
						var document = new XmlDocument();
						document.LoadXml(cnsXmlText);
						var readeer = XmlReader.Create(new StringReader(document.InnerXml), settings);
						while (readeer.Read())
						{ }
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				invalidReason = ex.Message;
			}
			return invalidReason;
		}

		void DoAllUpdatesAndProcessing(CNScargoStatus cnsXmlMessage, EDIMessage receivedEdiMessage)
		{
			CusEntryHeader entryHeader = null;
			if (!string.IsNullOrEmpty(cnsXmlMessage.MessageDetail.CHIEFEntryNo))
			{
				var entryHeaders = GbExtensionHelpers.GetCusEntryHeaderFromCusEntryNumberAndDate(cnsXmlMessage.EntryNumber, receivedEdiMessage.Factory, cnsXmlMessage.EntryDate);
				if (entryHeaders.Length == 1)
				{
					entryHeader = entryHeaders[0];
				}
				else
				{
					// also limit on UCN
					entryHeader = (from CusEntryHeader ceh in entryHeaders
								   where
										(ceh.CH_MasterUCR == cnsXmlMessage.UcnNumberProperlyTruncated
										|| (GBCustomsDataRegistry.Instance.AllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating.Value && ceh.CH_MasterUCR == cnsXmlMessage.UcnNumberVerbatim))
								   select ceh).FirstOrDefault();
				}
			}
			if (entryHeader == null)
			{
				entryHeader = cnsXmlMessage.GetEntryHeaderFromUcnUsingMucr(receivedEdiMessage);
			}

			if (entryHeader != null)
			{
				UpdateEntryBasedOnTypeOfClearanceOrHold(entryHeader, cnsXmlMessage, receivedEdiMessage);
				receivedEdiMessage.EM_MessageInterpretation = SendSuccessEmailShowingBaseEssentialsOfXmlAndReturnBody(cnsXmlMessage, receivedEdiMessage, entryHeader);
				receivedEdiMessage.EM_MessageNum = new ZString(cnsXmlMessage.MessageHeader.HeaderID).Left(EDIMessage.Schema.EM_MessageNumMaxLength);
				SaveCnsXmlToEDocs(entryHeader, cnsXmlMessage);
				ServiceLogger.Log(LogType.Information, delegate
				{ return string.Format("Successfully processed CnsXml for entry {0}, message {1}", entryHeader.EntryNumber, receivedEdiMessage.EM_MessageNum); });
			}
			else
			{   // Found no entry header
				ServiceLogger.Log(LogType.Warning, delegate
				{ return string.Format("Could not update dbo.CusEntryHeader since no matching entry header was found. UCN={0}, Chief entry number={1}, Chief entry date={2}, message number={3}", cnsXmlMessage.MessageDetail.UCN, cnsXmlMessage.MessageDetail.CHIEFEntryNo, cnsXmlMessage.MessageDetail.CHIEFEntryDate, receivedEdiMessage.EM_MessageNum); });
				receivedEdiMessage.EM_Status = EDIMessage.Status.Failed;
				SendWarningAboutNotFindingTheEntryNumber(cnsXmlMessage, receivedEdiMessage);
			}
			if (receivedEdiMessage.Interchange != null)
			{
				receivedEdiMessage.Interchange.EI_Status = EDIInterchange.Status.Received;
			}
		}

		void UpdateEntryBasedOnTypeOfClearanceOrHold(CusEntryHeader entryHeader, CNScargoStatus cnsXmlMessage, EDIMessage receivedEdiMessage)
		{
			if (!string.IsNullOrEmpty(cnsXmlMessage.MessageDetail.CHIEFEntryRoute) && entryHeader.CH_EntryStatus != "CLR" && cnsXmlMessage.MessageDetail.Clearance != "CL")
			{
				entryHeader.CH_EntryStatus = StatusChecker.GetStatusCodeFromRouteOfEntryStatic(cnsXmlMessage.MessageDetail.CHIEFEntryRoute);
			}

			receivedEdiMessage.EM_Status = EDIMessage.Status.Received;
			receivedEdiMessage.EM_LinkedObject = entryHeader;
			receivedEdiMessage.EM_GB = entryHeader.Branch.PK;

			entryHeader.ApplyOrRemoveHoldOrClear(cnsXmlMessage);
			if (cnsXmlMessage.IsClear)
			{
				GbExtensionHelpers.UpdateEntryHeaderToCleared(entryHeader, cnsXmlMessage.DateStampHeaderId);
			}

			ServiceLogger.Log(LogType.Information, delegate
			{ return string.Format("Updated CusEntryHeader to {2}: {0}; PK {1}; message number={3}", entryHeader.EntryNumber, entryHeader.PK.ToString(), entryHeader.CH_EntryStatus, receivedEdiMessage.EM_MessageNum); });
		}

		void SendWarningAboutNotFindingTheEntryNumber(CNScargoStatus cnsXmlMessage, EDIMessage ediMessage)
		{
			string subject = string.Format("Customs clearance advice for UCN={0} - entry and job not found", cnsXmlMessage.MessageDetail.UCN);
			string body = "A clearance advice email was received from CNS, but the job could not be found. The entry number in the message did not match any entry in your system.  The advice message is attached.";
			SendEmailToOriginatingUserShared(subject, body, ediMessage, CreateMailAttachmentFromCnsXmlText(cnsXmlMessage.OriginalXml), null,
					GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCnsErrors, "", ediMessage.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty),
					GBCustomsDataRegistry.Instance.NotificationCnsErrors);
		}

		string SendSuccessEmailShowingBaseEssentialsOfXmlAndReturnBody(CNScargoStatus cnsXmlMessage, EDIMessage ediMessage, CusEntryHeader entryHeader)
		{
			ZString declarationNumber = entryHeader.Declaration.JE_DeclarationReference;
			ZString entryNumber = entryHeader.EntryNumber;

			string preformattedHtml = cnsXmlMessage.ToString();
			ZString typeOfAdvice = cnsXmlMessage.MessageHeader.MessageType.ToString();
			if (cnsXmlMessage.MessageHeader.MessageType != MessageType.CLEARED)
			{
				string holdType = new CnsHoldTypes().GetDescriptionFromCode(cnsXmlMessage.MessageDetail.Hold);
				preformattedHtml = string.Format("<p><b>N.B.</b> A <font color='red'>{0}</font> of type {1} ({2}) was toggled for this entry</p>", cnsXmlMessage.MessageHeader.MessageType.ToString(), cnsXmlMessage.MessageDetail.Hold, holdType)
								+ preformattedHtml;
				if (cnsXmlMessage.IsClear)
				{
					typeOfAdvice += "/Cleared";
				}
			}

			ZString body = string.Format("<h2>Customs {4} advice for {0} / {1}</h1> <h2>UCN={3}</h2> {2}", declarationNumber, entryNumber, preformattedHtml, cnsXmlMessage.UcnNumberProperlyTruncated, typeOfAdvice);
			ZString subject = string.Format("Customs {3} advice for  {0} / {1} / UCN={2}", declarationNumber, entryNumber, cnsXmlMessage.UcnNumberProperlyTruncated, typeOfAdvice);

			SendEmailToOriginatingUserShared(subject, body, ediMessage, null, entryHeader,
					GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCnsXml, "", Guid.Empty, entryHeader.RegistryBranchPK, Guid.Empty),
					GBCustomsDataRegistry.Instance.NotificationCnsXml);

			return body;
		}

		internal void SendEmailToOriginatingUserShared(ZString subject, ZString body, EDIMessage ediMessage, AttachmentDef attachment, CusEntryHeader entry,
				Guid notificationRegistryItemPk, IRegistryItem notificationRegistryItem)
		{
			HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
			GlbStaff originalSender = null;
			if (entry != null)
			{
				ZQuery staffQuery = new ZQuery(GlbStaffSchema.GS_Code, entry.Declaration.JE_GS_NKCusAgent);
				originalSender = entry.Factory.LoadTop1<GlbStaff>(staffQuery);
			}
			EmailDef emailDef = emailSender.CreateEmail(subject, body);
			if (originalSender == null)
			{
				ServiceLogger.Log(LogType.Warning, delegate
				{ return "No originating sender was found, unable to return the CnsXml directly to them. Selecting a postmaster instead. Message number=" + ediMessage.EM_MessageNum; });
				ZQuery gs = new ZQuery(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
				originalSender = ediMessage.Factory.LoadTop1<GlbStaff>(gs);
				emailDef.Subject += " (fallback to postmaster)";
			}

			if (attachment != null)
			{
				emailDef.Attachments.Add(attachment);
			}

			new Customs.Business.EmailSender(Logger).SendNotification(
				emailDef,
				originalSender,
				GBCustomsDataRegistry.Instance.CustomsResponseNotifications,
				notificationRegistryItemPk,
				notificationRegistryItem,
				ediMessage.Factory);
			ServiceLogger.Log(LogType.Information, delegate
			{ return "Sent email [" + subject + "] to " + emailDef.Recipients.RecipientsAsDelimitedString(); });
		}

		void SendWarningToPostMasterAboutDuffCnsXml_DoesNotSendToUserOnlyGroup(string formattedCnsXmlText, Func<string> getDescription, EDIMessage receivedMessage,
				Guid notificationRegistryItemPk, IRegistryItem notificationRegistryItem)
		{
			EmailDef email = new EmailDef();
			email.Subject = "Could not parse CNS XML clearance/hold/release message " + receivedMessage.EM_MessageNum;
			email.Body = Core.Constants.ProductName + " could not parse a CNS XML message due to it being in an invalid format. The message is attached for manual attention. " + getDescription();
			email.Attachments.Add(CreateMailAttachmentFromCnsXmlText(formattedCnsXmlText));
			var notifier = new Customs.Business.EmailSender(new LoggingInformation());
			notifier.SendNotification(
												email,
												notificationRegistryItemPk,
												notificationRegistryItem,
												receivedMessage.Factory
											);
			ServiceLogger.Log(LogType.Error, getDescription);
		}

		AttachmentDef CreateMailAttachmentFromCnsXmlText(string cnsText)
		{
			AttachmentDef attachment = new AttachmentDef("Invalid CnsXml " + ZDateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".xml", StringToBytes(cnsText));
			return attachment;
		}

		byte[] StringToBytes(string input)
		{
			ASCIIEncoding encoding = new ASCIIEncoding();
			return encoding.GetBytes(input);
		}

		void SaveCnsXmlToEDocs(CusEntryHeader cusEntryHeader, CNScargoStatus cnsXmlMessage)
		{
			DocManagerInfo docManagerInfo = ((IDocManagerSupport)cusEntryHeader.Declaration).DocManagerInfo;
			string fileName = "Clearance advice " + CargoWise.IO.MakeFilenameSafe.MakeSafe(cusEntryHeader.EntryNumber) + ".CnsXml.xml";
			var printFile = docManagerInfo.AddFileOrDocument(ZBlob.FromAscii(cnsXmlMessage.OriginalXml), fileName, Core.Constants.RefDocTypes.ClearanceAdvice, false);
			printFile.Description = "Clearance advice (XML) from CNS";
			if (EDocsSaver == null)
			{
				throw new InvalidOperationException("You must set an EDocsSaver before trying to save to eDocs");
			}
			EDocsSaver.QueueForSaving(docManagerInfo);
		}

		protected override string ApplicationCodeCore => CnsCompassServiceTask.Code;

		protected override string MessageFriendlyNameCore
		{
			get { return CnsCompassServiceTask.FriendlyName; }
		}

		protected override ZQuery MessageFilterCore
		{
			get
			{
				var filter = base.MessageFilterCore;
				filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, SubCodeCore);
				return filter;
			}
		}

		public static string XmlSubCode = "CNS";

		protected virtual string SubCodeCore { get { return XmlSubCode; } }

		public bool ThisIsTheFirstTimeWeHaveTriedThisMessage(EDIMessage receivedEdiMessage)
		{
			return receivedEdiMessage.EM_SystemLastEditUser.IsEmpty || receivedEdiMessage.EM_SystemLastEditUser == User.ServiceUserCode;
		}
	}
}
