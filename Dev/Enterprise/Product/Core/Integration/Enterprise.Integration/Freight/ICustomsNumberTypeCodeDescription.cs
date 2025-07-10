using CargoWise.Integration;

namespace Enterprise.Integration.Freight
{
	public interface ICustomsNumberTypeCodeDescription : ICodeDescription
	{
		bool IsUnique { get; }
		bool IsAutomation { get; }
	}
}