using System.Collections.Generic;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class DynamicTemplate : ITemplate
	{
		#region ITemplate Members

		void ITemplate.InstantiateIn(Control container)
		{
			foreach (Control control in Controls)
			{
				container.Controls.Add(control);
			}
		}

		#endregion

		public List<Control> Controls
		{
			get
			{
				if (controls == null)
				{
					controls = new List<Control>();
				}
				return controls;
			}
		}
		List<Control> controls;
	}
}
