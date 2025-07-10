using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business.Design;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Design;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

#if !WINZOR

using Enterprise.RemoteDesktopServices.Server.TrackingInfo;

#endif

namespace Enterprise.ZArchitecture.GUI
{
	[DefaultDataSourceBindingMember(".")]
	[SuppressFormDesignerAnalysis]
	[ContainerControlBaseClass]
	public partial class ZUserControl : // Architecture control
		KUserControl,
		IIsOnGrid,
		IIsVisibleForBindingControl,
		IFetchHintGenerator,
		IDesignTimeDataSourceType,
		ICaptionRenderingSupport,
		IHaveTooltipsForMigration,
		IControlVisibilityRelationshipProviderSource,
		IResCaptionedControl,
		IBoundOnPreSaveValidation,
		IHotkeyProvider,
		IEditableInViewMode,
		ISkipSettingControlEnabled,
		ISkipSettingChildControlReadOnly
	{
		public ZUserControl()
		{
			if (DesignModeFinder.IsDesigning)
			{
				DesignTimeEnvironment.InitializeDesignTimeEarlyWithoutServiceProvider();
			}
#if DEBUG
			if (DesignModeFinder.IsDesigning)
			{
				TypeDescriptor.AddAttributes(DesignerActionExtenderProvider, DesignTimeVisibleAttribute.No);
				TypeDescriptor.AddAttributes(VisibilityConfigurationProvider, DesignTimeVisibleAttribute.No);
				TypeDescriptor.AddAttributes(VisibilityRelationshipProvider, DesignTimeVisibleAttribute.No);
				TypeDescriptor.AddAttributes(LabelCaptionRenderProvider, DesignTimeVisibleAttribute.No);
			}
#endif
			var created = DesignTimeDataSourceTypeHelper;
			if (!DesignModeFinder.IsDesigning)
			{
				BindingSource.DataSourceType = typeof(object);
			}
			devInfoPopupManager = new DevInfoPopupManager(this);
		}

		readonly internal DevInfoPopupManager devInfoPopupManager;

		protected readonly ControlVisibilityConfigurationProvider VisibilityConfigurationProvider = new ControlVisibilityConfigurationProvider();
		protected readonly ControlVisibilityRelationshipProvider VisibilityRelationshipProvider = new ControlVisibilityRelationshipProvider();
		protected readonly LabelCaptionRenderProvider LabelCaptionRenderProvider = new LabelCaptionRenderProvider();

		#region Site

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				DesignTimeEnvironment.InitializeDesignTimeWithServiceProvider(value);
#if DEBUG
				DesignerInheritedFormsSizeAndLocationFixer.Fix(this);
#endif
			}
		}

		#endregion

		#region Font

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

		// used by reflection at design time
		bool ShouldSerializeFont()
		{
			return (Font == OFont.GetFont());
		}

		#endregion

		#region ShouldSerializeTabPageMethods

		internal const string ShouldSerializeTabPageMethodsPropertyName = "ShouldSerializeTabPageMethods";
		[Category(ZGUIConstants.DesignerCategory)]
		public bool ShouldSerializeTabPageMethods
		{
			get { return shouldSerializeTabPageMethods ?? false; }
			set { shouldSerializeTabPageMethods = value; }
		}
		bool? shouldSerializeTabPageMethods;

		protected bool ShouldSerializeShouldSerializeTabPageMethods()
		{
			var designerHost = Site == null ? null : (IDesignerHost)Site.GetService(typeof(IDesignerHost));
			return designerHost != null && designerHost.RootComponent == this && shouldSerializeTabPageMethods != null;
		}

		internal const string IsShouldSerializeTabPageMethodsSpecifiedPropertyName = "IsShouldSerializeTabPageMethodsSpecified";
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsShouldSerializeTabPageMethodsSpecified
		{
			get { return shouldSerializeTabPageMethods != null; }
		}

		#endregion

		#region Drag and Drop

		[DefaultValue(true)]
		public override bool AllowDrop
		{
			get { return true; }
		}

		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			base.OnDragEnter(drgevent);
