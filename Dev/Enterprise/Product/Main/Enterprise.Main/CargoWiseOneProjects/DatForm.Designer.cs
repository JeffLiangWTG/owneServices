using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Startup
{
	public partial class DatForm : Form
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Hidden form for DAT testing setup only")]
		private void InitializeComponent()
		{
			//
			// DatForm
			//

			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(256, 134);
			this.Name = "DatForm";
			this.Text = "Distributed AutoTester";
		}
	}
}
