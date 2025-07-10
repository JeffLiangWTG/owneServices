using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public sealed class TemporaryStorageCUSFINMessageProcessor : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<ICUSFIN>, ICUSFIN>
	{
		public TemporaryStorageCUSFINMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("4F0DA421-F370-4856-8846-07CD689E6729", "Temporary Storage CUSFIN Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSFIN> message) => null;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSFIN> message)
		{
			var status = EDIMessage.Status.Discarded;
			var dataProvider = message.DataProvider;

			if (dataProvider != null)
			{
				status = EDIMessage.Status.ProcessedOK;

				var missingRegisters = new ZStringBuilder();
				var invalidPackageQty = new ZStringBuilder();
				var linesToUpdateCustomsStatus = new Dictionary<ZString, List<(ZInt LineNumber, ZString CustomsStatus)>>();
				var additionalRegistrationNumber = dataProvider.AdditionalRegistrationNumber;
				var additionalReferenceNumber = dataProvider.AdditionalReferenceNumber;
				var mrn = dataProvider.MRN;
				var referenceType = dataProvider.CompletionType;
				var reference = GetReference(referenceType, additionalRegistrationNumber, additionalReferenceNumber, mrn);
				foreach (var goodsItem in dataProvider.GoodsItems)
				{
					var referencedRegistrationNumber = goodsItem.ReferencedRegistrationNumber;
					var actualRegNumber = referencedRegistrationNumber.IsEmpty ? (ZString)goodsItem.MRN : referencedRegistrationNumber;
					var referencedSequenceNumber = goodsItem.ReferencedSequenceNumber;
					var referencedRegHeader = CusTempStorageRegHeader.Load(factory, actualRegNumber);
					var referencedRegLine = referencedRegHeader?.GetRegLine(referencedSequenceNumber);
					if (referencedRegLine != null)
					{
						SubscribeDocumentLinking(referencedRegHeader);
						var packageQuantity = goodsItem.Quantity;
						if (Custodian_IsCW1MessagingOrganizationAndInterchangeRecipient_Or_UnknownOrganization(factory, referencedRegLine.SRL_CustodianIdentifier, referencedRegLine.SRL_CustodianIdentifierBranchNo, dataProvider.InterchangeRecipientReferenceNumber, dataProvider.InterchangeRecipientSubsidiaryNumber)
							&& packageQuantity >= 0)
						{
							var packageQuantityToSet = GetPackageQuantity(goodsItem.CancellationFlag, packageQuantity);
							CreateRegLineTransaction(referencedRegLine, packageQuantityToSet, reference, referenceType, TransactionTypes.Codes.Transaction, dataProvider.MessageIdentifier, 0, ZString.Empty, invalidPackageQty);
							UpdateRegLineCustomsStatus(referencedRegLine);
							UpdateRegHeaderCustomsStatus(referencedRegHeader);
							AddToLinesToUpdateCustomsStatusDictionary(linesToUpdateCustomsStatus, actualRegNumber, ZInt.Parse(referencedSequenceNumber), referencedRegLine.SRL_CustomsStatus);
						}
					}
					else
					{
						missingRegisters.Append(Res.GetString("26b1650d-8d4b-4c72-ad3e-21aa1442d3d7", "Reference Number: {0}; Sequence Number: {1}", actualRegNumber, referencedSequenceNumber));
					}
				}

				factory.CreateStmNoteForEdiMessage(message.PK, GetNoteText(missingRegisters, invalidPackageQty));
				foreach (var item in linesToUpdateCustomsStatus)
				{
					UpdateTempStorageLinesCustomsStatus(factory, item.Key, item.Value.ToImmutableHashSet());
				}

				var referencedRegistrationNumbers = dataProvider.GoodsItems.Select(x => x.ReferencedRegistrationNumber);
				var movementReferenceNumbers = dataProvider.GoodsItems.Select(x => (ZString)x.MRN);
				var additionalNumbers = new ZString[] { additionalReferenceNumber, additionalRegistrationNumber, mrn };
				var goodsItemsRegistrationNumbers = referencedRegistrationNumbers.Concat(movementReferenceNumbers);
				message.SetLogbookRegistrationNumber(goodsItemsRegistrationNumbers.Concat(additionalNumbers));
			}
			message.EM_Status = status;
		}

		void UpdateRegLineCustomsStatus(CusTempStorageRegLine regLine)
		{
			regLine.SRL_CustomsStatus = regLine.SRL_PackagesRemaining.IsEmpty ? CustomsStatusList.Codes.FIN : CustomsStatusList.Codes.PAC;
		}

		void UpdateRegHeaderCustomsStatus(CusTempStorageRegHeader regHeader)
		{
			if (!TryUpdateRegHeaderCustomsStatus(regHeader, (line) => line.SRL_CustomsStatus == CustomsStatusList.Codes.DEL || line.SRL_CustomsStatus == CustomsStatusList.Codes.FIN, CustomsStatusList.Codes.FIN))
			{
				regHeader.SRH_Status = CustomsStatusList.Codes.PAC;
			}
		}

		void AddToLinesToUpdateCustomsStatusDictionary(Dictionary<ZString, List<(ZInt LineNumber, ZString CustomsStatus)>> linesToUpdate, ZString referenceNumber, ZInt lineNumber, ZString customsStatus)
		{
			if (linesToUpdate.TryGetValue(referenceNumber, out var value))
			{
				value.Add((lineNumber, customsStatus));
			}
			else
			{
				linesToUpdate.Add(referenceNumber, new List<(ZInt LineNumber, ZString CustomsStatus)> { (lineNumber, customsStatus) });
			}
		}

		void UpdateTempStorageLinesCustomsStatus(BusinessObjectFactory factory, ZString referenceNumber, ImmutableHashSet<(ZInt LineNumber, ZString CustomsStatus)> lineNumbersAndStatusToSet)
		{
			var cusEntries = CusEntryNumber.Load(factory, CusEntryNumberTypes.Germany.SumAEntryNumber, referenceNumber, Core.Constants.CountryCodes.Germany);
			if (cusEntries.Length > 0)
			{
				var cusprlDeclarationQuery = new ZQuery(CusTempStorageDecSchema.STH_DeclarationType, TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger);
				cusprlDeclarationQuery.AddToFilter(CusTempStorageDecSchema.PK, cusEntries.Select(x => x.CE_ParentID));
				var declarations = factory.Load<CusTempStorageDec>(cusprlDeclarationQuery);
				foreach (var declaration in declarations)
				{
					var jobReference = declaration.StorageHeader.SJH_JobReference;
					var linesToUpdate = declaration.CusTempStorageLines.Cast<CusTempStorageLine>().Where(x => lineNumbersAndStatusToSet.Any(y => y.LineNumber == x.TSL_LineNo));
					foreach (var line in linesToUpdate)
					{
						var statusToSet = lineNumbersAndStatusToSet.Single(x => x.LineNumber == line.TSL_LineNo).CustomsStatus;
						line.TSL_CustomsStatus = statusToSet;
						Logger.Log(FormattableString.Invariant($"Update the status to {statusToSet} on line number {line.TSL_LineNo} for Temporary Storage Header {jobReference}"), Integration.LogType.Information);
					}
				}
			}
		}

		static ZInt GetPackageQuantity(ZString cancellationFlag, ZInt packageQuantity) => cancellationFlag == "J" ? packageQuantity : (ZInt)(packageQuantity * -1);

		static ZString GetReference(ZString referenceType, ZString additionalRegistrationNumber, ZString additionalReferenceNumber, string mrn)
		{
			switch (referenceType)
			{
				case TransactionReferenceTypes.Codes.AUFT:
				case TransactionReferenceTypes.Codes.KONS:
				case TransactionReferenceTypes.Codes.MANU:
				case TransactionReferenceTypes.Codes.ZB:
				case TransactionReferenceTypes.Codes.WVV:
					return  additionalRegistrationNumber.IsEmpty ? mrn : additionalRegistrationNumber;
				default:
					return additionalReferenceNumber;
			}
		}
	}
}
