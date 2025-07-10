using System.Data;

namespace CargoWise.Integration
{
	public interface IConcurrencyException
	{
		string Message { get; }
		DataRow Row { get; }
	}
}
