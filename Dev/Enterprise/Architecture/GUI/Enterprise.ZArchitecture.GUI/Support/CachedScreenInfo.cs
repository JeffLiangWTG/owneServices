using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Core.Forms
{
	/// <summary>
	/// This class has primarily been written to overcome strange behaviour in .NET's Screen class
	/// where it performs a Garbage Collection and Thread.Sleep() if the Screen object is accessed
	/// on any form with a large number of controls. 
	/// 
	/// This class will cache all screen infos on startup and the application should refer to it,
	/// rather then the Screen object itself. It also handles resolution changes and remote sessions
	/// using Terminal Services / Remote Desktop / Citrix.
	/// </summary>

	public enum HorizontalState { OffScreenLeft, Visible, OffScreenRight }
	public enum VerticalState { OffScreenTop, Visible, OffScreenBottom }

	public sealed class CachedScreenInfo : ICachedScreenInfo
	{
		internal CachedScreenInfo()
		{
		}

		public static CachedScreenInfo Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CachedScreenInfo();
					instance.PopulateInfo();
				}

				return instance;
			}
		}

		[ThreadStatic]
		static CachedScreenInfo instance;

		/// <summary>
		/// A list of all screen working areas.
		/// </summary>
		public Rectangle[] ScreenInfos => Screens.Select(screen => screen.WorkingArea).ToArray();

		/// <summary>
		/// A list of all screen bounds
		/// </summary>
		public List<Rectangle> BoundsInfos => Screens.Select(screen => screen.Bounds).ToList();

		/// <summary>
		/// Working area of the screen deemed as primary by the OS.
		/// </summary>
		public Rectangle PrimaryScreenInfo => PrimaryScreen.WorkingArea;

		/// <summary>
		/// A collection of information for each screen
		/// </summary>
		public IEnumerable<IPhysicalScreenInfo> Screens => allScreens;

		//IEnumerable<IPhysicalScreenInfo> ICachedScreenInfo.Screens => Screens;

		/// <summary>
		/// IScreen of the screen deemed as primary by the OS.
		/// </summary>
		public IPhysicalScreenInfo PrimaryScreen { get; private set; }

		/// <summary>
		/// Call this method at the start of the application so that it initialises the Cache of 
		/// screen information.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "The one place where it should be called")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "The one place where it should be called")]
		public void PopulateInfo()
		{
			lock (lockObj)
			{
				FieldInfo fieldInfo = null;

				var screens = Screen.AllScreens;
				try
				{
					fieldInfo = typeof(Screen).GetField("screens", BindingFlags.NonPublic | BindingFlags.Static);

					if (fieldInfo != null)
					{
						fieldInfo.SetValue(null, null);
					}

					allScreens.Clear();

					for (var i = 0; i < 3; ++i)
					{
						if (screens != null && !screens.Any(screen => screen == null))
						{
							break;
						}
						screens = Screen.AllScreens;

						Thread.Sleep(10);
					}

					foreach (var systemScreen in screens)
					{
						var dpiInfo = GetDpiInfo(systemScreen);
						var screen = new PhysicalScreenInfoImpl
						{
							IsPrimary = systemScreen.Primary,
							WorkingArea = systemScreen.WorkingArea,
							Bounds = systemScreen.Bounds,
							Depth = systemScreen.BitsPerPixel,
							DpiX = dpiInfo.X,
							DpiY = dpiInfo.Y
						};

						allScreens.Add(screen);

						if (systemScreen.Primary)
						{
							PrimaryScreen = screen;
						}
					}
				}
				catch (NullReferenceException ex)
				{
					var message = new StringBuilder();
					if (screens == null)
					{
						message.AppendLine("Screen.AllScreens is null.");
					}
					else
					{
						var allScreensMsg = allScreens == null ? "<null>" : allScreens.Count.ToString();
						message.Append("allScreens is count ? ").AppendLine(allScreensMsg);

						message.Append("Screen.AllScreens Count ? ").AppendLine(screens.Length.ToString());
						message.Append("does Screen.AllScreens contain any null screen ? ").AppendLine(screens.Any(screen => screen == null).ToString());
						message.Append("Screens contains any WorkingArea set to null ? ").AppendLine(screens.Any(screen => screen?.WorkingArea == null).ToString());
						message.Append("Screens contains any Bounds set to null ? ").AppendLine(screens.Any(screen => screen?.Bounds == null).ToString());
					}
					throw new NullReferenceException($"{ex.Message}\r\n{message}", ex);
				}
			}
		}

		/// <summary>
		/// Return screen information for the screen that contains the Control passed in.
		/// </summary>
		/// <param name="control">A control</param>
		/// <returns>Working area of the relevant screen</returns>
		public Rectangle FromControl(Control control)
		{
			var controlCentre = ControlDpiScalingHelper.NewScaledPoint(control.Left + control.Width / 2, control.Top + control.Height / 2, false);
			return FromPoint(control is Form ? controlCentre : control.Parent.PointToScreen(controlCentre));
		}

		public HorizontalState GetHorizontalState(Point point)
		{
			var result = HorizontalState.OffScreenLeft;

			foreach (var screen in Screens)
			{
				if (screen.WorkingArea.Top <= point.Y && screen.WorkingArea.Bottom >= point.Y)
				{
					if (screen.WorkingArea.Left <= point.X)
					{
						if (screen.WorkingArea.Right >= point.X)
						{
							result = HorizontalState.Visible;
							break;
						}

						result = HorizontalState.OffScreenRight;
					}
					else
					{
						result = HorizontalState.OffScreenLeft;
					}
				}
			}

			return result;
		}

		public Rectangle getCurrentScreen(Point point)
		{
			IPhysicalScreenInfo first = null;
			foreach (var screen in Screens)
			{
				if (screen.WorkingArea.Left <= point.X && screen.WorkingArea.Right >= point.X && screen.WorkingArea.Top <= point.Y && screen.WorkingArea.Bottom >= point.Y)
				{
					return screen.WorkingArea;
				}

				if (first == null)
				{
					first = screen;
				}
			}

			return first.WorkingArea;
		}

		public VerticalState GetVerticalState(Point point, int height)
		{
			var dropDownBottomEdge = point.Y + height;
			var dropDownTopEdge = point.Y - height;
			var result = VerticalState.Visible;
			var rect = getCurrentScreen(point);

			if (rect.Bottom >= dropDownBottomEdge)
			{
				result = VerticalState.Visible;
			}
			else
			{
				result = rect.Top > dropDownTopEdge ? VerticalState.OffScreenTop : VerticalState.OffScreenBottom;
			}

			return result;
		}

		public bool IsVisible(Control control, Point location)
		{
			_ = control.PointToScreen(location);
			return true;
		}

		/// <summary>
		/// Return screen information for the screen that contains 
		/// the Location passed in.
		/// </summary>
		/// <param name="location">A Point containing location information</param>
		/// <returns>Working area of the relevant screen</returns>
		public Rectangle FromPoint(Point location)
		{
			foreach (var screen in Screens)
			{
				if (screen.WorkingArea.Contains(location))
				{
					return screen.WorkingArea;
				}
			}

			return PrimaryScreen.WorkingArea;
		}

		public bool Contains(Rectangle rect)
		{
			foreach (var screen in Screens)
			{
				if (screen.WorkingArea.Contains(rect))
				{
					return true;
				}
			}

			return false;
		}

		#region Implementation

