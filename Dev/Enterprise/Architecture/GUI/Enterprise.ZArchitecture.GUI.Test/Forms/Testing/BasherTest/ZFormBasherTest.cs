using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
#if !WINZOR
using CargoWise.Main.Navigation.WPF;
#endif
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestsSubclassesOf(typeof(ZForm), typeof(TestExcludeZWinFormsAllHaveFormBashersAttribute), ExcludeClientDlls = false)]
	public abstract class ZFormBasherTest : BasherTest
	{
		/// <summary>
		/// if tested type is form derived from ZForm FormToBashType does not need to be overridden
		/// however some tests deriving from ZFormBasherTest are testing Controls so TestedType in such tests
		/// is not derived from ZForm - they need to override FormToBashType
		/// </summary>
		public virtual Type FormToBashType => BashType;

		public sealed override Form GetFormToBash()
		{
			var result = GetFormToBashCore();

			if (result is ZForm zForm)
			{
				zForm.IsFormToBash_ForTestOnly = true;
			}

			if (!FormToBashType.IsInstanceOfType(result))
			{
				throw new InvalidOperationException("FormToBashType must be consistent with the return value of GetFormToBash()");
			}
			return result;
		}

		protected abstract Form GetFormToBashCore();

		#region Unneccessary Validation

		[RequiresSTA]
		public virtual void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			var calledOn = new Dictionary<BusinessObject, StackTrace>();

			Factory.MarkedAsNeedingValidation += (BusinessObject obj) =>
			{
				if (obj.IsInDatabase)
				{
					if (!calledOn.ContainsKey(obj))
					{
						calledOn.Add(obj, new StackTrace(true));
					}
				}
			};

			using (var form = GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
			}

			var builder = new StringBuilder();
			builder.AppendLine("MarkAsNeedingValidation was called on " + calledOn.Count + " objects");

			foreach (var pair in calledOn)
			{
				var each = pair.Key;
				builder.AppendLine(each.GetType().FullName + ", PK: " + each.PK);
				builder.AppendLine(pair.Value.ToString());
				builder.AppendLine();
			}

			Assert(builder.ToString(), calledOn.Count == 0);
		}

		#endregion

		#region Memory Leaks

		[SnailTest]
		[ExpectNoExceptions]
		[RequiresSTA]
		public virtual void TestBashingForm()
		{
				Assert(true);
				CheckForMemoryLeaks(BashForm);
		}

		#endregion

		#region Missing Buttons

		[ExpectNoExceptions]
		[RequiresSTA]
		public virtual void TestMissingButtonsInGerman()
		{
			Assert(true);

			using (CargoWiseOne.ResourceStrings.Res.TemporarilySwitchLanguage(Enterprise.Core.Constants.Languages.German))
			{
				foreach (var testForm in FormsToBash)
				{
					using (testForm)
					{
						testForm.Show();
						Application.DoEvents();
						CheckForMissingButtons(testForm);
						UserIdleWorker.Flush();
					}
				}
			}

			ReportExceptions();
		}

		void CheckForMissingButtons(Form formToBash)
		{
			foreach (Control control in formToBash.Controls)
			{
				CheckForMissingButtons(control);
			}
		}

		void CheckForMissingButtons(Control controlToBash)
		{
			if (!(controlToBash is ZGrid))
			{
				var controls = new ArrayList(controlToBash.Controls);

				foreach (Control control in controls)
				{
					if (!(control is TabPage))
					{
						CheckForMissingButtons(control);
					}
				}
			}

			if (!(controlToBash is Form))
			{
				if (controlToBash is ZToolStrip)
				{
					new PositionChecker(this).CheckControlPosition(controlToBash);
					new ZToolStripBasher().Bash(controlToBash, this);
				}
			}
			else
			{
				CheckForMissingButtons(controlToBash as Form);
			}
		}

		#endregion

		#region HasChanges on Saved Object

		[GuiTest]
		[RequiresSTA]
		public virtual void TestHasChangesOnPreviouslySavedObject()
		{
			Assert(true);
			foreach (var testForm in FormsToBash)
			{
				AssertHasChangesOnPreviouslySavedObject(testForm);
			}
		}

		void AssertHasChangesOnPreviouslySavedObject(Form formToBash)
		{
			using (formToBash)
			{
				if (formToBash is ZForm && AllowSaveOnFormForTestHasChanges)
				{
					var entity = ((ZForm)formToBash).BusinessEntity;
					if (entity is BusinessObject)
					{
						var bizO = entity as BusinessObject;
						if (bizO != null)
						{
							bizO.FillWithValidTestData();
							var hasException = false;
							try
							{
								if (!(bizO.Factory is ReadOnlyBusinessObjectFactory))
								{
									bizO.Factory.Save();
								}
							}
							catch
							{
								hasException = true;
							}
							if (!hasException)
							{
								AssertEquals("PreCondition : HasChanges", false, bizO.HasChanges);
								formToBash.Show();
								Application.DoEvents();
								AssertEquals("HasChanges after showing form", false, bizO.HasChanges);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Only override this property and return false if your top level bizO is generated from a dataset 
		/// but doesn't have a table in the DB. Doing this will make TestHasChangesOnPreviouslySavedObject pass.
		/// </summary>
		protected virtual bool AllowSaveOnFormForTestHasChanges
		{
			get { return true; }
		}

		#endregion

		#region Binding Tabs on Idle

		[ExpectNoExceptions]
		[RequiresSTA]
		public virtual void TestBindingAllTabsOnIdle()
		{
			using (var form = GetFormToBash())
			{
				var zform = form as ZForm;
				if (zform != null && zform.BusinessEntity != null)
				{
					form.Show();
					Application.DoEvents();
					zform.BusinessEntity.HasChanges = true;
					UserIdleWorker.Flush();
				}
			}
		}

		#endregion

		#region PlugIns

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestAuditPlugIn()
		{
			using (var form = GetFormToBash())
			{
				var zform = form as ZForm;

				if (zform != null && zform.BusinessEntity != null)
				{
					if (zform.PlugIns.IsPlugInAvailable(ControllerIDs.Audit))
					{
						var bizObj = zform.BusinessEntity as BusinessObject;
						AssertNotNull("Business Object of form with audit plug-in", bizObj);

						var expectedAuditTable = string.Format(CultureInfo.InvariantCulture,
							"[{0}].[{1}].[{2}]",
							CargoWise.Data.Db.AuditDatabaseName,
							bizObj.PKSchemaColumn.TableSchema.SqlSchemaName,
							bizObj.TableName);
						var auditTableIdSql = string.Format(CultureInfo.InvariantCulture, "SELECT OBJECT_ID(N'{0}', N'U')", expectedAuditTable);

						Assert(
							"Business Object audit table " + expectedAuditTable + " does not exist",
							((IDbConnected)Factory).Connection.ExecuteScalar(auditTableIdSql) != DBNull.Value);
					}
				}
			}
		}

		#endregion

		#region Form Size

		[GuiTest]
		[RequiresSTA]
		public virtual void TestMinimumSizeNotTooBig()
		{
			using (var testForm = GetFormToBash())
			{
				var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);
				var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(768);
				var typicalTaskbarHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(43);

				var maxSizeWidth = minScreenWidthSupported;
				var maxSizeHeight = minScreenHeightSupported - typicalTaskbarHeight;

				Assert(
					"Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(),
testForm.MinimumSize.Width <= maxSizeWidth);
				Assert(
					"Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(),
					testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		[GuiTest]
		[RequiresSTA]
		public void TestFormIsResizable()
		{
			var errorBuilder = new ZStringBuilder();

			if (!AllowFormSizeFixed)
			{
				using (var testForm = GetFormToBash())
				{
					var initialFormWidth = testForm.Width;
					var initialFormHeight = testForm.Height;

					testForm.Show();
					Application.DoEvents();

					var finalFormWidth = testForm.Width;
					var finalFormHeight = testForm.Height;

					var isWidthChanged = finalFormWidth != initialFormWidth;
					var isHeightChanged = finalFormHeight != initialFormHeight;

					if (isWidthChanged || isHeightChanged)
					{
						if (!(testForm.WindowState == FormWindowState.Maximized && testForm.FormBorderStyle == FormBorderStyle.None)
								&& testForm.FormBorderStyle != FormBorderStyle.Sizable)
						{
							errorBuilder.AppendLine(@"Set FormBoarderStyle property to FormBorderStyle.Sizable.");
						}

#if !WINZOR

						if (isWidthChanged && !testForm.HorizontalScroll.Enabled)
						{
							errorBuilder.AppendLine(@"Set HorizontalScoll.Enabled to true.");
						}

						if (isHeightChanged && !testForm.VerticalScroll.Enabled)
						{
							errorBuilder.AppendLine(@"Set VerticalScroll.Enabled to true.");
						}

#endif
					}
				}
			}

			if (errorBuilder.Length > 0 && !skippedFormType.Contains(FormToBashType.FullName))
			{
				errorBuilder.Prepend(
					"Any form that dynamically changes size must be resizable and have its scrollbars enabled, please change this form according to the following steps: \r\n");
				Fail(errorBuilder.ToString());
			}
			else if (errorBuilder.Length == 0 && skippedFormType.Contains(FormToBashType.FullName))
			{
				errorBuilder.AppendLine(
					string.Format(@"Good job for fixing the resizable issue of this form, please remove the form type name '{0}' from the field 'skippedFormType' of ZFormBasherTest.", FormToBashType.FullName));
				Fail(errorBuilder.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool AllowFormSizeFixed => false;

		readonly HashSet<string> skippedFormType = new HashSet<string>()
		{
			"Enterprise.Accounting.GUI.CashBook.Transfer.BankTransferForm",
			"Enterprise.Accounting.GUI.JobInvoicing.ChangeTransactionDatesMessageBox",
			"Enterprise.CommissionManagement.GUI.CommissionAgreementApproveProgressForm",
			"Enterprise.CommissionManagement.GUI.RequestApprovalForm",
			"Enterprise.Warehouse.Transactions.GUI.DocketsLabelOptionForm",
			"Enterprise.Warehouse.Transactions.GUI.LabelOptionsForm",
			"Enterprise.Warehouse.Transactions.GUI.WhsDocumentInventoryOptionsForm",
			"Enterprise.ZArchitecture.GUI.GlowDataWizardImportForm",
			"Enterprise.Client.JAS.GUI.Cognos.CognosCsvExportForm",
			"Enterprise.Client.JAS.GUI.JXCImporterForm",
			"Enterprise.Client.JAS.GUI.PreShipmentExporterForm",
			"Enterprise.Client.UPE.GUI.DataImport.Level1DataImportForm"
		};

		#endregion

		#region Splitter index

		[RequiresSTA]
		public void TestSplitterIndex()
		{
			TestSplitterIndexCore();
		}

		protected virtual void TestSplitterIndexCore()
		{
			Assert(true);

			foreach (var testForm in FormsToBash)
			{
				using (testForm)
				{
					testForm.Show();
					Application.DoEvents();
					CheckForSplitterIndex(testForm);
					UserIdleWorker.Flush();
				}
			}
		}

		void CheckForSplitterIndex(Form formToBash)
		{
			var errorBuilder = new ZStringBuilder();
			CheckForSplitterIndex(formToBash, errorBuilder);
			if (errorBuilder.Length > 0)
			{
				Fail(errorBuilder.ToString());
			}
		}

		void CheckForSplitterIndex(Control controlToBash, ZStringBuilder errorBuilder)
		{
			var splitContainers = new Dictionary<string, Orientation>();
			var splitContainerNeedToBeFixed = new List<string>();

			foreach (Control control in controlToBash.Controls)
			{
				if (control is KSplitter splitter)
				{
					if (splitter.Dock == DockStyle.Top || splitter.Dock == DockStyle.Bottom)
					{
						splitContainerNeedToBeFixed = splitContainers.Where(p => p.Value == Orientation.Horizontal).Select(q => q.Key)
							.ToList();
					}
					else
					{
						splitContainerNeedToBeFixed = splitContainers.Where(p => p.Value == Orientation.Vertical).Select(q => q.Key)
							.ToList();
					}

					if (splitContainerNeedToBeFixed.Count > 0)
					{
						errorBuilder.AppendLine();
						errorBuilder.AppendLine(string.Format(
							@"KSplitContainers should be added after KSplitter if they have the same parent control to avoid layout issues, please move the adding code of the KSplitter before these KSplitContainers.
KSplitter: {0}", ControlDescription.GetControlPath(control)));
						foreach (var item in splitContainers)
						{
							errorBuilder.AppendLine("KSplitContainer: " + item);
						}
						splitContainerNeedToBeFixed.Clear();
						splitContainers.Clear();
					}
				}
				else if (control is KSplitContainer splitContainer)
				{
					splitContainers.Add(ControlDescription.GetControlPath(control), splitContainer.Orientation);
				}
				CheckForSplitterIndex(control, errorBuilder);
			}
		}

		#endregion

		#region BashForm

		internal void BashForm()
		{
			ZFilterGridModule.ReturnSingleRow = true;

			try
			{
				foreach (var testForm in FormsToBash)
				{
					if (!testForm.IsDisposed)
					{
						BashForm(testForm);
					}
				}
			}
			finally
			{
				ZFilterGridModule.ReturnSingleRow = false;
			}
		}

		protected void BashForm(Form testForm)
		{
			ZLabelCaptionCache.Instance.ClearCache();

			using (testForm)
			{
				try
				{
					OnceOnlyBashTabPages.Clear();
					OnceOnlyBashedTabPages.Clear();
					testForm.Show();
					if (testForm.IsDisposed)
					{
						throw new Exception("The form being bashed was prematurely disposed. Please investigate why");
					}
					BashScenario(testForm);
					UnbindAndRebind(testForm); // expect no exception
					AssertExpectedExecuteAllFetchHintsBeforeValidateAll(testForm);
				}
				finally
				{
					OnceOnlyBashTabPages.Clear();
					OnceOnlyBashedTabPages.Clear();
				}

				UserIdleWorker.Flush();
			}
		}

		protected void AssertExpectedExecuteAllFetchHintsBeforeValidateAll(Form testForm)
		{
			if (testForm is ZForm)
			{
				AssertEquals("ExecuteAllFetchHintsBeforeValidateAll should be as expected", ExpectedExecuteAllFetchHintsBeforeValidateAll, ((ZForm)testForm).ExecuteAllFetchHintsBeforeValidateAll);
			}
		}

		protected virtual bool ExpectedExecuteAllFetchHintsBeforeValidateAll
		{
			get { return true; }
		}

		protected virtual void BashScenario(Form testForm)
		{
			try
			{
				ExposeAllTabPages(testForm);

				Application.DoEvents(); // required for binding to start
				ZGridControlBasher.ExposeAllColumnsInAllGrids(testForm, this);

				BashedTabPages.Clear();
				SetupTabPagesThatNeedToBeBashedOnlyOnce(testForm);
				CheckMainTabControlIsNotMultiline(testForm);

				//while (testForm.Visible)
				//{
				//	Application.DoEvents();
				//}

				BashControl(testForm);
				TestSave(testForm as ZForm);
				Assert("AcceptButton only works if it is a Control type", testForm.AcceptButton == null || testForm.AcceptButton is Control);
			}
			finally
			{
				BashedTabPages.Clear();
			}
		}

		static void UnbindAndRebind(Form form)
		{
			if (TypeDescriptor.GetAttributes(form)[typeof(SuppressRebindBasherTestAttribute)] == null)
			{
				var dataBoundControl = form as IDataBoundControl;
				if (dataBoundControl != null)
				{
					var dataSource = dataBoundControl.DataSource;
					var dataMember = dataBoundControl.DataMember;
					dataBoundControl.SetDataBinding(null, "");
					dataSource = LoadInNewFactoryIfPossible(dataSource);
					dataBoundControl.SetDataBinding(dataSource, dataMember);
				}
			}
		}

		static object LoadInNewFactoryIfPossible(object dataSource)
		{
			var businessObject = dataSource as BusinessObject;
			var result = dataSource;
			if (businessObject != null &&
				!(businessObject is NonPersistentBusinessObject) &&
				businessObject.IsInDatabase &&
				businessObject.Factory.GetType() == typeof(BusinessObjectFactory))
			{
				result = new BusinessObjectFactory().Load(businessObject.GetType(), businessObject.PK);
			}
			return result;
		}

		void CheckMainTabControlIsNotMultiline(Form testForm)
		{
			foreach (var control in testForm.Controls)
			{
				var tabControl = control as TabControl;
				if (tabControl != null && tabControl.Multiline)
				{
					if (AllowedMultilineTabControls == null || !AllowedMultilineTabControls.Contains(tabControl.Name))
					{
						AddError("TabControl " + tabControl.Name + " has Multiline set to true. By default tabs on tab controls should be in single-line layout.");
					}
				}
			}
		}

		protected virtual IList<string> AllowedMultilineTabControls
		{
			get { return null; }
		}

		#endregion

		#region BashControl

		protected ArrayList BashedTabPages = new ArrayList();
		static readonly int ControlHasBeenBashedKey = ControlExtensions.CreateUserDataKey();

		bool HasControlBeenBashed(Control control)
		{
			return control.GetUserData(ControlHasBeenBashedKey) != null;
		}

		void MarkControlHasBeenBashed(Control control)
		{
			control.SetUserData(ControlHasBeenBashedKey, new object());
		}

		protected virtual void BashControl(Control controlToBash)
		{
			if (controlToBash == null)
			{
				throw new ArgumentNullException(nameof(controlToBash));
			}

			try
			{
				if (IsControlBashable(controlToBash))
				{
					if (!HasControlBeenBashed(controlToBash) || controlToBash is ZGrid)
					{
						UserIdleWorker.Flush();
						Application.DoEvents();
						if (!(controlToBash is Label)
						)
						{
							controlToBash.Focus();
						}
						if (!IsControlBashable(controlToBash))
						{
							return;
						}
						BashControlAndMarkAsBashed(controlToBash);
						EnsureHasBindingMember(controlToBash);
						Application.DoEvents();
					}

					if (!(controlToBash is ZGrid))
					{
						var controls = new ArrayList(controlToBash.Controls);

						foreach (Control control in controls)
						{
							if (!(control is TabPage)
#if WINZOR
								&& !(control is ToolStripItem)
#endif
								)
							{
								BashControl(control);
							}
						}
					}
					else
					{
						CheckBindedBusinessObjectCollectionThrowsNoExceptionForCodeFromDescription(controlToBash);
					}

					if (controlToBash is TabControl)
					{
						var tabControl = controlToBash as TabControl;

						for (var i = 0; i < tabControl.TabPages.Count; i++)
						{
							using (OperationFreezeChecker.EnsureOperationDoesntFreeze($"Assigning {tabControl.Name}.SelectedIndex = {i} with SubTabName {tabControl.TabPages[i].Text} took a very long time to respond."))
							{
								tabControl.SelectedIndex = i;
							}
							Application.DoEvents();

							var tabPageToBash = tabControl.SelectedTab;
							if (!BashedTabPages.Contains(tabPageToBash))
							{
								if (!OnceOnlyBashedTabPages.Contains(tabPageToBash))
								{
									if (tabPageToBash.Parent == null)
									{
										//TabPageToBash.Parent = TabControl; // this isn't a good solution.
										Fail(string.Format("TabPage.Parent ({0}) property is null", tabPageToBash.Name));
									}
									// Some screens may force selected tab page changes, and there is no need to re-bash a tab page that has already been bashed.
									BashControl(tabPageToBash);
									if (OnceOnlyBashTabPages.Contains(tabPageToBash))
									{
										OnceOnlyBashedTabPages.Add(tabPageToBash);
									}
								}
								BashedTabPages.Add(tabControl.SelectedTab);
							}
						}
					}

#if !WINZOR
					if (controlToBash is ElementHost)
					{
						var wpfControl = controlToBash as ElementHost;
						if (wpfControl.Child != null)
						{
							var errors = XamlEmbeddedControlRetriever.CheckMultilingualControl(wpfControl.Child);
							errors.AddRange(XamlEmbeddedControlRetriever.CheckControlPosition(wpfControl.Child));
							foreach (var error in errors)
							{
								AddError(error);
							}
						}
					}
#endif

					if (!(controlToBash is Form))
					{
						new PositionChecker(this).CheckControlPosition(controlToBash);
					}

					if (controlToBash is ZLabel)
					{
						var ctrl = controlToBash as ZLabel;

						var ext = ctrl.GetExtension<ILabelCaptionRenderer>();
						if (ext.Options != StringRenderingOptions.Truncate && ext.IsCaptionTruncated)
						{
							AddError(
								ControlDescription.GetControlPath(ctrl)
								+ " - Label text is being implicitly truncated; increase your label size, shorten your text or explicitly mark the label as truncated."
								+ " Text=\"" + ctrl.Text + "\"");
						}
					}
				}
			}
			catch (Exception e)
			{
				if (IsReportableException(e))
				{
					if (e is ZFormBashingExceptionWhereCallStackIsMeaningless)
					{
						AddError("Notification From: " + ControlDescription.GetControlPath(controlToBash) + "\r\n" + e.Message + "\r\n");
					}
					else
					{
						AddFailure("Died on Control: " + ControlDescription.GetControlPath(controlToBash), e);
					}
				}
			}
		}

		void EnsureHasBindingMember(Control control)
		{
			var bindingSource = KBindingSource.GetBindingSource(control);
			if (bindingSource != null && bindingSource.DataSource != null)
			{
				var dataBoundControl = control as IDataBoundControl;
				if (!control.GetType().Namespace.StartsWith("System.") &&
					((IExtenderProvider)bindingSource).CanExtend(control) &&
					(dataBoundControl == null || dataBoundControl.DataSource == null) &&
					!(control is Button) &&
					string.IsNullOrEmpty(control.GetBindingMember()) &&
					!(control is Label) &&
					!(control is ProgressBar) &&
					!(control is PictureBox) &&
					!IsSmallerPartOfCompositeControl(control) &&
					!HasAncestorWithBindingMemberSuppressAttribute(control) &&
					!(control is ZGrid && ((ZGrid)control).DataSource is BusinessObjectCollection) &&
					!ShouldIgnoreMissingBindingMember(control))
				{
					AddError("Control '" + control.Name + "' does not have a BindingMember (or BindTo) set.");
				}
			}
		}

		protected virtual bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "econversationMessageTextBox";
		}

		static bool IsSmallerPartOfCompositeControl(Control control)
		{
			var current = control.Parent;
			while (current != null)
			{
				if (!string.IsNullOrEmpty(current.GetBindingMember()))
				{
					return true;
				}
				current = current.Parent;
			}
			return false;
		}

		static bool HasAncestorWithBindingMemberSuppressAttribute(Control control)
		{
			var current = control;
			while (current != null)
			{
				if (TypeDescriptor.GetAttributes(current)[typeof(SuppressBindingMemberBashingTestAttribute)] != null)
				{
					return true;
				}
				current = current.Parent;
			}
			return false;
		}

		protected bool IsControlBashable(Control control)
		{
			var result = control.Visible;
			result &= control.Enabled;
			result &= control.GetType().GetCustomAttributes(typeof(FormBasherTestPopupExcludeAttribute), false).Length == 0;
			return result;
		}

		protected virtual void SetupTabPagesThatNeedToBeBashedOnlyOnce(Form testForm)
		{
		}

		protected List<TabPage> OnceOnlyBashTabPages
		{
			get { return onceOnlyBashTabPages ?? (onceOnlyBashTabPages = new List<TabPage>()); }
		}
		List<TabPage> onceOnlyBashTabPages;

		protected virtual TabPage[] GetOnceOnlyBashTabPages()
		{
			return Array.Empty<TabPage>();
		}

		List<TabPage> OnceOnlyBashedTabPages
		{
			get { return onceOnlyBashedTabPages ?? (onceOnlyBashedTabPages = new List<TabPage>()); }
		}
		List<TabPage> onceOnlyBashedTabPages;

		void BashControlAndMarkAsBashed(Control control)
		{
			var bashers = GetControlBashers(control);
			foreach (var basher in bashers)
			{
				basher.Bash(control, this);
			}
			MarkControlHasBeenBashed(control);
		}

		protected virtual IControlBasher[] GetControlBashers(Control control)
		{
			if (CurrentTestName?.Contains(nameof(TestMissingResourceStringDataAttributeForBoundProperty)) ?? false)
			{
				return ControlBasherFactory.GetForVerifyingResourceStringAttributeForBoundProperty();
			}

			return ControlBasherFactory.Get(control);
		}

		#endregion

		#region Implementation

		protected bool IsReportableException(Exception ex)
		{
			for (; ex != null; ex = ex.InnerException)
			{
				if (ex is ModuleFeatureNotSupportedException)
				{
					return false;
				}
			}
			return true;
		}

		protected void TestSave(ZForm form)
		{
			if (form != null && !(form is ZChildForm) && form.BusinessEntity != null)
			{
				try
				{
					form.FireSaveButton();
				}
				catch (Exception ex)
				{
					if (IsReportableException(ex))
					{
						AddFailure("Form died on saving", ex);
					}
				}
			}
		}

		#endregion

		#region Test (for this test)

		[TestedType(typeof(ZTestForm))]
		class FormTesterTest : ZFormBasherTest
		{
			protected override Form GetFormToBashCore()
			{
				return new ZTestForm(Dummy);
			}

			protected override void SetUp()
			{
				base.SetUp();

				Dummy = Factory.New<DummyBusinessObject>();
				AssertNotNull("Dummy should be created!", Dummy);
			}

			protected DummyBusinessObject Dummy;
		}

		#endregion

		#region Import Wizard Tests

		public void CheckBindedBusinessObjectCollectionThrowsNoExceptionForCodeFromDescription(Control control)
		{
			var grid = control as ZGrid;

			if (grid.List != null && grid.ShowImportDataMenuItem)
			{
				var importCollectionInfoProvider = grid.GetImportCollectionInfoProvider();
				if (importCollectionInfoProvider != null)
				{
					try
					{
						BusinessObject bo;
						if (importCollectionInfoProvider.ImportCollectionInfo.Collection.Count == 0)
						{
							if (importCollectionInfoProvider.ImportCollectionInfo.Collection is IActiveBusinessObjectCollection)
							{
								bo = ((IBindingList)importCollectionInfoProvider.ImportCollectionInfo.Collection).AddNew() as BusinessObject;
							}
							else
							{
								bo = importCollectionInfoProvider.ImportCollectionInfo.Collection.AddNew();
							}
						}
						else
						{
							bo = importCollectionInfoProvider.ImportCollectionInfo.Collection[0] as BusinessObject;
						}

						foreach (var propertyInfo in importCollectionInfoProvider.ImportCollectionInfo.Properties)
						{
							var provider = propertyInfo.GetFindBoxListProvider(bo) as IFindBoxListProviderDescriptionEx;
							if (provider != null)
							{
								provider.CodeFromDescription("description");
							}
						}
						//TODO: iterate test collection and child collections
					}
					catch (Exception ex)
					{
						var path = grid.Name;
						var parent = grid.Parent;
						while (parent != null)
						{
							path = parent.Name + " -> " + path;
							parent = parent.Parent;
						}
						Assert("ZGrid " + path + " throws exception: " + ex.Message, false);
					}
				}
			}
		}

		#endregion

		#region Grid Data Binding
		[DeveloperOnlyTest]
		[RequiresSTA]
		public void TestBusinessObjectCollectionsNotBoundToMultipleGrids()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				Application.DoEvents(); // required for binding to start
				TabPageNotificationsExposer.ExposeTabPageNotifications(form, new string[] { "dummy" }, false);
				CheckGridsDataBindings(form);
			}
		}

		static void CheckGridsDataBindings(Form form)
		{
			var controlsByCollection = new Dictionary<IBusinessObjectCollection, IList<Control>>();
			foreach (var grid in form.FindAll<ZGrid>())
			{
				grid.ForceBindingIncludingParents();

				if (grid.List is IBusinessObjectCollection collection)
				{
					AddToList(controlsByCollection, collection, grid);
				}
			}

			var errors = new Dictionary<string, IList<string>>();
			foreach (var pair in controlsByCollection)
			{
				var faultyControls = new HashSet<Control>();
				for (var i1 = 0; i1 < pair.Value.Count; i1++)
				{
					var control1 = pair.Value[i1];
					for (var i2 = i1 + 1; i2 < pair.Value.Count; i2++)
					{
						var control2 = pair.Value[i2];
						if (IsOnDifferentTabs(control1, control2))
						{
							faultyControls.Add(control1);
							faultyControls.Add(control2);
						}
					}
				}

				foreach (var controlPath in faultyControls.Select(GetControlPathAsString).OrderBy(p => p))
				{
					AddToList(errors, pair.Key.GetType().Name, controlPath);
				}
			}

			CombineAssertions(() =>
			{
				AssertGroupedErrorList("Following bizo collections are bound to multiple grids (by collection object).", errors);
			});
		}

		static bool IsOnDifferentTabs(Control control1, Control control2)
		{
			var path1 = GetControlPath(control1);
			var path2 = GetControlPath(control2);

			var lastCommonParent = -1;
			for (var i = 0; i < path1.Count && i < path2.Count && path1[i] == path2[i]; i++)
			{
				lastCommonParent = i;
			}

			return lastCommonParent >= 0 && path1[lastCommonParent] is TabControl;
		}

		static void AddToList<TKey, TValue>(IDictionary<TKey, IList<TValue>> dictionary, TKey key, TValue value)
		{
			if (!dictionary.TryGetValue(key, out var list))
			{
				list = new List<TValue>();
				dictionary.Add(key, list);
			}

			list.Add(value);
		}

		static string GetControlPathAsString(Control control)
		{
			return string.Join("/", GetControlPath(control).Select(c => c.Name ?? string.Empty));
		}

		static IReadOnlyList<Control> GetControlPath(Control control)
		{
			var path = new List<Control>();
			for (; control != null; control = control.Parent)
			{
				path.Add(control);
			}

			path.Reverse();
			return path;
		}
		#endregion

		#region Incorrect IDs

		[DeveloperOnlyTest]
		[RequiresSTA]
		public void TestCorrectModuleIdAndMenuSection()
		{
			Assert(true);
			using (var form = GetFormToBash())
			{
				if (typeof(ZChildForm).IsAssignableFrom(form.GetType()))
				{
					return;
				}

				var controller = ObjectFactory.Get<IServiceRequestController>();
				var module = controller.GetModuleId(form);
				AssertNotEquals("Please set ControllerID for " + form.GetType() + ". To do this, please assign ControllerID in the form constructor to ensure F1 hotkey assigns a new incident to the correct area.", string.Empty, module);
				AssertNotEquals("Menu section should not be Other for " + form.GetType() +
					". If the form is a module form, please update the section in ModuleTreeLoader. Otherwise, inherit the form the ICustomerServiceMenuSectionCodeOverridable interface and implement the SectionCode property.",
					MandatoryCustomerServiceMenuSectionList.Codes.Other, controller.GetCustomerServiceMenuSectionCode(module, form));
			}
		}

		#endregion

		#region Missing ResourceStringData Attribute on a bound property

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestMissingResourceStringDataAttributeForBoundProperty()
		{
			Assert(true);
			BashForm();
			ReportExceptions();
		}

		#endregion

		protected override void TearDown()
		{
			base.TearDown();
			CultureInfo.CurrentCulture = DefaultCulture.Instance;
		}
	}
}
