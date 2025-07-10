using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using BondedWarehouseMessageProcessor = Enterprise.Customs.Business.MessageProcessors.BondedWarehouseMessageProcessor;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsDEPRELMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IDEPREL>, IDEPREL>
	{
		public NctsDEPRELMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("4C320B34-0B9B-4B46-85F4-C8279A016304", "NCTS DEPREL Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDEPREL> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override bool UpdateWarehouseCore => CurrentMessage?.EM_LinkedObject is NctsDepartureMovementHeader movementHeader && !movementHeader.BM_OA_WarehouseAddress.IsEmpty && !movementHeader.Header.Bills.Any(b => b.IsOutwardOrderImported);

		protected override IWarehouseIntegrationSupporter WarehouseIntegrationSupporter => ((NctsDepartureMovementHeader)CurrentMessage.EM_LinkedObject).Header;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IDEPREL> message)
		{
			var dataProvider = message.DataProvider;
			var movementReferenceNumber = dataProvider.MovementReferenceNumber;
			var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var header = movementHeader.Header;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(movementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);

			var movementReferenceEntryNumber = header.MovementReferenceEntryNumber;
			movementReferenceEntryNumber.CE_EntryNum = movementReferenceNumber;
			movementReferenceEntryNumber.CE_IssueDate = ZDateTime.Now;
			movementReferenceEntryNumber.CE_ExpiryDate = dataProvider.LimitDate;

			movementHeader.BM_ExportDate = dataProvider.LimitDate;
			movementHeader.BM_ValuationDate = movementReferenceEntryNumber.CE_IssueDate;

			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;
			attachedDocumentsCached = message.AttachedDocuments;
			emailSubjectSuffixProvider = new NCTSEmailSubjectSuffixProvider(movementHeader);

			movementHeader.GuaranteeTransactionCoordinator.ConfirmTransactions();

			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, header
				, Res.GetString("F7094FC9-A3F6-4B54-BEF5-BFCDFF5FAE27", "NCTS Departure Release Message")
				, GetEmailBody(header, movementReferenceNumber)
				, false
				, message.Branch
				, header
				, dataProvider.ReferencedMessageIdentifier);

			if (!header.MovementReferenceNumber.IsEmpty && header.Bills.Any(b => b.IsOutwardOrderImported))
			{
				ProcessOutwardOrderImported(header, factory);
			}
		}

		void ProcessOutwardOrderImported(NctsHeader header, BusinessObjectFactory factory)
		{
			var pivots = factory.Load<WhsDocketJobPivot>(new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, header.PK));
			var dockets = factory.Load<WhsDocket>(new ZQuery(WhsDocketSchema.PK, pivots.Select(x => x.WV_WD_Docket)));
			var docketIds = dockets.Select(x => x.WD_DocketID);

			var docketLineQuery = new ZDBOnlyQuery(typeof(IWhsDocketLine)).AddToFilter(WhsDocketLineSchema.WE_WD, dockets.Select(x => x.PK));
			var docketLines = factory.Load<IWhsDocketLine>(docketLineQuery);

			foreach (var cargoDesc in header.Bills.SelectMany(bill => bill.GoodsItems).Where(x => docketIds.Contains(x.BY_BondedWHSOrderNumber)))
			{
				var docketForBill = dockets.Single(docket => docket.WD_DocketID == cargoDesc.BY_BondedWHSOrderNumber);
				var orderLine = docketLines.Single(x => x.WE_LineNo == cargoDesc.BY_BondedWHSOrderLineNumber && x.WE_WD == docketForBill.PK);
				var bwhAttribute = BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsDocketLine(orderLine, factory);

				if (bwhAttribute != null)
				{
					bwhAttribute.WB_EntryKey = header.MovementReferenceNumber;
					bwhAttribute.WB_EntryLineNo = (ZShort)cargoDesc.BY_DeclarationGoodsItemNumber;
				}
			}
		}

		NCTSEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			AttachDocumentsToEmail(email, attachedDocumentsCached);
			return email;
		}

		ZString GetEmailBody(NctsHeader header, string movementReferenceNumber)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("902722E6-F811-40CC-9174-6C62220285F2", "Your NCTS Declaration Message for Job {0} have a Release Message. For details please follow the Link to the Job.", header.BH_JobReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("168A29B6-2B8C-4E01-9602-D05EA0777392", "MRN:"), movementReferenceNumber);
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}

		protected override BondedWarehouseMessageProcessor GetNewBondedWarehouseMessageProcessor(Action<EmailDef, EDIMessage> sendEmail)
		{
			return new BondedWarehouseNctsHeaderMessageProcessor(BwhMessagePk, null, sendEmail);
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
	}
}
