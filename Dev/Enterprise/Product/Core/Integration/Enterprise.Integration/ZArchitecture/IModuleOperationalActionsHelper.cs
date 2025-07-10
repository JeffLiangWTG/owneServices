using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public interface IModuleOperationalActionsHelper
	{
		bool SupportsOperationalActions(IZFilterGridModule module);
		IEnumerable<object> OperationalActionNames(IZFilterGridModule module, BusinessObjectFactory factory); //actually returns IEnumerable<MultilingualString>, but that isn't available here
		void AddOperationalActionsIntoActionsMenu(IComponent component);
	}
}
