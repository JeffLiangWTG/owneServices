using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRInterchangeProvider : InterchangeProviderBase
	{
		public BRInterchangeProvider(NonDependentEDIMessageCollection messageCollection)
			: base(messageCollection)
		{
		}

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override Type InterchangeType => typeof(BREDIInterchange);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count > 0)
			{
				var message = messages[0] as BREDIMessage;
				interchange.EI_TransportType = GetTransportTypeCode();
				SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, BREDIInterchange.BRCustoms, GlbCompany.CurrentCompany.LicenceKeyIdentifier);

				var dictionary = new Dictionary<string, string>()
				{
					{ Enterprise.xTMessaging.Shared.Constants.CustomMsgAttributes.MessageSubType,  message.EM_MessageSubType }
				};

				var infos = message.EM_ApplicationReference.Split("|");
				var attributes = GetCustomMsgAttributesNeedToBePopulated(message).ToArray();
				for (var i = 0; i < attributes.Length; i++)
				{
					dictionary.Add(attributes[i], infos.ElementAtOrDefault(i));
				}

				interchange.SetHeaderTextWithAttributeDictionary(dictionary);
				interchange.EI_SessionGUID = ZGuid.NewZGuid();

				if (ShouldAddToEDocs(message))
				{
					interchange.Factory.Saved -= AddToEDocs;
					interchange.Factory.Saved += AddToEDocs;
				}
			}
		}

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => EDIInterchange.Status.Queued;

		protected virtual ZString GetTransportTypeCode() => EDIInterchange.TransportType.xT;

		protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;

		protected virtual bool ShouldAddToEDocs(EDIMessage message) => false;

		protected virtual string GetEDocFileName(EDIMessage message, JobDeclaration declaration) => $"{declaration.JE_DeclarationReference}.{Core.Constants.FileFormats.XML}";

		protected virtual IEnumerable<string> GetCustomMsgAttributesNeedToBePopulated(EDIMessage message)
		{
			if (message.EM_MessageType == MessageTypeList.Codes.SUB)
			{
				if (message.EM_MessageSubType == SubscriptionMessageTypesList.Codes.CAN)
				{
					yield return Constants.CustomMsgAttributes.SubscriptionId;
				}
			}
			else if (message.EM_MessageType == MessageTypeList.Codes.CAT)
			{
				if (message.EM_MessageSubType == EDIMessageSubTypeList.Codes.ManufacturerZipFile)
				{
					yield return Constants.CustomMsgAttributes.RootCNPJ;
				}
				else if (message.EM_MessageSubType == EDIMessageSubTypeList.Codes.CatalogZipFile || message.EM_MessageSubType == EDIMessageSubTypeList.Codes.OperatorZipFile)
				{
					yield return Constants.CustomMsgAttributes.RootCNPJ;
					yield return Constants.CustomMsgAttributes.DisplayDisabled;
				}
			}
			else
			{
				yield return Enterprise.xTMessaging.Shared.Constants.CustomMsgAttributes.ReferenceNumber;

				if (message.EM_MessageType == MessageTypeList.Codes.LPC)
				{
					if (message.EM_MessageSubType == LPCOEntryActionCodeList.Codes.REQ)
					{
						yield return Constants.CustomMsgAttributes.MessageRequirement;
					}
				}
				else if (message.EM_MessageType == MessageTypeList.Codes.CDD)
				{
					if (message.EM_MessageSubType == ImportEntryActionCodeList.Codes.CVH || message.EM_MessageSubType == ImportEntryActionCodeList.Codes.RET || message.EM_MessageSubType == ImportEntryActionCodeList.Codes.DIA)
					{
						yield return Constants.CustomMsgAttributes.VersionNumber;
					}
				}
				else if (message.EM_MessageType == MessageTypeList.Codes.CIH)
				{
					if (message.EM_MessageSubType != EDIMessageSubTypeList.Codes.Original)
					{
						yield return Constants.CustomMsgAttributes.VersionNumber;
					}
				}
				else if (message.EM_MessageType == MessageTypeList.Codes.CIL)
				{
					yield return Constants.CustomMsgAttributes.VersionNumber;

					if (message.EM_MessageSubType == EDIMessageSubTypeList.Codes.Deletion)
					{
						yield return Constants.CustomMsgAttributes.ItemNumber;
					}
				}
			}
		}

		void AddToEDocs(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= AddToEDocs;

			if (savedSuccessfully)
			{
				var interchange = Interchanges.FirstOrDefault();
				var message = interchange?.ContainedMessages.Cast<EDIMessage>().FirstOrDefault();
				var declaration = (message?.EM_LinkedObject as CusEntryHeader)?.Declaration;

				if (interchange != null && message != null && declaration != null)
				{
					var contentString = interchange.EI_BodyText;
					if (!contentString.IsEmpty)
					{
						var documentFactory = declaration.DocManagerInfo.MasterFactory;

						var content = System.Text.Encoding.UTF8.GetBytes(contentString);
						var filename = GetEDocFileName(message, declaration);

						documentFactory.AddFileOrDocument(declaration.PK, Core.Constants.DocManagerCodes.JobDeclaration, content, filename, Core.Constants.RefDocTypes.RequestDocument, "", false);
						documentFactory.Save();
					}
				}
			}
		}

		protected override void AppendMessageTextToMessageBody(StringBuilder stringBuilder, IEnumerable<ZString> messageTextList, EDIInterchange interchange)
		{
			if (interchange.ContainedMessages.All(x => x is BREDIMessage message && message.IsProductMessage))
			{
				stringBuilder.Append(ProductMessageBuilder.JoinProductMessages(messageTextList.Select(x => x.ToString())));
			}
			else if (interchange.ContainedMessages.All(x => x is BREDIMessage message && message.IsForeignOperatorMessage))
			{
				stringBuilder.Append(ForeignOperatorMessageBuilder.JoinForeignOperatorMessages(messageTextList.Select(x => x.ToString())));
			}
			else if (interchange.ContainedMessages.All(x => x is BREDIMessage message && message.IsProductLinkMessage))
			{
				stringBuilder.Append(ManufacturersToProductMessageBuilder.JoinManufacturersToProductMessages(messageTextList.Select(x => x.ToString())));
			}
			else
			{
				base.AppendMessageTextToMessageBody(stringBuilder, messageTextList, interchange);
			}
		}
	}
}
