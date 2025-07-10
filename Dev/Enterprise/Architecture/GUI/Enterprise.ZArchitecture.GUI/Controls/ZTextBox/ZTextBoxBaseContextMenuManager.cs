using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZTextBoxBaseContextMenuManager : Disposable
	{
		protected ZTextBoxBaseContextMenuManager(TextBoxBase textBox, IMacroBox macroBox)
		{
			if (textBox.IsDisposed)
			{
				throw new ArgumentException("cannot access to textBox if it has already disposed.");
			}
			this.textBox = textBox;
			this.MacroBox = macroBox;
			textBox.MouseDown += new MouseEventHandler(textBox_MouseDown);
			textBox.KeyDown += new KeyEventHandler(textBox_KeyDown);
		}

		public ZTextBoxBaseContextMenuManager(IDataBoundControl dataBoundControl, TextBoxBase textBox, IMacroBox macroBox)
			: this(textBox, macroBox)
		{
			this.textTemplatesFactory = new TextTemplatesFactory(dataBoundControl);
		}

		public ZTextBoxBaseContextMenuManager(CurrencyManager dataSource, string mappingName, TextBoxBase textBox, IMacroBox macroBox)
			: this(textBox, macroBox)
		{
			this.textTemplatesFactory = new TextTemplatesFactory(dataSource, mappingName, textBox);
		}

		internal IMacroBox MacroBox { get; set; }

		void textBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				InitializeContextMenu();
			}
		}

		internal void InitializeContextMenu()
		{
			if (textBox.ContextMenuStrip != null)
			{
				return;
			}
			textBox.ContextMenuStrip = new ContextMenuStrip();
			textBox.ContextMenuStrip.Opening += new CancelEventHandler(ContextMenuStrip_Opening);
			textBox.ContextMenuStrip.Opened += ContextMenuStripOpened;

			textBox.ContextMenuStrip.Items.Add(templatesItem = new ZToolStripMenuItem(Res.GetData("9A4A1013-09C4-4b91-829F-D9FCC13BD8C5", "Insert Template")));
			templatesItem.DropDown.Opening += new CancelEventHandler(TemplatesItem_Opening);
			templatesItem.DropDown.Items.Add(createTemplateItem = new ZToolStripMenuItem(Res.GetData("5F71AC5F-C856-4957-8AF4-1FDEA288DAD9", "Create Template"), new EventHandler(CreateTemplate_Click)));
			templatesItem.DropDown.Items.Add(manageTemplatesItem = new ZToolStripMenuItem(Res.GetData("750E5054-C668-4e95-A6FE-25B918397B7B", "Manage Template")));
			manageTemplatesItem.DropDown.Items.Add("-");
			manageTemplatesItem.DropDown.Opening += new CancelEventHandler(ManageTemplatesItem_Opening);
			templatesSeperator = textBox.ContextMenuStrip.Items.Add("-");

			textBox.ContextMenuStrip.Items.Add(insertMacroItem = new ZToolStripMenuItem(
				MacroBox.InsertMacroCaption ?? Res.GetData("ad0b78b8-88b4-4ead-a6f0-be8fcbe7228a", "Insert Macro"), MacroBox.InsertMacroHandler));
			textBox.ContextMenuStrip.Items.Add(previewMacroItem = new ZToolStripMenuItem(Res.GetData("0261f0f4-c9b2-4e9a-876e-1cbc5f668b1b", "Preview"), MacroBox.PreviewMacroHandler));
			insertMacroSeparator = textBox.ContextMenuStrip.Items.Add("-");

			textBox.ContextMenuStrip.Items.Add(findItem = new ZToolStripMenuItem(Res.GetData("CFC7E78E-1435-4F38-9E61-DA1E6C95B121", "Find"), new EventHandler(Find_Click)));
			textBox.ContextMenuStrip.Items.Add(undoItem = new ZToolStripMenuItem(Res.GetData("5B525041-F327-4f5d-A1C4-DAE675E6ADE6", "Undo"), new EventHandler(Undo_Click)));
			textBox.ContextMenuStrip.Items.Add("-");
			textBox.ContextMenuStrip.Items.Add(cutItem = new ZToolStripMenuItem(Res.GetData("7DB0B61C-400F-4b77-9876-EB592D1DDAE7", "Cut"), new EventHandler(Cut_Click)));
			textBox.ContextMenuStrip.Items.Add(copyItem = new ZToolStripMenuItem(Res.GetData("C121339D-486D-459a-ACE7-4F3CC3EA962B", "Copy"), new EventHandler(Copy_Click)));
			textBox.ContextMenuStrip.Items.Add(pasteItem = new ZToolStripMenuItem(Res.GetData("B081DF66-7BE4-4cdc-9B14-35AA6F0B5CEF", "Paste"), new EventHandler(Paste_Click)));
			textBox.ContextMenuStrip.Items.Add(deleteItem = new ZToolStripMenuItem(Res.GetData("F97A1DEF-E72F-4ca2-8326-BADCEA879677", "Delete"), new EventHandler(Delete_Click)));
			textBox.ContextMenuStrip.Items.Add("-");
			textBox.ContextMenuStrip.Items.Add(new ZToolStripMenuItem(Res.GetData("31382C1A-B55E-48d1-A9FE-E5176A9B8369", "Select All"), new EventHandler(SelectAll_Click)));
			AddExtraMenuItems();
		}

		protected virtual void AddExtraMenuItems()
		{
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (textBox.ContextMenuStrip != null)
				{
					textBox.ContextMenuStrip.Dispose();
				}
				textBox.MouseDown -= new MouseEventHandler(textBox_MouseDown);
				textBox.KeyDown -= new KeyEventHandler(textBox_KeyDown);
			}
		}

		public event EventHandler ContextMenuStripOpened;

		internal readonly TextBoxBase textBox;
		readonly TextTemplatesFactory textTemplatesFactory;

		internal TextTemplatesFactory TextTemplatesFactory
		{
			get { return textTemplatesFactory; }
		}

		protected ToolStripMenuItem templatesItem;
		ToolStripMenuItem createTemplateItem;
		ToolStripMenuItem manageTemplatesItem;
		ToolStripItem templatesSeperator;

		ToolStripMenuItem insertMacroItem;
		ToolStripMenuItem previewMacroItem;
		ToolStripItem insertMacroSeparator;

		ToolStripMenuItem undoItem;
		ToolStripMenuItem cutItem;
		ToolStripMenuItem copyItem;
		protected ToolStripMenuItem pasteItem;
		ToolStripMenuItem deleteItem;
		ToolStripMenuItem findItem;

		internal virtual void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			ContextMenuManagerContextMenuStripOpening?.Invoke(sender, e);

			templatesItem.Visible = templatesSeperator.Visible = textTemplatesFactory.TemplatesSupported && textBox.MaxLength >= 20;
			var isPassword = textBox as TextBox != null && (textBox as TextBox).PasswordChar != '\0';
			templatesItem.Enabled = !textBox.ReadOnly && !isPassword;

			insertMacroItem.Visible = MacroBox.InsertMacroHandler != null;
			insertMacroItem.Enabled = !textBox.ReadOnly;
			previewMacroItem.Visible = MacroBox.PreviewMacroHandler != null;
			insertMacroSeparator.Visible = MacroBox.InsertMacroHandler != null || MacroBox.PreviewMacroHandler != null;

			undoItem.Enabled = textBox.CanUndo && !textBox.ReadOnly;
			cutItem.Enabled = !textBox.ReadOnly && textBox.SelectionLength > 0;
			copyItem.Enabled = textBox.SelectionLength > 0;
			pasteItem.Enabled = !textBox.ReadOnly; //&& SafeClipboard.GetDataObject() != null;
			deleteItem.Enabled = !textBox.ReadOnly && textBox.SelectionLength > 0;

			findItem.Enabled = findItem.Visible = textBox is ZTextBox zTextBox && zTextBox.EnableFindDialog;
		}
		internal event EventHandler ContextMenuManagerContextMenuStripOpening;

		void Find_Click(object sender, EventArgs e)
		{
			if (textBox is ZTextBox zTextBox)
			{
				zTextBox.ShowFindDialog();
			}
		}

		void Undo_Click(object sender, EventArgs e)
		{
			if (textBox is ZAutoCompleteTextBox autoCompleteTextBox)
			{
				autoCompleteTextBox.Undo();
				return;
			}

			textBox.Undo();
		}

		void Cut_Click(object sender, EventArgs e)
		{
			textBox.Cut();
		}

		void Copy_Click(object sender, EventArgs e)
		{
			textBox.Copy();
		}

		void Paste_Click(object sender, EventArgs e)
		{
			textBox.Paste();
		}

		void Delete_Click(object sender, EventArgs e)
		{
			textBox.SelectedText = "";
		}

		void SelectAll_Click(object sender, EventArgs e)
		{
			textBox.SelectAll();
		}

		void TemplatesItem_Opening(object sender, CancelEventArgs e)
		{
			try
			{
				templatesItem.DropDown.SuspendLayout();

				while (templatesItem.DropDown.Items[0] != createTemplateItem)
				{
					templatesItem.DropDown.Items.RemoveAt(0);
				}
				var templates = TextTemplatesFactory.GetAllTemplatesForControl();
				var editableTemplates = false;
				if (templates != null && templates.Length > 0)
				{
					var textMacroProcessor = ObjectFactory.Get<ITextMacroProcessor>();
					var position = 0;
					var securityProvider = new StmNoteTemplateSecurityProvider();

					foreach (var template in templates)
					{
						var escapedDescription = template.S8_Description.Replace(@"\\", EscapeCharacters);
						var splitDescription = escapedDescription.Split(@"\");
						var leafMenuItemDescription = splitDescription[splitDescription.Length - 1].Replace(EscapeCharacters, @"\");
						var leafMenuItem = CreateMenuItem(
							template,
							leafMenuItemDescription,
							new EventHandler(InsertTemplateItem_Click),
							textMacroProcessor.Replace(
								template.S8_TemplateText, TextTemplatesFactory.ContextBusinessObjects,
								shouldEscapeAllSpecialCharacters: MacroBox.ShouldEscapeAllSpecialCharacters
							));
						AddMenuItemToSubMenu(leafMenuItem, templatesItem.DropDown.Items, splitDescription, ref position);
						if (!editableTemplates && securityProvider.IsTemplateEditableByUser(template))
						{
							editableTemplates = true;
						}
					}
					templatesItem.DropDown.Items.Insert(position++, new ToolStripSeparator());
				}
				createTemplateItem.Enabled = templates != null;
				manageTemplatesItem.Enabled = editableTemplates;
			}
			finally
			{
				templatesItem.DropDown.ResumeLayout();
				templatesItem.DropDown.PerformLayout();
			}
		}

		ZToolStripMenuItem CreateMenuItem(StmNoteTemplate template, string description, EventHandler onClick, string toolTipText)
		{
			var item = new ZToolStripMenuItem(description, onClick);
			item.Tag = template;
			if (MacroBox.IsMacroControl)
			{
				var expressionTemplate = ConvertToExpressionNoteTemplate(template);
				item.ToolTipText = expressionTemplate.TemplateText;
			}
			else
			{
				item.ToolTipText = toolTipText;
			}
			return item;
		}

	void AddMenuItemToSubMenu(ZToolStripMenuItem leafMenuItem, ToolStripItemCollection rootMenuCollection, ZString[] splitDescription, ref int position)
		{
			var menuCollection = rootMenuCollection;
			for (int i = 0; i < splitDescription.Length - 1; i++)
			{
				if (!string.IsNullOrWhiteSpace(splitDescription[i]))
				{
					ZToolStripMenuItem subMenu = menuCollection.Cast<ZToolStripMenuItem>().FirstOrDefault(mi => mi.Text == splitDescription[i].Replace(EscapeCharacters, @"\"));
					if (subMenu == null)
					{
						subMenu = new ZToolStripMenuItem(splitDescription[i].Replace(EscapeCharacters, @"\"));
						AddMenuItemToCollection(subMenu, rootMenuCollection, menuCollection, ref position);
					}
					menuCollection = subMenu.DropDownItems;
				}
			}

			if (menuCollection.Cast<ZToolStripMenuItem>().All(mi => mi.Text != leafMenuItem.Text))
			{
				AddMenuItemToCollection(leafMenuItem, rootMenuCollection, menuCollection, ref position);
			}
		}

		void AddMenuItemToCollection(ZToolStripMenuItem menuItem,
			ToolStripItemCollection rootCollection,
			ToolStripItemCollection currentCollection,
			ref int position)
		{
			if (currentCollection == rootCollection)
			{
				currentCollection.Insert(position++, menuItem);
			}
			else
			{
				currentCollection.Add(menuItem);
			}
		}

		void CreateTemplate_Click(object sender, EventArgs e)
		{
			var template = TextTemplatesFactory.New();
			template.S8_TemplateText = textBox.Text;
			using (var form = GetTemplateForm(template))
			{
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		void ManageTemplatesItem_Opening(object sender, CancelEventArgs e)
		{
			var securityProvider = new StmNoteTemplateSecurityProvider();
			manageTemplatesItem.DropDown.Items.Clear();
			var position = 0;
			foreach (var template in TextTemplatesFactory.GetAllTemplatesForControl())
			{
				if (securityProvider.IsTemplateEditableByUser(template))
				{
					var escapedDescription = template.S8_Description.Replace(@"\\", EscapeCharacters);
					var splitDescription = escapedDescription.Split(@"\");
					var leafMenuItemDescription = splitDescription[splitDescription.Length - 1].Replace(EscapeCharacters, @"\");
					var leafMenuItem = CreateMenuItem(
						template,
						leafMenuItemDescription,
						new EventHandler(ManageTemplateItem_Click),
						template.S8_TemplateText);
					AddMenuItemToSubMenu(leafMenuItem, manageTemplatesItem.DropDown.Items, splitDescription, ref position);
				}
			}
		}

		internal void textBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F4 && e.Modifiers == Keys.None && !textBox.ReadOnly)
			{
				InitializeContextMenu();
				if (textTemplatesFactory.TemplatesSupported && textBox.MaxLength >= 20)
				{
					if (templatesItem.DropDown == null)
					{
						throw new InvalidOperationException("templatesItem.DropDown should not be null for textBox_KeyDown.");
					}
					templatesItem.DropDown.OwnerItem = null;
					templatesItem.DropDown.Show(textBox, textBox.GetPositionFromCharIndex(textBox.SelectionStart + textBox.SelectionLength));
					templatesItem.Invalidate();
				}
			}
		}

		public event EventHandler InsertingTemplateText;

		void InsertTemplateItem_Click(object sender, EventArgs e)
		{
			if (InsertingTemplateText != null)
			{
				InsertingTemplateText(this, EventArgs.Empty);
			}

			var template = (StmNoteTemplate)((ToolStripMenuItem)sender).Tag;

			if (template == null)
			{
				ErrorReporter.ReportOnce("The .Tag of object sender is null. \n It should not be, as if it was before, it would have blown up in TemplatesItem_Opening  or ManageTemplatesItem_Opening).");// Developer Notification.
				return;
			}

			string replacement;
			if (MacroBox.IsMacroControl)
			{
				var expressionTemplate = ConvertToExpressionNoteTemplate(template);

				if (expressionTemplate == null)
				{
					ErrorReporter.ReportOnce("expressionTemplate is null. This would indicate that the expressionTemplate PK has not been found in the Factory."); // Developer Notification.
					return;
				}

				if (expressionTemplate.Placeholders.Count > 0)
				{
					using (var form = new PlaceholderReplacementForm(expressionTemplate))
					{
						ZFormModaliser.ShowDialogAndDispose(form);
					}
					replacement = expressionTemplate.Replacement;
				}
				else
				{
					replacement = expressionTemplate.TemplateText;
				}
			}
			else
			{
				replacement = ObjectFactory.Get<ITextMacroProcessor>().Replace(template.S8_TemplateText, TextTemplatesFactory.ContextBusinessObjects, shouldEscapeAllSpecialCharacters: MacroBox.ShouldEscapeAllSpecialCharacters);
			}

			var maxLengthAllowed = textBox.SelectionLength + (textBox.MaxLength - textBox.Text.Length);
			if (replacement.Length > maxLengthAllowed)
			{
				var result = UserNotificationProvider.Show(Res.GetString("2bf67c05-70c6-4c75-9fe3-122b00a6482d", "The text you are attempting to insert is too long to fit in this field (maximum {0} characters).\r\nDo you want to truncate the text to fit?", textBox.MaxLength), Res.GetString("0ce7768d-7722-46de-b5f0-e4e4a6513a7e", "Maximum field length exceeded"), MessageBoxButtons.YesNo, DialogResult.No);
				if (result == DialogResult.Yes)
				{
					replacement = replacement.Substring(0, maxLengthAllowed);
				}
				else
				{
					return;
				}
			}

			textBox.Focus();
			textBox.SelectedText = replacement;

			if (MacroBox.IsMacroControl)
			{
				var expressionDescription = TextTemplatesFactory.ContextBusinessObjects[0] as IHaveExpressionDescription;
				expressionDescription?.SetExpressionDescription(template.S8_Description);
			}
		}

		void ManageTemplateItem_Click(object sender, EventArgs e)
		{
			using (var form = GetTemplateForm(TextTemplatesFactory.Edit((StmNoteTemplate)((ToolStripMenuItem)sender).Tag)))
			{
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		internal IUserNotification UserNotificationProvider
		{
			get { return userNotificationProvider ?? Globals.Message; }
			set { userNotificationProvider = value; }
		}
		IUserNotification userNotificationProvider;

		internal TextTemplateForm GetTemplateForm(StmNoteTemplate template)
		{
			var templateForm = MacroBox.IsMacroControl && textBox.Parent is ZMacrosFindBox macroFindBox
				? new ExpressionTemplateForm(ConvertToExpressionNoteTemplate(template), TextTemplatesFactory.ContextBusinessObjects, textBox.Multiline, macroFindBox.RootTypes, MacroBox.ShouldEscapeAllSpecialCharacters)
				: new TextTemplateForm(template, TextTemplatesFactory.ContextBusinessObjects, textBox.Multiline, MacroBox.ShouldEscapeAllSpecialCharacters);

			templateForm.OpeningMacroTag = MacroBox.MacroOpeningBracket;
			templateForm.ClosingMacroTag = MacroBox.MacroClosingBracket;
			templateForm.HideMacroFields = MacroBox.HideMacroFields;
			templateForm.HideDataFields = MacroBox.HideDataFields;
			templateForm.XmlType = MacroBox.XmlType;
			templateForm.UseMcrEvaluator = MacroBox.UseMcrEvaluator;
			templateForm.DefaultCollectionIndex = MacroBox.DefaultCollectionIndex;

			return templateForm;
		}

		ExpressionNoteTemplate ConvertToExpressionNoteTemplate(StmNoteTemplate template)
		{
			return template.Factory.Load<ExpressionNoteTemplate>(template.PK);
		}

#if DEBUG

		public bool GetTextTemplateEnabled_ForTest()
		{
			return templatesItem.Enabled;
		}

#endif
		const string EscapeCharacters = "@#$%^";
	}
}
