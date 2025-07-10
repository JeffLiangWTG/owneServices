using System.Collections.Generic;
using CargoWise.Types;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.DocumentScanning.Business;

public static class EDocsParsingHelper
{
	public static bool IsDocumentParsingEnabled() =>
		IsParseTypeEnabled(SharedConstants.ParseType.Code.CommercialInvoice) ||
		IsParseTypeEnabled(SharedConstants.ParseType.Code.AccountPayableInvoice);

	public static bool IsParseTypeEnabled(ZString parseType) => parseType.ToString() switch
	{
		SharedConstants.ParseType.Code.AccountPayableInvoice => DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.Value,
		SharedConstants.ParseType.Code.CommercialInvoice => DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.Value,
		_ => false
	};

	public static bool IsSupportedParseType(ZString parseType) => new HashSet<string>()
	{
		SharedConstants.ParseType.Code.AccountPayableInvoice,
		SharedConstants.ParseType.Code.CommercialInvoice
	}.Contains(parseType.ToString());
}
