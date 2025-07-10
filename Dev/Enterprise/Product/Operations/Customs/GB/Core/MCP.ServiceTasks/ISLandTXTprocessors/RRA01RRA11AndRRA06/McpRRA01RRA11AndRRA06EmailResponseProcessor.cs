namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using CargoWise.Application;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.Business.MultiLineAddInfos;
	using Enterprise.Customs.GB.Business.Declaration;
	using Enterprise.Customs.GB.Registry;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Modules;

	class McpRRA01RRA11AndRRA06EmailResponseProcessor
	{
		public readonly LoggingInformation Logger;

		const string EmailSubjectTemplatePart1 = "Release/removal advice for ";
		const string EmailSubjectTemplatePart2 = " [JOB] / [UCN] / [CONT] / [BOL]";
		string simpleHtmlBody;

		public McpRRA01RRA11AndRRA06EmailResponseProcessor(MatchOrHoldAction holdAction, LoggingInformation logger)
		{
			this.matchOrHoldAction = holdAction;
			Logger = logger;
		}

		public string SimpleHtmlBody
		{
			get { return simpleHtmlBody; }
		}

		public EmailDef SendEmailResponse(CusEntryHeader cusEntryHeader, RRA01AndRRA11Message rraMessage, BusinessObjectFactory factory)
		{
			EmailDef email = GenerateEmail(cusEntryHeader, rraMessage, factory); // Will already warn postmaster
			GlbStaff user;
			SetUserForThisJob(cusEntryHeader, Logger, out user);
			if (cusEntryHeader != null)
			{
				QueueEmail(factory, email, user, Logger, rraMessage.MessageType, cusEntryHeader.RegistryBranchPK);
			}
			return email;
		}

		public static void QueueEmail(BusinessObjectFactory factory, EmailDef email, GlbStaff user, LoggingInformation logger, ZString notificationCode, Guid branchGuid)
		{
			Customs.Business.EmailSender emailSender = new Customs.Business.EmailSender(logger);

			var guid = GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationMcp, notificationCode, Guid.Empty, branchGuid, Guid.Empty);
			var notificationItem = GBCustomsDataRegistry.Instance.GetNotificationItem(notificationCode, "", GBCustomsDataRegistry.Instance.NotificationMcp);

			logger.DebugLog($"Notification Code: {notificationCode} Item: {notificationItem.Name} Response Notifications: {GBCustomsDataRegistry.Instance.CustomsResponseNotifications}");

			if (user != null && !user.GS_EmailAddress.IsEmpty)
			{
				emailSender.SendNotification(
					email,
					user,
					GBCustomsDataRegistry.Instance.CustomsResponseNotifications,
					guid,
					notificationItem,
					factory);

				logger.DebugLog($"Sending notification to user email: {user.GS_EmailAddress}");
			}
			else
			{   // No user, e.g. outbound message not sent; or user has no email address
				emailSender.SendNotification(email, guid, notificationItem, factory);

				logger.DebugLog($"Email sent to Mcp notification group");
			}
		}

		internal static void SetUserForThisJob(CusEntryHeader cusEntryHeader, LoggingInformation logger, out GlbStaff user)
		{
			user = null;
			if (cusEntryHeader != null)
			{
				if (cusEntryHeader.Messages.LastOutgoingNonSystemAndNonNullUserMessage != null)
				{
					user = cusEntryHeader.Messages.LastOutgoingNonSystemAndNonNullUserMessage.UserWhoQueuedThisRecord;
				}
			}

			logger.DebugLog(user == null ?
				"No user for job found" :
				$"User for job set to: {user.GS_Code}");
		}

		public EmailDef GenerateEmail(CusEntryHeader cusEntryHeader, RRA01AndRRA11Message rraMessage, BusinessObjectFactory factory)
		{
			ZStringBuilder htmlBody = new ZStringBuilder();
			string shipmentDetailsFromTemplateWithRealValues = string.Empty;
			string hyperlinkedText = "(Unknown job)";
			HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
			EmailDef userEmailDef = null;

			if (cusEntryHeader != null)
			{
				shipmentDetailsFromTemplateWithRealValues = ReplaceTemplateWithRRAvalue(EmailSubjectTemplatePart2, rraMessage, cusEntryHeader.Declaration); // will look like this:  Release/Removal Advice  for 12345678 / ABCU123456 / HKabc123
				string uri = GetHyperlinkUriForDeclaration(cusEntryHeader.Declaration);
				hyperlinkedText = MakeHtmlAnchor(uri, shipmentDetailsFromTemplateWithRealValues);  // will look like this: Release/Removal Advice for <a href="something">12345678 / ABCU123456 / HKabc123</a>
				AppendRRAvaluesToHtmlBody(rraMessage, htmlBody, cusEntryHeader.Declaration);
				simpleHtmlBody = string.Format(CultureInfo.CurrentCulture, "<h3>{0}</h3> {1} {2} ", string.Format(CultureInfo.CurrentCulture, Title, rraMessage.MessageType), htmlBody, GetMatchOrHoldActionExplained(cusEntryHeader));
				userEmailDef = emailSender.CreateEmail(EmailSubjectTemplatePart1 + shipmentDetailsFromTemplateWithRealValues, simpleHtmlBody, cusEntryHeader.RegistryCompanyPK, cusEntryHeader.RegistryBranchPK, null);
				Logger.DebugLog("Release/Removal Advice email generated");
			}
			else
			{
				htmlBody.Append("A response was received from Destin8 for an unknown entry. The " + rraMessage.MessageType + "'s details are: <pre>");
				htmlBody.Append(rraMessage.ToString());
				htmlBody.Append("</pre>Since no related entry could be found, no job has been updated.");
				simpleHtmlBody = htmlBody.ToStringWithNewLineBetweenAppends();
				shipmentDetailsFromTemplateWithRealValues = hyperlinkedText;
				userEmailDef = emailSender.CreateEmail(EmailSubjectTemplatePart1 + shipmentDetailsFromTemplateWithRealValues, simpleHtmlBody);
				Logger.DebugLog("Unknown entry email generated");
			}

			htmlBody.Prepend("A response message has been received from Destin8.<br />");
			htmlBody.Prepend("<br /><strong>" + EmailSubjectTemplatePart1 + hyperlinkedText + "</strong><br /><br />");

			if (cusEntryHeader == null)
			{   // Send a copy of the email to the postmasters group
				EmailDef postmasterEmailDef = new EmailDef();
				postmasterEmailDef.Body = userEmailDef.Body;
				postmasterEmailDef.Subject = userEmailDef.Subject;
				Env.OutgoingCustomsMailManager.CreateAndSaveToPostmasterGroup(postmasterEmailDef, factory);
				Logger.DebugLog("Email sent to PostMasterGroup");
			}
			return userEmailDef;
		}

		ZString GetMatchOrHoldActionExplained(CusEntryHeader entry)
		{
			switch (matchOrHoldAction)
			{
				case MatchOrHoldAction.HoldsStillExist:
					return "<h5>Holds still exist for the following UCNs</h5> <p>" + ListRemaingHeldUcns(entry) + "</p>";
				case MatchOrHoldAction.MatchedByMucr:
					return "<b>This RRA matched the entry by MUCR</b>";
				case MatchOrHoldAction.LastHoldRemoved:
					return "<b>There are no remaining holds on this entry</b>";
				case MatchOrHoldAction.MatchedByEntryNumber:
					return "<b>This RRA matched the entry by CHIEF entry number</b>";
				case MatchOrHoldAction.MatchedByCdsEntryNumber:
					return "<b>This RRA matched the entry by CDS entry number</b>";
				default:
					return "";
			}
		}

		string ListRemaingHeldUcns(CusEntryHeader entry)
		{
			var sb = new ZStringBuilder();
			foreach (CusAddInfo<MaritimeUcnThatIsHeld> ucn in entry.MaritimeUcnsThatAreHeld)
			{
				sb.Append(string.Format(CultureInfo.CurrentCulture, "{0} ({1})", ucn.Data.NW_UCN, ucn.Data.NW_HoldTypeComments));
			}
			return sb.ToStringWithDelimiterBetweenAppends("<br>\r\n");
		}

		public static string GetHyperlinkUriForDeclaration(JobDeclaration declaration)
		{
			return ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.JobDeclaration, declaration.PK.ToGuid());
		}

		public static string MakeHtmlAnchor(string uri, ZString linkText)
		{
			return "<a href=\"" + uri + "\">" + linkText + "</a>";
		}

		void AppendRRAvaluesToHtmlBody(RRA01AndRRA11Message rra, ZStringBuilder htmlBody, JobDeclaration declaration)
		{
			string htmlTemplate = GetEmailBodyFromHtmTemplate(rra, declaration);  // gets the text of an HTML table from an embedded resource stream and plugs-in the values from the RRA object.
			htmlBody.Append(htmlTemplate);
		}

		string GetEmailBodyFromHtmTemplate(RRA01AndRRA11Message rraMessage, JobDeclaration declaration)
		{
			string resourceName = "Enterprise.Customs.GB.MCP.ServiceTasks.ISLandTXTprocessors.RRA01RRA11AndRRA06.EmailTemplateToSendNotificationToUser.htm";
			string result = null;

			using (Stream newStream = GetType().Assembly.GetManifestResourceStream(resourceName))
			{
				byte[] emailBytes = new byte[newStream.Length];
				int readLength = newStream.Read(emailBytes, 0, emailBytes.Length);

				if (newStream.Length != readLength)
				{
					throw new InvalidOperationException("Error reading email template.");
				}

				result = Encoding.ASCII.GetString(emailBytes);
			}

			return ReplaceTemplateWithRRAvalue(result, rraMessage, declaration);
		}

		string ReplaceTemplateWithRRAvalue(string htmlTemplate, RRA01AndRRA11Message rraMessage, JobDeclaration declaration)
		{
			htmlTemplate = htmlTemplate.Replace("[BOL]", rraMessage.BlNumber);
			htmlTemplate = htmlTemplate.Replace("[JOB]", declaration.JE_DeclarationReference);
			htmlTemplate = htmlTemplate.Replace("[BERTH]", rraMessage.BerthCode);
			htmlTemplate = htmlTemplate.Replace("[CONT]", rraMessage.Container);
			htmlTemplate = htmlTemplate.Replace("[DATE]", rraMessage.DateOfStatusEvent.ToShortDateString() + " " + rraMessage.DateOfStatusEvent.ToShortTimeString());
			htmlTemplate = htmlTemplate.Replace("[MARKS]", rraMessage.MarksAndNumbers);
			htmlTemplate = htmlTemplate.Replace("[NOP]", rraMessage.Packages.ToString());
			htmlTemplate = htmlTemplate.Replace("[UCN]", rraMessage.UCN.UcnWithoutLclSuffix);
			htmlTemplate = htmlTemplate.Replace("[USER]", rraMessage.UserName);
			htmlTemplate = htmlTemplate.Replace("[PIECES]", rraMessage.Packages.ToString());
			htmlTemplate = htmlTemplate.Replace("[WEIGHT]", rraMessage.Weight.ToString());
			htmlTemplate = htmlTemplate.Replace("[ENTRY]", rraMessage.CHIEFEntryNumber.IsEmpty ? ZString.Empty : rraMessage.ChiefDetailsFormatted);
			htmlTemplate = htmlTemplate.Replace("[REMOVAL]", rraMessage.HasRemovalDetails ? rraMessage.RemovalDetailsFormatted : ZString.Empty);
			htmlTemplate = htmlTemplate.Replace("[HOLDS]", rraMessage.AreThereAnyHolds() ? rraMessage.HoldsString : ZString.Empty);

			return htmlTemplate;
		}

		public static string Title => "Release/removal advice ({0}) from Destin8";

		readonly MatchOrHoldAction matchOrHoldAction;
	}
}
