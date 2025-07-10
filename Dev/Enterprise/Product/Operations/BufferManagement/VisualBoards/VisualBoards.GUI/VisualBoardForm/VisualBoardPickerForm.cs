using System;
using CargoWise.Async;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public partial class VisualBoardPickerForm : ZChildForm
	{
		public VisualBoardPickerForm()
		{
			InitializeComponent();
		}

		public VisualBoardPickerForm(BoardPickerViewModel boardViewModel)
			: base(boardViewModel)
		{
			InitializeComponent();
			SetFormText();
		}
		public new BoardPickerViewModel BusinessEntity
		{
			get { return (BoardPickerViewModel)base.BusinessEntity; }
		}

		void ShowBoardButton_Click(object sender, EventArgs e)
		{
			var model = BusinessEntity;

			model.Validation.ValidateAll();
			if (!model.HasErrors)
			{
				MainThreadRunner.RunOnMainThread(() =>
				{
					var factory = new BusinessObjectFactory { NameForDebugging = "VisualBoardPickerForm" };
					ShowBoard(model.GetBoard(factory));
				});
				Close();
			}
			else
			{
				base.ShowErrorsDialog();
			}
		}

		protected virtual void ShowBoard(IVisualBoardProvider provider)
		{
			VisualBoardFormDisplayer.ShowBoard(provider);
		}

		void SetFormText()
		{
			Text = Res.GetString("9A7176D0-E085-49FF-BB79-0050B3CD7686", "Search Board");
		}
#if DEBUG
		public void ShowBoardButton_Click_ExposedForTest()
		{
			ShowBoardButton_Click(null, null);
		}

#endif
	}
}
