using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR98;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R98)]
	public class GOVCBRR98Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR98DataProvider().GetMessageData(textReader);

				if (message.EM_MessageSubType.IsEmpty)
				{
					var shipmentCnt = messageData.GoodsShipment.Count();
					message.EM_MessageSubType = shipmentCnt > 1 ? OneOrMultiple.MUL : OneOrMultiple.ONE;
				}

				if (message.EM_MessageSubType == OneOrMultiple.ONE)
				{
					var shipment = messageData.GoodsShipment.FirstOrDefault();
					if (shipment != null)
					{
						var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, shipment.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
						message.EM_LinkedObject = entry;

						message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, shipment);
						SendNotification(message, entry, messageData, shipment);
					}
				}
				else
				{
					foreach (var shipment in messageData.GoodsShipment)
					{
						var clonedMessage = (EDIMessage)message.Clone();
						clonedMessage.EM_MessageSubType = OneOrMultiple.ONE;
						clonedMessage.EM_Status = EDIMessage.Status.Queued;
						using (var reader = clonedMessage.GetEM_MessageTextReader())
						{
							var response = KRXmlObjectSerializer.Deserialize<Response>(reader);
							response.Declaration.GoodsShipment = new Collection<ResponseDeclarationGoodsShipment>(response.Declaration.GoodsShipment.Where(x => x.GovernmentAgencyGoodsItem.PreviousDocument.Id.Value == shipment.ExportDeclarationNumber).ToArray());
							clonedMessage.SetEM_MessageTextOrDataSource(KRXmlObjectSerializer.Serialize(response));
						}
					}
				}
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRR98MessageData messageData, IGoodsShipment shipment)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, shipment),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R98}]",
				EntryNumber = shipment.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		ZString CreateUserFriendlyMessageInterpretation(IGOVCBRR98MessageData messageData, IGoodsShipment shipment)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "수신인", messageData.DeclarantName });
			tableContents.WriteRow(new string[] { "신고번호", MessageFunctions.DeclarationNumberFormat(shipment.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "신고일자", shipment.DeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "수리일자", shipment.EntryReleaseDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "적재기간", shipment.LoadingDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "수출자", shipment.ExporterCompanyName });
			tableContents.WriteRow(new string[] { "품명", shipment.TradeName });
			tableContents.WriteRow(new string[] { "포장개수", shipment.TotalPackQty.ToString() });
			tableContents.WriteRow(new string[] { "중량", shipment.TotalGrossWeightInKG.ToString() });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
