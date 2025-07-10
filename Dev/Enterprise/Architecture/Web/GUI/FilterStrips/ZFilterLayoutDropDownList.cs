using System.Drawing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	#region SuppressResourceStringsCheckRegion

	public class ZFilterLayoutDropDownList : ZDropDownList
	{
		public void MakeCurrentFilterUnsaved()
		{
			if (SelectedItem != null)
			{
				SelectedItem.Attributes["style"] = "color:" + KnownColor.GrayText;
			}
		}

		public ZString OnClientChange
		{
			get { return Attributes["onchange"]; }
			set { Attributes.Add("onchange", value); }
		}

		public void Rebind()
		{
			Bind(BusinessEntity);
		}
	}

	#endregion
}
