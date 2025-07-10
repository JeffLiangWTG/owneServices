using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMBoardForm : ZTemplateForm, IFilterPreviewableWithSubObject
	{
		public BMBoardForm()
		{
			InitializeComponent();
		}

		public BMBoardForm(BMBoard bmBoard)
			: base(bmBoard)
		{
			InitializeComponent();

			foreach (var section in bmBoard.Sections)
			{
				if (section.MS_SectionType == BMConstants.ComponentSectionType)
				{
					DefaultChannelsProvider.RefreshChannels(section.SectionConfiguration, section.Factory);
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdatePreviews();
			BoardSectionConfigControl.SectionConfigChanged += BoardSectionConfigControl_SectionConfigChanged;
		}

		public new BMBoard DataSource
		{
			get { return (BMBoard)base.DataSource; }
		}

		void BoardSectionConfigControl_SectionConfigChanged(object sender, EventArgs e)
		{
			UpdatePreviews();
		}

		void PreviewTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!DataSource.Sections.Any(s => s.HasErrors()))
			{
				UpdatePreviews();
			}
		}

#if DEBUG
		public virtual
#endif

		void UpdatePreviews()
		{
			if (!IsPreviewDisabled)
			{
				if (PreviewTabControl.SelectedTab == SectionPreviewTabPage)
				{
					var selectedItem = BoardSectionConfigControl.GetSelectedSection();
					BoardSectionPreviewControl.UpdatePreview(selectedItem != null ? new[] { selectedItem } : null);
				}
				else
				{
					if (DataSource != null)
					{
						BoardPreviewControl.UpdatePreview(DataSource.Sections);
					}
				}
			}
		}

		public bool IsPreviewDisabled => activePreviewDisablerCount > 0;

		int activePreviewDisablerCount;

		public IDisposable DisablePreviewUpdating()
		{
			return new PreviewDisabler(this);
		}

		class PreviewDisabler : IDisposable
		{
			readonly BMBoardForm form;
			bool isDisposed;

			public PreviewDisabler(BMBoardForm form)
			{
				this.form = form;
				form.activePreviewDisablerCount++;
			}

			public void Dispose()
			{
				if (!isDisposed)
				{
					form.activePreviewDisablerCount--;
					isDisposed = true;
				}
			}
		}

		protected override bool SupportsEDocs => true;

		protected override bool ShowAuditTab => true;

		void ViewVisualBoardButton_Click(object sender, EventArgs e)
		{
			var viewBoardButton = (ZButton)sender;
			var board = DataSource;
			if ((board.HasChanges || !board.IsInDatabase) && DisplayMode != ODisplayMode.ReadOnly)
			{
				var message = Res.GetString("e05ecf3c-c81a-40f9-8b75-9e0c88b77938", "You must save this form before trying to preview the Visual Board. Would you like to save now?");
				var caption = Res.GetString("a0082606-1e18-40a8-a797-1e5025252cf6", "Save changes?");

				var result = Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, DialogResult.OK);
				if (result == DialogResult.OK)
				{
					var saveResult = FireSaveButton();
					if (saveResult == ContinueWithSave.Yes)
					{
						ShowBoard(board, viewBoardButton);
					}
				}
			}
			else
			{
				ShowBoard(board, viewBoardButton);
			}
		}

		void ExperimentalSettingsButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new ExperimentalSettingsForm(
				new ExperimentalSettingsProvider(((BMBoard)BusinessEntity).PK, new BusinessObjectFactory())));
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			var result = ContinueWithDelete.Yes;
			var slideshowPivots = ((BMBoard)BusinessEntity).SlideshowPivots;
			if (slideshowPivots.Any())
			{
				var slideshowDeletionWarning = Res.GetString("7b2db1a2-9586-4cfa-9bd5-40698cc4ac1f", "(slide show will be deleted)");
				var names = string.Concat(slideshowPivots.Select(x => "\r\n" + x.Slideshow.MD_Name + (x.Slideshow.BoardPivots.Count == 1 ? " " + slideshowDeletionWarning : string.Empty)));
				var message = Res.GetString("e4b3c658-b85b-44d9-8f23-6aa5ae1e8c61", "This board will be permanently deleted and unlinked from the following slide shows:\r\n{0}\r\n\r\nDo you want to proceed?", names);
				var dialogResult = Globals.Message.Show(message, Res.GetString("164f1ba7-7074-4257-b1fa-5832366daf3f", "Delete Confirmation"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel);
				result = dialogResult == DialogResult.OK ? ContinueWithDelete.Yes : ContinueWithDelete.No;
			}

			return result;
		}

		static void ShowBoard(BMBoard board, ZButton viewBoardButton)
		{
			viewBoardButton.Enabled = false;
			var oldButtonText = viewBoardButton.Text;
			viewBoardButton.Text = Res.GetString("FF6D7891-EB3D-4F47-B718-A42E8F42DFB3", "Opening...");
			void formShownShowingOrFailedToShow()
			{
				viewBoardButton.Text = oldButtonText;
				viewBoardButton.Enabled = true;
			}
			// If the board is shown or _showing_, the button will get enabled again.
			// Enabling on showing may seem unjustified; however, normally the user will not be able to press the button again until the form is shown except this rare case:
			// The user clicks the View Board button in BMBoardForm, closes the BMBoardForm, opens is again and clicks the View Board button while the board opens.
			// In this case the button will immediately get enabled again. However, this case is rare and requires the board to open very slowly, thus acceptable.
			// Anyway, the board will not be open more than once.
			VisualBoardFormDisplayer.ShowBoard(board, formShownShowingOrFailedToShowCallback: formShownShowingOrFailedToShow);
		}

		#region ZForm Overrides

		protected override string FormatRecentItemCaption(string caption)
		{
			return Res.GetString("38CDD77A-8954-46AB-8EEC-5D51240C32CA", "{0} (configuration)", caption);
		}

		#endregion

		#region IFilterPreviewableWithSubObject Members

		public BusinessObject GetObjectForPreview(string filterControlIdentifier)
		{
			if (filterControlIdentifier == SchematicComponentSectionDescriptor.FilterIdentifier)
			{
				return BoardSectionConfigControl.BoardSectionsGrid.GetCurrent();
			}

			throw new UnidentifiedFilterControlException(BMFilterStripWrapperControl.UnidentifiedFilterControlExceptionMessage);
		}

		#endregion
	}
}
