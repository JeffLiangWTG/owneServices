using System;
using System.Drawing;
using CargoWise.Windows.UI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine.GUI
{
	[Immutable]
	class DpiScalingHelper : IDpiScalingHelper
	{
		public float DpiX => ControlDpiScalingHelper.DpiX;

		public float DpiY => ControlDpiScalingHelper.DpiY;

		public Point NewScaledPoint(int x, int y, bool isInStandardDpi = true) => ControlDpiScalingHelper.NewScaledPoint(x, y, isInStandardDpi);

		public Size NewScaledSize(int width, int height, bool isInStandardDpi = true) => ControlDpiScalingHelper.NewScaledSize(width, height, isInStandardDpi);

		public Size NewScaledSize(Size size, bool isInStandardDpi = true) => ControlDpiScalingHelper.NewScaledSize(size, isInStandardDpi);

		public int ScaleToCurrentDpiX(int valueUnscaled) => ControlDpiScalingHelper.ScaleToCurrentDpiX(valueUnscaled);

		public int ScaleToCurrentDpiY(int valueUnscaled) => ControlDpiScalingHelper.ScaleToCurrentDpiY(valueUnscaled);

		public void SetHeight<T>(T element, int width, bool isOnStandardDpi) where T : class => ControlDpiScalingHelper.SetHeight(element, width, isOnStandardDpi);

		public void SetHeight<T>(ref T element, int width, bool isOnStandardDpi) => ControlDpiScalingHelper.SetHeight(ref element, width, isOnStandardDpi);

		public void SetLeft<T>(ref T element, int left, bool isOnStandardDpi) => ControlDpiScalingHelper.SetLeft(ref element, left, isOnStandardDpi);

		public void SetTop<T>(ref T element, int top, bool isOnStandardDpi) => ControlDpiScalingHelper.SetTop(ref element, top, isOnStandardDpi);

		public void SetWidth<T>(T element, int width, bool isOnStandardDpi) where T : class => ControlDpiScalingHelper.SetWidth(element, width, isOnStandardDpi);

		public void SetWidth<T>(ref T element, int width, bool isOnStandardDpi) => ControlDpiScalingHelper.SetWidth(ref element, width, isOnStandardDpi);

		public void SetX<T>(ref T element, int x, bool isOnStandardDpi) => ControlDpiScalingHelper.SetX(ref element, x, isOnStandardDpi);

		public void SetY<T>(ref T element, int y, bool isOnStandardDpi) => ControlDpiScalingHelper.SetY(ref element, y, isOnStandardDpi);

		public int UnscaleFromCurrentDpiX(int valueScaled) => ControlDpiScalingHelper.UnscaleFromCurrentDpiX(valueScaled);

		public int UnscaleFromCurrentDpiY(int valueScaled) => ControlDpiScalingHelper.UnscaleFromCurrentDpiY(valueScaled);

		#if DEBUG
		public IDisposable OverrideDPI_ForTesting(float dpiX, float dpiY) => ControlDpiScalingHelper.OverrideDPI_ForTesting(dpiX, dpiY);
		#endif
	}
}
