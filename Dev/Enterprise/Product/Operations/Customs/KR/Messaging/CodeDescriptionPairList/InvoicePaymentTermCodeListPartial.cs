using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	partial class InvoicePaymentTermCodeListPartial :
		Integration.Customs.KR.IKRInvoicePaymentTermCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList() => new InvoicePaymentTermCodeList();
	}
}
