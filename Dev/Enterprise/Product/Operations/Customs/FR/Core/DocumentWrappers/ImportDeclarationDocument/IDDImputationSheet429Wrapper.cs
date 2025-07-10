using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDDImputationSheet429Wrapper : IDDImputationSheetWrapper<MSupportingDocumentType01FR>
{
	public static IDDImputationSheet429Wrapper New(MSupportingDocumentType01FR supportingDocument, ZString goodsItemNumber, BusinessObjectFactory factoryToWrap)
	{
		return new IDDImputationSheet429Wrapper(supportingDocument, goodsItemNumber, factoryToWrap);
	}

	public IDDImputationSheet429Wrapper(MSupportingDocumentType01FR supportingDocument, ZString goodsItemNumber, BusinessObjectFactory factory) : base(supportingDocument, factory)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
		this.goodsItemNumber = goodsItemNumber;
	}

	readonly MSupportingDocumentType01FR supportingDocument;

	readonly ZString goodsItemNumber;

	public override ZString GoodsItemNumber => goodsItemNumber;

	public override ZString DocumentType => supportingDocument.Type;

	public override ZString DocumentReference => supportingDocument.ReferenceNumber;

	public override ZString LineItemNumber => supportingDocument.DocumentLineItemNumber;

	public override ZString Information => supportingDocument.ComplementOfInformation;

	public override ZString Quantity => supportingDocument.Quantity.ToString();

	public override ZString MeasurementUnitAndQualifier => supportingDocument.MeasurementUnitAndQualifier;

	public override ZString Amount => supportingDocument.Amount.ToString();

	public override ZString Currency => supportingDocument.Currency;
}
