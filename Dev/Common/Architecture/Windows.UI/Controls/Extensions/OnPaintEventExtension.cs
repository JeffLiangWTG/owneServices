using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI.Interop;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Extension for .NET controls which doesn't raise Paint event.
	/// Usage example:
	/// <c>
	///	OnPaintEventExtension paintExtension;
	///
	///	public ZTextBox()
	///	{
	///		....
	///		paintExtension = new OnPaintEventExtension(this);
	///	}
	///	
	///	protected override void OnResize(EventArgs e)
	///	{
	///		paintExtension.OnResize(e);
	///		base.OnResize(e);
	///	}
	///
	///	protected override void WndProc(ref Message message)
	///	{
	///		paintExtension.WndProc(ref message);
	///		base.WndProc(ref message);
	///	}
	///	
	///	public void Dispose(bool disposing)
	///	{
	///		if (disposing)
	///		{
	///			...
	///			paintExtension.Dispose();
	///		}
	///
	///		base.Dispose(disposing);
	///	}
	/// </c>
	/// <remarks>
	///		DO NOT FORGET TO CALL "DISPOSE()" WHEN DISPOSING CONTROL
	/// </remarks>
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class OnPaintEventExtension : IDisposable
	{
		public OnPaintEventExtension(Control control)
		{
			Argument.NotNull(control, nameof(control));
			this.control = control;
		}

		public bool Skip { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
		public void OnResize(EventArgs e)
		{
			if (!IsActive())
			{
				return;
			}

			BuildBitmap();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wnd")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public void WndProc(ref Message message)
		{
			if (!IsActive())
			{
				return;
			}

			switch (message.Msg)
			{
				case WindowMessages.WM_ERASEBKGND:

					//removes flicker
					return;

				case WindowMessages.WM_PAINT:

					// the designer host does not call OnResize()                    
					if (internalBitmap == null)
					{
						BuildBitmap();
					}
					//BuildBitmap() does not always set internalGraphics to not-null, such as if Control Width or Height is 0
					if (internalBitmap == null)
					{
						break;
					}

					using (var internalGraphics = Graphics.FromImage(internalBitmap))
					{
						if (internalGraphics == null)
						{
							break;
						}

						// set up 
						var updateRect = new NativeMethods.RECT();

						if (UnsafeNativeMethods.GetUpdateRect(message.HWnd, ref updateRect, true) == 0)
						{
							break;
						}

						var paintStruct = new NativeMethods.PAINTSTRUCT();

						try
						{
							var screenHdc = UnsafeNativeMethods.BeginPaint(new HandleRef(this, message.HWnd), ref paintStruct);

							using (var screenGraphics = Graphics.FromHdc(screenHdc))
							{
								// draw internal graphics
								var hdc = IntPtr.Zero;

								try
								{
									hdc = internalGraphics.GetHdc();
									var printClientMessage = Message.Create(control.Handle, WindowMessages.WM_PRINTCLIENT, hdc, IntPtr.Zero);
									InvokeMethod("DefWndProc", printClientMessage);
								}
								finally
								{
									if (hdc != IntPtr.Zero)
									{
										internalGraphics.ReleaseHdc(hdc);
									}
								}

								// raise missing OnPaint() call
								InvokeMethod("OnPaint", new PaintEventArgs(internalGraphics, Rectangle.FromLTRB(
									updateRect.Left,
									updateRect.Top,
									updateRect.Right,
									updateRect.Bottom)));

								// draw screen graphics
								screenGraphics.DrawImage(internalBitmap, 0, 0);
							}
						}
						catch (InvalidOperationException ex)
						{
							if (ex.Message != "Object is currently in use elsewhere.") // See Issue 00852474.
							{
								throw;
							}
						}
						catch (ExternalException ex)
						{
							if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") // See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.
							{
								throw;
							}
						}
						finally
						{
							UnsafeNativeMethods.EndPaint(new HandleRef(this, message.HWnd), ref paintStruct);
						}
					}
					return;
			}
		}

		#region Dispose

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		public void Dispose()
		{
			DisposeBitmap();
		}

		void DisposeBitmap()
		{
			if (internalBitmap != null)
			{
				internalBitmap.Dispose();
				internalBitmap = null;
			}
		}

		#endregion

		#region Events

		EventHandlerList Events => events ?? (events = (EventHandlerList)EventsProperty.GetValue(control, null));
		EventHandlerList events;

		static object PaintEventKey
		{
			get
			{
				if (paintEventKey == null)
				{
#if WINZOR
					throw new NotSupportedException(@"Common\Architecture\Windows.UI\CargoWise.Windows.UI.Winzor.csproj should have this file excluded from the project");
#elif NET
					const string fieldName = "s_paintEvent";
#else
					const string fieldName = "EventPaint";
#endif
					var field = typeof(Control).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
					paintEventKey = field.GetValue(null);
				}

				return paintEventKey;
			}
		}
		[ThreadSafe]
		static object paintEventKey;

		static PropertyInfo EventsProperty => eventsProperty ?? (eventsProperty = typeof(Component).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance));

		[ThreadSafe]
		static PropertyInfo eventsProperty;

#endregion

		#region Implementation

		readonly Control control;
		Bitmap internalBitmap;

		bool IsActive()
		{
			return !Skip && HasSubscribers();
		}

		bool HasSubscribers()
		{
			return Events[PaintEventKey] != null;
		}

		void InvokeMethod(string methodName, object arg)
		{
			var method = typeof(Control).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
			method?.Invoke(control, new[] { arg });
		}

		void BuildBitmap()
		{
			if (internalBitmap == null || internalBitmap.Width != control.Width || internalBitmap.Height != control.Height)
			{
				if (control.Width > 0 && control.Height > 0)
				{
					DisposeBitmap();

					internalBitmap = new Bitmap(control.Width, control.Height);
				}
			}
		}

		#endregion
	}
}
