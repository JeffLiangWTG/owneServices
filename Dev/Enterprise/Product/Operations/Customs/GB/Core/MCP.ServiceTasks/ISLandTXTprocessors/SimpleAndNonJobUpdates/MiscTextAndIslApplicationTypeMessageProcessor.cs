namespace Enterprise.Customs.GB.MCP.ServiceTasks.Misc
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.GB.Business;
	using Enterprise.Customs.GB.Business.Declaration;
	using Enterprise.Customs.GB.Registry;
	using Enterprise.Integration;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.MessageProcessors;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	/// <summary>
	/// Takes one EdiMEssage string and chops it up into MiscTextAndIslMessage objects. ApplicationTypeMessageProcessor.
	/// </summary>
	public class MiscTextAndIslApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor, IUcnProvider
	{
		ILogger ServiceLogger { get; set; }

		public MiscTextAndIslApplicationTypeMessageProcessor(ILogger serviceLogger)
			: base(new LoggingInformation())
		{
			this.ServiceLogger = serviceLogger;
		}

		protected override void ProcessMessageCore(EDIMessage ediMessage)
		{
			incomingMessage = ediMessage;
			CusEntryHeader entryHeader = null;
			var resultStatus = EDIMessage.Status.Failed;
			if (!ediMessage.EM_ApplicationReference.IsEmpty)
			{
				entryHeader = this.GetEntryHeaderFromUcnUsingMucr(incomingMessage);
				if (entryHeader != null)
				{
					// Message pertains to job
					ZString preformattedHtml = SendSuccessEmailShowingPrettyMiscText(entryHeader, ediMessage.EM_MessageSubType);
					ServiceLogger.Log(LogType.Information, delegate
					{ return "Successfully processed MiscTextAndIsl for entry " + entryHeader.EntryNumber; });
					entryHeader.Messages.Add(incomingMessage);
					incomingMessage.EM_MessageInterpretation = preformattedHtml;
					incomingMessage.EM_MessageNum = ZDateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
					resultStatus = EDIMessage.Status.Received;
				}
			}
			if (ediMessage.EM_ApplicationReference.IsEmpty || entryHeader == null)
			{
				// Maybe a jobless message or the application reference did not give a match to a job, e.g. LUM - send to group
				Enterprise.Customs.Business.EmailSender emailSender = new Enterprise.Customs.Business.EmailSender(Logger);
				EmailDef email = new EmailDef();
				var title = GetTitleFromCode(ediMessage.EM_MessageSubType);
				email.Body = string.Format(CultureInfo.InvariantCulture, "<h3>" + title + " from MCP.</h3> <pre>{0}</pre>", incomingMessage.EM_MessageText);
				email.Subject = title + " from MCP";
				email.ContentType = EmailContentTypes.HTML;
				emailSender.SendNotification(email,
					GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationMcp, incomingMessage.EM_MessageSubTypeDescription, ediMessage.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty),
					GBCustomsDataRegistry.Instance.GetNotificationItem(incomingMessage.EM_MessageSubTypeDescription, "", GBCustomsDataRegistry.Instance.NotificationMcp),
												incomingMessage.Factory
												);
				resultStatus = EDIMessage.Status.Received;
			}
			incomingMessage.EM_Status = resultStatus;
			if (incomingMessage.Interchange != null)
			{
				incomingMessage.Interchange.EI_Status = EDIInterchange.Status.Received;
			}
		}

		ZString SendSuccessEmailShowingPrettyMiscText(CusEntryHeader entryHeader, string messageCode)
		{
			ZString declarationNumber = entryHeader.Declaration.JE_DeclarationReference;
			ZString entryNumber = entryHeader.EntryNumber;
			ZString trimmedString = incomingMessage.EM_MessageText;
			var title = GetTitleFromCode(messageCode);
			ZString preformattedHtml = "<h3>" + title + "</h3> " + ParseCauOrCsnMessage(messageCode, trimmedString) + "<pre>" + trimmedString + "</pre>";
			ZString subject = string.Format(CultureInfo.InvariantCulture, "{2} for  {0} / {1}", declarationNumber, entryNumber, title);
			RRA12.RRA12ApplicationTypeMessageProcessor.SendEmailToOriginatingUserShared(subject, preformattedHtml, incomingMessage, null, entryHeader, ServiceLogger, Logger);
			return preformattedHtml;
		}

		string ParseCauOrCsnMessage(string messageCode, string messageText)
		{
			var sb = new HtmlTableCreator(new[] { "Field", "Value" });
			var dictionary = new SortedDictionary<int, string>();
			dictionary.Add(1, "UCN");
			dictionary.Add(2, "New Nominated Agent");
			dictionary.Add(4, "Unit ID");
			dictionary.Add(5, "BOL");
			dictionary.Add(9, "Full BOL");
			if (messageCode.StartsWith("CSN", StringComparison.OrdinalIgnoreCase))
			{
				dictionary.Add(3, "Cargo Broker");
				dictionary.Add(10, "Amalgamated UCN");
			}
			else if (messageCode.StartsWith("CAU", StringComparison.OrdinalIgnoreCase))
			{
				dictionary.Add(3, "Old Nominated Agent");
			}

			try
			{
				var elements = Regex.Split(messageText, "~");
				foreach (var namedField in dictionary)
				{
					if (elements.Length >= namedField.Key)
					{
						sb.WriteRow(namedField.Value, elements[namedField.Key]);
					}
				}
				return sb.ToHtml();
			}
			catch (ArgumentException)
			{ }
			catch (IndexOutOfRangeException)
			{ }
			return "";
		}

		string GetTitleFromCode(string messageCode)
		{
			var titles = new Dictionary<string, string>();
			titles.Add("LUM", "Miscellaneous advice");
			titles.Add("CSN", "Self-nomination advice");
			titles.Add("CAU", "Re-nomination advice");
			var title = titles.ContainsKey(messageCode) ? titles[messageCode] : titles["LUM"];
			return title;
		}

		EDIMessage incomingMessage;

		protected override string ApplicationCodeCore
		{
			get { return Constants.ServiceTasksCode.MiscTextAndIslServiceTaskCode; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return MiscTextAndIslServiceTask.FriendlyName; }
		}

		#region IUcnProvider Members

		public ZString UcnNumberProperlyTruncated
		{
			get { return new UniqueConsignmentNumber(incomingMessage.EM_ApplicationReference).ProperlyTruncatedUCN; }
		}

		public ZString UcnNumberVerbatim
		{
			get { return new UniqueConsignmentNumber(incomingMessage.EM_ApplicationReference).Raw; }
		}

		#endregion
	}
}
