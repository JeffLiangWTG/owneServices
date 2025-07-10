using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public abstract class TemporaryStorageMessageProcessor<TEDIMessage, TDataProvider> : DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage>
		where TDataProvider : IDataProvider
		where TEDIMessage : AtlasInboundEDIMessage<TDataProvider>
	{
		protected TemporaryStorageMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.DECustomsAtlasSystem;

		protected static string MissingRegistersNoteText => Res.GetString("80945b86-df59-4c2c-b8af-c35342a24f93", "This is the list of Reference Number and Sequence Number system could not locate matching Register records for:");

		protected static string InvalidPackageQtyNoteText => Res.GetString("60f55898-3015-46c0-bea2-6e8609bca490", "This is the list of Register Lines where Package Qty would cause negative Packages Remaining value:");

		protected static string MissingDecLineNoteText => Res.GetString("31729f24-e389-4f7b-81e2-096d48532fc2", "This is the list of Sequence Numbers system could not locate matching {0} records for:", nameof(CusTempStorageLine));

		protected sealed override BusinessObject GetDocumentLinkingObject(BusinessObject businessObject) => businessObject is CusTempStorageDec dec ? dec.StorageHeader : businessObject;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var result = ZGuid.Invalid;
			if (linkedObject is CusTempStorageDec dec)
			{
				var storageHeader = dec.StorageHeader;
				if (storageHeader != null)
				{
					result = storageHeader.SJH_GB;
				}
			}
			return result;
		}

		protected override List<AttachedDocument> GetAttachedDocuments(TEDIMessage message) => message.AttachedDocuments;

		protected void CreateRegLineTransaction(CusTempStorageRegLine regLine, ZInt packageQtyToSet, ZString reference, ZString referenceType, ZString transactionType, ZString internalRefNumber, ZDecimal grossWeight, ZString comments, ZStringBuilder errorBuilder)
		{
			if (!CheckSRL_PackagesRemainingConstraintViolation(regLine, packageQtyToSet, errorBuilder))
			{
				var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
				regLineTransaction.SRT_PackageQty = packageQtyToSet;
				regLineTransaction.SRT_Reference = reference;
				regLineTransaction.SRT_ReferenceType = referenceType;
				regLineTransaction.SRT_TransactionType = transactionType;
				regLineTransaction.SRT_InternalReferenceNumber = internalRefNumber;
				regLineTransaction.SRT_GrossWeight = grossWeight;
				regLineTransaction.SRT_Comments = comments;
			}
		}

		protected static ZBool TryUpdateRegHeaderCustomsStatus(CusTempStorageRegHeader header, Func<EU.TemporaryStorage.Business.CusTempStorageRegLine, bool> validTempStorageLine, ZString headerStatusToSet)
		{
			var result = header.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().All(validTempStorageLine);
			if (result)
			{
				header.SRH_Status = headerStatusToSet;
			}
			return result;
		}

		protected void FinalizeNoteText(ZStringBuilder noteStringBuilder, ZString noteText)
		{
			if (!noteStringBuilder.IsEmpty)
			{
				noteStringBuilder.Prepend(noteText);
			}
		}

		protected bool Custodian_IsCW1MessagingOrganizationAndInterchangeRecipient_Or_UnknownOrganization(BusinessObjectFactory factory, ZString custodianEoriNumber, ZString custodianEoriBranch, ZString interchangeRecipientEoriNumber, ZString interchangeRecipientEoriBranch)
		{
			var result = false;
			if (factory.IsCW1MessagingOrganization(custodianEoriNumber, custodianEoriBranch))
			{
				if (custodianEoriNumber == interchangeRecipientEoriNumber && custodianEoriBranch == interchangeRecipientEoriBranch)
				{
					result = true;
				}
			}
			else
			{
				result = true;
			}
			return result;
		}

		protected ZString GetNoteText(ZStringBuilder missingRegisters, ZStringBuilder invalidPackageQty)
		{
			var noteText = ZString.Empty;
			FinalizeNoteText(missingRegisters, MissingRegistersNoteText);
			FinalizeNoteText(invalidPackageQty, InvalidPackageQtyNoteText);
			foreach (var stringBuilder in new ZStringBuilder[] { missingRegisters, invalidPackageQty })
			{
				if (!stringBuilder.IsEmpty)
				{
					noteText += noteText.IsEmpty ? stringBuilder.ToStringWithNewLineBetweenAppends() : "\r\n\r\n" + stringBuilder.ToStringWithNewLineBetweenAppends();
				}
			}
			return noteText;
		}

		protected BusinessObject GetLinkedObjectFromReference(BusinessObjectFactory factory, string referenceNumber, string mrn = null) => CusTempStorageRegHeader.Load(factory, referenceNumber, mrn);

		protected override ZString GetMessageIdentifier(TEDIMessage message) => message.DataProvider?.MessageIdentifier ?? ZString.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected void CreateOrUpdateRegHeader<T>(BusinessObjectFactory factory, ref CusTempStorageRegHeader regHeader, AtlasInboundEDIMessage<T> message, ZString referenceNumber, ZString statusCode)
			where T : IUnderCustomsControl
		{
			var dataProvider = message.DataProvider;
			if (regHeader == null)
			{
				regHeader = factory.New<CusTempStorageRegHeader>();
				regHeader.SRH_Reference = referenceNumber;
				regHeader.SRH_Status = statusCode;
				if (message.EM_LinkedObject == null)
				{
					message.EM_LinkedObject = regHeader;
				}
			}
			regHeader.SRH_ArrivalDate = dataProvider.ArrivalDate;
			regHeader.SRH_PresentationDate = dataProvider.PresentationDate;
			regHeader.SRH_PreviousReferenceType = dataProvider.PreviousReferenceType;
			regHeader.SRH_PreviousReference = dataProvider.PreviousReferenceNumber;
			regHeader.SRH_CustomsOffice = dataProvider.CustomsOfficeReferenceNumber;
			regHeader.SRH_InternalReference = dataProvider.LocalReferenceNumber;
		}

		protected void UpdateRegLine(CusTempStorageRegLine regLine, IUnderCustomsControlGoodsItem goodsItem)
		{
			regLine.SRL_OwnerReferenceType = goodsItem.OwnerReferenceType;
			regLine.SRL_OwnerReference = goodsItem.OwnerReferenceNumber;
			regLine.SRL_LocationOfGoods = goodsItem.LocationOfGoods;
			regLine.SRL_GoodsDescription = goodsItem.GoodsDescription;
			regLine.SRL_PackageType = goodsItem.PackageType;
			regLine.SRL_CustodianIdentifier = goodsItem.CustodianReferenceNumber;
			regLine.SRL_CustodianIdentifierBranchNo = goodsItem.CustodianSubsidiaryNumber;
			regLine.SRL_GoodsOwnerIdentifier = goodsItem.DisposalEntitledTraderReferenceNumber;
			regLine.SRL_GoodsOwnerIdentifierBranchNo = goodsItem.DisposalEntitledTraderSubsidiaryNumber;
			regLine.SRL_UnionStatus = goodsItem.CustomsGoodsStatus;
			if (!goodsItem.LimitDate.IsEmpty)
			{
				regLine.SRL_LimitDate = goodsItem.LimitDate;
			}
		}

		protected void UpdateDeclarationFromLinkedObject(BusinessObject linkedObject, ZString referenceNumber)
		{
			if (linkedObject is CusTempStorageDec declaration)
			{
				declaration.STH_MessageStatus = EDIMessageStatusList.Codes.ProcessedOK;
				declaration.ReferenceNumber = referenceNumber;
			}
		}

		protected void UpdateTempStorageLinesCustomsStatus(BusinessObjectFactory factory, ZString referenceNumber, ImmutableHashSet<ZInt> lineNumbersInMessage, ZString statusCode)
		{
			var cusEntries = CusEntryNumber.Load(factory, CusEntryNumberTypes.Germany.SumAEntryNumber, referenceNumber, Core.Constants.CountryCodes.Germany);
			if (cusEntries.Length > 0)
			{
				var cusprlDeclarationQuery = new ZQuery(CusTempStorageDecSchema.STH_DeclarationType, TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger);
				cusprlDeclarationQuery.AddToFilter(CusTempStorageDecSchema.PK, cusEntries.Select(x => x.CE_ParentID));
				var declarations = factory.Load<CusTempStorageDec>(cusprlDeclarationQuery);
				foreach (var declaration in declarations)
				{
					var linesToUpdate = declaration.CusTempStorageLines.Cast<CusTempStorageLine>().Where(x => lineNumbersInMessage.Contains(x.TSL_LineNo));
					foreach (var line in linesToUpdate)
					{
						line.TSL_CustomsStatus = statusCode;
						Logger.Log(FormattableString.Invariant($"Update the status to {statusCode} on line number {line.TSL_LineNo} for Temporary Storage Header {declaration.StorageHeader.SJH_JobReference}"), Integration.LogType.Information);
					}
				}
			}
		}

		protected IRegistryItem GetLinkedObjectDependentEmailGroupRegistryItem(BusinessObject linkedObject, IRegistryItem emailGroupRegistryItemIfSolicitedMessage)
		{
			IRegistryItem result = null;
			if (linkedObject is CusTempStorageDec)
			{
				result = emailGroupRegistryItemIfSolicitedMessage;
			}
			else if (linkedObject is CusTempStorageRegHeader)
			{
				result = EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited;
			}
			return result;
		}

		bool CheckSRL_PackagesRemainingConstraintViolation(CusTempStorageRegLine regLine, ZInt packageQtyToSet, ZStringBuilder errorBuilder)
		{
			var violation = false;
			var packagesRemaining = regLine.CalculatePackageQtySumFromTransactions();
			if (packagesRemaining + packageQtyToSet < 0)
			{
				errorBuilder.Append(Res.GetString("e2dac7ca-b028-4aef-933a-a1bccf152658", "Reference Number: {0}; Sequence Number: {1}; Packages Remaining: {2}; Package Quantity: {3}", regLine.RegHeader.SRH_Reference, regLine.SRL_LineNumber.ToString(), packagesRemaining.ToString(), packageQtyToSet.ToString()));
				violation = true;
			}
			return violation;
		}
	}
}
