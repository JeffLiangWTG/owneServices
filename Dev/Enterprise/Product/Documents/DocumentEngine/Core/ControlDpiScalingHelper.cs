using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using CargoWise.Application;

namespace Enterprise.DocumentEngine
{
	static class ControlDpiScalingHelper
	{
		public const int BaseDpiX = 96;
		public const int BaseDpiY = 96;

		public static float DpiX => ScalingHelper.DpiX;

		public static float DpiY => ScalingHelper.DpiY;

		public static int ScaleToCurrentDpiX(int valueUnscaled) => ScalingHelper.ScaleToCurrentDpiX(valueUnscaled);

		public static int ScaleToCurrentDpiY(int valueUnscaled) => ScalingHelper.ScaleToCurrentDpiY(valueUnscaled);

		public static int UnscaleFromCurrentDpiX(int valueScaled) => ScalingHelper.UnscaleFromCurrentDpiX(valueScaled);

		public static int UnscaleFromCurrentDpiY(int valueScaled) => ScalingHelper.UnscaleFromCurrentDpiY(valueScaled);

		public static Point NewScaledPoint(int x, int y, bool isInStandardDpi = true) => ScalingHelper.NewScaledPoint(x, y, isInStandardDpi);

		public static Size NewScaledSize(int width, int height, bool isInStandardDpi = true) => ScalingHelper.NewScaledSize(width, height, isInStandardDpi);

		public static Size NewScaledSize(Size size, bool isInStandardDpi = true) => ScalingHelper.NewScaledSize(size, isInStandardDpi);

		public static void SetHeight<T>(T element, int width, bool isOnStandardDpi) where T : class => ScalingHelper.SetHeight(element, width, isOnStandardDpi);

		public static void SetHeight<T>(ref T element, int width, bool isOnStandardDpi) => ScalingHelper.SetHeight(ref element, width, isOnStandardDpi);

		public static void SetWidth<T>(T element, int width, bool isOnStandardDpi) where T : class => ScalingHelper.SetWidth(element, width, isOnStandardDpi);

		public static void SetWidth<T>(ref T element, int width, bool isOnStandardDpi) => ScalingHelper.SetWidth(ref element, width, isOnStandardDpi);

		public static void SetX<T>(ref T element, int x, bool isOnStandardDpi) => ScalingHelper.SetX(ref element, x, isOnStandardDpi);

		public static void SetY<T>(ref T element, int y, bool isOnStandardDpi) => ScalingHelper.SetY(ref element, y, isOnStandardDpi);

		public static void SetLeft<T>(ref T element, int left, bool isOnStandardDpi) => ScalingHelper.SetLeft(ref element, left, isOnStandardDpi);

		public static void SetTop<T>(ref T element, int top, bool isOnStandardDpi) => ScalingHelper.SetTop(ref element, top, isOnStandardDpi);

		public static int MarkAsScaled(int valueUnscaled)
		{
			return valueUnscaled;
		}

#if DEBUG
		public static IDisposable OverrideDPI_ForTesting(float dpiX, float dpiY) => ScalingHelper.OverrideDPI_ForTesting(dpiX, dpiY);
#endif

		static IDpiScalingHelper ScalingHelper
		{
			get
			{
				if (scalingHelper == null)
				{
					scalingHelper = ObjectFactory.Get<IDpiScalingHelper>();
				}
				return scalingHelper;
			}
		}
		[SuppressMessage("CargoWiseOne", "CW1021")]
		static IDpiScalingHelper scalingHelper;
	}
}
