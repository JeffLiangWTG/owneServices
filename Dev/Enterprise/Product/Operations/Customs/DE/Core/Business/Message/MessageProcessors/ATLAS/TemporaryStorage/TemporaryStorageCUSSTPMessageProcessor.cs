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
	public sealed class TemporaryStorageCUSSTPMessageProcessor : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<ICUSSTP>, ICUSSTP>
	{
		public TemporaryStorageCUSSTPMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool MustHaveLinkedObject => true;

		protected override string MessageFriendlyNameCore => Res.GetString("C2F82B89-DFD9-4625-A932-BC1EB77EA9D5", "Temporary Storage CUSSTP Message Processor");

		protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSSTP> message)
		{
			var regHeader = (CusTempStorageRegHeader)message.EM_LinkedObject;
			var dataProvider = message.DataProvider;
			var missingRegisters = new ZStringBuilder();
			var invalidPackageQty = new ZStringBuilder();
			foreach (var goodsItem in dataProvider.GoodsItems)
			{
				var regLine = regHeader.GetRegLine(goodsItem.SequenceNumber);
				if (regLine != null)
				{
					regLine.SRL_CustomsStatus = GetCustomsStatus(goodsItem.CustomsGoodsStatus, message.PK);
					var comment = Res.GetString("14f6ff3e-db4d-449a-a5b7-9fe0e460c7c5", "Status Changed to:{0}", regLine.SRL_CustomsStatus);
					CreateRegLineTransaction(regLine, ZInt.Zero, ZString.Empty, ZString.Empty, TransactionTypes.Codes.StatusChange, dataProvider.MessageIdentifier, ZDecimal.Zero, comment, invalidPackageQty);
				}
				else
				{
					missingRegisters.Append(Res.GetString("f1f01ea0-c2f5-4b03-be5c-ba95226e280f", "Reference Number: {0}; Sequence Number: {1}", regHeader.SRH_Reference, goodsItem.SequenceNumber));
				}
			}
			SendEmail(message, regHeader);
			FinalizeNoteText(missingRegisters, MissingRegistersNoteText);
			factory.CreateStmNoteForEdiMessage(message.PK, missingRegisters.ToStringWithNewLineBetweenAppends());
			message.SetLogbookRegistrationNumber(new ZString[] { dataProvider.ReferenceNumber, dataProvider.MRN });
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSSTP> message) => GetLinkedObjectFromReference(message.Factory, message.DataProvider?.ReferenceNumber, message.DataProvider?.MRN);

		ZString GetCustomsStatus(ZString submittedCode, ZGuid messagePK)
		{
			switch (submittedCode)
			{
				case "0000":
				case "0100":
					return CustomsStatusList.Codes.TST;
				case "0001":
				case "0101":
				case "0102":
				case "0104":
				case "0105":
					return CustomsStatusList.Codes.LCK;
				default:
					{
						Logger.LogWarning(Res.GetString("66BEFFAC-2DB6-4735-843B-AD28391630AE", "We get an unknown Customs Intervention Code '{0}' in the message with PK '{1}'.", submittedCode, messagePK));

						return ZString.Empty;
					}
			}
		}

		void SendEmail(AtlasInboundEDIMessage<ICUSSTP> message, CusTempStorageRegHeader regHeader)
		{
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, regHeader
				, Res.GetString("AE2EF1D8-D94A-4AAD-BF74-95A1D7BEF5D5", "SumA CUSSTP – Announcement of a Control")
				, GetEmailBody(regHeader.SRH_Reference, message.DataProvider)
				, false
				, message.Branch
				, regHeader
				, () => null);
		}

		static string GetEmailBody(string srhReference, ICUSSTP provider)
		{
			var referenceNumber = provider.ReferenceNumber;
			var localReferenceNumber = provider.LocalReferenceNumber;
			var mrn = provider.MRN;
			var notificationDateTime = provider.NotificationDateTime;

			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("2BD73C06-5590-47F3-8D43-B08035CF8F2D", @"Your SumA Register {0} received an Announcement of a Control. For details please follow the link to the register.", srhReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			var tableCreator = new HtmlTableCreator();

			if (!string.IsNullOrEmpty(mrn))
			{
				tableCreator.WriteRow(Res.GetString("9BC66107-0BE2-4D7A-A133-86DA1C755484", "MRN"), mrn);
			}
			if (!referenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("AA779C7B-86B3-414D-A09C-2E029046EC92", "Registration Number"), referenceNumber);
			}
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("CEB664F5-44CC-4E5C-A29A-A4A6F2CD9F6D", "Local Reference Number"), localReferenceNumber);
			}
			if (!notificationDateTime.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("CB279ECD-3FD5-4BCE-80A2-C504FFF43319", "Notification Date/Time"), notificationDateTime);
			}

			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}
	}
}
