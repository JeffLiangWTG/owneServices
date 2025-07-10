using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI.RichEdit
{
	public delegate void ZRichTextBoxPopupFormClosed(ZRichTextBoxPopupForm form);

	public class RichTextPopupEventArgs : EventArgs
	{
		public ZRichTextBoxPopupForm Form { get; }

		public RichTextPopupEventArgs(ZRichTextBoxPopupForm form)
		{
			Form = Argument.NotNull(form, nameof(form));
		}
	}

	/// <summary>
	/// An external form for editing the content on an ZRichTextBox in a larger size.
	/// </summary>
	public partial class ZRichTextBoxPopupForm : ZChildForm
	{
		readonly System.ComponentModel.Container components;

		public event ZRichTextBoxPopupFormClosed FinishedEditing;

		public ZRichTextBoxPopupForm(IDataBoundControl parent, ResourceStringData caption = null)
		{
			InitializeComponent();
			RichEdit.IsPopupButtonVisible = false;

			if (caption != null)
			{
				CaptionResourceString = caption;
			}

			if (parent != null)
			{
				if (RichEdit.contextMenuManager != null)
				{
					RichEdit.contextMenuManager.Dispose();
				}
				RichEdit.contextMenuManager = new ZRichTextBoxContextMenuManager(parent, RichEdit.RichEdit, new NoMacroBox());
				var richTextBox = parent as ZRichTextBox;
				if (richTextBox != null)
				{
					RichEdit.ParentZForm = richTextBox.ParentZForm;

					if (richTextBox.ReadOnlyCascadeToPopup)
					{
						RichEdit.ReadOnly = richTextBox.ReadOnly;
					}
				}
			}
		}
#if !WINZOR
		public string Rtf
		{
			get { return RichEdit.Rtf; }
			set { RichEdit.Rtf = value; }
		}

		public int SelectionStart
		{
			get { return RichEdit.SelectionStart; }
			set { RichEdit.SelectionStart = value; }
		}

		public int SelectionLength
		{
			get { return RichEdit.SelectionLength; }
			set { RichEdit.SelectionLength = value; }
		}
#else
		public string Html
		{
			get { return RichEdit.Html; }
			set { RichEdit.Html = value; }
		}
#endif

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			CloseForm();
		}

		void ZRichTextBoxPopupForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			CloseForm();
		}

		void CloseForm()
		{
			if (FinishedEditing != null)
			{
				FinishedEditing(this);
			}
			if (!closing)
			{
				closing = true;
				Close();
			}
			closing = false;
		}
		bool closing;

		#endregion
	}
}
