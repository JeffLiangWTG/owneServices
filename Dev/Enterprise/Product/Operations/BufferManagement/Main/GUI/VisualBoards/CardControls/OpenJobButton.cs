using System;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class OpenJobButton : ZButton
	{
		public OpenJobButton(ITaskCardComponentParent parent)
		{
			this.parent = parent;
		}

		readonly ITaskCardComponentParent parent;

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (parent != null)
			{
				parent.ShowParent();
			}
		}
	}
}
