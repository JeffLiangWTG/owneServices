using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IHaveTooltipsForMigration
	{
		ToolTipCollectorForMigration ToolTipForMigration { get; }
	}

	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	[ProvideProperty("ToolTip", typeof(Control))]
	public class ToolTipCollectorForMigration : Component, IExtenderProvider
	{
		[DefaultValue("")]
		public string GetToolTip(Control control)
		{
			string result;
			if (!ToolTips.TryGetValue(control, out result))
			{
				result = "";
			}
			return result;
		}

		public void SetToolTip(Control control, string description)
		{
			ToolTips[control] = description;
		}

		readonly Dictionary<Control, string> ToolTips = new Dictionary<Control, string>();
		#region IExtenderProvider Members

		bool IExtenderProvider.CanExtend(object extendee)
		{
			return extendee is Control;
		}

		#endregion
	}
}