#if !WINZOR
			TrackingInfoLogger.Instance?.LogDragDropEvents("ZUserControl_DragEnter", GetType().Name, Name);
#endif
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragDrop(this, drgevent);
			base.OnDragDrop(drgevent);
			FilterStripParent?.UserControl_DragDrop(this, drgevent);

#if !WINZOR
			TrackingInfoLogger.Instance?.LogDragDropEvents("ZUserControl_DragDrop", GetType().Name, Name);
#endif
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragOver(this, drgevent);
			base.OnDragOver(drgevent);
			FilterStripParent?.UserControl_DragOver(this, drgevent);
		}

		protected override void OnDragLeave(EventArgs drgevent)
		{
			base.OnDragLeave(drgevent);
			FilterStripParent?.UserControl_DragLeave(this, drgevent);

#if !WINZOR
			TrackingInfoLogger.Instance?.LogDragDropEvents("ZUserControl_DragLeave", GetType().Name, Name);
#endif
		}

		ZFilterStrip FilterStripParent
		{
			get
			{
				if (filterStripParent == null)
				{
					var parent = base.Parent;
					while (parent != null)
					{
						if (parent is ZFilterStrip filter)
						{
							filterStripParent = filter;
							break;
						}
						parent = parent.Parent;
					}
				}
				return filterStripParent;
			}
		}
		ZFilterStrip filterStripParent;

		#endregion

		#region Z Binding Support

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual IBusiness DataSourceForBinding
		{
			get { return null; }
		}

		protected virtual bool CheckForBindingErrors()
		{
			return true;
		}

		#endregion

		#region Tab Skipping ReadOnly Fields

		public virtual HotkeyRegister Hotkeys { get; } = new HotkeyRegister();
		public virtual string TypeNameForDisplay => Res.GetString("897e5f09-ce24-4821-834a-cdfd46fd8ae5", "Control");

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			try
			{
				return (ProcessHotkeys && Hotkeys.ProcessCmdKey(this, keyData)) || base.ProcessCmdKey(ref msg, keyData);
			}
			catch
			{
				throw;
			}
		}

		protected virtual bool ProcessHotkeys { get; } = true;

		protected override bool ProcessTabKeyCore(bool forward)
		{
			bool result;
			var active = this.GetFrontMostActiveControl();

			if (active == null || active.GetReadOnly())
			{
				result = base.ProcessTabKeyCore(forward);
			}
			else
			{
				result = SelectNextControl(active, forward);
			}

			return result;
		}

		public TabbedNavigationEventHandler OnTabSelectNextControl;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1091:Do Not Set CausesValidation to false", Justification = "temporary disable of validation")]
		internal bool SelectNextControl(Control active, bool forward)
		{
			var handler = OnTabSelectNextControl;

			if (handler != null)
			{
				var args = new TabbedNavigationEventArgs { Forward = forward };

				handler(this, args);

				if (args.Handled)
				{
					return true;
				}
			}

			//Hello! Before changing this code, verify that you do not break WI00292154.
			//(That is, when tabbing out of a field and committing a value that causes the next field to stop being read-only, we must tab into the newly non-read-only field.)
			//Thanks!
			CommitReadOnlyBindings(active);

			if (active == ActiveControl)
			{
				var oldCausesValidationValue = active.CausesValidation;
				try
				{
					active.CausesValidation = false; // temporary disable of validation
					return this.SelectNextControlNonTabStopNonReadOnly(ActiveControl, forward, true, false);
				}
				finally
				{
					active.CausesValidation = oldCausesValidationValue;
				}
			}
			else
			{
				return this.SelectNextControlNonTabStopNonReadOnly(ActiveControl, forward, true, false);
			}
		}

		protected void CommitReadOnlyBindings(Control control)
		{
			control.PerformControlValidation();
		}

		#region TabbedNavigationEventHandler

		public class TabbedNavigationEventArgs : EventArgs
		{
			public bool Handled { get; set; }
			public bool Forward { get; set; }
		}

		public delegate void TabbedNavigationEventHandler(object sender, TabbedNavigationEventArgs e);

		#endregion

		#endregion

		#region Select

		protected override void SelectCore(bool directed, bool forward)
		{
			ControlExtensions.SelectNonReadOnly(this, directed, forward);
		}

		#endregion

		#region OnBindingContextChanged

		bool inOnBindingContextChanged;

		protected override void OnBindingContextChanged(EventArgs e)
		{
			if (!inOnBindingContextChanged)
			{
				try
				{
					inOnBindingContextChanged = true;
					base.OnBindingContextChanged(e);
				}
				catch (ArgumentException ex)
				{
					if (IsHandleCreated && !this.IsDisposedOrHasDisposedParent() && DataSource != null)
					{
						var rethrow = false;
						try
						{
							//try one more time
							try
							{
								base.OnBindingContextChanged(e);
							}
							catch (Exception ex1) when (!ex1.IsCriticalException())
							{
								ReportBindingException(ex);
							}
						}
						catch (Exception ex1) when (!ex1.IsCriticalException())
						{
							rethrow = true;
						}
						if (rethrow)
						{
							throw;
						}
					}
					else
					{
						// Control is disposed or unbound - ignore this exception
					}
				}
				finally
				{
					inOnBindingContextChanged = false;
				}
			}

			if (!hasFirstBound && ShouldRegisterToBeBoundOnPreSaveValidation)
			{
				RegisterControlToBeBoundOnPreSaveValidation();
			}
		}

		void RegisterControlToBeBoundOnPreSaveValidation()
		{
			var parentForm = (ZForm)FindForm();
			if (parentForm != null)
			{
				parentForm.RegisterControlToBeBoundOnPreSaveValidation(this);
			}
		}

		protected virtual bool ShouldRegisterToBeBoundOnPreSaveValidation
		{
			get { return false; }
		}

		#endregion

		#region IDataBoundControl

		bool hasFirstBound;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null && !hasFirstBound)
			{
				OnBackColorChanged(EventArgs.Empty); // Force theme and custom back color initialization on all childrens even if usercontrol's back color was not changed
				OnAfterFirstBinding(EventArgs.Empty);
				hasFirstBound = true;
			}
		}

		protected virtual void OnAfterFirstBinding(EventArgs e)
		{
			FireAfterFirstBinding();
		}

		void FireAfterFirstBinding()
		{
			if (AfterFirstBinding != null)
			{
				AfterFirstBinding(this, EventArgs.Empty);
			}
		}

		public event EventHandler AfterFirstBinding;

		#endregion

		#region IIsOnGrid

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool IsOnGrid
		{
			get { return fIsOnGrid; }
			set
			{
				fIsOnGrid = value;

				foreach (Control childControl in Controls)
				{
					var childControlOnGrid = childControl as IIsOnGrid;

					if (childControlOnGrid != null)
					{
						childControlOnGrid.IsOnGrid = value;
					}
				}

				OnGridChanged();
			}
		}
		bool fIsOnGrid;

		protected virtual void OnGridChanged()
		{
		}

		#endregion

		#region IIsVisibleForBindingControl

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsVisibleForBinding
		{
			get { return Visible; }
			set
			{
				if (!fInVisibleForBinding)
				{
					fInVisibleForBinding = true;
					try
					{
						if (Visible != value)
						{
							Visible = value;
							OnIsVisibleForBindingChanged();
						}
					}
					finally
					{
						fInVisibleForBinding = false;
					}
				}
			}
		}

		public event EventHandler IsVisibleForBindingChanged;

		protected virtual void OnIsVisibleForBindingChanged()
		{
			if (IsVisibleForBindingChanged != null)
			{
				IsVisibleForBindingChanged(this, EventArgs.Empty);
			}
		}

		bool fInVisibleForBinding;

		#endregion

		#region IFetchHintGenerator Members

		void IFetchHintGenerator.AddFetchHint(object dataSource, string dataMember)
		{
			AddFetchHints(dataSource, dataMember);
		}

		protected virtual void AddFetchHints(object dataSource, string dataMember)
		{
			BindingSource.AddFetchHints(dataSource, dataMember);
		}

		#endregion

		#region IDesignTimeDataSourceType

		TopLevelDataSourceTypeHelper DesignTimeDataSourceTypeHelper
		{
			get { return designTimeDataSourceTypeHelper ?? (designTimeDataSourceTypeHelper = new TopLevelDataSourceTypeHelper(this)); }
		}
		TopLevelDataSourceTypeHelper designTimeDataSourceTypeHelper;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string DataSourceAssemblyName
		{
			get { return DesignTimeDataSourceTypeHelper.DataSourceAssemblyName; }
			set { DesignTimeDataSourceTypeHelper.DataSourceAssemblyName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string DataSourceTypeName
		{
			get { return DesignTimeDataSourceTypeHelper.DataSourceTypeName; }
			set { DesignTimeDataSourceTypeHelper.DataSourceTypeName = value; }
		}

		public override Type DataSourceType
		{
			get { return DesignTimeDataSourceTypeHelper.DataSourceType; }
		}

		Type ITopLevelDataSourceType.DataSourceType
		{
			get { return DesignModeFinder.IsDesigning ? DataSourceType : RuntimeDataSourceType; }
		}

		Type RuntimeDataSourceType
		{
			get { return CurrentDataItem != null ? CurrentDataItem.GetType() : DesignTimeDataSourceTypeHelper.DataSourceType; }
		}

		#endregion

		#region ICaptionRenderingSupport Members

		[Category(ZGUIConstants.DesignerCategory)]
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool? CaptionRenderingEnabled
		{
			get { return captionRenderingEnabled; }
			set
			{
				if (captionRenderingEnabled != value)
				{
					captionRenderingEnabled = value;
					OnCaptionRenderingEnabledChanged(EventArgs.Empty);
				}
			}
		}
		bool? captionRenderingEnabled;

		protected bool ShouldSerializeCaptionRenderingEnabled()
		{
			return ((ICaptionRenderingSupport)this).ShouldSerializeCaptionRenderingEnabled();
		}

		public event EventHandler CaptionRenderingEnabledChanged;

		void OnCaptionRenderingEnabledChanged(EventArgs e)
		{
			if (CaptionRenderingEnabledChanged != null)
			{
				CaptionRenderingEnabledChanged(this, e);
			}
		}

		#endregion

		#region IHaveTooltipsForMigration Members

		[Browsable(false)]
		public ToolTipCollectorForMigration ContainerControlToolTip
		{
			get { return ((IHaveTooltipsForMigration)this).ToolTipForMigration; }
		}

		ToolTipCollectorForMigration IHaveTooltipsForMigration.ToolTipForMigration
		{
			get { return toolTipCollectorForMigration ?? (toolTipCollectorForMigration = new ToolTipCollectorForMigration()); }
		}
		ToolTipCollectorForMigration toolTipCollectorForMigration;

		#endregion

		#region Implementation

		protected override KBindingSource NewBindingSource()
		{
			return new ZBindingSource();
		}

		public new ZBindingSource BindingSource
		{
			get { return (ZBindingSource)base.BindingSource; }
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		protected override Binding CreateBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled)
		{
			return new KBinding(propertyName, dataSource, dataMember, formattingEnabled);
		}

		#endregion

		#region IControlVisibilityRelationshipProviderSource Members

		ControlVisibilityRelationshipProvider IControlVisibilityRelationshipProviderSource.VisibilityRelationshipProvider
		{
			get { return VisibilityRelationshipProvider; }
		}

		#endregion

		#region ReportGraphicsDisplayFailure

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		public static void ReportGraphicsDisplayFailure(Exception e)
		{
			if (HasReportedGraphicsDisplayFailure)
			{
				return;
			}
			HasReportedGraphicsDisplayFailure = true;
			try
			{
				Globals.Message.ShowWarning(Res.GetString("2b052cb4-93b2-41c7-9676-060b89d57e28",
					"Exception occurred while drawing a form: {0}.\r\nThis indicates low memory or exhaustion of another Windows resource.\r\nIf problems persist, save your work and restart. You may also need to close/restart other programs, restart the computer and/or free up hard disk space."
					, e.Message));
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } }
		}

		#endregion

		#region ReportBindingException

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debug information")]
		void ReportBindingException(Exception ex)
		{
			var dataSourceName = "<null>";
			var bizo = DataSource as BusinessObject;
			if (bizo != null)
			{
				dataSourceName = string.Format("'{0}' ({1})", bizo.HumanReadableName, bizo.GetType().FullName);
			}
			else if (DataSource != null)
			{
				dataSourceName = DataSource.GetType().FullName;
			}

			ErrorReporter.ReportOnce(
				"ZUserControl_BindingException",
				string.Format("Exception during (re)binding control '{0}' ({1}) to data source {2}, data member '{3}'.", Name, GetType().FullName, dataSourceName, DataMember),
				ex);

			if (bindingLinkLabel == null)
			{
				var label = new ZLinkLabel();
				label.CaptionResourceString = Res.GetData("76435267-19de-4f78-9718-9111284c5fd6", "Binding error...");
				label.LinkColor = label.ActiveLinkColor = label.VisitedLinkColor = Color.Red;
				label.Location = ControlDpiScalingHelper.NewScaledPoint(1, 1);
				label.LinkClicked += BindingErrorLabelClicked;
				Controls.Add(label);
				label.BringToFront();

				bindingLinkLabel = label;
			}
		}

		void BindingErrorLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Globals.Message.ShowWarning(
				Res.GetString("78a98ae5-1e49-4e57-92fa-eaad34f5936f", "There was internal error binding this control(s) to data source, and some edit controls can work incorrectly. In such case try to save your changes and reopen this form."),
				Res.GetString("bb9d6d40-f879-4756-970a-3e422726be06", "Binding error."));

			var linkLabel = sender as ZLinkLabel ?? bindingLinkLabel;
			if (linkLabel != null && !linkLabel.IsDisposed)
			{
				linkLabel.LinkClicked -= BindingErrorLabelClicked;
				linkLabel.Dispose();
			}
			bindingLinkLabel = null;
		}

		ZLinkLabel bindingLinkLabel;

		#endregion

		#region Validation

		public void ValidateAndShowErrorsIfAny()
		{
			var dataSource = DataSource as BusinessObject;

			if (dataSource != null)
			{
				var form = FindForm() as ZForm;

				if (form != null)
				{
					form.PerformValidation();

					if (dataSource.HasErrors)
					{
						form.ShowErrorsDialog(dataSource);
					}
				}
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (devInfoPopupManager != null)
					{
						devInfoPopupManager.Dispose();
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion

		[Browsable(true)]
		public virtual ResourceStringData CaptionResourceString
		{
			get { return captionResourceString ?? ResourceStringData.Empty; }
			set
			{
#if DEBUG
				if (DesignMode && value == null)
				{
					return;
				}
#endif
				captionResourceString = value;
				this.RefreshCaptionLabel();
			}
		}
		ResourceStringData captionResourceString;

		bool ShouldSerializeCaptionResourceString()
		{
			return !CaptionResourceString.IsEmpty();
		}

		[DefaultValue(false)]
		public bool EditableInViewMode { get; set; }

		[DefaultValue(false)]
		public bool SkipSettingControlEnabled { get; set; }
		[DefaultValue(false)]
		public bool SkipSettingChildControlReadOnly { get; set; }
	}
}
