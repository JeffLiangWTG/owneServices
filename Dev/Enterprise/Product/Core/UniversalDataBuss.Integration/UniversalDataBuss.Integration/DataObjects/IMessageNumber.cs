using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IMessageNumber : IDataObject
	{
		MessageNumberType? Type { get; set; }

		ZString? Value { get; set; }
	}
}
