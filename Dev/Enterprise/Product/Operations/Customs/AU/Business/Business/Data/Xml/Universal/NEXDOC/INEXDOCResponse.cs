using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	interface INEXDOCResponse
	{
		ZString JobNumber { get; }
		ZString RexNumber { get; }
		ZString RexStatus { get; }
		ZString ExportPermitNumber { get; }
		ZString CustomsAuthorityNumber { get; }
		ZString HtmlTemplatePath { get; }
	}
}
