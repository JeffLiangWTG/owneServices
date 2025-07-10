using System;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class DateAcceptabilityLegendForm : ZChildForm
	{
		public DateAcceptabilityLegendForm()
		{
			InitializeComponent();

			DateAcceptabilityHintLabel2.Text = Res.GetString("a632faa2-3a58-4c1f-9be2-d6f995445d47", @"{0} Red line: indicates the agreed delivery date
{0} Square: indicates delivery at any time within the period is acceptable
{0} Triangle: indicates that delivery is more acceptable the higher the triangle
{0} Flat line: indicates that delivery is undesirable during this period", BMConstants.BulletPointCharacter);

			MainStatusBar.Visible = false;
			ControlDpiScalingHelper.SetHeight(this, this.Height - MainStatusBar.Height, false);
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
