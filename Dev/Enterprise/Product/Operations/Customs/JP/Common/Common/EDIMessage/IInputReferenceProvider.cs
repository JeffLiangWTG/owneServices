using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public interface IInputReferenceProvider
{
	ZString InputReference { get; }
}
