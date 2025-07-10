using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IGlobalSearchBusinessObjectProvider
	{
		BusinessObject BusinessObjectForController { get; }
	}
}
