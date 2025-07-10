using System;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class SaveButton : ZButton
	{
		public SaveButton(ITaskCardComponentParent parent)
		{
			this.parent = parent;
		}

		readonly ITaskCardComponentParent parent;

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (parent != null)
			{
				parent.Save();
			}
		}
	}
}
