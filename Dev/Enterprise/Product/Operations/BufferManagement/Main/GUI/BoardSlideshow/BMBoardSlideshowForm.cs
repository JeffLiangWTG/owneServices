using System;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMBoardSlideshowForm : ZTemplateForm
	{
		public BMBoardSlideshowForm(BMBoardSlideshow slideShow)
			: base(slideShow)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;

		void RunSlideShowButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.HasChanges)
			{
				if (Globals.Message.Show(
					Res.GetString("544404A0-A499-44EC-A2CD-281A13AA4C8D", "You need to save this slide show before running it. Would you like to save now?"),
					Res.GetString("81E60325-5FA9-46E4-BB8F-845509BC95C7", "Save"),
					MessageBoxButtons.YesNo,
					DialogResult.Yes) == DialogResult.Yes && ValidateAndSave() == ContinueWithSave.Yes)
				{
					VisualBoardFormDisplayer.ShowBoard((BMBoardSlideshow)BusinessEntity);
				}
			}
			else
			{
				VisualBoardFormDisplayer.ShowBoard((BMBoardSlideshow)BusinessEntity);
			}
		}

		void EditBoard_Click(object sender, EventArgs e)
		{
			var slideshow = (BMBoardSlideshow)BusinessEntity;
			var boards = slideshow.BoardPivots.ToArray();

			if (boards.Length == 0 || BoardsGrid.CurrentRowIndex >= boards.Length)
			{
				ShowSelectBoardError();
			}
			else
			{
				var board = boards[BoardsGrid.CurrentRowIndex]?.Board;

				if (board == null || board.IsDeleted)
				{
					ShowSelectBoardError();
				}
				else
				{
					MainThreadRunner.RunOnMainThread(() =>
					{
						var form = ZControllerFactory.Create(ControllerIDs.BMBoard).ShowEditForm(board);

						if (form != null) // form can be null when the user has neither edit nor view rights to show the form
						{
							if (!IsDisposed && !Disposing)
							{
								ZFormModaliser.Show((Form)form, this);
							}
							else
							{
								form.Show();
							}
						}
					});
				}
			}
		}

		void ShowSelectBoardError()
		{
			Globals.Message.Show(Res.GetString("6c1fdec2-ae00-4672-8754-5ef47dd5443b", "Please select a board."));
		}
	}
}
