using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.VisualBoards.GUI.Test
{
	public class VisualBoardTableLayoutPanel_TestingSubclass : KTableLayoutPanel
	{
		public VisualBoardTableLayoutPanel_TestingSubclass(KTableLayoutPanel panelToCopy)
		{
			foreach (Control control in panelToCopy.Controls)
			{
				this.Controls.Add(control);
			}
		}

		public bool UsurpDisposal { get; set; }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (UsurpDisposal)
			{
				UsurpDisposal = false; // so that this can be disposed eventually without a manual setting of this field, which is annoying :)
				throw new InvalidOperationException("OwO what's all this then *notices modified collection*");
			}
		}
	}
}
