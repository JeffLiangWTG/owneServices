using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public interface IModuleManualSortCollection
	{
		void Load(ZQuery query, ListSortDescriptionCollection sorts);
	}
}