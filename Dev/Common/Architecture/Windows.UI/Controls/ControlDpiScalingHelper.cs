using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	[Immutable]
	struct DpiScalingValues
	{
		public readonly float DpiX;
		public readonly float DpiY;

		public readonly float ScaleFactorX;
		public readonly float ScaleFactorY;
		public readonly float UnscaleFactorX;
		public readonly float UnscaleFactorY;

		public DpiScalingValues(float dpiX, float dpiY)
		{
			DpiX = dpiX;
			DpiY = dpiY;

			ScaleFactorX = DpiX / ControlDpiScalingHelper.BaseDpiX;
			ScaleFactorY = DpiY / ControlDpiScalingHelper.BaseDpiY;
			UnscaleFactorX = ControlDpiScalingHelper.BaseDpiX / DpiX;
			UnscaleFactorY = ControlDpiScalingHelper.BaseDpiY / DpiY;
		}
	}

#if !WINZOR
	[CLSCompliant(false)]
#endif
	public static class ControlDpiScalingHelper
	{
		#region Scaling constants

		public const int BaseDpiX = 96;
		public const int BaseDpiY = 96;

		[ThreadSafe]
		public static readonly SizeF DpiScaleDimensions = new SizeF(6F, 13F);
		public static readonly AutoScaleMode DpiScaleMode = AutoScaleMode.None;

#if DEBUG
		[ThreadSafe()] // Only exchanged for tests
		static DpiScalingValues currentState = new DpiScalingValues(GetDpiX(), GetDpiY());
#else
		static readonly DpiScalingValues currentState = new DpiScalingValues(GetDpiX(), GetDpiY());
#endif

		public static float DpiX => currentState.DpiX;
		public static float DpiY => currentState.DpiY;

#if !WINZOR

		static float GetDpiX()
		{
			using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
			{
				return graphics.DpiX;
			}
		}

		static float GetDpiY()
		{
			using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
			{
				return graphics.DpiY;
			}
		}

#else

		static float GetDpiX() => BaseDpiX;

		static float GetDpiY() => BaseDpiY;

#endif

		#region OverrideDPI_ForTesting
#if DEBUG
		public static IDisposable OverrideDPI_ForTesting(float dpiX, float dpiY)
		{
			return new Common.DisposableAction(
				() => currentState = new DpiScalingValues(dpiX, dpiY),
				() => currentState = new DpiScalingValues(GetDpiX(), GetDpiY()));
		}
#endif
		#endregion

		#endregion // Scaling constants

		#region Scaling functions

		[return: DpiState(DpiState.ScaleX)]
		public static int ScaleToCurrentDpiX(int valueUnscaled)
		{
			return RoundToNearestRoundHalfTowardNegativeInfinity(valueUnscaled * currentState.ScaleFactorX);
		}

		[return: DpiState(DpiState.ScaleY)]
		public static int ScaleToCurrentDpiY(int valueUnscaled)
		{
			return RoundToNearestRoundHalfTowardNegativeInfinity(valueUnscaled * currentState.ScaleFactorY);
		}

		[return: DpiState(DpiState.Unscaled)]
		public static int UnscaleFromCurrentDpiX(int valueScaled)
		{
			return RoundToNearestRoundHalfTowardNegativeInfinity(valueScaled * currentState.UnscaleFactorX);
		}

		[return: DpiState(DpiState.Unscaled)]
		public static int UnscaleFromCurrentDpiY(int valueScaled)
		{
			return RoundToNearestRoundHalfTowardNegativeInfinity(valueScaled * currentState.UnscaleFactorY);
		}

		[return: DpiState(DpiState.ScaledVariant)]
		public static int MarkAsScaled(int valueUnscaled)
		{
			return valueUnscaled;
		}

		#endregion // Scaling functions

		#region Scaled constructors

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static Padding NewScaledPadding(int all, bool isInStandardDpi = true)
		{
			var scaledAll = isInStandardDpi ? ScaleToCurrentDpiX(all) : all;
			return new Padding(scaledAll, scaledAll, scaledAll, scaledAll);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static Padding NewScaledPadding(int left, int top, int right, int bottom, bool isInStandardDpi = true)
		{
			int scaledLeft, scaledTop, scaledRight, scaledBottom;
			if (isInStandardDpi)
			{
				scaledLeft = ScaleToCurrentDpiX(left);
				scaledTop = ScaleToCurrentDpiY(top);
				scaledRight = ScaleToCurrentDpiX(right);
				scaledBottom = ScaleToCurrentDpiY(bottom);
			}
			else
			{
				scaledLeft = left;
				scaledTop = top;
				scaledRight = right;
				scaledBottom = bottom;
			}
			return new Padding(scaledLeft, scaledTop, scaledRight, scaledBottom);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static Point NewScaledPoint(int x, int y, bool isInStandardDpi = true)
		{
			if (isInStandardDpi)
			{
				return new Point(ScaleToCurrentDpiX(x), ScaleToCurrentDpiY(y));
			}

			return new Point(x, y);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static Point NewScaledPoint(Size size, bool isInStandardDpi = true)
			=> new Point(NewScaledSize(size, isInStandardDpi));

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static Rectangle NewScaledRectangle(int x, int y, int width, int height, bool isInStandardDpi = true)
		{
			if (isInStandardDpi)
			{
				return new Rectangle(ScaleToCurrentDpiX(x), ScaleToCurrentDpiY(y), ScaleToCurrentDpiX(width), ScaleToCurrentDpiY(height));
			}

			return new Rectangle(x, y, width, height);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static Rectangle NewScaledRectangle(Point location, Size size, bool isInStandardDpi = true)
		{
			if (isInStandardDpi)
			{
				return new Rectangle(NewScaledPoint(location.X, location.Y), NewScaledSize(size.Width, size.Height));
			}

			return new Rectangle(location, size);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static Size NewScaledSize(Size size, bool isInStandardDpi = true)
		{
			if (isInStandardDpi)
			{
				return new Size(ScaleToCurrentDpiX(size.Width), ScaleToCurrentDpiY(size.Height));
			}

			return size;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static Size NewScaledSize(int width, int height, bool isInStandardDpi = true)
		{
			if (isInStandardDpi)
			{
				return new Size(ScaleToCurrentDpiX(width), ScaleToCurrentDpiY(height));
			}

			return new Size(width, height);
		}

		#endregion // Scaled sizes

		#region Generic setters

		[ThreadSafe]
		static readonly Lazy<Dictionary<Type, Func<int, object>>> _constructorCache =
			new Lazy<Dictionary<Type, Func<int, object>>>(() => new Dictionary<Type, Func<int, object>>());

		internal static object ReflectAndSetProperty(object element, string propertyName, int value)
		{
			var elementType = element.GetType();
			var property = elementType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			var propertyType = property.PropertyType;
			var setter = property.SetMethod;
			object[] parameters;
			if (propertyType.IsAssignableFrom(typeof(int)))
			{
				parameters = new object[] { value };
			}
			else
			{
				// If the setter for the property doesn't accept ints, then int must be convertible to the 
				// destination type, or the destination type should be able to be constructed using an int
				if (!_constructorCache.Value.TryGetValue(propertyType, out var constructorDelegate))
				{
					var constructor = propertyType.GetConstructor(new Type[] { typeof(int) });
					if( constructor != null )
					{
						var parameter = Expression.Parameter(typeof(int));
						var newExpression = Expression.New(constructor, parameter);
						constructorDelegate = Expression.Lambda<Func<int, object>>(newExpression, parameter).Compile();
					}
					else
					{
						constructorDelegate = value => Convert.ChangeType(value, propertyType);
					}
					_constructorCache.Value[propertyType] = constructorDelegate;
				}
				var converted = constructorDelegate(value);
				parameters = new object[] { converted };
			}
			setter.Invoke(element, parameters);
			return element;
		}

		#region SuppressResourceStringsCheckRegion

		#region This avoids reflection, and is not CLS compliant

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static void SetHeight(Control element, int height, bool isOnStandardDpi)
		{
			element.Height = isOnStandardDpi ? ScaleToCurrentDpiY(height) : height;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static void SetWidth(Control element, int width, bool isOnStandardDpi)
		{
			element.Width = isOnStandardDpi ? ScaleToCurrentDpiX(width) : width;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static void SetLeft(Control element, int left, bool isOnStandardDpi)
		{
			element.Left = isOnStandardDpi ? ScaleToCurrentDpiX(left) : left;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static void SetTop(Control element, int top, bool isOnStandardDpi)
		{
			element.Top = isOnStandardDpi ? ScaleToCurrentDpiY(top) : top;
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static void SetHeight(ref Control element, int height, bool isOnStandardDpi) => SetHeight(element, height, isOnStandardDpi);

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static void SetWidth(ref Control element, int width, bool isOnStandardDpi) => SetWidth(element, width, isOnStandardDpi);

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static void SetLeft(ref Control element, int left, bool isOnStandardDpi) => SetLeft(element, left, isOnStandardDpi);

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static void SetTop(ref Control element, int top, bool isOnStandardDpi) => SetTop(element, top, isOnStandardDpi);

		#endregion

		public static void SetHeight<T>(ref T element, int height, bool isOnStandardDpi)
		{
			element = (T)ReflectAndSetProperty(element, "Height", isOnStandardDpi ? ScaleToCurrentDpiY(height) : height);
		}

		public static void SetWidth<T>(ref T element, int width, bool isOnStandardDpi)
		{
			element = (T)ReflectAndSetProperty(element, "Width", isOnStandardDpi ? ScaleToCurrentDpiX(width) : width);
		}

		public static void SetLeft<T>(ref T element, int left, bool isOnStandardDpi)
		{
			element = (T)ReflectAndSetProperty(element, "Left", isOnStandardDpi ? ScaleToCurrentDpiX(left) : left);
		}

		public static void SetTop<T>(ref T element, int top, bool isOnStandardDpi)
		{
			element = (T)ReflectAndSetProperty(element, "Top", isOnStandardDpi ? ScaleToCurrentDpiY(top) : top);
		}

		public static void SetRight<T>(ref T element, int right, bool isOnStandardDpi)
		{
			element = (T)ReflectAndSetProperty(element, "Right", isOnStandardDpi ? ScaleToCurrentDpiX(right) : right);
		}

		public static void SetBottom<T>(ref T element, int bottom, bool isOnStandardDpi)
		{
			element = (T)ReflectAndSetProperty(element, "Bottom", isOnStandardDpi ? ScaleToCurrentDpiY(bottom) : bottom);
		}

		public static void SetX<T>(ref T element, int x, bool isOnStandardDpi)
		{
			element = (T)ReflectAndSetProperty(element, "X", isOnStandardDpi ? ScaleToCurrentDpiX(x) : x);
		}

		public static void SetY<T>(ref T element, int y, bool isOnStandardDpi)
		{
			element = (T)ReflectAndSetProperty(element, "Y", isOnStandardDpi ? ScaleToCurrentDpiY(y) : y);
		}

		public static void SetHeight<T>(T element, int height, bool isOnStandardDpi) where T : class
		{
			element = (T)ReflectAndSetProperty(element, "Height", isOnStandardDpi ? ScaleToCurrentDpiY(height) : height);
		}

		public static void SetWidth<T>(T element, int width, bool isOnStandardDpi) where T : class
		{
			element = (T)ReflectAndSetProperty(element, "Width", isOnStandardDpi ? ScaleToCurrentDpiX(width) : width);
		}

		public static void SetLeft<T>(T element, int left, bool isOnStandardDpi) where T : class
		{
			element = (T)ReflectAndSetProperty(element, "Left", isOnStandardDpi ? ScaleToCurrentDpiX(left) : left);
		}

		public static void SetTop<T>(T element, int top, bool isOnStandardDpi) where T : class
		{
			element = (T)ReflectAndSetProperty(element, "Top", isOnStandardDpi ? ScaleToCurrentDpiY(top) : top);
		}

		public static void SetRight<T>(T element, int right, bool isOnStandardDpi) where T : class
		{
			element = (T)ReflectAndSetProperty(element, "Right", isOnStandardDpi ? ScaleToCurrentDpiX(right) : right);
		}

		public static void SetBottom<T>(T element, int bottom, bool isOnStandardDpi) where T : class
		{
			element = (T)ReflectAndSetProperty(element, "Bottom", isOnStandardDpi ? ScaleToCurrentDpiY(bottom) : bottom);
		}

		public static void SetX<T>(T element, int x, bool isOnStandardDpi) where T : class
		{
			element = (T)ReflectAndSetProperty(element, "X", isOnStandardDpi ? ScaleToCurrentDpiX(x) : x);
		}

		public static void SetY<T>(T element, int y, bool isOnStandardDpi) where T : class
		{
			element = (T)ReflectAndSetProperty(element, "Y", isOnStandardDpi ? ScaleToCurrentDpiY(y) : y);
		}

		#endregion // SuppressResourceStringsCheckRegion

		#endregion // Generic setters

		[DpiState(DpiState.ScaleX)]
		public static readonly int OnePixel = MarkAsScaled(1); // Can't be a const as they're inlined and then identified as Unscaled

		public static int RoundToNearestRoundHalfTowardNegativeInfinity(double value)
		{
			// Subtract 0.5 and take the next greater integer.
			return (int)Math.Ceiling(value - 0.5);
		}
	}
}
