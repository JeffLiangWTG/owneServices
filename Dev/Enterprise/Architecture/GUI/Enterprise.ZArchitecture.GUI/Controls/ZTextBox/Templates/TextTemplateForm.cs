using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	[TestExcludeZWinFormHasTypedConstructor]
	public partial class TextTemplateForm : ZChildForm
	{
		public TextTemplateForm()
		{
			InitializeComponent();
		}

		public TextTemplateForm(StmNoteTemplate template, BusinessObject[] context, bool multiLine, bool shouldEscapeAllSpecialCharacters = false)
			: base(template)
		{
			this.context = context;

			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			templateTextBox.LostFocus += new EventHandler(templateTextBox_LostFocus);
			templateTextBox.Multiline = multiLine;
			ShouldEscapeAllSpecialCharacters = shouldEscapeAllSpecialCharacters;
		}

		readonly BusinessObject[] context;
		internal TextTemplatePreviewForm previewForm;
		bool ShouldEscapeAllSpecialCharacters { get; }

		public bool HideMacroFields { get; set; }

		public bool HideDataFields { get; set; }

		public bool ShowXmlFields { get; set; }

		public string OpeningMacroTag { get; set; }

		public string ClosingMacroTag { get; set; }

		public int DefaultCollectionIndex { get; set; } = 1;

		public Type XmlType { get; set; }

		public bool UseMcrEvaluator { get; set; }

#if DEBUG
		internal
#endif
 IMapTreePresentationManager mapTreePresenter;

		StmNoteTemplate Template
		{
			get { return (StmNoteTemplate)BusinessEntity; }
		}

		void deleteButton_Click(object sender, EventArgs e)
		{
			DisplayMode = ODisplayMode.Delete;
			this.OnApplyButtonClick(sender, e); //Save (Delete) via normal process
		}

		void insertMacroButton_Click(object sender, EventArgs e)
		{
			if (mapTreePresenter == null)
			{
				mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>();
				mapTreePresenter.ParentTypes = GetMapTreeParentTypes();
				mapTreePresenter.MacroSelected += new MacroSelectedEventHandler(mapTreePresenter_MacroSelected);
				mapTreePresenter.ShowEditField = AllowMacroEditing;

				if (OpeningMacroTag != null)
				{
					mapTreePresenter.OpeningMacroTag = OpeningMacroTag;
				}

				if (ClosingMacroTag != null)
				{
					mapTreePresenter.ClosingMacroTag = ClosingMacroTag;
				}

				mapTreePresenter.HideDataFields = HideDataFields;
				mapTreePresenter.HideMacroFields = HideMacroFields;
				mapTreePresenter.ShowXmlFields = ShowXmlFields;
				mapTreePresenter.UseMcrEvaluator = UseMcrEvaluator;
				mapTreePresenter.DefaultCollectionIndex = DefaultCollectionIndex;
				mapTreePresenter.XmlType = XmlType;
			}
			if (UseMcrEvaluator)
			{
				var scope = new List<MacroScope>();
				foreach (var bizo in context)
				{
					scope.Add(new MacroScope(bizo));
				}
				mapTreePresenter.ParentBusinessObjects = scope.ToArray();
			}
			else
			{
				mapTreePresenter.ParentBusinessObjects = context;
			}
			mapTreePresenter.ParentTypes = GetMapTreeParentTypes();
			mapTreePresenter.ShowPresentationManagerForm(shouldEscapeAllSpecialCharacters: ShouldEscapeAllSpecialCharacters);
		}

		protected virtual Type[] GetMapTreeParentTypes()
		{
			var types = context.Select(x => x.GetType()).Distinct();
			foreach (var bizO in context)
			{
				if (bizO is IRootTypeProvider)
				{
					types = types.Union(((IRootTypeProvider)bizO).RootTypes);
				}
			}
			return types.ToArray();
		}

		protected virtual bool AllowMacroEditing
		{
			get { return false; }
		}

		void mapTreePresenter_MacroSelected(string macro)
		{
			templateTextBox.Focus();
			templateTextBox.SelectionStart = lastTemplateTextBoxSelectionStart;
			templateTextBox.SelectionLength = lastTemplateTextBoxSelectionLength;
			templateTextBox.SelectedText = macro;
		}

		void templateTextBox_LostFocus(object sender, EventArgs e)
		{
			lastTemplateTextBoxSelectionStart = templateTextBox.SelectionStart;
			lastTemplateTextBoxSelectionLength = templateTextBox.SelectionLength;
		}
		protected int lastTemplateTextBoxSelectionStart;
		protected int lastTemplateTextBoxSelectionLength;

		void previewButton_Click(object sender, EventArgs e)
		{
			if (previewForm == null || previewForm.IsDisposed)
			{
				previewForm = new TextTemplatePreviewForm();
			}
			previewForm.PreviewText = ObjectFactory.Get<ITextMacroProcessor>().Replace(Template.S8_TemplateText, context, shouldEscapeAllSpecialCharacters: ShouldEscapeAllSpecialCharacters);
			previewForm.Show();
			previewForm.Focus();
		}

		protected override void ShowNewForm()
		{
			this.Visible = false;
			var newTemplate = new BusinessObjectFactory().New<StmNoteTemplate>();
			newTemplate.S8_ContextID = Template.S8_ContextID;
			using (var form = new TextTemplateForm(newTemplate, context, templateTextBox.Multiline))
			{
				form.ShowDialog();
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.S | Keys.Control))
			{
				ZFormUtilities.EnsureSelectedControlValueCommitted(this);
				FireSaveButton();
			}
			else if (keyData == (Keys.S | Keys.Control | Keys.Shift))
			{
				ZFormUtilities.EnsureSelectedControlValueCommitted(this);
				postingButtonsUserControl.SaveAndCloseButton.PerformClick();
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

#if DEBUG

		public void ClickPreviewButton_ForTest() => previewButton.PerformClick();

		public string PreviewFormTextBoxText_ForTest => previewForm.textBox.Text;

#endif
	}
}
