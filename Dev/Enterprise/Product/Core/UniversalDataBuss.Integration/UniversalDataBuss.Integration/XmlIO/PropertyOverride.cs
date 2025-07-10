using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IPropertyOverride
	{
		string Name { get; }
		IZType Value { get; }
	}
}