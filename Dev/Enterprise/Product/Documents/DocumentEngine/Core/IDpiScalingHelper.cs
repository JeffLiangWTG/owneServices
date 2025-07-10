using System;
using System.Drawing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine
{
	[Immutable]
	public interface IDpiScalingHelper
	{
		float DpiX { get; }

		float DpiY { get; }

		int ScaleToCurrentDpiX(int valueUnscaled);

		int ScaleToCurrentDpiY(int valueUnscaled);

		int UnscaleFromCurrentDpiX(int valueScaled);

		int UnscaleFromCurrentDpiY(int valueScaled);

		Point NewScaledPoint(int x, int y, bool isInStandardDpi = true);

		Size NewScaledSize(int width, int height, bool isInStandardDpi = true);

		Size NewScaledSize(Size size, bool isInStandardDpi = true);

		void SetHeight<T>(T element, int width, bool isOnStandardDpi) where T : class;

		void SetHeight<T>(ref T element, int width, bool isOnStandardDpi);

		void SetWidth<T>(T element, int width, bool isOnStandardDpi) where T : class;

		void SetWidth<T>(ref T element, int width, bool isOnStandardDpi);

		void SetX<T>(ref T element, int x, bool isOnStandardDpi);

		void SetY<T>(ref T element, int y, bool isOnStandardDpi);

		void SetLeft<T>(ref T element, int left, bool isOnStandardDpi);

		void SetTop<T>(ref T element, int top, bool isOnStandardDpi);

#if DEBUG
		IDisposable OverrideDPI_ForTesting(float dpiX, float dpiY);
#endif
	}
}
