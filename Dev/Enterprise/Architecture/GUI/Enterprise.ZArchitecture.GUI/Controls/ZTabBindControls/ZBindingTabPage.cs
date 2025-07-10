using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class ZBindingTabPage : ZTabPage
	{
		protected ZBindingTabPage()
		{
			ReportLegacyOnlyError();
		}

		#region Legacy Support Only

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related, Programmatic constant")]
		void ReportLegacyOnlyError()
		{
			var type = GetType();
			var fullName = type.FullName;
			var baseFullName = type.BaseType.FullName;
			var baseBaseFullName = type.BaseType.BaseType.FullName;
			if (fullName != "Enterprise.Customs.GUI.BaseDeclarationTabPage" &&
				baseFullName != "Enterprise.Customs.GUI.BaseDeclarationTabPage" &&
				baseBaseFullName != "Enterprise.Customs.GUI.BaseDeclarationTabPage" &&
				fullName != "Enterprise.Freight.Agency.GUI.BindingTab" &&
				baseFullName != "Enterprise.Freight.Agency.GUI.BindingTab" &&
				baseBaseFullName != "Enterprise.Freight.Agency.GUI.BindingTab" &&
				fullName != "Enterprise.MasterFiles.GUI.ZExceptionsTabPage" &&
				fullName != "Enterprise.MasterFiles.GUI.ZMilestonesTabPage" &&
				fullName != "Enterprise.MasterFiles.GUI.ZWorkflowTriggersTabPage" &&
				fullName != "Enterprise.BufferManagement.GUI.WorkflowManagementTabPage" &&
				fullName != "Enterprise.BufferManagement.GUI.ExtensionMethods+BindingTabPageImplementation" &&
				fullName != "Enterprise.BufferManagement.GUI.ExtensionMethods+AlwaysEnabledBindingTabPage" &&
				fullName != "Enterprise.MasterFiles.GUI.TemplateJobSchedulingTabPage" &&
				fullName != "Enterprise.MasterFiles.GUI.ZWorkflowTabPage" &&
				baseFullName != "Enterprise.MasterFiles.GUI.ZWorkflowTabPage" &&
				baseBaseFullName != "Enterprise.MasterFiles.GUI.ZWorkflowTabPage" &&
				fullName != "Enterprise.MasterFiles.GUI.TaskWithDetailsAndFilterTab" &&
				baseFullName != "Enterprise.MasterFiles.GUI.TaskWithDetailsAndFilterTab" &&
				fullName != "Enterprise.MasterFiles.GUI.TaskWithDetailsAndFilterTab" &&
				baseBaseFullName != "Enterprise.MasterFiles.GUI.TaskWithDetailsAndFilterTab" &&
				fullName != "Enterprise.Services.OperationalActions.GUI.ActionMethodTabPage" &&
				fullName != "Enterprise.ZArchitecture.GUI.ZTabPagePlugIn" &&
				baseFullName != "Enterprise.ZArchitecture.GUI.ZTabPagePlugIn" &&
				baseBaseFullName != "Enterprise.ZArchitecture.GUI.ZTabPagePlugIn" &&
				fullName != "Enterprise.ZArchitecture.GUI.ZActivityLoggingTabPage" &&
				fullName != "Enterprise.ZArchitecture.GUI.ZLogsTabPage" &&
				fullName != "Enterprise.ZArchitecture.GUI.ZStmALogTabPage" &&
				fullName != "Enterprise.ZArchitecture.GUI.ZStmNoteTabPage" &&
				fullName != "Enterprise.ZArchitecture.GUI.ZStmNoteForRegistryItemTabPage" &&
				fullName != "Enterprise.ZArchitecture.GUI.Testing.ZStmNoteTestTabPage" &&
				!fullName.StartsWith("Castle.Proxies.") &&
				!type.Name.StartsWith("Test") &&
				!type.Name.StartsWith("Mock") &&
				!type.Name.EndsWith("Exposed"))
			{
				ErrorReporter.ReportOnce(
					"ZBindingTabPageLegacy_" + fullName,
					"ZBindingTabPage is a legacy component, with ZTabPage as its replacement. " +
					"Consider creating a ZUserControl for your controls if you need to override SetDataBinding().");
			}
		}

		#endregion

		#region Binding

		public bool IsBound { get; private set; }

		public void Bind()
		{
			SetDataBinding(DataSource, DataMember);
		}

		bool BindingEnabled
		{
			get { return ParentTabControl != null && ParentTabControl.BindingEnabled; }
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			if (!IsBound && FindForm() != null)
			{
				if (dataSource == null)
				{
					throw new Exception(string.Format(
						"Could not find an object or collection to bind to for the TabPage '{0}'!", Name));
				}

				try
				{
					if (ParentTabControl != null && ParentTabControl.SelectedTab == this)
					{
						this.SuspendDrawing();
					}

					OnBinding(EventArgs.Empty);
					SetDataBindingCore(dataSource, dataMember);
					OnBound(EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					using (UserIdleWorker.Suspend()) // so that this error, and not subsequent UserIdleWorker errors, are reported to edi
					{
						Globals.Message.ShowError(Res.GetString("016ffb27-b29d-4e90-abe1-a982d896ea18", "An error occurred while opening this tab page. Please close and re-open the form. If you continue to work, you may experience further errors."));
					}
					throw;
				}
				finally
				{
					if (!this.IsDisposed && ParentTabControl != null && ParentTabControl.SelectedTab == this)
					{
						this.ResumeDrawing();
					}
				}
			}
		}

		public void ResetBinding()
		{
			IsBound = false;
		}

		protected override bool IsBindingOrWasFirstShown
		{
			get { return IsBound || base.IsBindingOrWasFirstShown; }
		}

		public event EventHandler Binding;

		void OnBinding(EventArgs e)
		{
			if (Binding != null)
			{
				Binding(this, e);
			}
			NotifyBindingOrShowing();
		}

		public event EventHandler Bound;

		void OnBound(EventArgs e)
		{
			if (Bound != null)
			{
				Bound(this, e);
			}
			NotifyTabInitialized();
		}

		internal virtual bool DelayBinding
		{
			get { return false; }
		}

		protected override void PostSetVisibleCore(bool value)
		{
			//Based class calls NotifyTabInitialized here, but ZBindingTabPage will call it later (on Bound event)
		}

		#endregion

		#region SetDataBindingCore

		protected virtual void SetDataBindingCore(object dataSource, string dataMember)
		{
			if (BindingEnabled)
			{
				IsBound = true;
				BindZUserControls(this, dataSource, dataMember);
			}
		}

		void BindZUserControls(Control control, object dataSource, string dataMember)
		{
			foreach (Control child in control.Controls)
			{
				var userControl = child as ZUserControl;
				if (userControl != null)
				{
					var bindingSource = KBindingSource.GetBindingSource(this);
					if (bindingSource != null)
					{
						if (userControl != null && string.IsNullOrEmpty(bindingSource.GetBindingMember(userControl)))
						{
							userControl.SetDataBinding(dataSource, dataMember);
						}
					}
				}
				else
				{
					BindZUserControls(child, dataSource, dataMember);
				}
			}
		}

		ZUserControl ContainerForTab()
		{
			Control current = this;
			while (current != null)
			{
				if (current is ZUserControl && TypeDescriptor.GetAttributes(current)[typeof(CompositeFieldControlAttribute)] == null)
				{
					return (ZUserControl)current;
				}
				current = current.Parent;
			}
			return null;
		}

		#endregion

		#region DataSource / DataMember

		protected virtual object DataSource
		{
			get
			{
				object result = null;
				var containerForTab = ContainerForTab();
				if (containerForTab != null && containerForTab.DataSourceForBinding != null)
				{
					result = containerForTab.DataSourceForBinding;
				}
				if (result == null && !this.IsDisposed)
				{
					var bindingSource = KBindingSource.GetBindingSource(this);
					result = bindingSource == null ? null : bindingSource.DataSource;
				}
				return result;
			}
		}

		protected virtual string DataMember
		{
			get
			{
				var result = "";
				var containerForTab = ContainerForTab();
				if ((containerForTab == null || containerForTab.DataSourceForBinding == null) && !this.IsDisposed)
				{
					var bindingSource = KBindingSource.GetBindingSource(this);
					result = bindingSource == null ? "" : bindingSource.GetFullBindingMember(this);
				}
				return result;
			}
		}
		#endregion
	}
}
