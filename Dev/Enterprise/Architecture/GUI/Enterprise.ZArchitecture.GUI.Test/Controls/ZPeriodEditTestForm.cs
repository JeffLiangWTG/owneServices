using System.Drawing;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZPeriodEditTestForm : ZChildForm
	{
		public ZPeriodEditTestForm(BusinessObject bizObj)
			: base(bizObj)
		{
		}

		public ZPeriodEdit PeriodEdit;
		public ZButton Button;

		protected override void InitializeComponent()
		{
			PeriodEdit = new ZPeriodEdit();
			PeriodEdit.BindTo = "Z0_Number";

			Button = new ZButton();
			Button.Location = new Point(0, 30);

			Controls.Add(Button);
			Controls.Add(PeriodEdit);
		}
	}
}
