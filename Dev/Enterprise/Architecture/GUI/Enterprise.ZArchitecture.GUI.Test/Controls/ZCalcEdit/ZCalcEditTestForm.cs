using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCalcEditTestForm : ZChildForm
	{
		public ZCalcEditTestForm(DummyBusinessObject bizObj)
			: base(bizObj)
		{
		}

		public TestZCalcEdit TestCalcEdit;
		public ZCalcEditWithTextChangeHistory TestCalcEditWithTextChangeHistory;
		public static string BindTo;

		protected override void InitializeComponent()
		{
			TestCalcEdit = new TestZCalcEdit();
			TestCalcEditWithTextChangeHistory = new ZCalcEditWithTextChangeHistory();
			TestCalcEdit.BindTo = BindTo;
			TestCalcEditWithTextChangeHistory.BindTo = BindTo;
			Controls.Add(TestCalcEdit);
			Controls.Add(TestCalcEditWithTextChangeHistory);

			base.InitializeComponent();
		}
	}
}
