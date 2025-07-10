using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class ConsignmentItemPreviousDocument : IConsignmentItemPreviousDocument
{
	public ConsignmentItemPreviousDocument(NctsPreviousDocument previousDocument)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		InitializeLazy();
	}

	#region IConsignmentItemPreviousDocument

	string IConsignmentItemPreviousDocument.ReferenceNumber => lazyReferenceNumber.Value;
	Lazy<string> lazyReferenceNumber;

	string IConsignmentItemPreviousDocument.DocumentType => lazyDocumentType.Value;
	Lazy<string> lazyDocumentType;

	string IConsignmentItemPreviousDocument.ComplementOfInformation => lazyComplementOfInformation.Value;
	Lazy<string> lazyComplementOfInformation;

	string IConsignmentItemPreviousDocument.PackageType => lazyPackageType.Value;
	Lazy<string> lazyPackageType;

	int? IConsignmentItemPreviousDocument.NumberOfPackages => lazyNumberOfPackages.Value;
	Lazy<int?> lazyNumberOfPackages;

	string IConsignmentItemPreviousDocument.UnitOfQuantity => lazyUnitOfQuantity.Value;
	Lazy<string> lazyUnitOfQuantity;

	decimal? IConsignmentItemPreviousDocument.Quantity => lazyQuantity.Value;
	Lazy<decimal?> lazyQuantity;

	int? IConsignmentItemPreviousDocument.GoodsItemIdentifier => lazyGoodsItemIdentifier.Value;
	Lazy<int?> lazyGoodsItemIdentifier;

	#endregion

	#region Implementation

	void InitializeLazy()
	{
		lazyDocumentType = new Lazy<string>(() => previousDocument.CSI_Code);
		lazyReferenceNumber = new Lazy<string>(() => previousDocument.CSI_ReferenceNumber);
		lazyNumberOfPackages = new Lazy<int?>(GetNumberOfPackages);
		lazyPackageType = new Lazy<string>(() => previousDocument.CSI_PackType);
		lazyQuantity = new Lazy<decimal?>(GetQuantity);
		lazyUnitOfQuantity = new Lazy<string>(() => previousDocument.CSI_UnitOfQuantity);
		lazyGoodsItemIdentifier = new Lazy<int?>(GetGoodsItemIdentifier);
		lazyComplementOfInformation = new Lazy<string>(() => previousDocument.CSI_ReferenceNumber2);
	}

	int? GetNumberOfPackages()
	{
		var packQty = previousDocument.CSI_PackQty;
		if (packQty.IsEmpty && previousDocument.CSI_PackType.IsEmpty)
		{
			return null;
		}
		return packQty;
	}

	decimal? GetQuantity()
	{
		var quantity = previousDocument.CSI_Quantity;
		if (quantity.IsEmpty && previousDocument.CSI_UnitOfQuantity.IsEmpty)
		{
			return null;
		}
		return quantity;
	}

	int? GetGoodsItemIdentifier()
	{
		var itemNumber = previousDocument.CSI_ItemNumber;
		return !itemNumber.IsEmpty ? (int?)itemNumber : null;
	}

	#endregion

	readonly NctsPreviousDocument previousDocument;
}
