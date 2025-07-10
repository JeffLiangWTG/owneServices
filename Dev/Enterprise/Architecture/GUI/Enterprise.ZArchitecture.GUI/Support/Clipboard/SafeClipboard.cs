using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Safe Clipboard wrapper for handling ExternalException
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1088:Do Not Use System.Windows.Forms.Clipboard", Justification = "This is the SafeClipboard class that this analyzer recommends")]
	public static class SafeClipboard
	{
		#region controlSuitableForInvoke

		delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
			IntPtr lParam);

		static Thread GetControlOwnerThread(Control ctrl)
		{
			Thread thread = null;
			if (ctrl != null && ctrl.InvokeRequired)
			{
				ctrl.Invoke((Action)(() => thread = System.Threading.Thread.CurrentThread));
			}
			else
			{
				thread = System.Threading.Thread.CurrentThread;
			}

			return thread;
		}

		static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
		{
			var handles = new List<IntPtr>();

			foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
			{
				EnumThreadWindows(thread.Id,
					(hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
			}

			return handles;
		}

		static Control controlSuitableForInvoke()
		{
			if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
			{
				return null;
			}

			var currentProcess = Process.GetCurrentProcess();

			foreach (var windowHandle in EnumerateProcessWindowHandles(currentProcess.Id))
			{
				var control = Control.FromHandle(windowHandle);
				var thread = GetControlOwnerThread(control);
				if (thread != null && thread.GetApartmentState() == ApartmentState.STA)
				{
					return control;
				}
			}

			return null;
		}

		#endregion

		/// <summary>
		/// Removes all data from the Clipboard.
		/// <param name="control">A control that can be Invoked upon to ensure the clipboard operation is done in the main thread.</param>
		/// </summary>
		public static void Clear()
		{
			var control = controlSuitableForInvoke();

			try
			{
				// This is the only place where Clipboard is accessed.
				if (control != null && control.InvokeRequired)
				{
					control.Invoke(Clipboard.Clear); 
				}
				else
				{
					Clipboard.Clear();
				}
			}
			catch (ExternalException) { }
			catch (ObjectDisposedException) { }
		}

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Stopwatch lastClipboardPaste = InitLastClipboardPaste();
		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly int pasteDebounceMs = EnvProxy.Instance.Registry.PasteDebounceMs;
		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static long lastClipboardPasteMs = -pasteDebounceMs;

		static Stopwatch InitLastClipboardPaste()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			return stopwatch;
		}

		public static bool Debounce()
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				return true;
			}
