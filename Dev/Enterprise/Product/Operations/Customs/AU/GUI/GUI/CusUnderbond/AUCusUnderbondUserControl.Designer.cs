using System.Linq;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.AU.GUI
{
	partial class AUCusUnderbondUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			UnderbondsGrid.ControlAdded -= UnderbondsGrid_ControlAdded;
			foreach (var findBoxControl in UnderbondsGrid.Controls.OfType<ZGridFindBox>().Where(x => x.Name == "C4_UnderbondBySeaVessel"))
			{
				findBoxControl.PopupSelected -= AUCusUnderbondUserControl_PopupSelected;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		}

		#endregion
	}
}
