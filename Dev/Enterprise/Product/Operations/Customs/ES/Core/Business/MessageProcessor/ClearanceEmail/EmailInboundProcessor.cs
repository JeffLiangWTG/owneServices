using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Registry;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;

[assembly: MailSubscriber(typeof(Enterprise.Customs.ES.Business.EmailInboundProcessor))]
namespace Enterprise.Customs.ES.Business;

public class EmailInboundProcessor : NewBaseInterchangeRetriever
{
	public EmailInboundProcessor(ILogger serviceLogger)
	{
		ServiceLogger = serviceLogger;
	}
	ILogger ServiceLogger { get; set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	class SubjectInitialText
	{
		public const string Import = "Despacho DUA IMP:";
		public const string Ncts = "Despacho del tr";
		public const string T2L = "Despacho T2L:";
		public const string T2CPOUS = "Despacho JEC:";
		public const string Export = "Levante AES EXP:";
		public const string G5 = "Despacho de G5";
	}

	protected override ZString GetInterchangeText(MailItem item, bool decryptInNewThread)
	{
		var emailBody = item.IsMIMEEmail ? (ZString)item.GetMimeMessageBody() : item.MI_Body;
		return emailBody.Trim();
	}

	protected override EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
	{
		if (!string.IsNullOrEmpty(interchangeString))
		{
			var message = factory.New<ESEDIMessage>();
			var result = GetMessageTypeAndMrnFromSubject(mailItem.MI_Subject);
			message.EM_MessageType = result.messageType;
			message.EM_MessageText = interchangeString;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationReference = result.mrnCode;
			message.EM_Status = EDIMessage.Status.Queued;

			ServiceLogger.Log(LogType.Information, string.Format("Created EDIMessage from MailDBItem for EsCustomsEmail message {0}", interchangeString));
		}
		else
		{
			ServiceLogger.Log(LogType.Warning, string.Format("Received empty or invalid EsCustomsEmail message {0}", interchangeString));
		}

		return null;
	}

	protected override IMailFilter GetMailFilter() => CreateMailFilter();

	[MailFilter(MailFilterCodes.ESImportMailTask)]
	public static IMailFilter CreateMailFilter()
		=> new QueryMailFilter(MailFilterCodes.ESImportMailTask,
			statuses: new string[] { MailStatus.Queued },
			fromComparison: SQLComparisonOperator.Contains, from: new string[] { ESCustomsDataRegistry.Instance.CustomsClearanceEmailFrom.Value },
			subjectComparison: SQLComparisonOperator.StartsWith, subjects: new string[] {
																				SubjectInitialText.Import,
																				SubjectInitialText.Ncts,
																				SubjectInitialText.T2L,
																				SubjectInitialText.T2CPOUS,
																				SubjectInitialText.Export,
																				SubjectInitialText.G5,
																			},
			alsoApplyQuery: true);

	protected (ZString messageType, ZString mrnCode) GetMessageTypeAndMrnFromSubject(ZString subject)
	{
		ZString GetMrnCode(string startIndexText)
		{
			return subject.SubstringSafe(startIndexText.Length).Replace(" ", "");
		}

			ZString mrnCode = string.Empty;
			ZString messageType = string.Empty;

		if (subject.StartsWith(SubjectInitialText.Import, StringComparison.OrdinalIgnoreCase))
		{
			mrnCode = GetMrnCode(SubjectInitialText.Import).Split("-")[0];
			messageType = DeclarationMessageTypeList.Codes.ImportClearanceEmail;
		}
		else if (subject.StartsWith(SubjectInitialText.Ncts, StringComparison.OrdinalIgnoreCase))
		{
			mrnCode = GetMrnCode(SubjectInitialText.Ncts).Split(":")[1];
			messageType = DeclarationMessageTypeList.Codes.NctsClearanceEmail;
		}
		else if (subject.StartsWith(SubjectInitialText.T2L, StringComparison.OrdinalIgnoreCase))
		{
			mrnCode = GetMrnCode(SubjectInitialText.T2L);

			if (mrnCode.Contains(T2LExpeditionCharacter))
			{
				messageType = DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail;
			}
			else if (mrnCode.Contains(T2LClearanceCharacter))
			{
				messageType = DeclarationMessageTypeList.Codes.T2cClearanceEmail;
			}
		}
		else if (subject.StartsWith(SubjectInitialText.T2CPOUS, StringComparison.OrdinalIgnoreCase))
		{
			mrnCode = GetMrnCode(SubjectInitialText.T2CPOUS);
			messageType = DeclarationMessageTypeList.Codes.T2cClearanceEmail;
		}
		else if (subject.StartsWith(SubjectInitialText.Export, StringComparison.OrdinalIgnoreCase))
		{
			mrnCode = GetMrnCode(SubjectInitialText.Export).Replace(CanaryIslandsExportIdentifier, "");
			messageType = DeclarationMessageTypeList.Codes.ExportClearanceEmail;
		}
		else if (subject.StartsWith(SubjectInitialText.G5, StringComparison.OrdinalIgnoreCase))
		{
			mrnCode = GetMrnCode(SubjectInitialText.G5).Split(G5MRNIdentifier)[1];
			messageType = DeclarationMessageTypeList.Codes.G5ClearanceEmail;
		}

		return (messageType, mrnCode);
	}
	const string T2LExpeditionCharacter = "L";
	const string T2LClearanceCharacter = "M";
	const string CanaryIslandsExportIdentifier = "VEXCAN";
	const string G5MRNIdentifier = "MRN";
}
