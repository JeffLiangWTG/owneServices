using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemProducedDocumentWrapper : DocumentWrapper
{
	const string EmptyProperty = "---";

	public static EVVGoodsItemProducedDocumentWrapper New(IEvvGoodsItemProducedDocument producedDocument, BusinessObjectFactory factory)
		=> new EVVGoodsItemProducedDocumentWrapper(Argument.NotNull(producedDocument, nameof(producedDocument)), Argument.NotNull(factory, nameof(factory)));

	EVVGoodsItemProducedDocumentWrapper(IEvvGoodsItemProducedDocument producedDocument, BusinessObjectFactory factory)
		: base(producedDocument, factory)
	{
		this.producedDocument = producedDocument;
	}

	readonly IEvvGoodsItemProducedDocument producedDocument;

	public ZString DocumentType => string.IsNullOrEmpty(producedDocument.DocumentType)
		? EmptyProperty
		: RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Now).GetDescriptionFromCode(producedDocument.DocumentType) ?? producedDocument.DocumentType;

	public ZString DocumentReferenceNumber => string.IsNullOrEmpty(producedDocument.DocumentReferenceNumber) ? EmptyProperty : producedDocument.DocumentReferenceNumber;

	public ZString IssueDate => producedDocument.IssueDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? EmptyProperty;

	public ZString AdditionalInformation => string.IsNullOrEmpty(producedDocument.AdditionalInformation) ? EmptyProperty : producedDocument.AdditionalInformation;
}
