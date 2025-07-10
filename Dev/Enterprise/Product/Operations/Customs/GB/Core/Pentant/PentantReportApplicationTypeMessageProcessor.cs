using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Pentant
{
	internal class PentantReportApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		public PentantReportApplicationTypeMessageProcessor(ILogger serviceLogger, IEDocsDelayedSaver eDocsSaver)
			: base(new LoggingInformation())
		{
			this.ServiceLogger = serviceLogger;
			EDocsSaver = eDocsSaver;
		}

		protected override void ProcessMessageCore(EDIMessage ediMessage)
		{
			factory = ediMessage.Factory;
			if (ediMessage.EM_MessageText.StartsWith("E0EDI~", System.StringComparison.Ordinal))
			{
				ProcessE0(ediMessage);
			}
			else if (Regex.IsMatch(ediMessage.EM_MessageText, @"Pentant[[]PNT\d+[]]  Clearance Advice([^&]*)"))
			{
				ProcessCustomsClearanceReport(ediMessage);
			}
			else if (Regex.IsMatch(ediMessage.EM_MessageText, @"Pentant[[]PNT\d+[]]  Vehicle Clearance Advice([^&]*)"))
			{
				ProcessVehicleClearanceReport(ediMessage);
			}
			else if (ediMessage.EM_MessageText.StartsWith(CargoReportMessage.BeginMessage, StringComparison.OrdinalIgnoreCase))
			{
				new PentantCargoReportMesageProcessor(ServiceLogger, ediMessage).Process();
			}
			else if (Regex.IsMatch(ediMessage.EM_MessageText, @"Pentant[[]PNT\d+[]*E0]") && ediMessage.EM_MessageText.Contains("E0"))
			{
				ProcessE0Preformatted(ediMessage);
			}
			else
			{
				ProcessUnknownReport(ediMessage);
			}

			if (ediMessage.EM_Status == "QUE")
			{
				ediMessage.EM_Status = "FAL";

				if (ediMessage.EM_ReceiveTransmit == EDIMessage.Direction.Receive && ediMessage.Interchange != null)
				{
					ediMessage.Interchange.EI_Status = "FAL";
				}
			}
		}

		void ProcessE0(EDIMessage ediMessage)
		{
			//E0EDI~ZFSA99211112860~A1~010~E0~TNT~IM1802009-SCA-MAN-BLK~20~DOV~FSA~NNNN
			var lines = Regex.Split(ediMessage.EM_MessageText, System.Environment.NewLine);
			if (lines.Length > 1)
			{
				var elements = Regex.Split(lines[0], PentantConstants.SeparatorChar.ToString());
				if (elements.Length > 3)
				{
					var mucr = elements[1];
					var ics = elements[2];
					var irc = elements[3];
					var entry = FindEntryFromMucr(mucr, ediMessage);
					if (entry != null)
					{
						entry.Messages.Add(ediMessage);
						entry.CH_ImportClearanceStatusICS = ics;
						entry.CH_IrcInventoryReturnCode = irc;
						ediMessage.EM_MessageInterpretation = "<pre> " + ediMessage.EM_MessageText.Replace(lines[0], "") + " </pre>";
						UpdateMessageToUsefulValuesAndSendEmail(entry.Declaration, ediMessage, "E0");
						ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Added Pentant E0 report from message ID {0} to entry {1}, declaration {2}, using MUCR {3}", ediMessage.EM_MessageNum, entry.CH_BGMReference, entry.Declaration.JE_DeclarationReference, mucr));
					}
					else
					{
						SendEmailForProblem(ediMessage, "E0");
					}
				}
			}
		}

		void ProcessE0Preformatted(EDIMessage ediMessage)
		{
			var mucr = GetFirstGroupMatchFromMessageText(ediMessage, @"Customs Ref.*?\s+([A-Za-z0-9][^\r]*)");
			var entryDetails = Regex.Match(ediMessage.EM_MessageText, @"Entry Details.*?([0-9]{3}-[0-9A-Z]{7})-(([0-9]{2}\/[0-9]{2}\/[0-9]{4})|([0-9]{4}-[0-9]{2}-[0-9]{2}))");
			var acaReference = GetFirstGroupMatchFromMessageText(ediMessage, @"ACA Reference.*?\s+([A-Za-z0-9]([A-Za-z0-9][^\r]*))");
			var irc = GetFirstGroupMatchFromMessageText(ediMessage, @"IRC=(\d\d\d)?");
			var referenceType = "Entry number and date";
			var referenceValue = entryDetails.Value;

			var entry = FindEntryFromEntryNumberAndDate(entryDetails, ediMessage);

			if (entry == null)
			{
				entry = FindEntryFromMucr(mucr, ediMessage);
				referenceType = "MUCR";
				referenceValue = mucr;
			}
			if (entry == null)
			{
				entry = FindEntryFromACA(acaReference, ediMessage);
				referenceType = "ACA reference";
				referenceValue = acaReference;
			}

			if (entry != null)
			{
				entry.Messages.Add(ediMessage);
				entry.CH_IrcInventoryReturnCode = irc;
				ediMessage.EM_MessageInterpretation = "<pre> " + ediMessage.EM_MessageText + " </pre>";
				UpdateMessageToUsefulValuesAndSendEmail(entry.Declaration, ediMessage, "E0");
				ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Added Pentant E0 report from message ID {0} to entry {1}, declaration {2}, using {3} {4}", ediMessage.EM_MessageNum, entry.CH_BGMReference, entry.Declaration.JE_DeclarationReference, referenceType, referenceValue));
			}
			else
			{
				SendEmailForProblem(ediMessage, "E0");
			}
		}

		protected ZString GetFirstGroupMatchFromMessageText(EDIMessage ediMessage, string pattern)
		{
			var result = "";
			var regex = new Regex(pattern);
			var match = regex.Match(ediMessage.EM_MessageText);
			if (match.Success)
			{
				result = match.Groups[1].Value;
			}
			return result;
		}

		void ProcessCustomsClearanceReport(EDIMessage ediMessage)
		{
			var entryNumberAndDate = Regex.Match(ediMessage.EM_MessageText, @"Entry Details.*?([0-9]{3}-[0-9A-Z]{7})-(([0-9]{2}\/[0-9]{2}\/[0-9]{4})|([0-9]{4}-[0-9]{2}-[0-9]{2}))");
			if (entryNumberAndDate != null && entryNumberAndDate.Success)
			{
				var entryNumber = entryNumberAndDate.Groups[1].Value;
				var entryDate = entryNumberAndDate.Groups[2].Value;
				ZDateTime entryDateTime;
				if (ZDateTime.TryParseExact(entryDate, out entryDateTime, "yyyy-MM-dd") || ZDateTime.TryParseExact(entryDate, out entryDateTime, "dd/MM/yyyy"))
				{
					var entries = GbExtensionHelpers.GetCusEntryHeaderFromCusEntryNumberAndDate(entryNumber, ediMessage.Factory, entryDateTime);
					if (entries != null && entries.Length > 0)
					{
						var matchForEntryDate = Regex.Match(ediMessage.EM_MessageText, @"Clearance Advice\s+?([0-9]{2}/[0-9]{2}/[0-9]{4}\s+?[0-9]{2}:[0-9]{2})");
						if (matchForEntryDate != null && matchForEntryDate.Success)
						{
							var dateTime = matchForEntryDate.Groups[1].Value;
							ZDateTime entryClearanceDate;
							if (ZDateTime.TryParseExact(dateTime, out entryClearanceDate, "dd/MM/yyyy HH:mm"))
							{
								GbExtensionHelpers.UpdateEntryHeaderToCleared(entries[0], entryClearanceDate);
							}
							entries[0].Declaration.DocManagerInfo.AddFileOrDocument(System.Text.Encoding.Default.GetBytes(ediMessage.EM_MessageText), "Clearance Advice from Pentant.txt", "PUB");
							EDocsSaver.QueueForSaving(entries[0].Declaration.DocManagerInfo);
						}
						entries[0].Messages.Add(ediMessage);
						ediMessage.EM_MessageInterpretation = "<pre> " + ediMessage.EM_MessageText + "</pre>";
						UpdateMessageToUsefulValuesAndSendEmail(entries[0].Declaration, ediMessage, "CC");
					}
					else if (entries != null && entries.Length == 0)
					{
						ProcessUnknownReport(ediMessage, "CC");
					}
				}
			}
		}

		void ProcessVehicleClearanceReport(EDIMessage ediMessage)
		{
			var acasFound = new List<string>();
			var hyphens = "----------------------------";
			var textAfterHyphens = ediMessage.EM_MessageText.Substring(ediMessage.EM_MessageText.LastIndexOf(hyphens, StringComparison.OrdinalIgnoreCase) + hyphens.Length);

			var acaReferences = Regex.Matches(textAfterHyphens, @"\r\n\s+?[0-9]+?\.\s+?([A-Z0-9]{9})\s+?");
			if (acaReferences != null && acaReferences.Count > 0)
			{
				foreach (Match match in acaReferences)
				{
					var aca = match.Groups[1].Value;
					if (!string.IsNullOrEmpty(aca))
					{
						acasFound.Add(aca);
					}
				}
			}

			JobDeclaration decForMessage = null;
			foreach (var declaration in FindDeclarationsWithAcas(acasFound))
			{
				if (decForMessage == null)
				{
					decForMessage = declaration;
				}
				declaration.DocManagerInfo.AddFileOrDocument(System.Text.Encoding.Default.GetBytes(ediMessage.EM_MessageText), "Vehicle Clearance Advice from Pentant.txt", "PUB");
				EDocsSaver.QueueForSaving(declaration.DocManagerInfo);
			}
			if (decForMessage != null)
			{
				decForMessage.CustomsEntryHeaders[0]?.Messages.Add(ediMessage);
				UpdateMessageToUsefulValuesAndSendEmail(decForMessage, ediMessage, "VC");
			}
			else
			{
				ProcessUnknownReport(ediMessage, "VC");
			}
		}

		IEnumerable<JobDeclaration> FindDeclarationsWithAcas(List<string> acasFound)
		{
			var dbOnlySubQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			var query = new ZQuery(CusEntryNumSchema.CE_Category, "OTH");
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, "JobDeclaration");
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, "ACA");
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, "GB");
			query.AddToFilter(CusEntryNumSchema.CE_EntryNum, acasFound.ToArray());
			subQuery.AddToFilter(query);
			dbOnlySubQuery.AddSubQuery(subQuery, JoinCondition.And);
			dbOnlySubQuery.OrderBy = JobDeclarationSchema.JE_DeclarationReference.Name;
			return factory.Load<Customs.Business.BaseJobDeclaration>(dbOnlySubQuery).OfType<JobDeclaration>();
		}

		void ProcessUnknownReport(EDIMessage ediMessage, string reportCode = "unknown")
		{
			ediMessage.EM_MessageType = ApplicationCodeList.Codes.Pentant;
			ediMessage.EM_MessageSubType = "UNK";
			ediMessage.EM_Status = EDIMessage.Status.Received;
			if (ediMessage.Interchange != null)
			{
				ediMessage.Interchange.EI_Status = EDIMessage.Status.Received;
			}
			SendEmailForProblem(ediMessage, reportCode);
		}

		internal static void SendEmailForProblem(EDIMessage ediMessage, string reportType, string extraExplanationInfo = "")
		{
			var email = new HtmlNotificationEmailSender().CreateEmail(string.Format(CultureInfo.InvariantCulture, "Pentant report - report type {0}, no job was updated. " + extraExplanationInfo, reportType), ediMessage.EM_MessageText, GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), null);
			var emailSender = new Customs.Business.EmailSender(new LoggingInformation());
			emailSender.SendNotification(email,
						GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefPrints, "", Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty),
						GBCustomsDataRegistry.Instance.GetNotificationItem("", "", GBCustomsDataRegistry.Instance.NotificationChief),
						ediMessage.Factory);
		}

		CusEntryHeader FindEntryFromMucr(string mucr, EDIMessage ediMessage)
		{
			var pentantReport = new PentantReport(mucr);
			return pentantReport.GetEntryHeaderFromUcnUsingMucr(ediMessage);
		}

		CusEntryHeader FindEntryFromEntryNumberAndDate(Match entryNumberAndDate, EDIMessage ediMessage)
		{
			if (entryNumberAndDate != null && entryNumberAndDate.Success)
			{
				var entryNumber = entryNumberAndDate.Groups[1].Value;
				var entryDate = entryNumberAndDate.Groups[2].Value;
				ZDateTime entryDateTime;
				var dateParsed = ZDateTime.TryParseExact(entryDate, out entryDateTime, "yyyy-MM-dd");
				if (!dateParsed)
				{
					dateParsed = ZDateTime.TryParseExact(entryDate, out entryDateTime, "dd/MM/yyyy");
				}

				if (dateParsed)
				{
					var entryHeaders = GbExtensionHelpers.GetCusEntryHeaderFromCusEntryNumberAndDate(entryNumber, ediMessage.Factory, entryDateTime);
					if (entryHeaders.Length == 1)
					{
						return entryHeaders[0];
					}
				}
			}
			return null;
		}

		CusEntryHeader FindEntryFromACA(string acaReference, EDIMessage ediMessage)
		{
			CusEntryHeader header = null;
			var declarations = FindDeclarationsWithAcas(new List<string> { acaReference }).ToList();
			if (declarations.Count == 1)
			{
				if (declarations[0].CustomsEntryHeaders.Count == 1)
				{
					header = declarations[0].CustomsEntryHeaders[0];
				}
			}

			return header;
		}

		void UpdateMessageToUsefulValuesAndSendEmail(JobDeclaration declaration, EDIMessage ediMessage, string reportCode)
		{
			ediMessage.EM_MessageType = ApplicationCodeList.Codes.Pentant;
			ediMessage.EM_MessageSubType = reportCode;
			ediMessage.EM_Status = EDIMessage.Status.Received;
			if (ediMessage.Interchange != null)
			{
				ediMessage.Interchange.EI_Status = EDIMessage.Status.Received;
			}
			ediMessage.EM_GB = declaration.JE_GB;
			SendSuccessEmail(declaration, reportCode);
		}

		internal static void SendSuccessEmail(JobDeclaration declaration, string reportCode)
		{
			var hyperlinkedText = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.JobDeclaration, declaration.PK.ToGuid());
			var subject = string.Format(CultureInfo.InvariantCulture, "Pentant status update for UCN {0}, declaration {1}, report type {2}", declaration.JE_MasterUCR.IsEmpty ? (ZString)"(blank)" : declaration.JE_MasterUCR, declaration.JE_DeclarationReference, reportCode);
			var body = string.Format(CultureInfo.InvariantCulture, @"<p>A status update message was received from Pentant.</p>
										<p>Subject to correcting printing and eDocs options, a full version is available on paper and against the declaration's eDocs tab.</p>
										<p>To open the job, click here: {0}</p>",
										hyperlinkedText);
			var email = new HtmlNotificationEmailSender().CreateEmail(subject, body, declaration.CompanyPK.ToGuid(), declaration.RegistryBranchPK, null);
			var user = declaration.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, declaration.JE_GS_NKCusAgent));
			if (user == null && declaration.CustomsEntryHeaders.Count > 0 && declaration.CustomsEntryHeaders[0].Messages.LastOutgoingNonSystemAndNonNullUserMessage != null)
			{
				user = declaration.CustomsEntryHeaders[0].Messages.LastOutgoingNonSystemAndNonNullUserMessage.UserWhoQueuedThisRecord;
			}

			var emailSender = new Customs.Business.EmailSender(new LoggingInformation());
			if (user != null && !user.GS_EmailAddress.IsEmpty)
			{
				emailSender.SendNotification(
					email,
					user,
					GBCustomsDataRegistry.Instance.CustomsResponseNotifications,
					GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefPrints, reportCode, Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty),
					GBCustomsDataRegistry.Instance.GetNotificationItem(reportCode, "", GBCustomsDataRegistry.Instance.NotificationChief),
					declaration.Factory);
			}
			else
			{   // No user, e.g. outbound message not sent; or user has no email address
				emailSender.SendNotification(email,
					GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefPrints, reportCode, Guid.Empty, declaration.JE_GB.ToGuid(), Guid.Empty),
					GBCustomsDataRegistry.Instance.GetNotificationItem(reportCode, "", GBCustomsDataRegistry.Instance.NotificationChief),
					declaration.Factory);
			}
		}

		BusinessObjectFactory factory;
		ILogger ServiceLogger { get; set; }

		protected override string MessageFriendlyNameCore => ApplicationCodeList.Descriptions.Pentant;

		protected override string ApplicationCodeCore => ApplicationCodeList.Codes.Pentant;

		protected readonly IEDocsDelayedSaver EDocsSaver;

		class PentantReport : IUcnProvider
		{
			public PentantReport(string mucr)
			{
				this.mucr = mucr;
			}

			public ZString UcnNumberProperlyTruncated => mucr;
			public ZString UcnNumberVerbatim => mucr;

			readonly string mucr;
		}
	}
}
