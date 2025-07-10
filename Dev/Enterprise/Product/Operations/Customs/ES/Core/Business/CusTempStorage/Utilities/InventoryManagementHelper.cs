using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using static Enterprise.Customs.ES.Business.MessageSending.ESMessageSender;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public static class InventoryManagementHelper
{
	public static List<MessageBuilderData> InventoryManagementAction(BusinessObjectFactory factory, List<MessageBuilderData> messageBuildersData
		, Func<CusEntryHeader, ZString, ZString, IEnumerable<DataToReserveTSGoods>, MessageBuilderData, List<MessageBuilderData>, List<MessageBuilderData>> reserveTemporaryStorageGoodsWhenErrors)
	{
		var messageBuildersDataToContinue = new List<MessageBuilderData>();
		messageBuildersDataToContinue.AddRange(messageBuildersData);

		foreach (var builderData in messageBuildersData)
		{
			var messageBuilder = builderData.MessageBuilder;
			var entryHeader = builderData.EntryHeader;
			var goodsLocation = entryHeader.EntryInstruction.GoodsLocation.Address.AuthorisationNumber;

			var isInventoryManagementActionMessageTypesForImport = InventoryManagementActionMessageTypesForImport.Contains(messageBuilder.MessageType);
			var isInventoryManagementActionMessageTypesForEXS = InventoryManagementActionMessageTypesForEXS.Contains(messageBuilder.MessageType);
			var isInventoryManagementActionMessageTypesForH2 = InventoryManagementActionMessageTypesForH2.Contains(messageBuilder.MessageType);
			var isInventoryManagementActionMessageTypesForExport = InventoryManagementActionMessageTypesForExport.Contains(messageBuilder.MessageType);

			var premisesType = isInventoryManagementActionMessageTypesForExport ? CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility : CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

			if ((isInventoryManagementActionMessageTypesForImport || isInventoryManagementActionMessageTypesForEXS || isInventoryManagementActionMessageTypesForH2 || isInventoryManagementActionMessageTypesForExport)
				&& !goodsLocation.IsEmpty
				&& EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(entryHeader.CountryCode)
				&& EU.Business.TemporaryStorageHelper.IsLocationManagedInPremises(factory, goodsLocation, premisesType))
			{
				var isPDCAndCMPOrDVXAndCMP = (messageBuilder.MessageType == DeclarationMessageTypeList.Codes.TypeXDvdH2 || messageBuilder.MessageType == DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration)
										&& messageBuilder.MessageSubType == DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration;
				var docsToReserveGoods = GetDocToReserveGoods(isInventoryManagementActionMessageTypesForImport, isInventoryManagementActionMessageTypesForEXS, isInventoryManagementActionMessageTypesForH2, isInventoryManagementActionMessageTypesForExport);

				var (errorMessage, errorMessageVINs, dataToReserve) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(factory,
																												entryHeader.TemporaryStorageTransactionInternalReferenceNumber,
																												entryHeader.TemporaryStorageTransactionInternalReferenceType,
																												entryHeader.CH_BGMReference,
																												docsToReserveGoods,
																												goodsLocation,
																												entryHeader.GetEntryLineDataDeclaredToReserveTSGoods,
																												isAmendment: isPDCAndCMPOrDVXAndCMP,
																												formatDocRef: isInventoryManagementActionMessageTypesForExport ? null : DocumentHelper.GetDsdtMRNNumberFormat,
																												isLAME: isInventoryManagementActionMessageTypesForExport);

				if (!errorMessage.IsEmpty || !errorMessageVINs.IsEmpty)
				{
					(messageBuildersDataToContinue) = reserveTemporaryStorageGoodsWhenErrors(entryHeader, errorMessage, errorMessageVINs, dataToReserve, builderData, messageBuildersDataToContinue);
				}
				else
				{
					EU.Business.TemporaryStorageHelper.ReserveTemporaryStorageGoods(entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, dataToReserve);
				}
			}
		}

		return messageBuildersDataToContinue;
	}

	static ZString[] InventoryManagementActionMessageTypesForImport => new ZString[] { DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration };
	static ZString[] InventoryManagementActionMessageTypesForEXS => new ZString[] { DeclarationMessageTypeList.Codes.ExitSummaryDeclaration };
	static ZString[] InventoryManagementActionMessageTypesForH2 => new ZString[] { DeclarationMessageTypeList.Codes.DvdH2, DeclarationMessageTypeList.Codes.TypeXDvdH2 };
	static ZString[] InventoryManagementActionMessageTypesForExport => new ZString[] { DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageTypeList.Codes.ExportNotification };

	static ZString[] ImportPreviousDocsToReserveGoods => new ZString[] { PreviousDocumentHelper.PreviousDocumentCodeSUM };
	static ZString[] EXSPreviousDocsToReserveGoods => new ZString[] { PreviousDocumentHelper.PreviousDocumentCodeSUM, PreviousDocumentHelper.PreviousDocumentCodeXSUM, PreviousDocumentHelper.PreviousDocumentCodeN337 };
	static ZString[] H2PreviousDocsToReserveGoods => new ZString[] { PreviousDocumentHelper.PreviousDocumentCode337 };
	static ZString[] ExportSupportingDocsToReserveGoods => new ZString[] { SupportingDocumentHelper.SupportingDocumentCode1217 };

	static ZString[] GetDocToReserveGoods(bool isInventoryManagementActionMessageTypesForImport, bool isInventoryManagementActionMessageTypesForEXS, bool isInventoryManagementActionMessageTypesForH2, bool isInventoryManagementActionMessageTypesForExport)
	{
		if (isInventoryManagementActionMessageTypesForImport)
		{
			return ImportPreviousDocsToReserveGoods;
		}
		else if (isInventoryManagementActionMessageTypesForEXS)
		{
			return EXSPreviousDocsToReserveGoods;
		}
		else if (isInventoryManagementActionMessageTypesForH2)
		{
			return H2PreviousDocsToReserveGoods;
		}
		else if (isInventoryManagementActionMessageTypesForExport)
		{
			return ExportSupportingDocsToReserveGoods;
		}
		else
		{
			return Array.Empty<ZString>();
		}
	}
}
