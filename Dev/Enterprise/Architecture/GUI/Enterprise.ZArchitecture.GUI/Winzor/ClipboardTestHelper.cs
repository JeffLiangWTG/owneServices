#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Windows.Forms;
using AngleSharp.Common;
using CargoWise.Application;
using Microsoft.JSInterop;
using Moq;
using WinzorFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ClipboardTestHelper
	{
		public IDataObject ClipboardData
		{
			get
			{
				var dataObject = new DataObject();
				foreach (var item in ClipboardItems)
				{
					var format = item.Key;
					if (format == MediaTypeNames.Text.Plain)
					{
						format = DataFormats.Text;
					}
					else if (format == MediaTypeNames.Text.Html)
					{
						format = DataFormats.Html;
					}
					dataObject.SetData(format, item.Value);
				}
				return dataObject;
			}
		}

		public Dictionary<string, string> ClipboardItems { get; private set; } = new Dictionary<string, string>();

		public IDisposable MockClipboard()
		{
			var mockSafeClipboardHelper = new Mock<SafeClipboardHelper>();
			mockSafeClipboardHelper.Setup(x => x.GetDataObjectDict()).Returns(ClipboardItems);

			var clipboardItemsToGet = new Dictionary<string, ClipboardContent>();
			var mockJsRuntime = new Mock<IJSRuntime>();
			var mockModule = new Mock<IJSObjectReference>();
			mockModule.Setup(i => i.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>("clipboard.setDataObject", It.IsAny<object[]>()))
			.Callback((string s, object[] data) =>
			{
				var dict = data.Length > 1 ? data[1] as Dictionary<string, object> : null;
				ClipboardItems = dict?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as string ?? string.Empty);
				mockSafeClipboardHelper.Setup(x => x.GetDataObjectDict()).Returns(ClipboardItems);
				clipboardItemsToGet.Clear();
				foreach (var kvp in ClipboardItems)
				{
					clipboardItemsToGet.Add(kvp.Key, new ClipboardContent(kvp.Value, null));
				}
			});

			mockModule.Setup(i => i.InvokeAsync<Dictionary<string, ClipboardContent>>("clipboard.getDataObject", It.IsAny<object[]>())).ReturnsAsync(clipboardItemsToGet);

			mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { "/_content/WinzorFramework/js/module/clipboard.js" })).ReturnsAsync(mockModule.Object);

			var cwcs = WinzorDispatcher.Current.CurrentContext.Form?.CargoWiseClientServices;
			if (cwcs is not null)
			{
				cwcs.JSRuntime = mockJsRuntime.Object;
			}

			return ObjectFactory.Substitute(mockSafeClipboardHelper.Object);
		}

		public static T RetryIfCopyOrCutFailed<T>(Action action, string dataFormat = null, int retry = 10, int delay = 100) where T : class => null;
	}
}

#endif
