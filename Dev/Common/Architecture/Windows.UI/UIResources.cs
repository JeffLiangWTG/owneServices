using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Timers;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI.Interop;
using Enterprise.ZArchitecture.Core;
using Microsoft.Win32;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Retrieves user interface resource information from windows.
	/// </summary>
	public class UIResources
	{
		protected UIResources()
		{
		}

		/// <summary>
		/// Get a singleton instance of this class.
		/// </summary>
		public static UIResources Instance
		{
			get { return instance ?? (instance = new UIResources()); }
#if DEBUG
			set { instance = value; }
#endif
		}
		[ThreadStatic]
		static UIResources instance;

		protected virtual string WindowsRegistryKeyName
		{
			get { return (NoResString)@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"; }
		}

		/// <summary>
		/// Get the number of GDI objects currently in use.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gdi")]
		public virtual int GdiObjectsCount
		{
			get
			{
				try
				{
					return GdiObjectsCountUnsafe;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return 0;
				}
			}
		}

		public virtual int GdiObjectsCountUnsafe
		{
			get
			{
				using (var process = Process.GetCurrentProcess())
				{
					return !process.HasExited ? (int)SafeNativeMethods.GetGuiResources(process.Handle, NativeMethods.GuiResources.GR_GDIOBJECTS) : 0;
				}
			}
		}

		/// <summary>
		/// Get the number of user objects currently in use.
		/// </summary>
		public virtual int UserObjectsCount
		{
			get
			{
				try
				{
					return UserObjectsCountUnsafe;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return 0;
				}
			}
		}

		public virtual int UserObjectsCountUnsafe
		{
			get
			{
				using (var process = Process.GetCurrentProcess())
				{
					return !process.HasExited ? (int)SafeNativeMethods.GetGuiResources(process.Handle, NativeMethods.GuiResources.GR_USEROBJECTS) : 0;
				}
			}
		}

		/// <summary>
		/// get the number of window handles 
		/// </summary>
		public virtual int UserWindowHandlesCount
		{
			get
			{
				using (var process = Process.GetCurrentProcess())
				{
					return !process.HasExited ? process.HandleCount : 0;
				}
			}
		}

		/// <summary>
		/// The maximum number of GDI objects that may be created for the current process.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gdi")]
		public int MaximumGdiObjectCount
		{
			get
			{
				if (maximumGdiObjectCount == null)
				{
					maximumGdiObjectCount = -1;
					try
					{
						var key = Registry.LocalMachine.OpenSubKey(WindowsRegistryKeyName, false);
						if (key != null)
						{
							maximumGdiObjectCount = (int)key.GetValue("GDIProcessHandleQuota", -1);
						}
					}
					catch (SecurityException)
					{
					}
					catch (UnauthorizedAccessException)
					{
					}
				}
				return maximumGdiObjectCount.Value;
			}
		}
		int? maximumGdiObjectCount;

		/// <summary>
		/// The maximum number of User objects that may be created for the current process.
		/// </summary>
		public int MaximumUserObjectCount
		{
			get
			{
				if (maximumUserObjectCount == null)
				{
					maximumUserObjectCount = -1;
					try
					{
						var key = Registry.LocalMachine.OpenSubKey(WindowsRegistryKeyName, false);
						if (key != null)
						{
							maximumUserObjectCount = (int)key.GetValue("USERProcessHandleQuota", -1);
						}
					}
					catch (SecurityException)
					{
					}
					catch (UnauthorizedAccessException)
					{
					}
				}
				return maximumUserObjectCount.Value;
			}
		}
		int? maximumUserObjectCount;

		#region Monitoring UI Resources

		public IDisposable MonitorUIResourcesInBackground(EventHandler resourceNearlyOverflowHandler)
		{
			if (!monitorUIResourcesInBackground)
			{
				SetTimer();
				monitorUIResourcesInBackground = true;
				resourceNearlyOverflow += resourceNearlyOverflowHandler;
			}
			return new DisposableAction(() =>
			{
				monitorUIResourcesInBackground = false;
				resourceNearlyOverflow -= resourceNearlyOverflowHandler;
				monitorTimer?.Dispose();
			});
		}
		bool monitorUIResourcesInBackground;

		void SetTimer()
		{
			monitorTimer = new System.Timers.Timer(intervalTime.TotalMilliseconds);
			monitorTimer.Elapsed += OnTimedEvent;
			monitorTimer.Enabled = true;
		}

		void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			MonitoringGDIObjectUsage();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static System.Timers.Timer monitorTimer;

		public virtual TimeSpan intervalTime => new TimeSpan(0, 0, 30);

		public void MonitoringGDIObjectUsage()
		{
			if (MaximumGdiObjectCount != -1 && GdiObjectsCount > MaximumGdiObjectCount - 1000)
			{
				if (!withinGdiObjectNearlyOverflowCondition)
				{
					withinGdiObjectNearlyOverflowCondition = true;

					monitorTimer.Stop();
					OnUIResourcesNearlyOverflow(EventArgs.Empty);
				}
			}
			else
			{
				withinGdiObjectNearlyOverflowCondition = false;
			}
		}

		internal bool withinGdiObjectNearlyOverflowCondition;

		const int FormsOpenLimit = 10;

		internal int GetFormsOpenLimit()
		{
			return FormsOpenLimit;
		}

		protected virtual void OnUIResourcesNearlyOverflow(EventArgs e)
		{
			if (resourceNearlyOverflow != null)
			{
				resourceNearlyOverflow(this, e);
			}
		}
		EventHandler resourceNearlyOverflow;

		public string GetAllControlsInfo()
		{
			var reportUIObject = new ZStringBuilder();
			var openedForms = ZApplication.GetOpenForms();
			foreach (var form in openedForms)
			{
				reportUIObject.AppendLine((NoResString)"Form opened:")
					.AppendLine(form.Name)
					.AppendLine(form.Text)
					.AppendLine((NoResString)"============ Controls ============");
				GetAllControls(form).GroupBy(c => c).ForEach(groupBy => reportUIObject.AppendLine(groupBy.Key + (NoResString)" - Count: " + groupBy.Count()));
				reportUIObject.AppendLine();
			}
			return reportUIObject.ToString();
		}

		public IEnumerable<string> GetAllControls(Control control)
		{
			yield return control.GetType().FullName;

			foreach (var c in control.Controls.Cast<Control>())
			{
				foreach (var controlTypeName in GetAllControls(c))
				{
					yield return controlTypeName;
				}
			}
		}
		#endregion
	}
}
