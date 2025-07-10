using System;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class CloseCardButton : ZButton
	{
		public CloseCardButton(ITaskCardComponentParent parent)
		{
			this.parent = parent;
			BackgroundImage = Properties.Resources.glyphicons_207_remove_2;
			BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		}

		readonly ITaskCardComponentParent parent;

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (parent != null)
			{
				parent.Close();
			}
		}
	}
}
