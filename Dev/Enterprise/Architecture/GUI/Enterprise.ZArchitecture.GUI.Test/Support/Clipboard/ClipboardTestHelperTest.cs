using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ClipboardTestHelper
	{
		public IDataObject ClipboardData { get; private set; }

		public IDisposable MockClipboard()
		{
			var clipboardMock = new Mock<IClipboard>();

			clipboardMock.Setup(c => c.SetDataObject(It.IsAny<object>(), It.IsAny<bool>()))
				.Callback<object, bool>((data, copy) =>
				{
					ClipboardData = data as IDataObject;
				})
				.Returns(true);

			clipboardMock.Setup(c => c.GetDataObject()).Returns(() => ClipboardData);

			return ObjectFactory.Substitute(clipboardMock.Object);
		}

		public static void RetryActionOnlyForNativeClipboard(Action action)
		{
			for (int i = 0; i < 10; i++)
			{
				try
				{
					action.Invoke();
					Thread.Sleep(100);
				}
				catch (ExternalException)
				{
					// Ignore this exception and try again
				}
			}
		}

		public static T RetryGetForNativeClipboard<T>() where T : class
		{
			for (int i = 0; i < 10; i++)
			{
				try
				{
					Thread.Sleep(100);
					var result = GetValue(typeof(T), false) as T;
					return result;
				}
				catch (ExternalException)
				{
					// Ignore this exception and try again
				}
			}

			return GetValue(typeof(T)) as T;
		}

		public static T RetryIfCopyOrCutFailed<T>(Action action, string dataFormat = null, int retry = 10, int delay = 100) where T : class
		{
			var copiedData = GetValue(typeof(T), dataFormat: dataFormat) as T;

			var i = 0;
			Func<bool> shouldRetryFunc = () =>
			{
				if (copiedData is string stringData)
				{
					return string.IsNullOrEmpty(stringData);
				}

				if (copiedData is IDataObject || copiedData is DataObject)
				{
					var dataObject = copiedData as IDataObject;
					return dataObject == null || dataObject.GetFormats()?.Length == 0;
				}

				return copiedData == null;
			};

			while (shouldRetryFunc.Invoke() && i < retry)
			{
				Thread.Sleep(delay);
				action.Invoke();
				copiedData = GetValue(typeof(T), dataFormat: dataFormat) as T;
				i++;
			}

			return copiedData;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1088:Do Not Use System.Windows.Forms.Clipboard", Justification = "Testing SafeClipboard")]
		static object GetValue(Type type, bool isSafe = true, string dataFormat = null)
		{
			object result;
			if (!string.IsNullOrEmpty(dataFormat))
			{
				result = isSafe ? SafeClipboard.GetData(dataFormat) : Clipboard.GetData(dataFormat);
			}
			else if (type == typeof(IDataObject) || type == typeof(DataObject))
			{
				result = isSafe ? SafeClipboard.GetDataObject() : Clipboard.GetDataObject();
			}
			else if (type == typeof(string))
			{
				result = isSafe ? SafeClipboard.GetText() : Clipboard.GetText();
			}
			else
			{
				result = null;
			}

			return result;
		}
	}
}
