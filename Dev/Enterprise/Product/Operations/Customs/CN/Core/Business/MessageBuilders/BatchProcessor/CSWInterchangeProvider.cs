using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.CN.Business
{
	public class CSWInterchangeProvider : InterchangeProviderBase
	{
		public CSWInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;

		protected override Type InterchangeType => typeof(CNEDIInterchange);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var message = messages.Cast<EDIMessage>().Single();

			var declaration = (message.EM_LinkedObject as CusEntryHeader)?.Declaration;
			var companyPK = declaration?.RegistryCompanyPK ?? message.Branch.Company.PK.ToGuid();
			var branchPK = declaration?.RegistryBranchPK ?? message.Branch.PK.ToGuid();
			var recipient = CNCustomsDataRegistry.Instance.CNSWClientSetting.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty)?.EHubClientID ?? ZString.Empty;
			if (recipient.IsEmpty)
			{
				recipient = GlbCompany.CurrentCompany.LicenceKeyIdentifier + "_CSW";
			}

			CustomsGenericMessageHelper.PopulateGMDInterchange(interchange, recipient, message.EM_ApplicationCode, message.EM_MessageText);

			if (message.EM_MessageType == EDIMessageTypeList.Codes.DEC)
			{
				interchange.EI_BodyText = CreateZippedMessageText(message);
			}

			interchange.ContainedMessages.Add(message);
			message.EM_Status = EDIMessage.Status.Sent;
		}

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;

		string CreateZippedMessageText(EDIMessage message)
		{
			using (var zipStream = new MemoryStream())
			{
				using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create))
				{
					var decMessage = message.EM_MessageText;
					if (!decMessage.IsEmpty)
					{
						var fileName = $"{message.EM_ApplicationReference}_{ZDateTime.Now:yyyyMMddHHmmssfff}.xml";
						using (var stream = zipArchive.CreateEntry(fileName).Open())
						using (var decWriter = new StreamWriter(stream))
						using (var xmlWriter = XmlWriter.Create(decWriter, new XmlWriterSettings() { Indent = true }))
						{
							var element = XElement.Parse(decMessage);
							element.Attributes().Where(e => e.IsNamespaceDeclaration).Remove();
							element.Save(xmlWriter);
						}
					}

					foreach (var attachment in message.MessageAttachments.Cast<EDIMessageAttach>().Where(x => !x.EG_FileName.IsEmpty))
					{
						var imageData = attachment.GetAttachment()?.ImageData;
						if (imageData != null)
						{
							using (var stream = zipArchive.CreateEntry(attachment.EG_FileName).Open())
							using (var attachedDocumentWriter = new BinaryWriter(stream))
							{
								attachedDocumentWriter.Write(imageData);
							}
						}
					}
				}

				return Convert.ToBase64String(zipStream.ToArray());
			}
		}
	}
}
