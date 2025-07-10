using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.GUI.DocBuilder;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	partial class StmMenuDocumentConfigForm : ZChildForm, IButtonPostTextOverride, IStmMenuDocumentConfigForm
	{
		public StmMenuDocumentConfigForm(TemporaryStmMenuDocumentConfig temporaryDocumentConfig, IDocumentSupportable documentSupportable)
			: base(temporaryDocumentConfig)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, okButton, cancelButton);

			sectionsSplitContainer.Panel1MinSize = 250;
			sectionsSplitContainer.Panel2MinSize = 250;

			SectionsGrid.DoubleClick += new EventHandler(SectionsGrid_DoubleClick);
			ConfigItemsGrid.DoubleClick += new EventHandler(ConfigItemsGrid_DoubleClick);

			ExcludedFromDocPackCheckBox.ReadOnly = !temporaryDocumentConfig.S3_IsSystem;
			_ = new StmMenuDocumentConfigPresenter(this, temporaryDocumentConfig, documentSupportable);
		}

		public event EventHandler TemplateSectionDoubleClicked;
		public event EventHandler ConfigItemDoubleClicked;

		internal ZButton AddButton => addButton;
		internal ZButton RemoveButton => removeButton;
		internal ZButton OkButton => okButton;
		internal ZButton PreviewButton => previewButton;
		internal ZGrid ConfigItemsGrid => configItemsGrid;
		internal ZGrid SectionsGrid => sectionsGrid;
		internal ZCheckBox ExcludedFromDocPackCheckBox => excludedFromDocPackCheckBox;

		void SectionsGrid_DoubleClick(object sender, EventArgs e)
		{
			var mouseEventArgs = e as MouseEventArgs;
			if (mouseEventArgs != null)
			{
				if (sectionsGrid.HitTest(mouseEventArgs.Location).Row >= 0)
				{
					OnTemplateSectionDoubleClicked(sender, e);
				}
			}
		}

		protected virtual void OnTemplateSectionDoubleClicked(object sender, EventArgs e)
		{
			if (TemplateSectionDoubleClicked != null)
			{
				TemplateSectionDoubleClicked(sender, e);
			}
		}

		void ConfigItemsGrid_DoubleClick(object sender, EventArgs e)
		{
			var mouseEventArgs = e as MouseEventArgs;
			if (mouseEventArgs != null)
			{
				if (configItemsGrid.HitTest(mouseEventArgs.Location).Row >= 0)
				{
					OnConfigItemDoubleClicked(sender, e);
				}
			}
		}

		protected virtual void OnConfigItemDoubleClicked(object sender, EventArgs e)
		{
			if (ConfigItemDoubleClicked != null)
			{
				ConfigItemDoubleClicked(sender, e);
			}
		}

		#region Implementation

		public new TemporaryStmMenuDocumentConfig BusinessEntity
		{
			get { return (TemporaryStmMenuDocumentConfig)base.BusinessEntity; }
		}

		public StmMenuDocumentConfig CommittedDocConfig
		{
			get { return fCommittedDocConfig; }
		}
		StmMenuDocumentConfig fCommittedDocConfig;

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			// Do not ask to save on closing.
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			fCommittedDocConfig = BusinessEntity.Commit();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		void IStmMenuDocumentConfigForm.ShowError(string message, string caption)
		{
			Globals.Message.ShowError(message, caption);
		}

		#region Preview

		void IStmMenuDocumentConfigForm.ShowPreview(PrintTask printTask)
		{
			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Destination = DeliveryInstructionDestination.DocConfigPreview;

			printTask.Run(deliveryInstructions);
		}

		event EventHandler IStmMenuDocumentConfigForm.PreviewButtonClicked
		{
			add { previewButton.Click += value; }
			remove { previewButton.Click -= value; }
		}

		#endregion

		#region Sections

		TemplateSection[] IStmMenuDocumentConfigForm.GetSelectedSections()
		{
			return GetSelectedItems<TemplateSection>(sectionsGrid);
		}

		StmMenuDocumentConfigItem[] IStmMenuDocumentConfigForm.GetSelectedConfigItems()
		{
			return GetSelectedItems<StmMenuDocumentConfigItem>(ConfigItemsGrid);
		}

		#endregion

		#region IButtonTextOverride Members

		string IButtonPostTextOverride.PostButtonText
		{
			get { return Res.GetString("StmMenuDocumentConfigForm|PostButton", "OK"); }
		}

		#endregion

		public void ShowSectionPreview(SectionPreviewManager manager)
		{
			using (var form = new SectionPreviewForm(manager))
			{
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		public event EventHandler PreviewSectionButtonClicked
		{
			add { previewSectionButton.Click += value; }
			remove { previewSectionButton.Click -= value; }
		}

		public event EventHandler PreviewConfigItemButtonClicked
		{
			add { previewConfigItemButton.Click += value; }
			remove { previewConfigItemButton.Click -= value; }
		}

		event EventHandler IStmMenuDocumentConfigForm.AddButtonClicked
		{
			add { addButton.Click += value; }
			remove { addButton.Click -= value; }
		}

		event EventHandler IStmMenuDocumentConfigForm.RemoveButtonClicked
		{
			add { removeButton.Click += value; }
			remove { removeButton.Click -= value; }
		}

		event EventHandler IStmMenuDocumentConfigForm.MoveUpButtonClicked
		{
			add { moveUpButton.Click += value; }
			remove { moveUpButton.Click -= value; }
		}

		event EventHandler IStmMenuDocumentConfigForm.MoveDownButtonClicked
		{
			add { moveDownButton.Click += value; }
			remove { moveDownButton.Click -= value; }
		}

		void IStmMenuDocumentConfigForm.SelectConfigItems(StmMenuDocumentConfigItem[] configItems)
		{
			if (configItems.Length > 0)
			{
				ConfigItemsGrid.UnSelectAll();

				int firstIndex = ConfigItemsGrid.List.IndexOf(configItems[0]);

				foreach (var configItem in configItems)
				{
					int index = ConfigItemsGrid.List.IndexOf(configItem);
					ConfigItemsGrid.Select(index);
					firstIndex = Math.Min(index, firstIndex);
				}

				ConfigItemsGrid.ListManager.Position = firstIndex;
			}
		}

		T[] GetSelectedItems<T>(ZGrid grid) where T : BusinessObject
		{
			var result = grid.GetSelectedElements<T>();

			if (result.Length == 0)
			{
				result = (grid.ListManager.Position > -1) ? new T[] { (T)grid.ListManager.GetCurrent() } : Array.Empty<T>();
			}

			return result;
		}
	}
}
