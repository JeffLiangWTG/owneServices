using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCalcDropEditTest : ZControlBaseTestCase<ZCalcDropEdit>
	{
		public void TestOnMaxLengthChange()
		{
			using (var testForm = new TestForm(Dummy))
			{
				testForm.calcDropEdit.UnitShouldResizeByMaxLength = true;
				var dropEdit = testForm.calcDropEdit.Controls.Find("UnitDropEdit", true)[0] as ZDropEdit;
				dropEdit.MaxLength = 3;
				var width = dropEdit.Width;
				dropEdit.MaxLength = 2;
				AssertNotEquals(width, dropEdit.Width);

				width = dropEdit.Width;
				testForm.calcDropEdit.UnitShouldResizeByMaxLength = false;
				dropEdit.MaxLength = 3;
				AssertEquals(width, dropEdit.Width);
			}
		}

		public void TestValidatedWhenTabbedThrough()
		{
			using (var form = new TestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				Factory.Validation.ValidationRequested += (sender, e) =>
				{
					if (e.Property.Name == DummyBusinessObject.Schema.Z0_Decimal)
					{
						e.Property.AddError("Error");
					}
				};
				var notificationExtension = form.calcDropEdit.Extensions.Get<INotificationExtension>();
				AssertEquals("Notifications not raised before tabbed over", false, notificationExtension.Notifications.HasErrors());

				form.textBox.Focus();
				form.calcDropEdit.Focus();
				form.textBox.Focus();

				AssertEquals("Notifications raised after tabbed over", true, notificationExtension.Notifications.HasErrors());
			}
		}

		public void TestMovingBetweenInnerSubcontrolsRefreshesStatusbar()
		{
			using (var form = new TestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Precondition check: there should be 2 subcontrols in ZCalcDropEdit", 2, form.calcDropEdit.Controls.Count);

				form.calcDropEdit.Controls[0].Focus(); // AmountCalcEdit

				Factory.Validation.ValidationRequested += (sender, e) =>
				{
					if (e.Property.Name == DummyBizoSchema.Constants.Z0_Decimal)
					{
						e.Property.AddError("Error");
					}
				};
				Dummy.Validation.ValidateZ0_Decimal();

				AssertEquals("Statusbar still should show property caption", Dummy.Z0_DecimalInfo.HumanReadableName, form.StatusBarTextForTesting);

				form.calcDropEdit.Controls[1].Focus(); // UnitDropEdit

				AssertEquals("Should update text on statusbar", "Error", form.StatusBarTextForTesting);
			}
		}

		#region IDataBoundControl Members

		public void TestDataSourceType()
		{
			AssertEquals("DataSourceType required so that BindingSource.GetBindingMembersForCompileTimeCheck serialized", typeof(object), Control.DataSourceType);
		}

		#endregion

		#region Test Classes

		class TestForm : ZForm
		{
			public TestForm(DummyBusinessObject dummy)
			{
				textBox = new ZTextBox();
				Controls.Add(textBox);
				calcDropEdit = new ZCalcDropEdit();
				Controls.Add(calcDropEdit);
				calcDropEdit.BindToAmount = DummyBusinessObject.Schema.Z0_Decimal;
				calcDropEdit.BindToUnit = DummyBusinessObject.Schema.Z0_Number;

				BindingSource.DataSourceType = typeof(DummyBusinessObject);
				BindingSource.SetBindingMember(textBox, DummyBusinessObject.Schema.Z0_Code);
				BindingSource.SetBindingMember(calcDropEdit, ".");
				SetDataBinding(dummy, "");
			}

			public new KBindingSource BindingSource
			{
				get { return base.BindingSource; }
			}

			public ZTextBox textBox;
			public ZCalcDropEdit calcDropEdit;
		}

		#endregion

		#region Implementation

		protected override void BindControl()
		{
			Control.BindToAmount = DummyBusinessObject.Schema.Z0_Decimal;
			Control.BindToUnit = DummyBusinessObject.Schema.Z0_Description;
			Control.BindToList = "Collection";
			Control.SetDataBinding(Dummy, "");
		}

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "IsVisibleForBinding" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Text2"; }
		}

		protected override DummyBusinessObject GetDummyForBinding()
		{
			return Factory.New<DummyBusinessObjectWithNumberList>();
		}

		#endregion
	}
}
