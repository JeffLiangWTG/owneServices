using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Integration.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
#if DEBUG
	[DesignerSerializer(typeof(ControlCodeDomSerializerWithDelayedTabCreate), typeof(CodeDomSerializer))]
#endif
	public class ZTabPage : KTabPage, IListenForNotifications, IExtendedControl, IResCaptionedControl, ILicensedComponent
	{
		public ZTabPage()
		{
			CheckForNotifications = true;
			ShouldBeReadOnlyInViewMode = true;
			CheckForChildrenControlsVisibilityChange = true;

			ControlsInError = new ArrayList();
			ControlsInWarning = new ArrayList();
			ControlsInMessageError = new ArrayList();

			DisposableLeakListener.Instance.RegisterDisposable(this);
			if (!DesignModeFinder.IsDesigning)
			{
				BackColor = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.TabBackgroundColor;
				Font = OFont.GetFont();
				UserEventTracker.Instance.AddUserEventToControl(this);
			}
			Extensions = NewExtensionCollection();
		}

		#region Properties

		#region Visibility

		/// <summary>
		/// Changes the visibility of the tab page. If set to false, the tab page will be completely hidden. 
		/// If set to true, the tab page will be re-added in the correct position.
		/// 
		/// Corrects the behaviour of the existing .NET "Visible" method.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(true)]
		public bool TabVisible
		{
			get { return VisibilityManager != null && VisibilityManager.GetTabPageVisible(this); }
			set
			{
				if (!DesignModeFinder.IsDesigning && TabVisible != value && VisibilityManager != null)
				{
					VisibilityManager.SetTabPageVisible(value, this);
				}
			}
		}

		TabPageVisibilityManager VisibilityManager
		{
			get
			{
				if (fVisibilityManager == null && Parent != null && ParentTabControl != null)
				{
					fVisibilityManager = ParentTabControl.TabPageVisibilityManager;
				}
				return fVisibilityManager;
			}
		}

		TabPageVisibilityManager fVisibilityManager;

		internal bool HasBeenMadeVisible { get; set; }

		[Obsolete("TestTabPage.Visible has no effect. Use ZTabPage.TabVisible instead.", true)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool Visible
		{
			get { return base.Visible; }
			set { base.Visible = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(true)]
		public bool TabRelevant
		{
			get { return fTabRelevant; }
			set
			{
				fTabRelevant = value;
				TabVisible = value && CanBeVisibleWhenSettingRelevant();
			}
		}
		bool fTabRelevant = true;

		bool CanBeVisibleWhenSettingRelevant()
		{
			var tabControl = ParentTabControl;
			return tabControl != null
				&& (tabControl.CanTabPageBeVisibleOnSetRelevantDelegate == null || tabControl.CanTabPageBeVisibleOnSetRelevantDelegate(this));
		}

		#endregion

		#region Auto Size

		protected internal virtual bool IsAutoSized
		{
			get { return false; }
		}

		public int MinimumAutoSizedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(835);
		public int MinimumAutoSizedHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(400);

		#endregion

		#region Font

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override System.Drawing.Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

		#endregion

		#region Notifications

		[DefaultValue(false)]
		public bool HasNotifications
		{
			get { return HasErrors || HasWarnings || HasMessageErrors; }
		}

		[DefaultValue(false)]
		public bool HasErrors
		{
			get { return HasVisibleControl(ControlsInError); }
		}

		[DefaultValue(false)]
		public bool HasWarnings
		{
			get { return HasVisibleControl(ControlsInWarning); }
		}

		[DefaultValue(false)]
		public bool HasMessageErrors
		{
			get { return HasVisibleControl(ControlsInMessageError); }
		}

		#endregion

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(false)]
		[Description("Use this attribute for performance reasons - ie if your tab should not be bound on saving and should only ever be bound when clicked on.")]
		public virtual bool ExcludeFromBindingOnSave { get; set; }

		[DefaultValue(true)]
		public bool CheckForNotifications { get; set; }

		[DefaultValue(true)]
		public bool ShouldBeReadOnlyInViewMode { get; set; }

		[DefaultValue(true)]
		public bool CheckForChildrenControlsVisibilityChange { get; set; }

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override string Text
		{
			get { return base.Text; }
			set
			{
				var parentForm = FindForm();
				if (value != null && !DesignMode && (parentForm == null || !parentForm.IsDesignMode()))
				{
					var ampersandIndex = value.IndexOf(@"&", StringComparison.Ordinal);

					while (ampersandIndex >= 0 && ampersandIndex < value.Length - 1)
					{
						if (value[ampersandIndex + 1] != '&')
						{
							value = value.Remove(ampersandIndex, 1);
							ampersandIndex = value.IndexOf(@"&", ampersandIndex, StringComparison.Ordinal);
						}
						else
						{
							ampersandIndex = value.IndexOf(@"&", Math.Min(ampersandIndex + 2, value.Length - 1), StringComparison.Ordinal);
						}
					}
				}
				base.Text = value;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool AllowShowImage
		{
			get { return allowShowImage; }
			set
			{
				if (allowShowImage != value)
				{
					allowShowImage = value;
					UpdateNotificationIcon();
				}
			}
		}
		bool allowShowImage = true;

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				DesignTimeTextChecker.Subscribe(value, this);
			}
		}

		#endregion

		#region Add

		internal void DoAdded()
		{
			OnAdded();
			if (Added != null)
			{
				Added(this, EventArgs.Empty);
			}
		}

		protected virtual internal void OnAdded()
		{
			if (DelayedImageIndex != null)
			{
				ImageIndex = DelayedImageIndex.Value;
			}
		}

		public event EventHandler Added;

		protected virtual void SetDelayedImageIndex(int? index)
		{
			DelayedImageIndex = index;
			if (index > -1 && !TabVisible)
			{
				TabVisible = true;
			}
		}

		int? DelayedImageIndex;

		#endregion

		#region Tab Image

		internal void UpdateInitialTabImage()
		{
			if (!updateInitialTabImageRun && !this.IsDisposedOrHasDisposedParent())
			{
				UpdateInitialTabImageCore();
				updateInitialTabImageRun = true;
			}
		}
		bool updateInitialTabImageRun;

		protected virtual void UpdateInitialTabImageCore()
		{
		}

		#endregion

		#region Notifications

		void IListenForNotifications.NotifyAboutVisibilityChangeOfChildControl(Control control)
		{
			if (CheckForChildrenControlsVisibilityChange)
			{
				UpdateNotificationIcon_MaybeInvoke();
			}
		}

		void IListenForNotifications.NotifyAboutStateOfChildControl(Control control, INotificationType state)
		{
			if (control.Parent is IListenForNotifications && control.Parent != this)
			{
				((IListenForNotifications)this).NotifyAboutStateOfChildControl(control.Parent, state);
			}
			else
			{
				if (CheckForNotifications)
				{
					if (state == CargoWise.ComponentModel.NotificationType.Error)
					{
						SetControlInStateList(control, false, ControlsInMessageError);
						SetControlInStateList(control, true, ControlsInError);
						SetControlInStateList(control, false, ControlsInWarning);
					}
					else if (state == CargoWise.ComponentModel.NotificationType.Warning)
					{
						SetControlInStateList(control, false, ControlsInMessageError);
						SetControlInStateList(control, false, ControlsInError);
						SetControlInStateList(control, true, ControlsInWarning);
					}
					else if (state == CargoWise.EntityFramework.NotificationType.MessageError)
					{
						SetControlInStateList(control, true, ControlsInMessageError);
						SetControlInStateList(control, false, ControlsInError);
						SetControlInStateList(control, false, ControlsInWarning);
					}
					else
					{
						SetControlInStateList(control, false, ControlsInMessageError);
						SetControlInStateList(control, false, ControlsInError);
						SetControlInStateList(control, false, ControlsInWarning);
					}
					UpdateNotificationIcon_MaybeInvoke();
				}
			}
		}

		#endregion

		#region Related Controls

		internal ZTabControl ParentTabControl
		{
			get
			{
				return Parent == null && VisibilityManager != null
					? VisibilityManager.TabControl as ZTabControl
					: Parent as ZTabControl;
			}
		}

		protected override ControlCollection CreateControlsInstance()
		{
			return new AutoDisposeControlCollection(this);
		}

		#endregion

		#region Implementation

		readonly ArrayList ControlsInError;
		readonly ArrayList ControlsInWarning;
		readonly ArrayList ControlsInMessageError;

		bool updateNotificationIconPending;

		static bool HasVisibleControl(ArrayList controls)
		{
			var result = false;
			foreach (Control control in controls.ToArray())
			{
				if (ControlVisibleCalculator.IsSetVisible(control))
				{
					result = true;
				}
				else
				{
					controls.Remove(control);
				}
			}
			return result;
		}

		#region Notifications

		#region UpdateNotificationIcon

		public virtual void ClearNotificationImage()
		{
			if (ImageIndex != -1)
			{
				ImageIndex = -1;
			}
		}

		protected void UpdateNotificationIcon_MaybeInvoke()
		{
			if (this.IsHandleCreated)
			{
				this.BeginInvoke(UpdateNotificationIcon);
			}
			else
			{
				UpdateNotificationIcon();
			}
		}

		protected void UpdateNotificationIcon()
		{
			if (ParentTabControl != null && ParentTabControl.TabPages.Contains(this))
			{
				if (AllowShowImage)
				{
					var index = CalculateImageIndex();
					if (index >= 0)
					{
						if (ImageIndex != index)
						{
							ImageIndex = index;
						}
					}
					else
					{
						ClearNotificationImage();
					}
				}
				else
				{
					ClearNotificationImage();
				}
			}
			else
			{
				SetDelayedImageIndex(CalculateImageIndex());
			}
		}

		void UpdateNotificationIconOnIdle()
		{
			if (!updateNotificationIconPending)
			{
				QueueUserIdleWorkItem(this, 0, new MethodInvoker(delegate
				{
					updateNotificationIconPending = false;
					UpdateNotificationIcon();
				}));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static void QueueUserIdleWorkItem(Control workItemOwner, int millisecondsToRun, Delegate method, params object[] args)
		{
			UserIdleWorker.QueueWorkItem(workItemOwner, millisecondsToRun, method, args);
		}

		void ControlInState_Disposed(object sender, EventArgs e)
		{
			if (!Disposing && !IsDisposed)
			{
				var controlInState = (Control)sender;
				controlInState.Disposed -= ControlInState_Disposed;
				ControlsInError.Remove(controlInState);
				ControlsInWarning.Remove(controlInState);
				ControlsInMessageError.Remove(controlInState);

				UpdateNotificationIconOnIdle();
			}
		}

		#endregion

		#endregion

		protected void SetControlInStateList(Control controlInState, bool inError, ArrayList controlsInState)
		{
			if (inError && !controlsInState.Contains(controlInState))
			{
				controlsInState.Add(controlInState);
				controlInState.Disposed += ControlInState_Disposed;
			}
			else if (!inError && controlsInState.Contains(controlInState))
			{
				controlsInState.Remove(controlInState);
				controlInState.Disposed -= ControlInState_Disposed;
			}
		}

		int CalculateImageIndex()
		{
			var result = -1;

			if (HasErrors)
			{
				result = Icons.GetImageIndex(IconTypes.Error);
			}
			else if (HasMessageErrors)
			{
				result = Icons.GetImageIndex(IconTypes.MessageError);
			}
			else if (HasWarnings)
			{
				result = Icons.GetImageIndex(IconTypes.Warning);
			}

			return result;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				DesignTimeTextChecker.UnSubscribe(Site, this);
				DisposableLeakListener.Instance.UnRegisterDisposable(this);

				if (licensedComponentManager != null)
				{
					licensedComponentManager.Dispose();
				}
				ControlsInError.Clear();
				ControlsInWarning.Clear();
				ControlsInMessageError.Clear();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Drag and Drop

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragDrop(this, drgevent);
			base.OnDragDrop(drgevent);
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragOver(this, drgevent);
			base.OnDragOver(drgevent);
		}

		[DefaultValue(true)]
		public override bool AllowDrop
		{
			get { return true; }
		}

		#endregion

		#region TabInitialized

		bool tabInitializedFired;

		internal void NotifyTabInitialized()
		{
			if (!tabInitializedFired)
			{
				OnTabInitialized(EventArgs.Empty);
			}
		}

		public event EventHandler TabInitialized;

		protected virtual void OnTabInitialized(EventArgs e)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("ZTabPage.OnTabInitialized", Text))
			{
				tabInitializedFired = true;
				if (TabInitialized != null)
				{
					TabInitialized(this, e);
				}
			}
		}

		public void RunWhenTabInitialized(EventHandler method)
		{
			if (!tabInitializedFired && !DesignModeFinder.IsDesigning)
			{
				TabInitialized -= method;
				TabInitialized += method;
			}
			else
			{
				method(this, EventArgs.Empty);
			}
		}

		#endregion

		#region BindingOrFirstShown

		bool bindingOrFirstShownFired;
		internal const string BindingOrFirstShownPropertyName = "BindingOrFirstShown";

		public void NotifyBindingOrShowing()
		{
			if (!bindingOrFirstShownFired)
			{
				OnBindingOrFirstShown(EventArgs.Empty);
			}
		}

		public event EventHandler BindingOrFirstShown;

		protected virtual void OnBindingOrFirstShown(EventArgs e)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("ZTabPage.OnBindingOrFirstShown", Text))
			{
				bindingOrFirstShownFired = true;
				if (BindingOrFirstShown != null)
				{
					BindingOrFirstShown(this, e);
				}
			}
		}

		protected override void SetVisibleCore(bool value)
		{
			if (value)
			{
				if (LicenceCheckpoint != null)
				{
					var loginResponse = LicenceCheckpoint.Login(this);
					if (loginResponse == LicenceLoginResponse.Denied)
					{
						ReplaceAllWithCoveringLabel(LicenceCheckpoint.LastReasonForNotAllowing);
					}
				}

				NotifyBindingOrShowing();
			}

			var form = FindForm();
			using (PerformanceStatisticsCollector.StartMonitoring("SetTabVisible", form != null ? form.GetType().FullName : null))
			{
				base.SetVisibleCore(value);
				PostSetVisibleCore(value);
			}
		}

		protected virtual void PostSetVisibleCore(bool value)
		{
			if (value)
			{
				NotifyTabInitialized();
			}
		}

		public const string RunWhenBindingOrFirstShownMethod = "RunWhenBindingOrFirstShown";
		public void RunWhenBindingOrFirstShown(EventHandler method)
		{
			if (!bindingOrFirstShownFired && !DesignModeFinder.IsDesigning)
			{
				BindingOrFirstShown -= method;
				BindingOrFirstShown += method;
			}
			else
			{
				method(this, EventArgs.Empty);
			}
		}

		protected virtual bool IsBindingOrWasFirstShown
		{
			get { return Created && base.Visible; }
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZTabPage>().Result;
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		IControlExtensionCollection NewExtensionCollection()
		{
			return new ControlExtensionCollection(this) { new ZLabelCaptionRenderer() };
		}

		#endregion

		#region Security

		public void SetupSecurity(SecurityCheckpoint checkpoint, ILicenceCheckpoint licenceCheckpoint = null, string customMessage = null)
		{
			SetupSecurity(new SecurityCheckpoint[] { checkpoint }, licenceCheckpoint, customMessage);
		}

		public void SetupSecurity(SecurityCheckpoint[] checkpoints, ILicenceCheckpoint licenceCheckpoint = null, string customMessage = null)
		{
			var disallowedCheckpoints = checkpoints.Where(checkpoint => !checkpoint.IsAllowed).ToArray();

			if (disallowedCheckpoints.Length > 0)
			{
				if (string.IsNullOrEmpty(customMessage))
				{
					var messageBuilder = new ZStringBuilder();
					messageBuilder.AppendLine(disallowedCheckpoints[0].ErrorMessageForNotAllowed);
					for (var i = 1; i < disallowedCheckpoints.Length; i++)
					{
						messageBuilder.AppendLine();
						messageBuilder.AppendLine(disallowedCheckpoints[i].DisplayTextPathToSecurityRight);
					}
					ReplaceAllWithCoveringLabel(messageBuilder.ToString());
				}
				else
				{
					ReplaceAllWithCoveringLabel(customMessage);
				}
			}
			else
			{
				if (licenceCheckpoint != null)
				{
					LicenceCheckpoint = licenceCheckpoint;
				}
			}
		}

		internal void ReplaceAllWithCoveringLabel(string message)
		{
			if (coveringLabel == null)
			{
				coveringLabel = new ZLabel()
				{
					Dock = DockStyle.Fill,
					IsFontBold = true,
					Name = "coveringLabel",
					TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
					Text = message
				};
				Controls.Add(coveringLabel);
				coveringLabel.BringToFront();
			}
		}

		internal ZLabel coveringLabel;

		#endregion

		#region Licence

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(null)]
		public ILicenceCheckpoint LicenceCheckpoint
		{
			get;
			set;
		}

		LicensedComponentManager licensedComponentManager;
		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get { return licensedComponentManager ?? (licensedComponentManager = new LicensedComponentManager(this)); }
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

		public static ZString GetEscapedTabPageText(ZString tabPageText)
		{
			if (tabPageText.IndexOf('&') >= 0)
			{
				var newString = new StringBuilder();

				for (int i = 0; i < tabPageText.Length; ++i)
				{
					if (tabPageText[i] == '&')
					{
						if (i < tabPageText.Length - 1 && tabPageText[i + 1] == '&')
						{
							++i;
						}
						newString.Append("&&");
					}
					else
					{
						newString.Append(tabPageText[i]);
					}
				}

				tabPageText = newString.ToString();
			}
			return tabPageText;
		}
	}
}
