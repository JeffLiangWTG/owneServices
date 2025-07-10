using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ICodeDescriptionDataObject : ICodeDataObject
	{
		ZString? Description { get; set; }
	}
}
