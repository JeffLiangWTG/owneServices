using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GUITestHelperTest : ZControlBaseTestCase<ZCalcFindBox>
	{
		public void TestFindControl_By_ControlTypeAndName()
		{
			var childBusinessObject = Factory.New<DummyChildBusinessObject>();
			childBusinessObject.Z0_Code = "CHILD";

			using (var testForm = new TestForm(Dummy))
			{
				testForm.Show();

				var control = GUITestHelper.FindControl<ZCalcFindBox>(testForm.Controls, "CalcFindBox");
				AssertNotNull("CalcFindBox is found", control);
			}
		}

		public void TestFindControl_By_ControlTypeAndEmptyName()
		{
			var childBusinessObject = Factory.New<DummyChildBusinessObject>();
			childBusinessObject.Z0_Code = "CHILD";

			using (var testForm = new TestForm(Dummy))
			{
				testForm.Show();

				var control = GUITestHelper.FindControl<ZCalcFindBox>(testForm.Controls, string.Empty);
				AssertNotNull("CalcFindBox is found", control);
			}
		}

		public void TestFindControl_By_ControlTypeOnly()
		{
			var childBusinessObject = Factory.New<DummyChildBusinessObject>();
			childBusinessObject.Z0_Code = "CHILD";

			using (var testForm = new TestForm(Dummy))
			{
				testForm.Show();

				var control = GUITestHelper.FindControl<ZTextBox>(testForm.Controls);
				AssertNotNull("ZTextBox is found", control);
			}
		}

		#region Test configurations
		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "IsVisibleForBinding" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Text"; }
		}

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		protected override void BindControl()
		{
			Control.BindToAmount = DummyBusinessObject.Schema.Z0_Decimal;
			Control.BindToUnit = DummyBusinessObject.Schema.Z0_Guid;
			Control.BindToList = "Collection";
			Control.SetDataBinding(Dummy, DummyBusinessObject.Schema.Z0_Decimal);
		}

		class TestForm : ZChildForm
		{
			public TestForm(BusinessObject bizObj) : base(bizObj)
			{
			}

			ZCalcFindBox CalcFindBox;
			ZTextBox TextBox;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				CalcFindBox = new ZCalcFindBox();
				CalcFindBox.Name = "CalcFindBox";
				CalcFindBox.BindToAmount = DummyBusinessObject.Schema.Z0_AnotherDecimal;
				CalcFindBox.BindToUnit = DummyBusinessObject.Schema.Z0_Guid;
				CalcFindBox.BindToList = "Collection";
				CalcFindBox.Location = new Point(20, 20);

				TextBox = new ZTextBox();
				TextBox.Name = "TextBox";
				TextBox.BindTo = DummyBusinessObject.Schema.Z0_Code;
				TextBox.Location = new Point(20, 50);

				Controls.Add(CalcFindBox);
				Controls.Add(TextBox);
			}
		}

		#endregion
	}
}
