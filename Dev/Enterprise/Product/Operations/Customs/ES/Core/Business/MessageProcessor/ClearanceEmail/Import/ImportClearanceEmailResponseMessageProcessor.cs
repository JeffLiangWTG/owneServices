using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class ImportClearanceEmailResponseMessageProcessor : ESResponseMessageProcessor<IImportClearanceEmailProvider>
	{
		public ImportClearanceEmailResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		const int CSVDataLength = 16;
		const string CSVClearanceString = "CSVLevante";
		const string CSVImportCertificateString = "CSVCertificadodeImport";
		const string SeparatorString = ":";

		protected override string MessageFriendlyNameCore => (NoResString)"Import Clearance Email Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ImportClearanceEmail };
		protected override ZBool ShouldHaveSentInterchange => false;

		protected override IImportClearanceEmailProvider GetMessageProviderCore(EDIMessage message)
		{
			var messageText = message.EM_MessageText.Replace(" ", "");

			var csvClearance = GetCSVSpecificData(messageText, CSVClearanceString);
			if (csvClearance.IsEmpty)
			{
				throw new InvalidOperationException(Res.GetString("17F34082-C246-4DF6-84CD-F23369F8BD9F", "Email Body doesn't have the correct data"));
			}

			var csvImportCertificate = GetCSVSpecificData(messageText, CSVImportCertificateString);

			return new ImportClearanceEmailObject(csvClearance, csvImportCertificate);
		}

		ZString GetCSVSpecificData(ZString fullText, ZString initialLineText)
		{
			if (fullText.Contains(initialLineText))
			{
				int startIndex = fullText.IndexOf(initialLineText, StringComparison.OrdinalIgnoreCase) + initialLineText.Length;
				ZString startText = fullText.SubstringSafe(startIndex);
				int separatorIndex = startText.IndexOf(SeparatorString, StringComparison.OrdinalIgnoreCase) + SeparatorString.Length;
				return startText.SubstringSafe(separatorIndex, CSVDataLength);
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
			=> MessageProcessorHelper.GetRelevantBusinessObjectForEmailResponse<CusEntryHeader>(message, CusEntryNumberTypes.Standard.MovementReferenceNumber);

		protected override void ProcessMessageCore(EDIMessage message, CusEntryHeader linkedBusinessObject, IImportClearanceEmailProvider provider)
		{
			linkedBusinessObject.SetCSVClearanceNum(provider.CSVClearance);
			linkedBusinessObject.ZG_CSVImportCertificate = provider.CSVImportCertificate;

			var entryInstructionIsSubStyleBOrCOrZ = linkedBusinessObject.EntryInstruction?.IsSubStyleBOrCOrZ ?? ZBool.False;
			linkedBusinessObject.CH_EntryStatus = entryInstructionIsSubStyleBOrCOrZ ? MessageProcessorConstants.EntryStatusCodes.ClearedWithPendingComplementaryDeclarations : MessageProcessorConstants.EntryStatusCodes.Cleared;

			TriggerIQUMessage(linkedBusinessObject);

			SetMessageStatusAsReceived(message);
			SetMessageSubTypeAsAccepted(message);
		}

		void TriggerIQUMessage(CusEntryHeader entryHeader)
		{
			var declaration = entryHeader.Declaration;
			var broker = declaration.CusAgent;
			var brokerCertificate = declaration.JE_CustomsProfile;
			var certificateError = false;
			if (broker != null)
			{
				var certificate = CertificateHelper.GetCertificate(broker, brokerCertificate);
				if (certificate != null)
				{
					var messages = new List<ESEDIMessage>();
					try
					{
						var certificateObject = new CertificateObject(broker, certificate.GP_Name, certificate.GP_UserID);

						var builderManager = new ESMessageBuilderManager(DeclarationMessageTypeList.Codes.ImportQuery, DeclarationMessageSubTypeList.Codes.OriginalDeclaration, entryHeader, certificateObject);

						var messageBuildersData = ESMessageSender.GetImportQueryMessageBuilders(entryHeader, builderManager);
						ESMessageSender.Send(messageBuildersData, messages);

						if (messages.Any())
						{
							entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Logger.LogWarning(Res.GetString("B8308C44-9472-42D5-AE4F-21EEB7D4BC44", "Error when creating the Import Query message in entry") + " " + entryHeader.CH_BGMReference);
					}
				}
				else
				{
					certificateError = true;
				}
			}
			else
			{
				certificateError = true;
			}

			if (certificateError)
			{
				Logger.LogWarning(Res.GetString("9C5BA3AB-D9CB-41B9-8160-E881F6112403", "Can't trigger Import Query message, wrong broker or certificate in entry") + " " + entryHeader.CH_BGMReference);
			}
		}
	}
}
