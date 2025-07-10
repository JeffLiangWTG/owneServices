using System;
using System.Collections.Generic;
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
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public sealed class TemporaryStorageCUSTSTMessageProcessor : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<ICUSTST>, ICUSTST>
	{
		public TemporaryStorageCUSTSTMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("8A25D82B-5CD5-4BF9-8A1A-C36CFF3DB1BC", "Temporary Storage CUSTST Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override IRegistryItem GetEmailGroupRegistryItem() => emailGroupRegistryItem;

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSTST> message)
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

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSTST> message)
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
				CreateOrUpdateRegHeader(factory, ref regHeader, message, effectiveReferenceNumber, CustomsStatusList.Codes.TST);
				SubscribeDocumentLinking(regHeader);
				CreateOrUpdateLines(factory, regHeader, message, CustomsStatusList.Codes.TST);
				UpdateDeclarationFromLinkedObject(linkedObject, effectiveReferenceNumber);
				UpdateTempStorageLinesCustomsStatus(factory, effectiveReferenceNumber, dataProvider.GoodsItems.Select(x => ZInt.Parse(x.SequenceNumber)).ToImmutableHashSet(), CustomsStatusList.Codes.TST);
				SendEmail(message, message.EM_LinkedObject);
				message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });
			}
			message.EM_Status = status;
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var result = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(result, attachedDocumentsCached);
			return result;
		}

		void CreateOrUpdateLines(BusinessObjectFactory factory, CusTempStorageRegHeader regHeader, AtlasInboundEDIMessage<ICUSTST> message, ZString statusCode)
		{
			var dataProvider = message.DataProvider;
			var invalidPackageQty = new ZStringBuilder();
			var shouldSetRegHeaderStatus = false;
			foreach (var goodsItem in dataProvider.GoodsItems)
			{
				var sequenceNumber = goodsItem.SequenceNumber;
				var line = regHeader.GetRegLine(sequenceNumber);
				if (line == null)
				{
					line = regHeader.CusTempStorageRegLines.AddNew();
					CreateRegLineTransaction(line, goodsItem.PackageQty, ZString.Empty, ZString.Empty, TransactionTypes.Codes.OpeningBalance, dataProvider.MessageIdentifier, goodsItem.GrossWeight, ZString.Empty, invalidPackageQty);
					line.SRL_LineNumber = ZInt.Parse(sequenceNumber);
					line.SRL_GrossWeightUQ = "KGM";
					line.SRL_CustomsStatus = statusCode;
				}
				CreateAdjustmentTransaction(line, goodsItem, invalidPackageQty);
				UpdateRegLine(line, goodsItem);

				SetRegLineStatus(line, goodsItem, dataProvider, out var lineStatusSetToFin);
				shouldSetRegHeaderStatus |= lineStatusSetToFin;

				var fCUSTSTGoodsItem = goodsItem as ICUSTSTGoodsItem;
				CreateCustodianAddressStmNoteForRegHeader(regHeader, fCUSTSTGoodsItem, sequenceNumber);
				CreateCustodyPlaceAddressStmNoteForRegHeader(regHeader, fCUSTSTGoodsItem, sequenceNumber);
			}

			if (shouldSetRegHeaderStatus)
			{
				SetRegHeaderStatus(regHeader);
			}

			FinalizeNoteText(invalidPackageQty, InvalidPackageQtyNoteText);
			factory.CreateStmNoteForEdiMessage(message.PK, invalidPackageQty.ToStringWithNewLineBetweenAppends());
		}

		static void SetRegLineStatus(CusTempStorageRegLine line, IUnderCustomsControlGoodsItem goodsItem, ICUSTST dataProvider, out bool lineStatusSetToFin)
		{
			lineStatusSetToFin = false;
			if (dataProvider.RecipientReferenceNumber != goodsItem.CustodianReferenceNumber &&
				dataProvider.RecipientReferenceNumber != goodsItem.DisposalEntitledTraderReferenceNumber)
			{
				line.SRL_CustomsStatus = CustomsStatusList.Codes.FIN;
				lineStatusSetToFin = true;
			}
		}

		static void SetRegHeaderStatus(CusTempStorageRegHeader regHeader)
		{
			if (!TryUpdateRegHeaderCustomsStatus(regHeader, (line) => line.SRL_CustomsStatus == CustomsStatusList.Codes.DEL || line.SRL_CustomsStatus == CustomsStatusList.Codes.FIN, CustomsStatusList.Codes.FIN))
			{
				regHeader.SRH_Status = CustomsStatusList.Codes.PAC;
			}
		}

		void CreateAdjustmentTransaction(CusTempStorageRegLine regLine, IUnderCustomsControlGoodsItem goodsItem, ZStringBuilder errorBuilder)
		{
			var packagesRemaining = regLine.SRL_PackagesRemaining;
			if (packagesRemaining != goodsItem.PackageQty)
			{
				var packageQty = goodsItem.PackageQty - packagesRemaining;
				CreateRegLineTransaction(regLine, packageQty, ZString.Empty, ZString.Empty, TransactionTypes.Codes.Transaction, ZString.Empty, goodsItem.GrossWeight, ZString.Empty, errorBuilder);
			}
		}

		void CreateCustodianAddressStmNoteForRegHeader(CusTempStorageRegHeader regHeader, ICUSTSTGoodsItem goodsItem, ZString sequenceNumber)
		{
			if (goodsItem.CustodianReferenceNumber.IsEmpty)
			{
				var custodianAddress = goodsItem.CustodianAddress;
				if (custodianAddress != null)
				{
					var custodianAddressInfo = new ZStringBuilder();
					custodianAddressInfo.AppendLine(goodsItem.CustodianName);
					custodianAddressInfo.AppendLine(custodianAddress.Line);
					custodianAddressInfo.AppendLine(string.Join(" ", custodianAddress.Country, custodianAddress.Postcode, custodianAddress.City));
					custodianAddressInfo.AppendLine(custodianAddress.District);
					regHeader.CreateStmNote(custodianAddressInfo.ToString(), ZString.Format((NoResString)"Custodian Address for Line {0}", sequenceNumber)); // Debug Note
				}
			}
		}

		void CreateCustodyPlaceAddressStmNoteForRegHeader(CusTempStorageRegHeader regHeader, ICUSTSTGoodsItem goodsItem, ZString sequenceNumber)
		{
			if (goodsItem.CustodyPlaceCode.IsEmpty)
			{
				var custodyPlaceAddress = goodsItem.CustodyPlaceAddress;
				if (custodyPlaceAddress != null)
				{
					var custodyPlaceAddressInfo = new ZStringBuilder();
					custodyPlaceAddressInfo.AppendLine(goodsItem.CustodyPlaceInformation);
					custodyPlaceAddressInfo.AppendLine(custodyPlaceAddress.Line);
					custodyPlaceAddressInfo.AppendLine(string.Join(" ", custodyPlaceAddress.Postcode, custodyPlaceAddress.City));
					custodyPlaceAddressInfo.AppendLine(custodyPlaceAddress.District);
					regHeader.CreateStmNote(custodyPlaceAddressInfo.ToString(), ZString.Format((NoResString)"Custody Place for Line {0}", sequenceNumber)); // Debug Note
				}
			}
		}

		void SendEmail(AtlasInboundEDIMessage<ICUSTST> message, BusinessObject linkedObject)
		{
			attachedDocumentsCached = message.AttachedDocuments.Where(x => x.Type.Code.HasValue && ((string)x.Type.Code).Equals(Core.Constants.RefDocTypes.EntryPrint, StringComparison.OrdinalIgnoreCase)).ToArray();
			emailGroupRegistryItem = GetLinkedObjectDependentEmailGroupRegistryItem(linkedObject, EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements);
			var subject = Res.GetString("2E329625-0C95-4297-ACB3-E03AD3DFAA01", "SumA CUSTST - Temporary Storage");
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
			htmlBody.Append(Res.GetString("31C75FC9-5426-4D3C-A42C-BFDB916581D6", @"Your SumA {0} {1} received a Temporary Storage message. For details please follow the link to the SumA {0}.", jobDescription, reference));
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
				tableCreator.WriteRow(Res.GetString("2BFA1C08-5365-438E-BFB9-F1BADE4BD90B", "Registration Number"), referenceNumber);
			}
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("1FE6BB65-B688-47D1-AB4E-3C5A16EEBA4E", "Local Reference Number"), localReferenceNumber);
			}
			if (!presentationDate.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("7B44D77F-CEA9-4BC0-A2C5-5FB68A79A5FF", "Presentation Date"), presentationDate);
			}
			return tableCreator.ToHtml();
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
		IRegistryItem emailGroupRegistryItem;
	}
}
