using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public abstract class BasherTest : CountrySpecificTest, INotifications
	{
		#region Bound Lists Not Loaded on Access

		[GuiTest]
		[RequiresSTA]
		public virtual void TestBoundListsAreNotLoadedOnAccess()
		{
			Assert(true);

			try
			{
				foreach (var testForm in FormsToBash)
				{
					AssertBoundListsAreNotLoadedOnAccess(testForm);
				}
			}
			catch (ModuleGuiNotSupportedException)
			{
			}
		}

		void AssertBoundListsAreNotLoadedOnAccess(Form testForm)
		{
			if (testForm != null)
			{
				using (testForm)
				{
					testForm.Show();
					ExposeAllTabPages(testForm);
					Application.DoEvents();
					ZGridControlBasher.ExposeAllColumnsInAllGrids(testForm, this);
					failedControls = null;
					CheckControlsForLoadedBoundList("", testForm);
					ThrowFailureException();
				}

				if (FailedControls.Length > 0)
				{
					var errors = "These controls have bound lists that are loaded" + System.Environment.NewLine + System.Environment.NewLine + FailedControls.ToString();
					throw new Exception(errors);
				}
			}
		}

		protected void CheckControlsForLoadedBoundList(string stackDescription, Control hostControl)
		{
			if (hostControl.IsDisposed)
			{
				Fail("Found disposed control : " + hostControl.Name);
			}
			foreach (Control control in hostControl.Controls)
			{
				if (!(control is ZTabPagePlugIn))
				{
					try
					{
						if (!control.Visible
							&& (control.Parent.GetType().FullName.IndexOf("ZGuidFilterControl") > 0
							|| control.Parent.GetType().FullName.IndexOf("ZCodeFilterControl") > 0))
						{
							control.Visible = true;
						}

						var listProvider = control as IFindBoxListProvider;
						var listUserControl = control as ZListUserControl;

						if (listProvider != null && listUserControl.Visible && (listUserControl == null || listUserControl.RequiresList))
						{
							if (listProvider.List == null)
							{
								if (control.DataBindings.Count > 0)
								{
									var currencyManager = (CurrencyManager)control.DataBindings[0].BindingManagerBase;
									if (currencyManager != null)
									{
										var collection = currencyManager.List as IBusinessObjectCollection;
										if (collection != null)
										{
											AddNewForLoadedBoundListCheck(collection);
										}
									}
								}
							}

							if (listProvider.List != null && listProvider.List.IsLoaded)
							{
								FailedControls.Append(control.Name + " - " + ((IBindToList)control).BindToList + System.Environment.NewLine);
							}
						}

						CheckControlsForLoadedBoundList(stackDescription + " - " + control.Name, control);
					}
					catch (Exception e)
					{
						FailedControls.Append(control.Name + " threw an exception" + System.Environment.NewLine + System.Environment.NewLine + e.ToString() + System.Environment.NewLine + System.Environment.NewLine);
					}
				}
			}
		}

		protected virtual void AddNewForLoadedBoundListCheck(IBusinessObjectCollection collection)
		{
			collection.AddNew();
		}

		#endregion

		#region INotifications

		void INotifications.Add(INotification notification)
		{
			if (notification.Type == BasherTestNotificationType.UniqueFooterMessage)
			{
				if (!FooterMessages.Contains(notification.Message))
				{
					FooterMessages.Add(notification.Message);
				}
			}
			else
			{
				AddError(notification.Message);
			}
		}

		internal void AddError(string message)
		{
			Failures.Add(AddExtraDebuggingMessage(message));
		}

		internal void AddFailure(string message, Exception innerException)
		{
			Failures.Add(new Exception(AddExtraDebuggingMessage(message), innerException).ToString());
		}

		internal void AddFailures(IEnumerable<string> failures)
		{
			Failures.AddRange(failures);
		}

		#endregion

		#region Test form is fully translatable

		[GuiTest]
		[RequiresSTA]
		public virtual void TestFormIsFullyTranslatable()
		{
			Assert(true);

			if (ShouldTestFormIsFullyTranslatable)
			{
				try
				{
					var report = ZFormTranslatableBasherTest.BashTranslatable(
						this,
						() =>
						{
							var testForm = GetFormToBash();
							testForm.Show();
							ExposeAllTabPages(testForm);
							Application.DoEvents();
							ZGridControlBasher.ExposeAllColumnsInAllGrids(testForm, this);
							return testForm;
						});

					if (!string.IsNullOrEmpty(report))
					{
						AddError(report);
					}

					ReportExceptions();
				}
				catch (ModuleGuiNotSupportedException)
				{
				}
			}
		}

		protected virtual bool ShouldTestFormIsFullyTranslatable => true;

		public virtual bool AllowUntranslatableFormTitle()
		{
			return false;
		}

		#endregion

		#region Implementation

		protected virtual IEnumerable<Form> FormsToBash
		{
			get { yield return GetFormToBash(); }
		}

		public abstract Form GetFormToBash();

		protected virtual string AddExtraDebuggingMessage(string message)
		{
			return message;
		}

		readonly List<string> Failures = new List<string>();
		readonly List<string> FooterMessages = new List<string>();

		protected IBusiness CurrentBO;
		protected string LastHasChangesStack;

		protected void HasChangesChanged(object sender, EventArgs e)
		{
			if (CurrentBO != null && CurrentBO.HasChanges)
			{
				LastHasChangesStack = new StackTrace().ToString();
			}
		}

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			var result = false;
			var current = control.Parent;
			while (current != null && !result)
			{
				var allowTabBackwardOnChildren = current as IAllowTabBackwardBetweenSomeOfMyChildren;
				if (allowTabBackwardOnChildren != null)
				{
					result = allowTabBackwardOnChildren.AllowTabBackward(control, previousControl);
				}
				current = current.Parent;
			}
			if (!result)
			{
				result = AllowOverlap(control, previousControl);
			}
			return result;
		}

		protected virtual bool AllowTabBackwardCore(Control control, Control previousControl)
		{
			return false;
		}

		public bool AllowOverlap(Control control, Control siblingControl) => control.IsOverlapAllowed(siblingControl);

		public bool AllowOutsideOfParentControl(Control control) => control.IsAllowedOutsideOfParent();

		protected delegate void TestMethod();

		protected void CheckForMemoryLeaks(TestMethod testToRun)
		{
			InvokeTestMethod(testToRun);

			// App.DoEvents is used to ensure all windows messages on the message loop when closing are processesd to ensure all disposing and closing logic is run.
			Application.DoEvents();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			ReportExceptions();
		}

		protected internal void ReportExceptions()
		{
			var errors = string.Join("<BR><BR>\r\n", GetFailureMessages());
			if (!string.IsNullOrEmpty(errors))
			{
				//new MiniDump().Make();
				HtmlFail(errors + "<BR><BR>");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = false;
			EnvProxy.Instance.Registry.BorderWiseEnableMultilineTariffClassification = false;
		}

		protected override void TearDown()
		{
			base.TearDown();
			CultureInfo.CurrentCulture = DefaultCulture.Instance;
		}

		protected void InvokeTestMethod(TestMethod testToRun)
		{
			var testType = GetType();
			var tempTestCase = (BasherTest)Activator.CreateInstance(testType);

			testType.GetMethod("SetUp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tempTestCase, Array.Empty<object>());
			// Ensure the factory used for this test is independent of other actions in the derived testcase
			tempTestCase.ReleaseFactory();

			try
			{
				testToRun.Method.Invoke(tempTestCase, Array.Empty<object>());
				Application.DoEvents();
			}
			finally
			{
				testType.GetMethod("TearDown", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tempTestCase, Array.Empty<object>());
				Balloon.Instance.Hide();
				Application.DoEvents();
				tempTestCase.ReleaseFactory();
				Failures.AddRange(tempTestCase.Failures);
				FooterMessages.AddRange(tempTestCase.FooterMessages);

				tempTestCase = null;
			}
		}

		protected string GetChildrenWithHasChanges(IBusiness businessEntity)
		{
			var result = "";
			if (businessEntity.HasChanges)
			{
				result += businessEntity.GetType().FullName + "\r\n";
				foreach (var child in businessEntity.Children)
				{
					result += GetChildrenWithHasChanges(child);
				}
			}
			return result;
		}

		protected void ExposeAllTabPages(Control ctrl)
		{
			var form = ctrl as ZForm;

			if (form != null)
			{
				CurrentBO = form.BusinessEntity;

				if (CurrentBO != null)
				{
					if (CurrentBO.HasChanges)
					{
						AddHasChangesIsTrueException(form);
					}

					CurrentBO.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(HasChangesChanged);
					LastHasChangesStack = null;
				}
			}

			try
			{
				foreach (Control nextCtrl in ctrl.Controls)
				{
					var tabControl = nextCtrl as ZTabControl;

					if (tabControl != null)
					{
						foreach (ZTabPage page in tabControl.TabPages)
						{
							tabControl.SelectedTab = page;
							Application.DoEvents();

							var parentForm = ctrl.FindForm() as ZForm;

							if (parentForm != null)
							{
								parentForm.ActiveControl = null;
								if (parentForm.BusinessEntity != null && parentForm.BusinessEntity.HasChanges && !AllowHasChangesOnFormOpen)
								{
									var message = "BusinessEntity.HasChanges was set to true on tab page: " + page.Name + " with Text: " + page.Text + " on TabControl: " + tabControl.Name;

									message += "\r\nCall stack where changes were made:\r\n" + (LastHasChangesStack ?? "HasChangesChanged was not fired");
									message += "\r\nObjects with haschanges = true \r\n" + GetChildrenWithHasChanges(parentForm.BusinessEntity);
									AddError(message);
									break;
								}
							}
							if (page.Controls.Count == 0 && page.Text != "Miscellaneous" && !(page.GetType().Name != "ZStmNoteTabPage"))
							{
								AddError("'" + page.Text + "' does not have any controls.  TabPages should all have at least one control");
							}

							ExposeAllTabPages(page);
						}
					}
					else
					{
						ExposeAllTabPages(nextCtrl);
					}
				}
			}
			finally
			{
				if (ctrl is ZForm)
				{
					if (CurrentBO != null)
					{
						CurrentBO.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(HasChangesChanged);
						CurrentBO = null;
					}
				}
			}
		}

		protected virtual bool AllowHasChangesOnFormOpen
		{
			get { return false; }
		}

		protected virtual void AddHasChangesIsTrueException(ZForm formWithChanges)
		{
			if (!AllowHasChangesOnFormOpen)
			{
				AddError("On the Top Level Object " + formWithChanges.BusinessEntity.GetType().ToString() +
					".HasChanges was set to true when the form: " + formWithChanges.Name +
					" with Caption: " + formWithChanges.Text + " loaded.");
			}
		}

		protected StringBuilder FailedControls
		{
			get { return failedControls ?? (failedControls = new StringBuilder()); }
		}
		StringBuilder failedControls;

		protected void ThrowFailureException()
		{
			var errors = string.Join("\r\n\t\n", GetFailureMessages());

			if (!string.IsNullOrEmpty(errors))
			{
				throw new Exception(errors);
			}
		}

		protected internal string[] GetFailureMessages()
		{
			var result = new List<string>();
			result.AddRange(Failures);
			result.AddRange(FooterMessages);
			return result.ToArray();
		}

		public Type BashType => TestedTypeHelper.GetTestedType(GetType());

		#endregion
	}
}
