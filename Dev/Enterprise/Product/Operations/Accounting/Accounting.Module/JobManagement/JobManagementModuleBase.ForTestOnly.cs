#if DEBUG

using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Module
{
	public partial class JobManagementModuleBase
	{
		public MenuItem[] GetNewStandardMenuItems_ForTestOnly()
		{
			return GetNewStandardMenuItems();
		}

		public BusinessObjectFactory Factory_ForTestOnly => Factory;
	}
}

#endif
