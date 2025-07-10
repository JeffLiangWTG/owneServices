using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using CargoWise.Application;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using MimeKit;

namespace Enterprise.Customs.JP.Common;

public class EDIInterchange : Messaging.Business.EDIInterchange,
	Integration.Customs.JP.IEDIInterchange,
	IMessageDataProvider,
	IxTMessageAttributeProvider
{
	public EDIInterchange(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	const string BoundaryString = "YYYYYY";

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EI_ApplicationCode = ApplicationCodes.JPCustoms;
	}

	public static EDIInterchange CreateFromMessage(EDIMessage message)
	{
		var interchange = message.Factory.New<EDIInterchange>();
		interchange.EI_BodyData = message.EM_MessageData;
		interchange.EI_InterchangeType = message.EM_MessageType;
		interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
		interchange.EI_ReceiveTransmit = message.EM_ReceiveTransmit;
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
		interchange.EI_To = GetDestinationParty();
		interchange.EI_GB = message.EM_GB;
		interchange.EI_GP = message.EM_GP;

		message.EM_EI = interchange.PK;

		return interchange;
	}

	static string GetDestinationParty()
	{
		var regKey = ObjectFactory.Get<IProductRegistration>()?.Key;
		return regKey != null ? $"{regKey.EnterpriseCode}{regKey.ServerCode}_JPC" : string.Empty;
	}

	#region IMessageDataProvider

	BinaryReader IMessageDataProvider.GetMessageData()
	{
		var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, PK)
			.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit)
			.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.JPCustoms);
		var message = Factory.LoadTop1<EDIMessage>(messageQuery);

		using var stream = new MemoryStream(GetMessageDataWithCredentialInfo());
		stream.Seek(0, SeekOrigin.Begin);

		using var mimeMessage = new MimeMessage();
		var naccsMailAddress = GetNACCSMailAddress();
		if (MailboxAddress.TryParse(naccsMailAddress, out var toAddress))
		{
			mimeMessage.To.Add(toAddress);
		}

		if (MailboxAddress.TryParse(GetMailBoxAddress(naccsMailAddress), out var fromAddress))
		{
			mimeMessage.From.Add(fromAddress);
		}

		mimeMessage.MimeVersion = new Version(1, 0);

		var textBody = new MimePart(new ContentType("text", "plain") { Charset = JPMessageUtils.MessageDataEncodingEUCJP.BodyName.ToUpper() }) { ContentTransferEncoding = ContentEncoding.EightBit };
		textBody.Content = new MimeContent(stream);

		var mimeStream = new MemoryStream();

		if (message?.MessageAttachments.Count > 0)
		{
			var multiPart = new Multipart("mix");
			multiPart.Boundary = BoundaryString;
			multiPart.Add(textBody);

			foreach (EDIMessageAttach attachment in message.MessageAttachments)
			{
				var filename = attachment.EG_FileName;
				var encodedFilename = JPMessageUtils.AttachmentFilenameJapaneseEncoding.GetBytes(filename);
				var filenameInBase64 = $"=?ISO-2022-JP?B?{Convert.ToBase64String(encodedFilename)}?=";
				var data = attachment.GetAttachment()?.ImageData ?? ZBlob.Empty;
				var mimeAttachment = new MimePart(MimeTypes.GetMimeType(filename));
				mimeAttachment.ContentType.Name = filenameInBase64;
				mimeAttachment.Content = new MimeContent(new MemoryStream(data));
				mimeAttachment.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
				mimeAttachment.ContentDisposition.FileName = filenameInBase64;
				mimeAttachment.ContentTransferEncoding = ContentEncoding.Base64;

				multiPart.Add(mimeAttachment);
			}

			mimeMessage.Body = multiPart;
		}
		else
		{
			mimeMessage.Body = textBody;
		}

		mimeMessage.WriteTo(mimeStream);
		mimeStream.Seek(0, SeekOrigin.Begin);
		return new BinaryReader(mimeStream);
	}

	byte[] GetMessageDataWithCredentialInfo()
	{
		var result = GetEI_BodyDataReader().ToByteArray();

		const int PasswordLength = 8;

		var credential = Factory.Load<GlbExternalPassword>(EI_GP);
		if (credential != null)
		{
			var messageContent = JPMessageUtils.ConvertMessageToString(JPMessageUtils.ExtractHeader(result));
			var password = credential.CurrentDecryptedPassword.SubstringSafe(0, PasswordLength).PadRight(PasswordLength);

			var headerWithPassword = JPMessageUtils.ConvertStringToMessage(messageContent.Replace(EDIMessage.PasswordPlaceHolder, password));
			Array.Copy(headerWithPassword, result, headerWithPassword.Length);
		}

		return result;
	}

	ZString GetMailBoxAddress(ZString naccsMailbox)
	{
		var company = Branch?.Company;
		var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(company);
		return wrapper?.MailboxCredential?.FullMailBoxAddress ?? ZString.Empty;
	}

	ZString GetNACCSMailAddress()
	{
		var key = Env.Instance.IsProductionSystem ? Constants.NACCSProdMailboxKey : Constants.NACCSTestMailboxKey;
		return new RefSysConfig.Loader(Factory).GetStringValue(key);
	}

	#endregion

	#region IxTMessageAttributeProvider

	Dictionary<string, string> IxTMessageAttributeProvider.GetMessageAttrDictionary()
	{
		return new Dictionary<string, string> { { Constants.DirectxT.ReceiverAttribute, EI_To } };
	}

	#endregion
}
