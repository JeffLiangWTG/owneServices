using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ICustomsEntryApplicationReference
{
	ZString Node { get; }
	ZString Subscriber { get; }
	ZString CustomsOffice { get; }
	ZString CustomsProfile { get; }
}
