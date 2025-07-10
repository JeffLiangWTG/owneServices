using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZYearEditTestForm : ZChildForm
	{
		public ZYearEditTestForm(DummyBusinessObject bizObj)
			: base(bizObj)
		{
		}

		public ZYearEdit TestYearEdit;
		public static string BindTo = string.Empty;

		protected override void InitializeComponent()
		{
			TestYearEdit = new ZYearEdit();
			TestYearEdit.BindTo = BindTo;
			Controls.Add(TestYearEdit);

			base.InitializeComponent();
		}
	}
}
