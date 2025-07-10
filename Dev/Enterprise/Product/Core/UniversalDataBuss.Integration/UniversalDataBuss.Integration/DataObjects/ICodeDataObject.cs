using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ICodeDataObject : IDataObject
	{
		ZString? Code { get; set; }
	}
}
