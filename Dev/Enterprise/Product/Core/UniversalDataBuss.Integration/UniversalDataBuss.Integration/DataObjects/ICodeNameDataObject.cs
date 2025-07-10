using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ICodeNameDataObject : ICodeDataObject
	{
		ZString? Name { get; set; }
	}
}
