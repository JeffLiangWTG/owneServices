using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IInterchangeFileNameStrategy
{
	ZString GetFileName();
}
