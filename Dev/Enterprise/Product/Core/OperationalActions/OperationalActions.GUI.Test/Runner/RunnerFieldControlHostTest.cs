using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	[TestedType(typeof(ZForm))]
	sealed class RunnerFieldControlHostTest : ZFormBasherTest
	{
		public void TestControlsAreBound()
		{
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());

			using (ZForm form = new ZForm(runner))
			{
				form.Size = new Size(400, 250);

				RunnerFieldControlHost host = new RunnerFieldControlHost();
				host.Dock = DockStyle.Fill;
				form.Controls.Add(host);
				host.SetDataBinding(runner, "");

				List<string> unboundControls = new List<string>();

				foreach (Control control in host.Controls)
				{
					if (control is Label)
					{
						continue; //skip labels
					}

					if (string.IsNullOrEmpty(control.GetBindingMember()))
					{
						if (string.IsNullOrEmpty(control.Name))
						{
							unboundControls.Add(control.GetType().FullName);
						}
						else
						{
							unboundControls.Add(control.GetType().FullName + " (" + control.Name + ")");
						}
					}
				}

				Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
				if (unboundControls.Count > 0)
				{
					result.Add("These controls are not bound", unboundControls);
				}

				AssertGroupedErrorList(result);
			}
		}

		public void TestPopulate_WithFields()
		{
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			Assert("precondition:", runner.Fields.Count > 0);

			using (ZForm form = new ZForm(runner))
			{
				form.Size = new Size(400, 250);

				RunnerFieldControlHost host = new RunnerFieldControlHost();
				host.Dock = DockStyle.Fill;

				form.Controls.Add(host);

				AssertEquals("precondition:", 0, host.Controls.Count);
				host.SetDataBinding(runner, "");
				AssertEquals("should have added 2 controls for each FieldDescriptor (1 label, 1 edit control)", runner.Fields.Count * 2, host.Controls.Count);
			}
		}

		public void TestPopulate_WithoutFields()
		{
			Action.FieldDescriptors.RemoveAndDeleteAll();
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			AssertEquals("precondition:", 0, runner.Fields.Count);

			using (ZForm form = new ZForm(runner))
			{
				form.Size = new Size(400, 250);

				RunnerFieldControlHost host = new RunnerFieldControlHost();
				host.Dock = DockStyle.Fill;

				form.Controls.Add(host);

				AssertEquals("precondition:", 0, host.Controls.Count);
				host.SetDataBinding(runner, "");
				AssertEquals("should have added controls", 1, host.Controls.Count);
				AssertEquals("added control should be a ZLabel", typeof(ZLabel), host.Controls[0].GetType());
				AssertEquals("added control should have the correct text", "This action has no fields defined.", host.Controls[0].Text);
			}
		}

		public void TestAllUsed()
		{
			Dictionary<Type, bool> lookup = new Dictionary<Type, bool>();

			foreach (OperationalActionFieldDescriptor descriptor in Action.FieldDescriptors)
			{
				var supporter = descriptor.FieldSupporter;

				if (supporter != null)
				{
					lookup[supporter.GetType()] = true;
				}
			}

			List<string> missingTypes = new List<string>();

			foreach (Type supporterType in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (typeof(OperationalActionFieldSupporter).IsAssignableFrom(supporterType) && !lookup.ContainsKey(supporterType))
				{
					missingTypes.Add(supporterType.FullName);
				}
			}

			if (missingTypes.Count > 0)
			{
				Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
				result.Add("These supporters are missing from the test setup.", missingTypes);
				AssertGroupedErrorList(result);
			}
			else
			{
				Assert(true);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			OperationalActionRunner runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords());
			ZForm form = new ZForm(runner);
			form.Size = new Size(400, 250);
			form.CaptionRenderingEnabled = true;

			RunnerFieldControlHost host = new RunnerFieldControlHost();
			host.Dock = DockStyle.Fill;
			host.CaptionRenderingEnabled = true;

			form.Controls.Add(host);

			host.SetDataBinding(runner, "");
			return form;
		}

		OperationalActionSupporter ActionSupporter
		{
			get { return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter); }
		}
		OperationalActionSupporter actionSupporter;

		OperationalActionContext Context
		{
			get
			{
				if (context == null)
				{
					context = new OperationalActionContext(ActionSupporter, "Module Name");

					foreach (PropertyInfo info in ActionSupporter.RootType.GetProperties())
					{
						var a = context.FieldSupporters[info.Name];
					}
				}
				return context;
			}
		}
		OperationalActionContext context;

		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
					{
						OperationalActionFieldDescriptor descriptor = action.FieldDescriptors.AddNew();
						descriptor.FieldCaption = string.Format("Caption 0");
						descriptor.FieldName = "Undefined field name";
					}

					int i = 0;
					foreach (OperationalActionFieldSupporter supporter in Context.FieldSupporters)
					{
						OperationalActionFieldDescriptor descriptor = action.FieldDescriptors.AddNew();
						descriptor.FieldCaption = string.Format("Caption {0}", i++);
						descriptor.FieldName = supporter.Field;
					}
				}
				return action;
			}
		}
		OperationalAction action;

		#endregion
	}
}
