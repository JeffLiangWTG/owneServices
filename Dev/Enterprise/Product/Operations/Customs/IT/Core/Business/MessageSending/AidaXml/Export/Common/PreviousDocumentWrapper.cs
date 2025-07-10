using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class PreviousDocumentWrapper : IPreviousDocument
{
	public PreviousDocumentWrapper(PreviousDocument previousDocument)
	{
		Argument.NotNull(previousDocument, nameof(previousDocument));

		lazyLineNo = new Lazy<int?>(() => previousDocument.CSI_ItemNumber.NullIfZero());
		lazyPackageType = new Lazy<string>(() => previousDocument.CSI_PackType);
		lazyReferenceNumber = new Lazy<string>(() => previousDocument.CSI_ReferenceNumber);
		lazyDocumentType = new Lazy<string>(() => previousDocument.CSI_Code);
		lazyUnitOfQuantity = new Lazy<string>(() => previousDocument.CSI_UnitOfQuantity);
		lazyNumberOfPackages = new Lazy<int?>(() => GetNumberOfPackages(previousDocument.CSI_PackQty));
		lazyQuantity = new Lazy<decimal?>(() => GetQuantity(previousDocument.CSI_Quantity));
	}

	readonly Lazy<int?> lazyLineNo;
	readonly Lazy<string> lazyPackageType;
	readonly Lazy<string> lazyReferenceNumber;
	readonly Lazy<string> lazyDocumentType;
	readonly Lazy<string> lazyUnitOfQuantity;
	readonly Lazy<int?> lazyNumberOfPackages;
	readonly Lazy<decimal?> lazyQuantity;

	int? IPreviousDocument.LineNo => lazyLineNo.Value;

	int? IPreviousDocument.NumberOfPackages => lazyNumberOfPackages.Value;

	string IPreviousDocument.PackageType => lazyPackageType.Value;

	decimal? IPreviousDocument.Quantity => lazyQuantity.Value;

	string IPreviousDocument.ReferenceNumber => lazyReferenceNumber.Value;

	string IPreviousDocument.DocumentType => lazyDocumentType.Value;

	string IPreviousDocument.UnitOfQuantity => lazyUnitOfQuantity.Value;

	int? GetNumberOfPackages(ZInt packageQuantity)
	{
		return string.IsNullOrWhiteSpace(lazyPackageType.Value) && packageQuantity.IsEmpty
			? null
			: packageQuantity;
	}

	decimal? GetQuantity(ZDecimal quantity)
	{
		return string.IsNullOrWhiteSpace(lazyUnitOfQuantity.Value) && quantity.IsEmpty
			? null
			: quantity;
	}
}
