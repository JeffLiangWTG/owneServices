using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Controls.Internal;
using CargoWise.Windows.UI.Testing;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Represents a window or dialog box that makes up an application's user interface.
	/// <summary><see cref="Form"/></summary>
	/// </summary>
	[SuppressFormDesignerAnalysis]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class KForm : Form, IDataBoundControl, ICompositeControlBindingSourceProvider
	{
		public KForm()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			BindingSource = NewBindingSource();
			BindingSource.ContainerControl = this;
			GCCollectorTracker = GCTracker.Track(this.GetType().ToString(), this);
			OnFormCreated();
#if !WINZOR
			if (!Environment.UserInteractive)
			{
				ErrorReporter.ReportOnce("NoUI:" + GetType().FullName, "Construction of a form without a winforms environment");
			}
#endif
#if DEBUG
#if !WINZOR
#pragma warning disable CW1161 //DoNotUseConstantStringLiterals
			Hotkeys.AddDescription(ShowControlInformationKey, "(Hold) Highlight bounds of control under mouse");
#pragma warning disable CW1161 //DoNotUseConstantStringLiterals
#else
			Overlay = new ControlInformationOverlayComponent() { HostForm = this, Dock = DockStyle.Fill };
			WinzorSpecificControls.Add(Overlay);
#endif
#endif
		}

		readonly GCTracker GCCollectorTracker;

		#region DPI scaling overrides

		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		#endregion // DPI scaling overrides

		#region DataSource / DataMember

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object DataSource
		{
			get { return BindingSource != null ? BindingSource.DataSource : null; }
			set { BindingSource.DataSource = value; }
		}

		public event EventHandler DataSourceChanged
		{
			add
			{
				if (dataSourceChanged == null)
				{
					BindingSource.DataSourceChanged += new EventHandler(OnDataSourceChanged);
				}
				dataSourceChanged += value;
			}
			remove
			{
				dataSourceChanged -= value;
				if (dataSourceChanged == null)
				{
					BindingSource.DataSourceChanged -= new EventHandler(OnDataSourceChanged);
				}
			}
		}
		event EventHandler dataSourceChanged;

		void OnDataSourceChanged(object sender, EventArgs e)
		{
			if (dataSourceChanged != null)
			{
				dataSourceChanged(this, e);
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string DataMember
		{
			get { return BindingSource.DataMember; }
			set { BindingSource.DataMember = value; }
		}

		#endregion

		#region CurrentDataItem

		[SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object CurrentDataItem
		{
			get
			{
				object result = DataSource;
				if (!string.IsNullOrEmpty(DataMember))
				{
					result = null;
					BindingManagerBase bm = BindingContext[DataSource, DataMember];
					CurrencyManager cm = bm as CurrencyManager;
					if (cm != null && DataSourceType != null && typeof(IList).IsAssignableFrom(DataSourceType))
					{
						result = cm.List;
					}
					else if (bm.Position != -1)
					{
						result = bm.Current;
					}
				}
				return result;
			}
		}

		#endregion

		#region IExtenderProviders

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		protected readonly KBindingSource BindingSource;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		protected readonly KNotificationProvider NotificationProvider = new KNotificationProvider();

		/// <summary>
		/// Create a new KBindingSource for the BindingSource field.
		/// </summary>
		protected virtual KBindingSource NewBindingSource()
		{
			return new KBindingSource();
		}

		#endregion

		#region WndProc

#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			if (!this.IsDesignMode())
			{
				switch (m.Msg)
				{
					case 70: //WM_WINDOWPOSCHANGING
					case 71: //WM_WINDOWPOSCHANGED
						unchecked
						{
							if (this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.Fixed3D || this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.FixedDialog
								|| this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.FixedSingle || this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.FixedToolWindow)
							{
#pragma warning disable WFDEV001 // 'Message.LParam' is obsolete: 'Casting to/from IntPtr is unsafe, use LParamInternal.'
								WindowPos mwp = (WindowPos)Marshal.PtrToStructure(m.LParam, typeof(WindowPos));
#pragma warning restore WFDEV001
								int noSize = mwp.flags & 1; // SWP_NOSIZE

								if (noSize == 0 && // Size is not ignored
									mwp.cy > this.Height) //form has expanded in all directions - bug in RDP, perhaps
								{
									//this.Text = string.Format("({0}, {1}, {2}, {3}) ({4}, {5}, {6}, {7}), {8}", this.Left, this.Top, this.Width, this.Height, mwp.x, mwp.y, mwp.cx, mwp.cy, m.Msg);
									int diff3 = mwp.cy - this.Height;
									int diff4 = mwp.cx - this.Width;
									if (diff3 == diff4 && diff3 < 1000)
									{
										mwp.y += diff3 / 2;
										mwp.x += diff3 / 2;
										mwp.cy -= diff3;
										mwp.cx -= diff3;
#pragma warning disable WFDEV001 // 'Message.LParam' is obsolete: 'Casting to/from IntPtr is unsafe, use LParamInternal.'
										Marshal.StructureToPtr(mwp, m.LParam, true);
#pragma warning restore WFDEV001
									}
								}
							}
						}
						break;
					case 6: // WM_ACTIVATE
					case 16: // WM_CLOSE
						if (ActiveControlIsDisposed())
						{
							ActiveControl = null;
						}
						break;
				}
			}

			base.WndProc(ref m);

			bool ActiveControlIsDisposed()
			{
				var activeControl = ActiveControl;
				while (activeControl != null)
				{
					if (activeControl.IsDisposed)
					{
						return true;
					}

					activeControl = (activeControl as ContainerControl)?.ActiveControl;
				}

				return false;
			}
		}

		public struct WindowPos
		{
			public IntPtr hwnd;
			public IntPtr hwndInsertAfter;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public int flags; //was uint but that's not CLS-compliant
		}

#endif
#if DEBUG && !WINZOR

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ControlInformationOverlayForm Overlay => LazyInitializer.EnsureInitialized(ref overlayForm);

		const Keys ShowControlInformationKey = Keys.Control | Keys.Shift | Keys.F;

		ControlInformationOverlayForm overlayForm;

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyData == ShowControlInformationKey)
			{
				var ctrl = FindControlAtCursor(this);
				if (ctrl != null && (!Overlay.Visible || ctrl != Overlay.HighlightedControl))
				{
					Overlay.CenterOnControl(ctrl, this);
				}
			}

			base.OnKeyDown(e);
		}

		static Control FindControlAtCursor(Form form)
		{
			return FindControlAtPoint(form, form.PointToClient(Cursor.Position));

			Control FindControlAtPoint(Control container, Point p)
			{
				foreach (Control c in container.Controls)
				{
					if (c.Visible && c.Bounds.Contains(p))
					{
						var pointRelativeToChild = ControlDpiScalingHelper.NewScaledPoint(p.X - c.Left, p.Y - c.Top, false);
						return FindControlAtPoint(c, pointRelativeToChild) ?? c;
					}
				}
				return null;
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (overlayForm != null && overlayForm.Visible)
			{
				overlayForm.Hide();
			}
		}

#endif

		#endregion

		#region Registering Design Time Services

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				new DesignerServices(value).TryRegister();
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		protected internal readonly DesignerActionExtenderProvider DesignerActionExtenderProvider = new DesignerActionExtenderProvider();

		#endregion

		#region Tabbing Support

		protected sealed override bool ProcessTabKey(bool forward)
		{
			return ProcessTabKeyCore(forward);
		}

		protected virtual bool ProcessTabKeyCore(bool forward)
		{
			return base.ProcessTabKey(forward);
		}

		protected sealed override void Select(bool directed, bool forward)
		{
			try
			{
				SelectCore(directed, forward);
			}
			finally
			{
				ContainerControlWithNoChildrenTabSkipper.AfterSelect(forward);
			}
		}

		protected virtual void SelectCore(bool directed, bool forward)
		{
			base.Select(directed, forward);
		}

		ContainerControlWithNoChildrenTabSkipper ContainerControlWithNoChildrenTabSkipper
		{
			get { return containerControlWithNoChildrenTabSkipper ?? (containerControlWithNoChildrenTabSkipper = new ContainerControlWithNoChildrenTabSkipper(this)); }
		}
		ContainerControlWithNoChildrenTabSkipper containerControlWithNoChildrenTabSkipper;

		#endregion

		#region Static events

		#region FormCreated

		void OnFormCreated()
		{
			if (FormCreated != null)
			{
				FormCreated(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// When the constructor of a KForm object has been run.
		/// </summary>
		public static event EventHandler FormCreated;

		#endregion

		#region FormDisposed

		void OnFormDisposed()
		{
			if (FormDisposed != null)
			{
				FormDisposed(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// When a KForm has been disposed.
		/// </summary>
		public static event EventHandler FormDisposed;

		#endregion

		#region QueryFormCaptionSuffix

		string OnQueryFormCaptionSuffix()
		{
			string result = "";
			if (QueryFormCaptionSuffix != null && QueryFormCaptionSuffixDisabled <= 0)
			{
				QueryFormCaptionEventArgs e = new QueryFormCaptionEventArgs();
				e.GlobalFormTopCaption = string.Empty;
				QueryFormCaptionSuffix(this, e);
				result = e.GlobalFormTopCaption ?? string.Empty;
			}
			return result;
		}

		public static IDisposable TemporarilyDisableQueryFormCaptionSuffix()
		{
			QueryFormCaptionSuffixDisabled++;
			return new DisposableAction(() => { QueryFormCaptionSuffixDisabled--; });
		}
		[ThreadStatic]
		static int QueryFormCaptionSuffixDisabled;

		/// <summary>
		/// Hook this event to apply a suffix to the caption of the form (ie. the Text property).
		/// </summary>
		public static event EventHandler<QueryFormCaptionEventArgs> QueryFormCaptionSuffix;

		#endregion

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			return base.ProcessCmdKey(ref msg, keyData) || Hotkeys.ProcessCmdKey(this, keyData) || GlobalHotkeys.ProcessCmdKey(this, keyData);
		}

		#endregion

		#region Text for caption suffix

		const string textSuffixSeparator = " - ";

		internal string currentFormCaptionSuffix;

		public override string Text
		{
			get
			{
				string captionSuffix = textSuffixSeparator + currentFormCaptionSuffix;
				string result = base.Text;
				if (result.EndsWith(captionSuffix, StringComparison.Ordinal))
				{
					result = result.Remove(result.Length - captionSuffix.Length, captionSuffix.Length);
				}
				return result;
			}
			set
			{
				currentFormCaptionSuffix = OnQueryFormCaptionSuffix();
				if (currentFormCaptionSuffix.Trim().Length > 0 && !value.EndsWith(currentFormCaptionSuffix, StringComparison.Ordinal))
				{
					value += textSuffixSeparator + currentFormCaptionSuffix;
				}
				base.Text = value;
			}
		}

		/// <summary>
		/// Get the Text property, including the suffix applied from QueryFormCaptionSuffix.
		/// </summary>
		public string TextIncludingSuffix
		{
			get { return base.Text; }
		}

		#endregion

		#region ControlCollection

		protected override Control.ControlCollection CreateControlsInstance()
		{
			return new ControlCollection(this);
		}

		[SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		[SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
		[SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
		public new class ControlCollection : AutoDisposeControlCollection
		{
			public ControlCollection(Control owner)
				: base(owner)
			{
			}
		}

		#endregion

		#region Dispose

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "CA rule incompatable with C# 6 feature")]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && !isDisposed)
				{
					if (DataSource != null)
					{
						try
						{
							SetDataBinding(null, "");
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							// just in case - as this was added recently
							ErrorReporter.ReportOnce("ExceptionInClearingOfDataBindingInDispose_" + GetType().Name, ex.Message, ex);
						}
					}
					BindingSource.Dispose();
					DesignerActionExtenderProvider.Dispose();
					DisposableLeakListener.Instance.UnRegisterDisposable(this);

					if (Menu != null)
					{
						Menu.Dispose();
					}

#if DEBUG && !WINZOR
					overlayForm?.Dispose();
#endif

					OnFormDisposed();
				}
			}
			finally
			{
				GCCollectorTracker?.NotifyDisposed(disposing);
				isDisposed = true;
				base.Dispose(disposing);
				CurrencyManagerListClear();
			}
		}

		bool isDisposed;

		void CurrencyManagerListClear()
		{
			BindingContext bindingContext = null;
			try
			{
				bindingContext = BindingContext;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return; //BindingContext can throw NRE when accessed
			}

			if (bindingContext != null)
			{
				MethodInfo setDataSourceMethod = typeof(CurrencyManager).GetMethod("SetDataSource", BindingFlags.NonPublic | BindingFlags.Instance);
				MethodInfo onCurrentItemChangedMethod = typeof(CurrencyManager).GetMethod("OnCurrentItemChanged", BindingFlags.NonPublic | BindingFlags.Instance);

				foreach (DictionaryEntry entry in bindingContext)
				{
					CurrencyManager bindingManager = ((WeakReference)entry.Value).Target as CurrencyManager;
					if (bindingManager != null)
					{
						if (bindingManager.Count == 1)
						{
							try
							{
								//To unhook PropertyManager's PropertyChanged on KPropertyDescriptor's valueChangedHandlers related to the form business object, we have to empty the CurrencyManager's list then reflect and call OnCurrentItemChanged.

								setDataSourceMethod.Invoke(bindingManager, new object[] { new List<BusinessObject>() });
								onCurrentItemChangedMethod.Invoke(bindingManager, new object[] { new EventArgs() });
							}
							catch (TargetInvocationException e)
							{
								if (e.InnerException != null && (e.InnerException is NotSupportedException || e.InnerException.InnerException is Win32Exception))
								{
									//wrapping NotSupportedException -> list is already not an IBindingList and tried to add new to it - just ignore
								}
								else
								{
									throw;
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region IDataBoundControl Members

		public virtual void SetDataBinding(object dataSource, string dataMember)
		{
			BindingSource.SetDataBinding(dataSource, dataMember);
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual Type DataSourceType
		{
			get { return BindingSource.DataSourceType; }
			set { BindingSource.DataSourceType = value; }
		}

		object IDataBoundControl.DataSource
		{ get { return DataSource; } }

		string IDataBoundControl.DataMember
		{ get { return DataMember; } }

		#endregion

		#region ICompositeControlBindingSourceProvider Members

		ICompositeControlBindingSource ICompositeControlBindingSourceProvider.BindingSource
		{
			get { return BindingSource; }
		}

		protected internal HotkeyRegister Hotkeys { get; } = new HotkeyRegister();
		protected internal virtual HotkeyRegister GlobalHotkeys => HotkeyRegister.GlobalHotkeys;

		#endregion
	}
}
