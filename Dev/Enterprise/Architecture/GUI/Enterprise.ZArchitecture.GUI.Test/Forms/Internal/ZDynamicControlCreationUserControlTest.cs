using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Internal
{
	public class ZDynamicControlCreationUserControlTest : TestCase
	{
		public void TestChangingUserControlTypeDisposesAllOldControls()
		{
			using (var form = new Form())
			using (var dynamicCreationControl = new ZDynamicControlCreationUserControl())
			{
				var control1 = new TextBox();
				var control2 = new TextBox();

				dynamicCreationControl.Controls.Add(control1);
				dynamicCreationControl.Controls.Add(control2);

				form.Controls.Add(dynamicCreationControl);
				form.Show();
				Application.DoEvents();

				AssertEquals("Precondition", 2, dynamicCreationControl.Controls.Count);

				dynamicCreationControl.UserControlType = typeof(TextBox);

				AssertEquals("Should have 1 new control created", 3, dynamicCreationControl.Controls.Count);
				Assert("Old control is still alive", !control1.IsDisposed);
				Assert("Old control is still alive", !control2.IsDisposed);

				Application.DoEvents();

				AssertEquals("Old controls should be removed", 1, dynamicCreationControl.Controls.Count);
				Assert("Should not contain old control", !dynamicCreationControl.Controls.Contains(control1));
				Assert("Should not contain old control", !dynamicCreationControl.Controls.Contains(control2));
				Assert("Old control should be disposed", control1.IsDisposed);
				Assert("Old control should be disposed", control2.IsDisposed);
			}
		}

		public void TestHostedControlCreatedAfterVisibilityAndUserControlTypeChange()
		{
			using (var form = new Form())
			using (var dynamicCreationControl = new ZDynamicControlCreationUserControl())
			{
				dynamicCreationControl.UserControlType = typeof(CheckBox);
				form.Controls.Add(dynamicCreationControl);
				form.Show();
				Application.DoEvents();

				AssertEquals("Should have 1 new control created", 1, dynamicCreationControl.Controls.Count);
				dynamicCreationControl.UserControlType = typeof(TextBox);
				dynamicCreationControl.Visible = false;
				Application.DoEvents();

				dynamicCreationControl.UserControlType = typeof(CheckBox);
				dynamicCreationControl.Visible = true;
				Application.DoEvents();
				AssertEquals("Should have 1 control", 1, dynamicCreationControl.Controls.Count);
			}
		}

		public void TestForceCreateHostedControl()
		{
			CombineAssertions(() =>
			{
				AssertForceCreateHostedControl(true);
				AssertForceCreateHostedControl(false);
			});
		}

		void AssertForceCreateHostedControl(bool visible)
		{
			using (var dynamicCreationControl = new ZDynamicControlCreationUserControl())
			{
				dynamicCreationControl.UserControlType = typeof(CheckBox);
				dynamicCreationControl.Visible = visible;
				var count = 0;
				dynamicCreationControl.HostedControlCreated += (s, e) => { count++; };

				var creationControl = (IDynamicControlCreationUserControl)dynamicCreationControl;
				creationControl.ForceCreateHostedControl();

				var hostedControl = dynamicCreationControl.HostedControl;
				AssertEquals(visible + "-HostedControl is created", 1, count);
				AssertType<CheckBox>(visible + "-HostedControl Type", hostedControl);

				creationControl.ForceCreateHostedControl();
				AssertEquals(visible + "-HostedControl will not be recreated", 1, count);
			}
		}
	}

	public class TestZDynamicUserControl_Binding : TestCaseWithFactory
	{
		public void TestDataBindingCount()
		{
			using (var form = new ZForm(Factory.NewWithValidTestData<DummyBusinessObject>()))
			using (var dynamicCreationControl = new ZDynamicControlCreationUserControl())
			{
				dynamicCreationControl.UserControlType = typeof(BindingTrackingTextBox);
				dynamicCreationControl.BindingMember = "Z0_Code";
				form.Controls.Add(dynamicCreationControl);
				form.Show();
				Application.DoEvents();

				AssertEquals(1, ((BindingTrackingTextBox)dynamicCreationControl.HostedControl).DataBindingCount);
			}
		}

		class BindingTrackingTextBox : ZTextBox
		{
			public int DataBindingCount;

			public override void SetDataBinding(object dataSource, string dataMember)
			{
				DataBindingCount++;
				base.SetDataBinding(dataSource, dataMember);
			}
		}
	}
}
