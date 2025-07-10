using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public interface INationalAdditionalCodeSupporter : ICusCodeDataWithOrderSupporter
	{
		NationalAdditionalCodeCollection NationalAdditionalCodes { get; }
	}
}
