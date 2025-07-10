
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IModuleFilter
	{
		FilterVisibility Visibility { get; set; }
		FilterPriority FilterPriority { get; set; }
		ZString OriginalCode { get; }
		ZQuery Query { get; }
	}
}
