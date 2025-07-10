using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._023)]
	class GOVCBR023Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR023DataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					using (entry.SuspendCESLog())
					{
						if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._11)
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.SGN;
						}
						else if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._12)
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.RDY;
						}
						else if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._13)
						{
							if (messageData.DocumentSubmitType == ImportAttachDocumentTypeList.Codes._14)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.EDC;
							}
							else
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CLR;
								entry.CH_EntryReleaseDate = messageData.NoticeDateTime;
							}
						}
						else if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._14)
						{
							if (messageData.DocumentSubmitType == ImportAttachDocumentTypeList.Codes._14)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.EDC;
							}
							else
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ACL;
								entry.CH_EntryReleaseDate = messageData.NoticeDateTime;
							}
						}
						else if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._15)
						{
							if (messageData.DocumentSubmitType == ImportAttachDocumentTypeList.Codes._14)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.EDC;
							}
							else
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.BAC;
								entry.CH_EntryReleaseDate = messageData.NoticeDateTime;
							}
						}
						else if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._16)
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ICG;
						}
						else if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._17)
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCG;
						}
						else if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._18)
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ADT;
						}
						else if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._19 || messageData.ResultType == ImportProcessResultTypeCodeList.Codes._21)
						{
							if (messageData.DocumentSubmitType == ZString.Empty)
							{
								if (messageData.CSCodes.Keys.Any(x => CargoSelectivityResultTypeCodeList.ToBeInspected(x)))
								{
									entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.INS;
								}
								else
								{
									entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.SRN;
								}
							}
							else if (messageData.DocumentSubmitType == ImportAttachDocumentTypeList.Codes._11 || messageData.CustomsOfficeContent.Contains((NoResString)"사후심사결과"))
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.NED;
							}
							else if (messageData.DocumentSubmitType == ImportAttachDocumentTypeList.Codes._12 || messageData.CustomsOfficeContent.Contains((NoResString)"종이서류 제출"))
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.NDC;
							}
							else if (messageData.DocumentSubmitType == ImportAttachDocumentTypeList.Codes._13 || messageData.CustomsOfficeContent.Contains((NoResString)"전자서류(원본)"))
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.OPD;
							}
						}
						else if (messageData.ResultType == ImportProcessResultTypeCodeList.Codes._20)
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCL;
							entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
						}

						if (ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(messageData.ResultType) && !messageData.NoticeNumber.IsEmpty)
						{
							UpdateStatementPrintAndDueDate();
						}

						void UpdateStatementPrintAndDueDate()
						{
							CusEntrySnapshot snapshot = entry.Snapshots.Cast<CusEntrySnapshot>().FirstOrDefault(x => x.CES_MessageType == ElectronicDocumentTypeList.Codes._929 && x.CES_Status == EntrySnapshotStatus.Lodged && x.CES_VersionNumber == 1);
							if (snapshot != null)
							{
								ImportEntryHeader dataProvider929 = null;
								using (var snapshotTextReader = snapshot.GetCES_SnapshotXmlReader())
								{
									dataProvider929 = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(snapshotTextReader);
								}
								ZString declarationPlanCode = dataProvider929?.DeclarationPlanCode ?? ZString.Empty;
								ZString paymentType = dataProvider929?.PaymentType ?? ZString.Empty;

								if (declarationPlanCode == ImportCustomsClearancePlanCodeList.Codes.G || paymentType != PaymentMethodCodeList.Codes._11)
								{
									var statement = new CusStatementHeader.Loader(message.Factory).Load(messageData.NoticeNumber, entry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
									if (statement != null)
									{
										ZDate noticeDate = new ZDate(messageData.NoticeDateTime);
										statement.B2_PrintDate = noticeDate;
										statement.B2_DueDate = noticeDate.AddDays(15);
									}
								}
							}
						}
					}
					entry.Logs.AddNew(new EventValue(Events.CustomsEntryStatus, eventTime: messageData.NoticeDateTime.ToOffset(), reference: entry.CH_EntryStatus));
					var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._929);
					message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;

					if (!messageData.CustomsOfficeContent.IsEmpty)
					{
						entry.CH_CustomsMessageRemarks = messageData.IssueDateTime.ToString(DateFormatType.DateTimeKorean) + "\r\n" + messageData.CustomsOfficeContent + "\r\n" + entry.CH_CustomsMessageRemarks;
					}

					var entryNum = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_ParentID == entry.PK && x.CE_EntryType == ElectronicDocumentTypeList.Codes._934);
					if (entryNum != null)
					{
						entryNum.CE_ExpiryDate = messageData.PaymentDate;
					}

					entry.CH_BGMReference = messageData.NoticeNumber;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR023MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._929 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._023}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR023MessageData messageData)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "300" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
			tableContents.WriteRow(new string[] { "처리결과 통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKoreanNoSecond) });
			tableContents.WriteRow(new string[] { "신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "전자납부번호", MessageFunctions.NoticeNumberFormat(messageData.NoticeNumber) });
			tableContents.WriteRow(new string[] { "처리 담당자", messageData.CustomsManagerID + messageData.CustomsManagerName });
			tableContents.WriteRow(new string[] { "심사결과부호", new ImportProcessResultTypeCodeList().GetDescriptionFromCode(messageData.ResultType) });
			tableContents.WriteRow(new string[] { "첨부서류구분", new ImportAttachDocumentTypeList().GetDescriptionFromCode(messageData.DocumentSubmitType) ?? ZString.Empty });
			tableContents.WriteRow(new string[] { "확정가격신고기한", messageData.PaymentDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "세관기재내용", messageData.CustomsOfficeContent });
			result.Append(tableContents.ToHtml());

			if (messageData.CSCodes != null)
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				detailContents.WriteRowWithFormatting(new CellWithFormatting("선별결과 내역", new NameValueCollection { { "align", "left" }, { "colspan", "2" } }, true));
				detailContents.WriteRowWithFormatting(new CellWithFormatting("C/S 결과부호", new NameValueCollection { { "align", "center" }, { "width", "200" } }), new CellWithFormatting("란번호", new NameValueCollection { { "align", "center" }, { "width", "300" } }));

				var list = new CargoSelectivityResultTypeCodeList();
				var styleColourRed = new NameValueCollection { { "style", "color:red" } };
				var emptyAttributes = new NameValueCollection();

				var sortedCSCodes = CargoSelectivityResultTypeCodeList.CodesInOrderOfImportance();
				foreach (var csCode in sortedCSCodes)
				{
					List<int> entryLines;
					if (messageData.CSCodes.TryGetValue(csCode, out entryLines))
					{
						var cellAttributes = CargoSelectivityResultTypeCodeList.ToBeInspected(csCode) ? styleColourRed : emptyAttributes;
						var cells = GetCargoSelectivityRow(csCode, entryLines, list, cellAttributes);
						detailContents.WriteRowWithFormatting(cells);
					}
				}

				foreach (var csCode in messageData.CSCodes.Keys)
				{
					if (!sortedCSCodes.Contains(csCode))
					{
						List<int> entryLines;
						if (messageData.CSCodes.TryGetValue(csCode, out entryLines))
						{
							var cellAttributes = CargoSelectivityResultTypeCodeList.ToBeInspected(csCode) ? styleColourRed : emptyAttributes;
							var cells = GetCargoSelectivityRow(csCode, entryLines, list, cellAttributes);
							detailContents.WriteRowWithFormatting(cells);
						}
					}
				}

				result.Append(detailContents.ToHtml());
			}

			return result.ToString();
		}

		static CellWithFormatting[] GetCargoSelectivityRow(string csCode, List<int> entryLines, CargoSelectivityResultTypeCodeList list, NameValueCollection rowAttributes)
		{
			entryLines.Sort();
			var entryLinesString = new ZStringBuilder(entryLines.Select(x => x.ToString()));
			var csDescCell = new CellWithFormatting(list.GetDescriptionFromCode(csCode) ?? csCode, rowAttributes);
			var entryLinesCell = new CellWithFormatting(entryLinesString.ToStringWithDelimiterBetweenAppends(", "), rowAttributes);
			return new CellWithFormatting[] { csDescCell, entryLinesCell };
		}
		#endregion
	}
}
