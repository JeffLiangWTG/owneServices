using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IFilterPreviewable
	{
		ZQuery GetAdditionalPreviewFilter(string moduleId, string dropDownCode);
	}
}
