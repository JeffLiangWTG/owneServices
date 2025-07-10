using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5TW;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5TW)]
	class GOVCBR5TWProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5TWDataProvider().GetMessageData(message.Factory, textReader);

				if (message.EM_MessageSubType.IsEmpty)
				{
					message.EM_MessageSubType = OneOrMultiple.ONE;
					if (messageData.Lines?.Cast<GOVCBR5TWLineMessageData>().GroupBy(x => x.AttachedDeclarationNumber).FirstOrDefault(x => x.Key != messageData.ImportDeclarationNumber) != null)
					{
						message.EM_MessageSubType = OneOrMultiple.MUL;
					}
				}
				if (message.EM_MessageSubType == OneOrMultiple.ONE || message.EM_MessageSubType == OneOrMultiple.OST)
				{
					var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
					if (entry != null)
					{
						message.EM_LinkedObject = entry;
					}
					message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData, MessageFunctions.MessageInterpretationMode.FullView);
					SendNotification(message, entry, messageData);
				}
				else
				{
					var goodsShipmentArray = messageData.Lines?.Cast<GOVCBR5TWLineMessageData>().GroupBy(x => x.AttachedDeclarationNumber);
					foreach (var shipment in goodsShipmentArray)
					{
						var clonedMessage = (EDIMessage)message.Clone();
						clonedMessage.EM_MessageSubType = OneOrMultiple.OST;
						clonedMessage.EM_Status = EDIMessage.Status.Queued;
						clonedMessage.EM_ApplicationReference = message.EM_MessageNum;
						clonedMessage.EM_MessageNum = ZString.Empty;

						using (var reader = clonedMessage.GetEM_MessageTextReader())
						{
							var response = KRXmlObjectSerializer.Deserialize<Response>(reader);
							if (messageData.ImportDeclarationNumber == shipment.Key)
							{
								response.Declaration.GoodsShipment = new Collection<ResponseDeclarationGoodsShipment>(response.Declaration.GoodsShipment.Where(x => x.AdditionalDocument.Id.Value == messageData.ImportDeclarationNumber).ToArray());
								response.Declaration.LoadingListQuantity.Value = response.Declaration.GoodsShipment.Count;
							}
							else
							{
								var entryLines = response.Declaration.GoodsShipment.Where(x => x.AdditionalDocument.Id.Value == shipment.Key).ToArray();
								var entryLine = entryLines[0];
								response.Declaration.FunctionalReferenceId.Value = entryLine.AttachedDocument.Id.Value;
								response.Declaration.PreviousDocument.Id.Value = entryLine.AdditionalDocument.Id.Value;
								response.Declaration.PreviousDocument.SequenceNumeric = entryLine.AdditionalDocument.SequenceNumeric;
								response.Declaration.VersionId.Value = entryLine.SequenceNumeric.ToString();
								response.Declaration.PreviousDocument.IssueDateTime = entryLine.AdditionalDocument.IssueDateTime;
								response.Declaration.AdditionalInformation.BeginningDateTime = entryLine.AdditionalInformation.BeginningDateTime;
								response.Declaration.AdditionalInformation.EndingDateTime = entryLine.AdditionalInformation.EndingDateTime;
								response.Declaration.AdditionalInformation.Content.Value = entryLine.AdditionalInformation.Content.Value;
								response.Declaration.AdditionalInformation.StatementDescription.Value = entryLine.AdditionalInformation.StatementDescription.Value;
								entryLines = entryLines.Skip(1).ToArray();
								response.Declaration.GoodsShipment = new Collection<ResponseDeclarationGoodsShipment>(entryLines);
								response.Declaration.LoadingListQuantity.Value = entryLines.Length;
							}
							clonedMessage.SetEM_MessageTextOrDataSource(KRXmlObjectSerializer.Serialize(response));
						}
					}
				}
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, GOVCBR5TWMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData, MessageFunctions.MessageInterpretationMode.Email),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5FE },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5TW}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, GOVCBR5TWMessageData messageData, MessageFunctions.MessageInterpretationMode mode)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입(납세)신고 정정 신청서" });
			tableContents.WriteRow(new string[] { "통지번호", messageData.FormattedNoticeNumber });
			tableContents.WriteRow(new string[] { "자료제출요구 문서 번호", MessageFunctions.RequestDocumentNumber(messageData.RequestDocumentNumber) });
			tableContents.WriteRow(new string[] { "납세의무자 상호", messageData.PayerCompanyName });
			tableContents.WriteRow(new string[] { "납세의무자 성명", messageData.PayerRepresentativeName });
			tableContents.WriteRow(new string[] { "수입신고번호", messageData.FormattedImportDeclarationNumber });
			tableContents.WriteRow(new string[] { "란번호", messageData.ImportEntryLineNo.ToString() });
			tableContents.WriteRow(new string[] { "수입신고일", messageData.ImportDeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "심사일자", messageData.ExamineStartDate.ToString(DateFormatType.DateKorean) + "~" + messageData.ExamineEndDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "심사내용", messageData.ContentDescription });
			tableContents.WriteRow(new string[] { "보정심사결과", messageData.CorrectionResult });
			tableContents.WriteRow(new string[] { "통지일자", messageData.NoticeDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "통지세관", "[" + messageData.NoticeCustomsOffice + "] " + messageData.NoticeCustomsOfficeName });
			tableContents.WriteRow(new string[] { "첨부수입신고정보 건수", messageData.DeclarationsCount.ToString() });
			tableContents.WriteRow(new string[] { "신고인부호", messageData.DeclarantID });
			result.Append(tableContents.ToHtml());

			if (messageData.DeclarationsCount > 0)
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				detailContents.WriteRowWithFormatting(new CellWithFormatting("보정심사결과 첨부내역", new NameValueCollection { { "align", "left" }, { "colspan", "5" } }, true));
				detailContents.WriteRowWithFormatting(new CellWithFormatting("란번호", new NameValueCollection { { "align", "center" } }), new CellWithFormatting("심사일자", new NameValueCollection { { "align", "center" } }), new CellWithFormatting("심사내용", new NameValueCollection { { "align", "center" } }), new CellWithFormatting("보정심사결과", new NameValueCollection { { "align", "center" } }), new CellWithFormatting("자료제출요구 문서번호", new NameValueCollection { { "align", "center" } }));
				var i = 0;
				foreach (var detals in messageData.Lines.Cast<GOVCBR5TWLineMessageData>())
				{
					if (mode == MessageFunctions.MessageInterpretationMode.Email && i > 9)
					{
						detailContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "5" } }));
						break;
					}
					detailContents.WriteRowWithFormatting(new CellWithFormatting(detals.ImportEntryLineNo.ToString()),
																	new CellWithFormatting(detals.ExamineStartDate.ToString(DateFormatType.DateKorean) + "~" + detals.ExamineEndDate.ToString(DateFormatType.DateKorean)),
																	new CellWithFormatting(detals.ContentDescription),
																	new CellWithFormatting(detals.CorrectionResult),
																	new CellWithFormatting(MessageFunctions.RequestDocumentNumber(detals.RequestDocumentNumber)));
					i += 1;
				}
				result.Append(detailContents.ToHtml());
			}

			return result.ToString();
		}
		#endregion
	}
}
