using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.ModulePlugIn.Testing
{
	public abstract class DummyModulePlugin : ZModulePlugin
	{
		public static bool RequiresMultiSelectValue;

		protected override List<MenuItem> GetActionMenuItemToAddCore()
		{
			return new List<MenuItem>() { new ZMenuItem("Dummy") };
		}

		protected override bool RequiresMultiSelectCore
		{
			get { return RequiresMultiSelectValue; }
		}
	}
}
