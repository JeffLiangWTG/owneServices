using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IEntryStyleCalculatorFallbackInfoProvider
	{
		ZString GetEntrySubStyleForCommonTransit(RefCountry country);
		ZString GetEntryStyleForInwardProcessingVATPayment();
	}
}
