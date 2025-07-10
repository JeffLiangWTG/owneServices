using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Microsoft.JSInterop;

namespace Enterprise.ZArchitecture.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1088:Do Not Use System.Windows.Forms.Clipboard", Justification = "This is the SafeClipboard class that this analyzer recommends")]
	public static class SafeClipboard
	{
#if DEBUG
#nullable enable
		public static string GetText() => (string)GetData(DataFormats.Text);

		public static object GetData(string format)
		{
			if (string.IsNullOrWhiteSpace(format))
			{
				return null!;
			}

			IDataObject? dataObject = GetServerDataObject();
			if (dataObject is not null)
			{
				return dataObject.GetData(format);
			}

			return null!;
		}

		public static IDataObject GetDataObject() => GetServerDataObject();
#nullable restore
#endif
		public static IDataObject GetServerDataObject() => Clipboard.GetServerDataObject();

		public static Dictionary<string, string> GetDataObjectDict() => Clipboard.GetDataObjectDict();

		public static async Task FetchClipboardDataAsync(IJSRuntime jSRuntime) => await Clipboard.FetchClipboardDataAsync(jSRuntime);

		public static bool SetDataObject(object data) => SetDataObject(data, false);

		public static bool SetDataObject(object data, bool copy)
		{
			Clipboard.SetDataObject(data, copy);
			return true;
		}

		public static bool SetData(string format, object data)
		{
			IDataObject obj2 = new DataObject();
			obj2.SetData(format, data);
			return SetDataObject(obj2, true);
		}

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

		public static void Clear() => Clipboard.Clear();

		public static bool Debounce() => false;

		public static string ClipboardNotAccessibleWarning => Res.GetString("d234e238-8210-40f8-8a93-561cf09f7678", "Windows Clipboard is not accessible at the moment. Try to repeat this operation later.");
	}
}
