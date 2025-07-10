using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IFilterPreviewableWithSubObject
	{
		BusinessObject GetObjectForPreview(string filterControlIdentifier);
	}
}
