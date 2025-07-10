using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	sealed class ActionMethodTabPageTest : TestCaseWithFactory
	{
		public void TestMultiBind()
		{
			Supporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
			OperationalActionMethod method = Supporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			OperationalActionMethodApplicator applicator = Runner.MethodApplicators[method];

			using (ZForm form = new ZForm(Runner))
			{
				ZTabPage standardPage = new ZTabPage();
				ActionMethodTabPage magicPage = new ActionMethodTabPage(method);

				ZTabControl control = new ZTabControl();
				control.Dock = DockStyle.Fill;
				control.TabPages.Add(standardPage);
				control.TabPages.Add(magicPage);

				form.Controls.Add(control);

				form.SetDataBinding(Runner, "");
				form.Show();
				Application.DoEvents();

				control.SelectedTab = magicPage;
				AssertChildren("should be constructed", magicPage, new Type[] { typeof(DummyApplicatorControl) });

				control.SelectedTab = standardPage;
				control.SelectedTab = magicPage;
				AssertChildren("should not be constructed again", magicPage, new Type[] { typeof(DummyApplicatorControl) });

				DummyApplicatorControl appControl = (DummyApplicatorControl)magicPage.Controls[0];
				AssertEquals("should be bound to the correct thing.", applicator, appControl.CurrentDataItem);
			}
		}

		#region Implementation

		void AssertChildren(string message, Control control, IEnumerable<Type> expectedTypes)
		{
			Type[] actualTypes = new Type[control.Controls.Count];

			for (int i = 0; i < actualTypes.Length; i++)
			{
				actualTypes[i] = control.Controls[i].GetType();
			}

			AssertContainsExactElementsInAnyOrder(message, expectedTypes, actualTypes);
		}

		OperationalActionRunner Runner
		{
			get
			{
				if (runner == null)
				{
					runner = new OperationalActionRunner(Action, typeof(DummyBusinessObject), new SelectedRecords());
				}
				return runner;
			}
		}
		OperationalActionRunner runner;

		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}
				return action;
			}
		}
		OperationalAction action;

		OperationalActionContext Context
		{
			get { return context ?? (context = new OperationalActionContext(Supporter, "Module Name")); }
		}
		OperationalActionContext context;

		OperationalActionSupporter Supporter
		{
			get { return supporter ?? (supporter = new MockOperationalActionSupportable().OperationalActionSupporter); }
		}
		OperationalActionSupporter supporter;

		#endregion
	}
}
