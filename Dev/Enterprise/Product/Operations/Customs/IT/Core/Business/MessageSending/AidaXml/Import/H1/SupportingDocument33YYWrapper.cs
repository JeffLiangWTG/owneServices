using System;
using CustomsMessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public class SupportingDocument33YYWrapper : CustomsMessageBuilder.ISupportingDocument
{
	public SupportingDocument33YYWrapper()
	{
	}

	decimal? CustomsMessageBuilder.ISupportingDocument.Amount => null;

	string CustomsMessageBuilder.ISupportingDocument.Code => Code33YY;

	string CustomsMessageBuilder.ISupportingDocument.Currency => null;

	DateTime? CustomsMessageBuilder.ISupportingDocument.ExpiryDate => null;

	string CustomsMessageBuilder.ISupportingDocument.IssuingAuthority => null;

	decimal? CustomsMessageBuilder.ISupportingDocument.Quantity => null;

	string CustomsMessageBuilder.ISupportingDocument.ReferenceNumber => ReferenceDash;

	string CustomsMessageBuilder.ISupportingDocument.UnitOfQuantity => null;

	int? CustomsMessageBuilder.ISupportingDocument.ItemNumber => null;

	const string Code33YY = "33YY";
	const string ReferenceDash = "-";
}
