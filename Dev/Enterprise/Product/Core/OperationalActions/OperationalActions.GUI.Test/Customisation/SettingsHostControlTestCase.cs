using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	sealed class SettingsHostControlTestCase : TestCaseWithFactory
	{
		public void TestShowCorrectSettingsControl()
		{
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			OperationalActionMethod withGui = ActionSupporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			OperationalActionMethod withoutGui = ActionSupporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithoutGUI);

			OperationalAction action = Factory.New<OperationalAction>();
			action.Context = Context;

			using (ZForm form = new ZForm(action.MethodDescriptors))
			{
				CurrencyManager currency = (CurrencyManager)form.BindingContext[action, "MethodDescriptors"];

				SettingsHostControl hostControl = new SettingsHostControl();
				hostControl.Dock = DockStyle.Fill;
				form.Controls.Add(hostControl);

				hostControl.SetDataBinding(action, "MethodDescriptors");

				form.Show();
				Application.DoEvents();
				AssertCoverLabel(hostControl, "No defined process selected.");

				OperationalActionMethodDescriptor descriptor1 = action.MethodDescriptors.AddNew();
				descriptor1.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
				descriptor1.MethodID = TestingConstants.DummyActionMethodWithoutGUI;

				currency.Position = 0;
				Application.DoEvents();
				AssertCoverLabel(hostControl, "This defined process has no settings.");

				descriptor1.MethodID = TestingConstants.DummyActionMethodWithGUI;
				Application.DoEvents();

				DummySettingsControl control = GetControl<DummySettingsControl>(hostControl);
				AssertNotNull("Expecting the DummySettingsControl", control);

				OperationalActionMethodDescriptor descriptor2 = action.MethodDescriptors.AddNew();
				descriptor2.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
				descriptor2.MethodID = TestingConstants.DummyActionMethodWithoutGUI;

				currency.Position = 1;
				Application.DoEvents();
				AssertCoverLabel(hostControl, "This defined process has no settings.");

				action.MethodDescriptors.RemoveAndDeleteAll();
				Application.DoEvents();
				AssertCoverLabel(hostControl, "No defined process selected.");
			}
		}

		public void TestCreatingSettingsControl_HostControlIsNotVisible_CreateCorrectSettingsControl()
		{
			ActionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);

			var action = Factory.New<OperationalAction>();
			action.Context = Context;

			var descriptor = action.MethodDescriptors.AddNew();
			descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			descriptor.MethodID = TestingConstants.DummyActionMethodWithGUI;

			var settings = (AutoDummyOperationalActionMethodSettings)descriptor.Settings;
			settings.DefaultExcuse = "MCLAREN";

			using (var testForm = new ZForm(action.MethodDescriptors))
			{
				var currency = (CurrencyManager)testForm.BindingContext[action, "MethodDescriptors"];

				var hostControl = new SettingsHostControl();
				hostControl.Dock = DockStyle.Fill;
				hostControl.Visible = false;

				testForm.Controls.Add(hostControl);
				hostControl.SetDataBinding(action, "MethodDescriptors");

				testForm.Show();

				currency.Position = 0;
				hostControl.Visible = true;
				Application.DoEvents();

				var control = GetControl<DummySettingsControl>(hostControl);
				AssertNotNull("Expecting the DummySettingsControl", control);
				AssertEquals("Binding to SettingsControl is OK", control.TextBoxValue, "MCLAREN");
			}
		}

		#region Implementation

		void AssertCoverLabel(SettingsHostControl hostControl, string text)
		{
			ZLabel label = GetControl<ZLabel>(hostControl);
			AssertEquals("text", text, label == null ? null : label.Text);
		}
		T GetControl<T>(SettingsHostControl hostControl)
			where T : Control
		{
			Control[] controls = new Control[hostControl.Controls.Count];
			hostControl.Controls.CopyTo(controls, 0);

			controls = Array.FindAll(controls, (c) => c.GetType() != typeof(UserControl));

			if (controls.Length == 0)
			{
				return null;
			}
			else if (controls.Length > 1)
			{
				StringBuilder builder = new StringBuilder("Too many controls:\r\n");

				foreach (Control control in controls)
				{
					builder.AppendFormat("{0}: {1}\r\n", control.GetType().Name, control.Name);
				}

				Fail(builder.ToString());
				return null; // should never be reached but needed to satisfy the compiler.
			}
			else
			{
				Control control = controls[0];
				AssertEquals("Control Type", typeof(T), control.GetType());
				AssertEquals("Docking", DockStyle.Fill, control.Dock);

				return (T)control;
			}
		}

		OperationalActionContext Context
		{
			get { return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name")); }
		}
		OperationalActionContext context;

		OperationalActionSupporter ActionSupporter
		{
			get { return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter); }
		}
		OperationalActionSupporter actionSupporter;

		#endregion
	}
}
