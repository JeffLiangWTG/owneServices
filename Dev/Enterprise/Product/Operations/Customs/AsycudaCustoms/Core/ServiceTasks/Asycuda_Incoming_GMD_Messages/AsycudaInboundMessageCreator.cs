using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks
{
	class AsycudaInboundMessageCreator : IInboundMessageCreator
	{
		public AsycudaInboundMessageCreator(ILogger serviceTaskLogger)
		{
			logger = serviceTaskLogger;
		}

		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var payload = interchange.EI_BodyText;

			if (payload.IsEmpty)
			{
				logger.Log(LogType.Error, "The body of the interchange is empty.");
				throw new AsycudaIncomingDataException(Res.GetString("CD5484A2-5AA7-45F6-93DC-9F027D8A445F", "The body of the interchange is empty."));
			}

			var bgmReference = AsycudaIncomingGMDMessageHelper.GetBGMReferenceFromXML(payload);

			if (string.IsNullOrEmpty(bgmReference))
			{
				logger.Log(LogType.Error, "Unable to extract the declaration reference from the Asycuda message.");
				throw new AsycudaIncomingDataException(Res.GetString("554FAA57-F464-4950-9C63-4C505D088DA3", "Unable to extract the declaration reference from the Asycuda message."));
			}

			var entry = AsycudaIncomingGMDMessageHelper.FindCusEntryHeaderByBGMReference(bgmReference, interchange.Branch, interchange.Factory);

			if (entry == null)
			{
				logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Declaration not found for declaration reference {0}.", bgmReference));
				throw new AsycudaIncomingDataException(string.Format(CultureInfo.InvariantCulture, Res.GetString("FFBCDA27-7866-4441-9E2D-99C3B583AB06", "Declaration not found for declaration reference {0}."), bgmReference));
			}

			var mainUser = AsycudaIncomingGMDMessageHelper.GetUserToReceiveAsycudaMessageViaEmail(entry);

			var ediMessage = entry.Factory.New<EDIMessage>();
			ediMessage.EM_IsTestMessage = false;
			ediMessage.EM_ApplicationCode = interchange.EI_InterchangeType;
			ediMessage.EM_MessageOwner = mainUser.GS_Code;
			ediMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			ediMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			ediMessage.EM_Status = EDIInterchange.Status.Received;
			ediMessage.EM_MessageText = AsycudaIncomingGMDMessageHelper.FormatXMLDocument(payload);
			ediMessage.EM_GB = interchange.EI_GB;
			ediMessage.EM_EI = interchange.PK;
			entry.Messages.Add(ediMessage);

			var declaration = entry.Declaration;
			var docManagerInfo = declaration.DocManagerInfo;
			var payloadInASCII = Encoding.ASCII.GetBytes(payload);
			var wrapper = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration);
			var univMessagingHelper = new AsycudaJobDeclarationUniversalMessagingHelper(wrapper);
			docManagerInfo.AddFileOrDocument(payloadInASCII, univMessagingHelper.GetAttachmentFileName(entry), Core.Constants.RefDocTypes.Manifest);
			docManagerInfo.Save();

			if (!mainUser.GS_EmailAddress.IsEmpty)
			{
				using (var stream = new MemoryStream(payloadInASCII))
				{
					univMessagingHelper.SendEmail(entry, stream, mainUser);
				}
			}

			AddSuccessNote(interchange, string.Format(CultureInfo.InvariantCulture, Res.GetString("1A151705-2DFF-4BCA-BEA2-64B4E3F495A2", "Linked Asycuda message to Declaration {0}."), bgmReference));
			logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Linked Asycuda message to Declaration {0}.", bgmReference));
		}

		static void AddSuccessNote(EDIInterchange interchange, ZString noteText)
		{
			var note = interchange.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DataImportLogNote.Description;
			note.ST_IsCustomDescription = false;
			note.ST_NoteText = noteText;
			note.ST_NoteContext = "AAA";
		}

		readonly ILogger logger;
	}
}
