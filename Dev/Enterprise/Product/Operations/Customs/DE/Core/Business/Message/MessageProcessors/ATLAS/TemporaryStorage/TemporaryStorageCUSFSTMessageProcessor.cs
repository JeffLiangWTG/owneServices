using System.Collections.Immutable;
using System.Linq;
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
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public sealed class TemporaryStorageCUSFSTMessageProcessor : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<IUnderCustomsControl>, IUnderCustomsControl>
	{
		public TemporaryStorageCUSFSTMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("D52D468B-1772-4DF2-A13B-B997979C40F1", "Temporary Storage CUSFST Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override IRegistryItem GetEmailGroupRegistryItem() => emailGroupRegistryItem;

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IUnderCustomsControl> message)
		{
			var dataProvider = message.DataProvider;
			BusinessObject result = null;
			if (message.DataProvider != null)
			{
				var factory = message.Factory;
				result = GetLinkedObjectFromOriginalMessage(factory, dataProvider.ReferencedMessageIdentifier);

				if (result == null)
				{
					result = GetLinkedObjectFromReference(factory, dataProvider.ReferenceNumber, dataProvider.MRN);
				}
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IUnderCustomsControl> message)
		{
			var status = EDIMessage.Status.Discarded;
			var dataProvider = message.DataProvider;
			var referenceNumber = dataProvider?.ReferenceNumber ?? ZString.Empty;
			var mrn = dataProvider?.MRN;
			if (!referenceNumber.IsEmpty || !string.IsNullOrEmpty(mrn))
			{
				var effectiveReferenceNumber = !referenceNumber.IsEmpty ? referenceNumber : (ZString)mrn;
				status = EDIMessage.Status.ProcessedOK;
				var linkedObject = message.EM_LinkedObject;
				var regHeader = linkedObject as CusTempStorageRegHeader ?? CusTempStorageRegHeader.Load(factory, referenceNumber, mrn);
				CreateOrUpdateRegHeader(factory, ref regHeader, message, effectiveReferenceNumber, CustomsStatusList.Codes.FIN);
				SubscribeDocumentLinking(regHeader);
				CreateOrUpdateRegLines(factory, regHeader, message, CustomsStatusList.Codes.FIN);
				UpdateDeclarationFromLinkedObject(linkedObject, effectiveReferenceNumber);
				UpdateTempStorageLinesCustomsStatus(factory, effectiveReferenceNumber, dataProvider.GoodsItems.Select(x => ZInt.Parse(x.SequenceNumber)).ToImmutableHashSet(), CustomsStatusList.Codes.FIN);
				SendEmail(message, message.EM_LinkedObject);
				message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn } );
			}
			message.EM_Status = status;
		}

		void CreateOrUpdateRegLines(BusinessObjectFactory factory, CusTempStorageRegHeader regHeader, AtlasInboundEDIMessage<IUnderCustomsControl> message, ZString statusCode)
		{
			var dataProvider = message.DataProvider;
			var invalidPackageQty = new ZStringBuilder();
			foreach (var goodsItem in dataProvider.GoodsItems)
			{
				var sequenceNumber = goodsItem.SequenceNumber;
				var line = regHeader.GetRegLine(sequenceNumber);
				if (line == null)
				{
					line = regHeader.CusTempStorageRegLines.AddNew();
					CreateFinalizationTransactions(line, dataProvider.MessageIdentifier, dataProvider.LocalReferenceNumber, goodsItem, invalidPackageQty);
					line.SRL_LineNumber = ZInt.Parse(sequenceNumber);
					line.SRL_GrossWeightUQ = "KGM";
					line.SRL_CustomsStatus = statusCode;
				}
				UpdateRegLine(line, goodsItem);
			}
			FinalizeNoteText(invalidPackageQty, InvalidPackageQtyNoteText);
			factory.CreateStmNoteForEdiMessage(message.PK, invalidPackageQty.ToStringWithNewLineBetweenAppends());
		}

		void CreateFinalizationTransactions(CusTempStorageRegLine line, ZString internalReferenceNumber, ZString localReferenceNumber, IUnderCustomsControlGoodsItem goodsItem, ZStringBuilder errorBuilder)
		{
			CreateRegLineTransaction(line, goodsItem.PackageQty, ZString.Empty, ZString.Empty, TransactionTypes.Codes.OpeningBalance, internalReferenceNumber, goodsItem.GrossWeight, localReferenceNumber, errorBuilder);
			CreateRegLineTransaction(line, -goodsItem.PackageQty, ZString.Empty, ZString.Empty, TransactionTypes.Codes.Transaction, internalReferenceNumber, goodsItem.GrossWeight, localReferenceNumber, errorBuilder);
		}

		void SendEmail(AtlasInboundEDIMessage<IUnderCustomsControl> message, BusinessObject linkedObject)
		{
			emailGroupRegistryItem = GetLinkedObjectDependentEmailGroupRegistryItem(linkedObject, EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements);
			var subject = Res.GetString("8A148931-95E4-4E0D-A40C-1CD29F80B1BB", "SumA CUSFST – Information on Completed C, X, D or Free Zone Goods");
			if (linkedObject is CusTempStorageDec declaration)
			{
				var storageHeader = declaration.StorageHeader;
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
					, storageHeader
					, subject
					, GetEmailBody((NoResString)"Declaration", storageHeader.SJH_JobReference, message.DataProvider)
					, false
					, message.Branch
					, declaration
					, () => declaration.Messages.LastSentOutgoingMessage);
			}
			else if (linkedObject is CusTempStorageRegHeader regHeader)
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
					, regHeader
					, subject
					, GetEmailBody((NoResString)"Register", regHeader.SRH_Reference, message.DataProvider)
					, false
					, message.Branch
					, regHeader
					, () => null);
			}
		}

		static string GetEmailBody(string jobDescription, string reference, IUnderCustomsControl provider)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("7F746945-80ED-45C8-AD33-69C32C8BDFDB", @"Your SumA {0} {1} received an Information on Completed C, X, D or Free Zone Goods. For details please follow the link to the SumA {0}.", jobDescription, reference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(GetTable(provider));
			return htmlBody.ToString();
		}

		static string GetTable(IUnderCustomsControl provider)
		{
			var referenceNumber = provider.ReferenceNumber;
			var localReferenceNumber = provider.LocalReferenceNumber;
			var mrn = provider.MRN;
			var presentationDate = provider.PresentationDate;
			var tableCreator = new HtmlTableCreator();
			if (!string.IsNullOrEmpty(mrn))
			{
				tableCreator.WriteRow(Res.GetString("66B7DC7B-C94E-4B56-BD5F-FE23EABF66CF", "MRN"), mrn);
			}
			if (!referenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("8E046DD0-F094-4789-B834-2779E7CAEE63", "Registration Number"), referenceNumber);
			}
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("AE2AEF6F-5C78-473A-84E4-859ABE4378DE", "Local Reference Number"), localReferenceNumber);
			}
			if (!presentationDate.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("DA6144C4-4BCC-4A89-8EC7-5E503B630248", "Presentation Date"), presentationDate);
			}
			return tableCreator.ToHtml();
		}

		IRegistryItem emailGroupRegistryItem;
	}
}