#if DEBUG
		internal
#endif
		sealed class PhysicalScreenInfoImpl : IPhysicalScreenInfo
		{
			public bool IsPrimary { get; set; }

			public Rectangle WorkingArea { get; set; }

			public Rectangle Bounds { get; set; }

			public int Depth { get; set; }

			public uint DpiX { get; set; }

			public uint DpiY { get; set; }
		}

		struct DpiInfo
		{
			public uint X;
			public uint Y;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Using screen coordinates to obtain DPI value for a screen")]
		static DpiInfo GetDpiInfo(Screen screen)
		{
			uint dpiX;
			uint dpiY;

			NativeMethods.GetDpiForMonitor(NativeMethods.MonitorFromPoint(new Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1), 2), 0, out dpiX, out dpiY);

			return new DpiInfo
			{
				X = dpiX,
				Y = dpiY,
			};
		}

		static class NativeMethods
		{
			[DllImport("User32.dll")]
			internal static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

			[DllImport("Shcore.dll")]
			internal static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] uint dpiType, [Out] out uint dpiX, [Out] out uint dpiY);
		}

#if DEBUG
		internal
#endif
		List<IPhysicalScreenInfo> allScreens = new List<IPhysicalScreenInfo>();
		static readonly object lockObj = new object();

		#endregion
	}
}
