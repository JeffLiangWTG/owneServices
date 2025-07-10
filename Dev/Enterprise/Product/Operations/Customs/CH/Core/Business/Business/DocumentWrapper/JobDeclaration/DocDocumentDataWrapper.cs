using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.CH.Business;

public class DocDocumentDataWrapper : DocBaseWrapper
{
	public static DocDocumentDataWrapper New(CusSupportingInfo document, BusinessObjectFactory factoryToWrap) => document == null ? null : new DocDocumentDataWrapper(document, factoryToWrap);

	DocDocumentDataWrapper(CusSupportingInfo document, BusinessObjectFactory factory) : base(document, factory)
	{
		this.document = document;
	}
	readonly CusSupportingInfo document;

	public ZInt LineItemNumber => document.CSI_ItemNumber;

	public ZString Type => document.CSI_Code;

	public ZString ReferenceNumber => document.CSI_ReferenceNumber;

	public ZInt GoodsItemNumber => document.CSI_ItemNumber;

	public ZString ComplementOfInformation => document.CSI_ReferenceNumber2;
}
