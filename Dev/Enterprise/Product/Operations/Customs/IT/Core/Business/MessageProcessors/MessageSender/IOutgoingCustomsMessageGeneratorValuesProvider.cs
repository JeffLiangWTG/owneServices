using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IOutgoingCustomsMessageGeneratorValuesProvider
{
	ZString GetSubType();

	ZString GetApplicationReference();

	BusinessObject Parent { get; }
}
