using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class CatalogDeferredAmendmentSavingOptions : DeferredAmendmentSavingOptions, IDeferredAmendmentSavingOptions
	{
		ZBool IDeferredAmendmentSavingOptions.ShouldTakeReasonForSavingWithoutSendingSeparately => ZBool.False;
	}
}
