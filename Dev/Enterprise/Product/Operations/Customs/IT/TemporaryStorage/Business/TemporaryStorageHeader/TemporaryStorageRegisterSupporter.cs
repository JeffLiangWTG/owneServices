using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageRegisterSupporter
{
	public TemporaryStorageRegisterSupporter(TemporaryStorageHeader header)
	{
		this.header = Argument.NotNull(header, nameof(header));
	}

	internal void CreateRegisterTransactionsForBill(ZString movementReferenceNumber)
	{
		var bill = header.Bills.SingleOrDefault(x => x.Mrn == movementReferenceNumber);
		if (bill == null || bill.RegisterHeader != null)
		{
			return;
		}

		var registerHeader = CreateRegisterHeader(bill);

		foreach (var item in bill.PackedItems)
		{
			var line = CreateRegisterLine(registerHeader, bill, item);
			CreateOpeningBalanceRegisterLineTransaction(line, item);
		}
	}

	CusTempStorageRegHeader CreateRegisterHeader(TemporaryStorageBill bill)
	{
		var registerHeader = header.Factory.New<CusTempStorageRegHeader>();
		registerHeader.SRH_Reference = "REF001"; // TODO Part 2: generate a progressive number
		registerHeader.SRH_InternalReference = header.AMA_JobReference;
		registerHeader.SRH_Status = TempStorageDeclarationStatusList.Codes.Open; // TODO Part 2: set status
		registerHeader.SRH_PreviousReferenceType = MessageSubTypeList.Codes.DeclarationOfTemporaryStorage;
		registerHeader.SRH_PreviousReference = bill.Mrn;
		registerHeader.SRH_PresentationDate = bill.RegistrationDate;
		registerHeader.SRH_TransportID = header.ArrivalTransportMeansCode;
		registerHeader.SRH_CustomsOffice = header.PresentationCustomsOffice;

		return registerHeader;
	}

	CusTempStorageRegLine CreateRegisterLine(CusTempStorageRegHeader registerHeader, TemporaryStorageBill bill, TemporaryStoragePackedItem item)
	{
		var line = registerHeader.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = item.API_LineNo;
		line.SRL_LimitDate = item.ReleaseDate.Date.IsEmpty ? ZDate.Empty : item.ReleaseDate.Date.AddDays(TemporaryStorageMaxDurationDays);
		line.SRL_LocationOfGoods = header.GoodsLocation?.CGL_AdditionalIdentifier ?? ZString.Empty;
		line.SRL_OwnerReference = item.RegistrationNo;
		line.SRL_OwnerReferenceType = item.RegistrationNo.SubstringSafe(0, 2);
		line.SRL_GoodsDescription = item.API_GoodsDescription;
		line.SRL_PackageType = item.Pack?.APA_PackUQ ?? ZString.Empty;
		line.SRL_GrossWeightUQ = item.API_GrossWeightUQ;
		// TODO Part 2: map Authorization

		foreach (var linkPackage in item.TemporaryStorageLinkPackages.Where(x => x.IsLinked))
		{
			var container = linkPackage?.Package?.Container;
			if (container == null)
			{
				continue;
			}

			var lineContainer = line.Containers.AddNew();
			lineContainer.CY_Data = container.ACN_ContainerNumber;
		}

		var registerLineItem = header.Factory.New<CusTempStorageRegLineItem>();
		registerLineItem.SRI_HouseBill = bill.ABL_BillNumber;

		var lineItemPivot = line.RegLineItemPivots.AddNew();
		lineItemPivot.SRV_SRI_Item = registerLineItem.PK;

		return line;
	}

	void CreateOpeningBalanceRegisterLineTransaction(CusTempStorageRegLine line, TemporaryStoragePackedItem item)
	{
		var transaction = line.CusTempStorageRegLineTransactions.AddNew();
		transaction.SRT_TransactionType = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		transaction.SRT_GrossWeight = item.API_GrossWeight;
		transaction.SRT_PackageQty = item.TemporaryStorageLinkPackages.Where(x => x.IsLinked && x.Package != null).Sum(x => x.Package.APA_PackQty);
		transaction.SRT_ReferenceType = TemporaryStorageApplicationCodesList.Codes.IST;
		transaction.SRT_Reference = header.AMA_JobReference;
		// TODO Part 2: map SRT_InternalReferenceType and SRT_internalReferenceNumber
	}

	const int TemporaryStorageMaxDurationDays = 90;

	readonly TemporaryStorageHeader header;
}
