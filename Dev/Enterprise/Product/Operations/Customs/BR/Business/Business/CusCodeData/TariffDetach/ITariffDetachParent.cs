using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public interface ITariffDetachParent
	{
		TariffDetachCollection TariffDetachs { get; }
		ZPropertyInfo TariffDetachConcatenatedInfo { get; }
	}
}