#endif
			if (pasteDebounceMs <= 0) //feature disabled
			{
				return true;
			}

			if (lastClipboardPaste.ElapsedMilliseconds - lastClipboardPasteMs < pasteDebounceMs)
			{
				return false;
			}
			else
			{
				lastClipboardPasteMs = lastClipboardPaste.ElapsedMilliseconds;
				return true;
			}
		}

		/// <summary>
		/// Retrieves data from the Clipboard in the specified format.
		/// </summary>
		/// <param name="format">The format of the data to retrieve. See <see cref="DataFormats"/> for predefined formats.</param>
		/// <param name="control">A control that can be Invoked upon to ensure the clipboard operation is done in the main thread.</param>
		/// <returns>An System.Object representing the Clipboard data or null if the Clipboard does not contain any data that is in the specified format or if ExternalException was fired.</returns>
		public static object GetData(string format)
		{
			var control = controlSuitableForInvoke();

			try
			{
				object data = null;

				if (control != null && control.InvokeRequired)
				{
					control.Invoke((Action)(() => data = Clipboard.GetData(format))); // This is the only place where Clipboard is accessed.
				}
				else
				{
					data = Clipboard.GetData(format); // This is the only place where Clipboard is accessed.
				}

				if (format == DataFormats.Html && data is string)
				{
					data = ((string)data).TrimEnd('\0');
				}

				return data;
			}
			catch (ExternalException)
			{
				return null;
			}
			catch (ObjectDisposedException)
			{
				return null;
			}
		}

		/// <summary>
		/// Retrieves data from clipboard.
		/// </summary>
		/// <param name="control">A control that can be Invoked upon to ensure the clipboard operation is done in the main thread.</param>
		/// <returns>IDataObject with Clipboard content. Null if there is nothing. Null if ExternalException.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static IDataObject GetDataObject()
		{
			var control = controlSuitableForInvoke();

			try
			{
				if (control != null && control.InvokeRequired)
				{
					IDataObject result = null;
					control.Invoke((Action)(() => result = Clipboard.GetDataObject())); // This is the only place where Clipboard is accessed.
					return result;
				}
				else
				{
					return Clipboard.GetDataObject(); // This is the only place where Clipboard is accessed.
				}
			}
			catch (ExternalException)
			{
				return null;
			}
			catch (ObjectDisposedException)
			{
				return null;
			}
		}

		/// <summary>
		/// Retrieves text data from the Clipboard.
		/// </summary>
		/// <returns>The Clipboard text data or <see cref="string.Empty"/> if the Clipboard does text data or if ExternalException was fired.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static string GetText()
		{
			var control = controlSuitableForInvoke();

			try
			{
				if (control != null && control.InvokeRequired)
				{
					var result = string.Empty;
					control.Invoke((Action)(() => result = Clipboard.GetText())); // This is the only place where Clipboard is accessed.
					return result;
				}
				else
				{
					return Clipboard.GetText(); // This is the only place where Clipboard is accessed.
				}
			}
			catch (ExternalException)
			{
				return string.Empty;
			}
			catch (ObjectDisposedException)
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Retrieves text data from the Clipboard.
		/// </summary>
		/// <param name="format">The requested text data format.</param>
		/// <returns>The Clipboard text data or <see cref="string.Empty"/> if the Clipboard does text data or if ExternalException was fired.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static string GetText(TextDataFormat format)
		{
			var control = controlSuitableForInvoke();

			try
			{
				if (control != null && control.InvokeRequired)
				{
					var result = string.Empty;
					control.Invoke((Action)(() => result = Clipboard.GetText(format))); // This is the only place where Clipboard is accessed.
					return result;
				}
				else
				{
					return Clipboard.GetText(format); // This is the only place where Clipboard is accessed.
				}
			}
			catch (ExternalException)
			{
				return string.Empty;
			}
			catch (ObjectDisposedException)
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Adds data to the Clipboard in the specified format.
		/// </summary>
		/// <param name="format">The format of the data to retrieve. See <see cref="DataFormats"/> for predefined formats.</param>
		/// <param name="data">An <see cref="object"/> representing the data to add.</param>
		/// <param name="control">A control that can be Invoked upon to ensure the clipboard operation is done in the main thread.</param>
		/// <returns>True if success, False if ExternalException was fired.</returns>
		public static bool SetData(string format, object data)
		{
			IDataObject obj2 = new DataObject();
			obj2.SetData(format, data);
			return SetDataObject(obj2, true);
		}

		/// <summary>
		/// Puts data to clipboard.
		/// </summary>
		/// <param name="data">Data to be put into Clipboard.</param>
		/// <param name="control">A control that can be Invoked upon to ensure the clipboard operation is done in the main thread.</param>
		/// <returns>True if operation succeed. False if ExternalException.</returns>
		public static bool SetDataObject(object data)
		{
			return SetDataObject(data, false);
		}

		/// <summary>
		/// Puts data to clipboard.
		/// </summary>
		/// <param name="data">Data to be put into Clipboard.</param>
		/// <param name="copy">True if you want data to remain on the Clipboard after this application exits; False otherwise.</param>
		/// <param name="control">A control that can be Invoked upon to ensure the clipboard operation is done in the main thread.</param>
		/// <returns>True if operation succeed. False if ExternalException.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public static bool SetDataObject(object data, bool copy)
		{
			var control = controlSuitableForInvoke();

			try
			{
				if (control != null && control.InvokeRequired)
				{
					control.Invoke(() => Clipboard.SetDataObject(data, false)); // This is the only place where Clipboard is accessed.
					if (copy)
					{
						var retry = 10;
						var flushed = 0;
						control.Invoke((Action)(() => flushed = OleFlushClipboard()));
						while (flushed != 0)
						{
							if (retry == 0)
							{
								throw new ExternalException();
							}
							retry--;
							Thread.Sleep(100);
							Application.DoEvents();
							control.Invoke((Action)(() => flushed = OleFlushClipboard()));
						}
					}
					return true;
				}
				else
				{
					if (data == null)
					{
						return false;
					}
					Clipboard.SetDataObject(data, false); // This is the only place where Clipboard is accessed.
					if (copy)
					{
						var retry = 10;
						while (OleFlushClipboard() != 0)
						{
							if (retry == 0)
							{
								throw new ExternalException();
							}
							retry--;
							Thread.Sleep(100);
							Application.DoEvents();
						}
					}
					return true;
				}
			}
			catch (ExternalException)
			{
				return false;
			}
			catch (ObjectDisposedException)
			{
				return false;
			}
		}

		/// <summary>
		/// Adds text data to the Clipboard, as unicode text.
		/// </summary>
		/// <param name="text">The text to add to the Clipboard.</param>
		/// <param name="control">A control that can be Invoked upon to ensure the clipboard operation is done in the main thread.</param>
		/// <returns>True if operation succeed. False if ExternalException.</returns>
		public static bool SetText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				throw new ArgumentNullException(nameof(text));
			}
			IDataObject data = new DataObject();
			data.SetData(DataFormats.UnicodeText, false, text);
			return SetDataObject(data, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("ole32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		static extern int OleFlushClipboard();

		public static string ClipboardNotAccessibleWarning
		{
			get
			{
				return Res.GetString("d234e238-8210-40f8-8a93-561cf09f7678", "Windows Clipboard is not accessible at the moment. Try to repeat this operation later.");
			}
		}

		/// <summary>Indicates whether there is data on the Clipboard in the System.Windows.Forms.TextDataFormat.Text
		/// or System.Windows.Forms.TextDataFormat.UnicodeText format, depending on the operating system.
		/// </summary>
		/// <returns>true if there is text data on the Clipboard; otherwise, false.</returns>
		/// <exception cref="ThreadStateException">The current thread is not in single-threaded apartment (STA) mode. Add the System.STAThreadAttribute to your application's Main method.</exception>
		public static bool ContainsText()
		{
			return Clipboard.ContainsText();
		}
	}
}
