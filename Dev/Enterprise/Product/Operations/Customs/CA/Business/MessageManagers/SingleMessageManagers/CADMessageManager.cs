using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Customs.CA.MessageDefinitions.CAD.Inbound;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class CADMessageManager : B3CADBaseMessageManager
	{
		public CADMessageManager(CADMessageWrapper wrapper, IUserNotification notification, B3DeferInstruction deferInstruction = null, bool fromServiceTask = false)
			: base(wrapper, notification, deferInstruction, new CADStatusCalculator(), fromServiceTask)
		{
			this.cadMessageWrapper = wrapper;
		}

		readonly CADMessageWrapper cadMessageWrapper;

		public bool SendMessage()
		{
			return SendMessage(cadMessageWrapper.messageSubType);
		}

		ZString GetCADMessageSubTypeByMessageSubType(MessageSubTypes type)
		{
			switch (type)
			{
				case MessageSubTypes.Create:
					return MessageSubTypeCodes.Codes.Original;
				case MessageSubTypes.Change:
				case MessageSubTypes.Amend:
					return MessageSubTypeCodes.Codes.Change;
				case MessageSubTypes.Withdraw:
					return MessageSubTypeCodes.Codes.Cancellation;
				default:
					return MessageSubTypeCodes.Codes.Undefined;
			}
		}

		protected override ZString MessageTypeForDisplay => MessageTypeList.Codes.CommercialAccountingDeclaration;

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new CADMessageBuilder(DataWrapper, GetCADMessageSubTypeByMessageSubType(actionCode));
		}

		protected override string EntryHasBeenLodged(JobDeclaration declaration)
		{
			if (declaration.IsCADLodged)
			{
				return Res.GetString("362DB014-CD3D-48FE-A16E-C97C34B2F71E",
					"A CAD has already been lodged for this Declaration.");
			}
			return ZString.Empty;
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			var result = base.CanSendThisMessage(actionCode, out messageText);

			if (result && BusinessObject is JobDeclaration declaration && declaration.SentCADMessageCount >= 999)
			{
				result = false;
				notification.ShowError(Res.GetString("AB932573-198F-4A18-BEC2-8F366E8E9264", "This declaration has already sent 999 CAD messages.\nPlease create a new declaration."), string.Empty);
			}

			return result;
		}

		protected override void ShowQueuedForSending(MessageSubTypes actionCodeToSend)
		{
			base.ShowQueuedForSending(actionCodeToSend);
			if (actionCodeToSend == MessageSubTypes.Change && cadMessageWrapper.IsAmendment && BusinessObject is JobDeclaration declaration)
			{
				declaration.B3EntryHeader.MergedLines.ForEach(x => x.RefreshAmendmentDetails());
			}
		}

		protected override void ActionWhenMessageCanNotSend()
		{
			base.ActionWhenMessageCanNotSend();
			cadMessageWrapper.ClearSendingActions();
		}

		protected override void RunAdditionalEDIMessageModification(IEnumerable<Enterprise.Messaging.Business.EDIMessage> messages)
		{
			base.RunAdditionalEDIMessageModification(messages);

			var message = messages.FirstOrDefault();
			if (message != null && message.EM_MessageSubType == MessageSubTypeCodes.Codes.Change)
			{
				var lastOriginalMessage = DataWrapper.Messages.GetLastMessage(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.CommercialAccountingDeclaration, EDIMessage.Direction.Transmit, ZString.Empty, MessageSubTypeCodes.Codes.Original);

				if (lastOriginalMessage != null && cadMessageWrapper.AmendmentActions is CADCorrectionMessageSendingActionCollection amendmentActions)
				{
					var documentMetaDataOrg = XmlObjectSerializer.Deserialize<DocumentMetaData>(lastOriginalMessage.EM_MessageText);
					var documentMetaDataAdj = XmlObjectSerializer.Deserialize<DocumentMetaData>(message.EM_MessageText);

					var commoditySequence = amendmentActions.Select(x => (x.InvoiceSequence, x.InvoiceLineSequence)).Distinct();
					var originalCommodityDict = documentMetaDataOrg.Declaration.GoodsShipment
						.SelectMany(gs => gs.GovernmentAgencyGoodsItem.Select(item => new { GoodsShipmentSeq = gs.SequenceNumeric, Item = item }))
						.Aggregate(
							new Dictionary<(uint?, uint?), DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity>(),
							(dict, x) =>
							{
								var key = (x.GoodsShipmentSeq, x.Item.SequenceNumeric);
								if (!dict.ContainsKey(key))
								{ dict[key] = x.Item; }
								return dict;
							});

					documentMetaDataAdj.Declaration.Status.ReleaseDateTime = documentMetaDataOrg.Declaration.Status.ReleaseDateTime;
					foreach (var newGoodsShipment in documentMetaDataAdj.Declaration.GoodsShipment)
					{
						for (var i = 0; i < newGoodsShipment.GovernmentAgencyGoodsItem.Count; i++)
						{
							var newCommodity = newGoodsShipment.GovernmentAgencyGoodsItem[i];
							if (!commoditySequence.Any(x => x.InvoiceSequence == newGoodsShipment.SequenceNumeric && x.InvoiceLineSequence == newCommodity.SequenceNumeric))
							{
								if (originalCommodityDict.TryGetValue((newGoodsShipment.SequenceNumeric, newCommodity.SequenceNumeric), out var originalCommodity))
								{
									newGoodsShipment.GovernmentAgencyGoodsItem[i] = originalCommodity;
									if (newCommodity.ExitDateTime != null)
									{
										newGoodsShipment.GovernmentAgencyGoodsItem[i].ExitDateTime = newCommodity.ExitDateTime;
									}
								}
							}
						}

						var ns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("xsi", "http://www.w3.org/2001/XMLSchema-instance"), new XmlQualifiedName("xsd", "http://www.w3.org/2001/XMLSchema"), new XmlQualifiedName(ZString.Empty, "urn:wco:datamodel:WCO:Declaration:1") });
						var setting = new XmlWriterSettings
						{
							Indent = true,
							OmitXmlDeclaration = false
						};

						message.EM_MessageText = XmlObjectSerializer.SerializeWithNamespaces(documentMetaDataAdj, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), setting, ns);
					}
				}
			}
		}
	}
}
