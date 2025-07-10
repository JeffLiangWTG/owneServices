using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.RichEdit;
using WinzorFramework.JSInterop;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZRichTextBox
	{
		#region Html / HtmlZBlob
		#if DEBUG
		internal
		#endif
		bool skipForTest
		{ get; set; }

		protected override bool SelectableByTabKey => false;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Html
		{
			get { return string.IsNullOrEmpty(RichEdit.Html) ? string.Empty : RichEdit.Html; }
			set
			{
				if (!IsDisposed)
				{
					var selStart = SelectionStart;
					if (string.IsNullOrEmpty(value))
					{
						RichEdit.Html = string.Empty;
					}
					else
					{
						RichEdit.Html = value;
					}
					if (selStart != -1)
					{
						SelectionStart = selStart;
					}
				}
			}
		}

		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBlob HtmlZBlob
		{
			get
			{
				//detect case where no actual changes occurred - just regeneration of html - and ignore
				//so that we don't have erroneous HasChanges
				if (CurrentHtmlBlob.IsEmpty || (Html != RichEdit.newHtmlSet || CurrentHtmlBlob.ToUTF8() != RichEdit.oldHtmlSet))
				{
					CurrentHtmlBlob = ZBlob.FromUTF8(Html);
				}
				return CurrentHtmlBlob;
			}
			set
			{
				if (!IsDisposed && !SuppressSetHtml)
				{
					Html = value.IsEmpty ? ZString.Empty : value.ToUTF8();

					CurrentHtmlBlob = value;
				}
			}
		}

		public event EventHandler HtmlZBlobChanged
		{
			add { RichEdit.TextChanged += value; }
			remove { RichEdit.TextChanged -= value; }
		}

#endregion

#region RichTextBox Sub-class

		protected internal partial class MyRichTextBox : RichTextBox
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message")]
			public new string Html
			{
				get { return base.Html; }
				set
				{
					try
					{
						//When we do this, differences in generator may cause non-visible changes to Html string content. Record change in value so we can detect
						//if any actual changes happened.
						oldHtmlSet = value;
						base.Html = value;
						newHtmlSet = base.Html;
					}
					catch (AccessViolationException ex)
					{
						ZRichTextBoxDiagnosticsCollector.Instance.ReportDeveloperErrorWithDiagnostics("AccessViolationInRichTextBoxWndProc", string.Format("There was a AccessViolationException in the setter for RichTextBox.Html which was unrecoverable: {0}", ex.Message));
						throw;
					}
					catch (Exception ex1) when (!ex1.IsCriticalException())
					{
						using (var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(value)))
						{
							base.LoadFile(stream, RichTextBoxStreamType.PlainText);
						}
					}
				}
			}

			internal string oldHtmlSet;
			internal string newHtmlSet;

			protected internal new bool ContentIsOutOfSyncWithClient => base.ContentIsOutOfSyncWithClient;

			protected override void OnMouseDown(MouseEventArgs e)
			{
				base.OnMouseDown(e);
				if (e.Button == MouseButtons.Left)
				{
					base.Focus(); // a hack borrowed from the old ZRichTextBox.WndProc
				}
			}
		}

		#endregion

		#region IDataBoundControl Members
		BindingManagerBase currencyManager;

		public void ClearUndoManager()
		{
			RichEdit.ClearUndoManager();
		}

		void OnTextContextChanged(object obj, EventArgs e)
		{
			this.ClearUndoManager();
		}
		#endregion

		#region IPastableControl Members

		public void PasteTextOnly()
		{
			InvokeRenderDispatcher(async () => await (GetJSInterop<IClipboardJSInterop>()?.PastePlainTextAsync(RichEdit.ElementReference) ?? Task.CompletedTask));
		}

		void RichTextBox_ImagesInserted(object sender, string[] images)
		{
			PasteDataObject(new DataObject(DataFormats.FileDrop, images));
		}

		#endregion

		#region Implementation
		protected override void OnValidating(CancelEventArgs e)
		{
			RichEdit.TryUpdateValueFromClient();
			this.SuppressSetHtml = true;
			base.OnValidating(e);

			if (e.Cancel)
			{
				this.SuppressSetHtml = false;
			}
		}

		protected override void OnValidated(EventArgs e)
		{
			try
			{
				this.SuppressSetHtml = false;
				base.OnValidated(e);
			}
			catch (InvalidOperationException)
			{
				// this sometimes happens while opening unusual attachments
			}
		}

		[DefaultValue("")]
		public string SelectedHtml
		{
			get { return RichEdit.SelectedHtml; }
			set { RichEdit.SelectedHtml = value; }
		}

		protected bool SuppressSetHtml
		{
			get
			{
				return suppressSetHtml;
			}
			set
			{
				suppressSetHtml = value;
			}
		}
		bool suppressSetHtml;

		ZBlob CurrentHtmlBlob = ZBlob.Empty;
		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZRichTextBox>()
				.Property("HtmlZBlob", ZBlob.Empty)
				.Property("ReadOnly", false, false)
				.Property("ReadOnlyForBinding", false, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Result;
		}

		#endregion
	}
}
