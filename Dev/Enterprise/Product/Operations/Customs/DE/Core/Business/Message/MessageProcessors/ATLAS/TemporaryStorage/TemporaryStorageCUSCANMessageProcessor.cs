using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public sealed class TemporaryStorageCUSCANMessageProcessor : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<ICUSCAN>, ICUSCAN>
	{
		public TemporaryStorageCUSCANMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("760470E9-1384-4029-9F0A-EE921E213B4A", "Temporary Storage CUSCAN Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSCAN> message)
		{
			var regHeader = (CusTempStorageRegHeader)message.EM_LinkedObject;
			var dataProvider = message.DataProvider;
			var missingRegisters = new ZStringBuilder();
			var invalidPackageQty = new ZStringBuilder();
			var messageIdentifier = dataProvider.MessageIdentifier;
			foreach (var goodsItem in dataProvider.GoodsItems)
			{
				var regLine = regHeader.GetRegLine(goodsItem.SequenceNumber);
				if (regLine != null)
				{
					var comment = Res.GetString("6ef699d1-5fa9-4809-a9b2-431de4d77171", "Canceled by Customs (CUSCAN)");
					CreateRegLineTransaction(regLine, -regLine.CalculatePackageQtySumFromTransactions(), ZString.Empty, TransactionReferenceTypes.Codes.MANU, TransactionTypes.Codes.Transaction, messageIdentifier, ZDecimal.Zero, comment, invalidPackageQty);
					UpdateRegLine(regLine, goodsItem);
				}
				else
				{
					missingRegisters.Append(Res.GetString("e6ff76a1-d617-4fce-8369-592135783b32", "Reference Number: {0}; Sequence Number: {1}", regHeader.SRH_Reference, goodsItem.SequenceNumber));
				}
			}
			UpdateRegHeaderCustomsStatus(regHeader);
			SendEmail(message, regHeader);
			factory.CreateStmNoteForEdiMessage(message.PK, GetNoteText(missingRegisters, invalidPackageQty));
			message.SetLogbookRegistrationNumber(new ZString[] { dataProvider.ReferenceNumber, dataProvider.MRN });
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSCAN> message)
		{
			var dataProvider = message.DataProvider;
			BusinessObject result = null;
			if (dataProvider != null)
			{
				var factory = message.Factory;
				result = GetLinkedObjectFromReference(factory, dataProvider.ReferenceNumber, dataProvider.MRN);
			}
			return result;
		}

		protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited;

		void UpdateRegHeaderCustomsStatus(CusTempStorageRegHeader header)
		{
			if (!TryUpdateRegHeaderCustomsStatus(header, (line) => line.SRL_CustomsStatus == CustomsStatusList.Codes.DEL, CustomsStatusList.Codes.DEL))
			{
				TryUpdateRegHeaderCustomsStatus(header, (line) => line.SRL_CustomsStatus == CustomsStatusList.Codes.DEL || line.SRL_CustomsStatus == CustomsStatusList.Codes.FIN, CustomsStatusList.Codes.FIN);
			}
		}

		void UpdateRegLine(CusTempStorageRegLine line, ICUSCANGoodsItem goodsItem) => line.SRL_CustomsStatus = goodsItem.CustomsGoodsStatus == "04" ? CustomsStatusList.Codes.FIN : CustomsStatusList.Codes.DEL;

		void SendEmail(AtlasInboundEDIMessage<ICUSCAN> message, CusTempStorageRegHeader regHeader)
		{
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, regHeader
				, Res.GetString("C394D293-8AFA-49A9-B403-556AC4E83312", "SumA CUSCAN – Customs Cancellation Information")
				, GetEmailBody(regHeader.SRH_Reference, message.DataProvider)
				, false
				, message.Branch
				, regHeader
				, () => null);
		}

		static string GetEmailBody(string reference, ICUSCAN provider)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("4B89CDBE-3E36-459C-BBE5-2E1A6E057D1A", "Your SumA Register {0} received a Customs Cancellation Information. For details please follow the link to the register.", reference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			var mrn = provider.MRN;
			var reason = provider.Reason;
			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("FE91F514-5F01-4CCE-A3F1-D77F5194BF53", "Registration Number"), provider.ReferenceNumber);
			if (!string.IsNullOrWhiteSpace(mrn))
			{
				tableCreator.WriteRow(Res.GetString("15BF0AD6-6839-4C8F-B7C3-0EE9485CE437", "MRN"), mrn);
			}
			if (!reason.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("0A67CA17-5193-4AA6-931D-CC8C5E247933", "Reason"), reason);
			}
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}
	}
}
