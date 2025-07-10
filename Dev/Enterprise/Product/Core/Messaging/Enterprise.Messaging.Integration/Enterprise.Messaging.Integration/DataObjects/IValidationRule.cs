using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IValidationRule : IDataObject
	{
		ZString? Code { get; set; }
		ZInt? Sequence { get; set; }
		ZString? MessageLog { get; set; }
		ZString? Result { get; set; }
	}
}
