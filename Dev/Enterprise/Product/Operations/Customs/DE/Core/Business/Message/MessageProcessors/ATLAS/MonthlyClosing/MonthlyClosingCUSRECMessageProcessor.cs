using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class MonthlyClosingCUSRECMessageProcessor : MonthlyClosingMessageProcessor<AtlasInboundEDIMessage<ICUSREC>, ICUSREC>
	{
		public MonthlyClosingCUSRECMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("5A15346C-080C-4F5A-A8DF-3DB99AAB6679", "Monthly Closing CUSREC Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSREC> message)
		{
			BusinessObject result = null;
			var dataProvider = message.DataProvider;
			if (dataProvider != null)
			{
				var referencedMessageIdentifier = dataProvider.ReferencedMessageIdentifier;
				if (!referencedMessageIdentifier.IsEmpty)
				{
					result = GetLinkedObjectFromOriginalMessage(message.Factory, referencedMessageIdentifier);
				}
				else
				{
					result = GetCusReconDeclarationFromMRN(message.Factory, message.Branch.GB_GC, dataProvider.MRN, dataProvider.ReferenceNumber);
				}
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSREC> message)
		{
			var declaration = (CusReconDeclaration)message.EM_LinkedObject;
			var dataProvider = message.DataProvider;
			var referenceNumber = dataProvider.ReferenceNumber;
			var mrn = dataProvider.MRN;
			var isHeaderError = dataProvider.NotificationSeverity.Any(x => x == NotificationTypeList.Codes.Error);

			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			var referenceAndMrnEmpty = string.IsNullOrEmpty(referenceNumber) && string.IsNullOrEmpty(mrn);
			var referenceOrMrnIsTemporary = RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber(referenceNumber) ||
				RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber(mrn);
			if (isHeaderError || referenceAndMrnEmpty || referenceOrMrnIsTemporary)
			{
				declaration.CRD_CustomsStatus = EntryStatus.REJ;
				declaration.CRD_MessageStatus = EDIMessage.Status.Rejected;
			}
			else
			{
				declaration.CRD_CustomsStatus = EntryStatus.RC2;
				declaration.CRD_MessageStatus = EDIMessage.Status.Received;

				var cusEntryNum = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				cusEntryNum.CE_EntryNum = referenceNumber ?? mrn;
				cusEntryNum.CE_IssueDate = dataProvider.RegistrationDate;

				UpdateCusReconEntryLines(factory, declaration, dataProvider);
				UpdateCusReconEntries(declaration);
			}

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, declaration
				, Res.GetString("C158EE1F-D4CF-4700-BD17-492FCB8785EE", "Monthly Closing CUSREC – Customs Receipt Message")
				, GetEmailBody(declaration, dataProvider)
				, false
				, message.Branch
				, declaration
				, () => GetOriginalMessage(factory, dataProvider.ReferencedMessageIdentifier));
		}

		void UpdateCusReconEntryLines(BusinessObjectFactory factory, CusReconDeclaration declaration, ICUSREC dataProvider)
		{
			var messageGoodsItems = new Dictionary<string, (string NotificationSeverity, string NotificationCode)>();
			var goodsItems = dataProvider.GoodsItems;

			var errorGoodsItems = goodsItems.Where(x => x.NotificationSeverity == NotificationTypeList.Codes.Error).Select(x => x.SequenceNumber)
				.ToImmutableHashSet();
			var warningGoodsItems = goodsItems.Where(x => x.NotificationSeverity == NotificationTypeList.Codes.Warning).Select(x => x.SequenceNumber)
				.Except(errorGoodsItems)
				.ToImmutableHashSet();
			var informationGoodsItems = goodsItems.Where(x => x.NotificationSeverity == NotificationTypeList.Codes.Information).Select(x => x.SequenceNumber)
				.Except(errorGoodsItems)
				.Except(warningGoodsItems)
				.ToImmutableHashSet();
			var distinctLineNumbers = errorGoodsItems.Union(warningGoodsItems).Union(informationGoodsItems)
				.Except(new ZString[] { "0" })
				.ToArray();

			foreach (var lineNumber in distinctLineNumbers)
			{
				ICUSRECGoodsItem currentGoodsItem = null;
				if (errorGoodsItems.Contains(lineNumber))
				{
					currentGoodsItem = goodsItems.FirstOrDefault(x => x.SequenceNumber == lineNumber && x.NotificationSeverity == NotificationTypeList.Codes.Error);
				}
				else if (warningGoodsItems.Contains(lineNumber))
				{
					currentGoodsItem = goodsItems.FirstOrDefault(x => x.SequenceNumber == lineNumber && x.NotificationSeverity == NotificationTypeList.Codes.Warning);
				}
				else if (informationGoodsItems.Contains(lineNumber))
				{
					currentGoodsItem = goodsItems.FirstOrDefault(x => x.SequenceNumber == lineNumber && x.NotificationSeverity == NotificationTypeList.Codes.Information);
				}
				messageGoodsItems.Add(lineNumber, (currentGoodsItem.NotificationSeverity, currentGoodsItem.NotificationCode));
			}

			var allCusReconEntryLines = declaration.CusReconEntries.SelectMany(x => x.CusReconEntryLines).ToArray();
			var originalMessageEntryLines = new HashSet<ZString>();
			var originalMessage = GetOriginalMessage(factory, dataProvider.ReferencedMessageIdentifier);
			if (originalMessage != null)
			{
				originalMessageEntryLines = originalMessage.Notes.GetNoteText(MonthlyClosingLinesNoteDescription).Split("|").ToHashSet();
			}

			foreach (var line in allCusReconEntryLines.Cast<CusReconEntryLine>())
			{
				if (messageGoodsItems.TryGetValue(line.CRL_LineNumber.ToString(), out var goodsItem))
				{
					UpdateAffectedCusReconLine(line, goodsItem.NotificationSeverity, goodsItem.NotificationCode, dataProvider);
				}
				else
				{
					var lineIsInOriginalMessage = originalMessageEntryLines.Contains(line.CRL_LineNumber.ToString());
					if (lineIsInOriginalMessage)
					{
						line.CRL_CustomsStatus = EntryStatus.RC2;
						UpdateInventory(line, dataProvider.ReferenceNumber, CurrentMessage.EM_MessageSubType);
						MapCusReconLineCurrentSnapshotToLodgedSnapshot(line);
					}
				}
			}
		}

		void UpdateAffectedCusReconLine(CusReconEntryLine line, string notificationSeverity, string notificationCode, ICUSREC dataProvider)
		{
			switch (notificationSeverity)
			{
				case NotificationTypeList.Codes.Information:
					UpdateLineStatusAndMapCurrentSnapshotToLodgedSnapshot(EntryStatus.RC2);
					UpdateInventory(line, dataProvider.ReferenceNumber, CurrentMessage.EM_MessageSubType);
					break;
				case NotificationTypeList.Codes.Error:
					if (line.CRL_CustomsStatus.IsEmpty)
					{
						line.CRL_CustomsStatus = EntryStatus.REJ;
					}
					else if (line.CRL_CustomsStatus != EntryStatus.REJ)
					{
						UpdateLineStatusAndMapCurrentSnapshotToLodgedSnapshot(EntryStatus.ERR);
					}

					if (notificationCode != ATLASNotificationCodes_819)
					{
						line.CurrentSnapshot?.Delete();
					}
					break;
				case NotificationTypeList.Codes.Warning:
					UpdateLineStatusAndMapCurrentSnapshotToLodgedSnapshot(EntryStatus.ERR);
					UpdateInventory(line, dataProvider.ReferenceNumber, CurrentMessage.EM_MessageSubType);
					break;
			}

			void UpdateLineStatusAndMapCurrentSnapshotToLodgedSnapshot(string customsStatus)
			{
				line.CRL_CustomsStatus = customsStatus;
				MapCusReconLineCurrentSnapshotToLodgedSnapshot(line);
			}
		}

		static void UpdateInventory(CusReconEntryLine line, string referenceNumber, string messageSubType)
		{
			switch (messageSubType)
			{
				case MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingBondedWarehouse:
				case MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingInwardProcessing:
				case MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingFreeCirculation when IsOutOfWarehouse():
					SetEntryKeyForInventory(line, referenceNumber);
					SetBondedEntryKeyForOrder(line, referenceNumber);
					break;
			}

			void SetEntryKeyForInventory(CusReconEntryLine cusReconEntryLine, string entryKey)
			{
				var query = new ZDBOnlyQuery(typeof(IWhsBondedWarehouseAttribute));
				query.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_EntryKey, cusReconEntryLine.ReconEntry.CRE_OriginalEntryNumber);
				query.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_EntryLineNo, cusReconEntryLine.CRL_OriginalEntryLineNumber);

				var docketLineQuery = new ZDBOnlySubQuery(typeof(IWhsDocketLine), WhsDocketLineSchema.PK);
				docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, SQLComparisonOperator.Equal, "INW");

				query.AddSubQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, docketLineQuery, JoinCondition.And);

				var attributes = cusReconEntryLine.Factory.Load<IWhsBondedWarehouseAttribute>(query);

				var newBondedEntryKey = $"{entryKey}-{cusReconEntryLine.CRL_LineNumber}";

				foreach (var attribute in attributes)
				{
					var lineNo = cusReconEntryLine.CRL_LineNumber;
					attribute.WB_EntryKey = entryKey;
					attribute.WB_EntryLineNo = lineNo;
					var orderLine = cusReconEntryLine.Factory.Load<IWhsDocketLine>(attribute.WB_ParentID);
					orderLine.WE_BondedEntryKey = newBondedEntryKey;
				}
			}

			void SetBondedEntryKeyForOrder(CusReconEntryLine cusReconEntryLine, string entryKey)
			{
				var originalBondedEntryKey =
					$"{cusReconEntryLine.ReconEntry.CRE_OriginalEntryNumber}-{cusReconEntryLine.CRL_OriginalEntryLineNumber}";
				var newBondedEntryKey = $"{entryKey}-{cusReconEntryLine.CRL_LineNumber}";
				var query = new ZDBOnlyQuery(typeof(IWhsDocketLine));
				query.AddToFilter(WhsDocketLineSchema.WE_BondedEntryKey, originalBondedEntryKey);
				query.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, "ORD");

				var orderLines = cusReconEntryLine.Factory.Load<IWhsDocketLine>(query);
				foreach (var orderLine in orderLines)
				{
					orderLine.WE_BondedEntryKey = newBondedEntryKey;
				}
			}

			bool IsOutOfWarehouse() =>
				line.ReconEntry?.EntryHeader?.EntryInstruction is
				{
					IsOutOfWarehouseWarehousing: true, CEI_OA_Warehouse.IsEmpty: false,
				};
		}

		void MapCusReconLineCurrentSnapshotToLodgedSnapshot(CusReconEntryLine line)
		{
			var lineCurrentSnapshot = line.CurrentSnapshot;
			if (lineCurrentSnapshot != null)
			{
				var lineLodgedSnapshot = line.LodgedSnapshot;

				// deserialize both
				var cur = CusReconEntryLineSnapshotBuilder.Deserialize(lineCurrentSnapshot.CRS_SnapshotXml);
				var ldg = CusReconEntryLineSnapshotBuilder.Deserialize(lineLodgedSnapshot.CRS_SnapshotXml);

				// merge
				var newldg = CusReconEntryLineSnapshotMerger.DoMerge(ldg, cur);

				lineLodgedSnapshot.CRS_SnapshotXml = CusReconEntryLineSnapshotBuilder.Serialize(newldg);
				lineCurrentSnapshot.Delete();
			}
		}

		void MapCusReconEntryCurrentSnapshotToLodgedSnapshot(CusReconEntry entry)
		{
			var entryCurrentSnapshot = entry.CurrentSnapshot;
			if (entryCurrentSnapshot != null)
			{
				var entryLodgedSnapshot = entry.LodgedSnapshot;

				// deserialize both
				var cur = CusReconEntrySnapshotBuilder.Deserialize(entryCurrentSnapshot.CRS_SnapshotXml);
				var ldg = CusReconEntrySnapshotBuilder.Deserialize(entryLodgedSnapshot.CRS_SnapshotXml);

				// merge
				var newldg = CusReconEntrySnapshotMerger.DoMerge(ldg, cur);

				entryLodgedSnapshot.CRS_SnapshotXml = CusReconEntrySnapshotBuilder.Serialize(newldg);
				entryCurrentSnapshot.Delete();
			}
		}

		void UpdateCusReconEntries(CusReconDeclaration declaration)
		{
			var acceptedLineCustomsStatuses = new ZString[] { EntryStatus.ERR, EntryStatus.RC2 }.ToHashSet();
			foreach (var entry in declaration.CusReconEntries.Cast<CusReconEntry>())
			{
				var hasAcceptedLines = entry.CusReconEntryLines.Any(x => acceptedLineCustomsStatuses.Contains(x.CRL_CustomsStatus));
				if (hasAcceptedLines)
				{
					MapCusReconEntryCurrentSnapshotToLodgedSnapshot(entry);
				}
				else
				{
					entry.CurrentSnapshot?.Delete();
				}
			}
		}

		static string GetEmailBody(CusReconDeclaration declaration, ICUSREC provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("65667D52-CE80-4BBF-A82F-6D208AD1A997", "Your Monthly Closing Declaration for Job {0} received a Customs Receipt Message. For details please follow the link to the job.", declaration.CRD_JobReferenceNumber));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var referenceNumber = provider.ReferenceNumber;
			var mrn = provider.MRN;
			var localReferenceNumber = provider.LocalReferenceNumber;
			var registrationDate = provider.RegistrationDate;

			if (!string.IsNullOrEmpty(referenceNumber) || !string.IsNullOrEmpty(mrn) || !localReferenceNumber.IsEmpty || !registrationDate.IsEmpty)
			{
				var tableCreator = new HtmlTableCreator();
				if (!string.IsNullOrEmpty(mrn))
				{
					tableCreator.WriteRow(Res.GetString("513AF0AE-B472-4CBE-A0D5-53215DF839A5", "MRN"), mrn);
				}

				if (!string.IsNullOrEmpty(referenceNumber))
				{
					tableCreator.WriteRow(Res.GetString("EE2050E3-5553-4EDC-9034-206E45B19210", "Registration Number"), referenceNumber);
				}

				if (!localReferenceNumber.IsEmpty)
				{
					tableCreator.WriteRow(Res.GetString("2E88AD1E-0D29-4497-916B-8D44907D1FF7", "Local Reference Number"), localReferenceNumber);
				}

				if (!registrationDate.IsEmpty)
				{
					tableCreator.WriteRow(Res.GetString("4E8981A4-E005-4FC3-82A4-B639DA4A1B95", "Registration Date"), registrationDate.ToString("dd.MM.yyyy"));
				}
				htmlBody.Append(tableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}

		public const string ATLASNotificationCodes_819 = "819";
		public const string MonthlyClosingLinesNoteDescription = "MonthlyClosingLines";
	}
}
