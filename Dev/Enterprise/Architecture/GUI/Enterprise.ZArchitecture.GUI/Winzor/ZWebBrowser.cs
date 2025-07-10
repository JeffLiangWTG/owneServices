using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.JSInterop;
using WinzorFramework;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZWebBrowser : Control
	{
		public ZWebBrowser()
		{
			InitializeComponent();
		}

		protected override EventAttribute EventAttributes => base.EventAttributes | EventAttribute.ContextMenu;

		public bool AllowWebBrowserDrop { get; set; }

		public bool ScriptErrorsSuppressed { get; set; }

		public WebBrowserReadyState ReadyState { get; }

		public Uri Url { get; set; }

		public string DocumentText { get; set; }

		public HtmlDocument Document { get; }
		public bool IsBusy { get; set; }
		public virtual void Navigate(string urlString)
		{ }
		public class HtmlDocument
		{
			public HtmlDocument OpenNew(bool replaceInHistory) => null;

			public void Write(string text)
			{
			}
			public HtmlElementCollection All { get; }
		}

		public bool IsWebBrowserContextMenuEnabled { get; set; }

		public bool WebBrowserShortcutsEnabled { get; set; }

		public event WebBrowserNavigatedEventHandler Navigated;

		public event WebBrowserDocumentCompletedEventHandler DocumentCompleted;

		protected override Size DefaultSize => new (250, 250);

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			GetJSInterop<IWebBrowserJSInterop>()?.PreloadInterop();
		}

		public async Task UpdateAnchorAttributesAsync()
		{
			await (GetJSInterop<IWebBrowserJSInterop>()?.UpdateAnchorAttributesAsync() ?? Task.CompletedTask);
		}

		public async Task ResetScrollPosition()
		{
			await (GetJSInterop<IWebBrowserJSInterop>()?.ResetScrollPositionAsync() ?? Task.CompletedTask);
		}

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZWebBrowser>()
				.Result;
		}

		#endregion

		void CopyMenuItem_Click(object sender, EventArgs e)
		{
		}

		void SelectAllMenuItem_Click(object sender, EventArgs e)
		{
		}
	}
}
