using System;
using System.Windows.Forms;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.NCTS.GUI;

public partial class ArrivalSummaryDeclarationUserControl : ZUserControl
{
	public ArrivalSummaryDeclarationUserControl()
	{
		InitializeComponent();
		SetupGoToUrlButton();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		SetupGoToUrlButtonVisibility();
	}

	void SetupGoToUrlButtonVisibility()
	{
		if (DataSource is NctsHeader nctsHeader)
		{
			GoToUrlButton.Visible = !nctsHeader.ArrivalSummaryDeclarationUrl.IsEmpty;

			if(!GoToUrlButton.Visible)
			{
				SummaryDeclarationTextBox.Width = GoToUrlButton.Location.X + GoToUrlButton.Width - SummaryDeclarationTextBox.Location.X;
			}
		}
	}

	void SetupGoToUrlButton()
	{
		GoToUrlButton.FlatStyle = FlatStyle.Standard;
		GoToUrlButton.BackgroundImage = Icons.GetImage(IconTypes.Globe20x16);
	}

	void GoToUrlButton_Click(object sender, EventArgs e)
	{
		if (DataSource is NctsHeader nctsHeader && !nctsHeader.ArrivalSummaryDeclarationUrl.IsEmpty)
		{
			WebUrlLauncher.Launch(nctsHeader.ArrivalSummaryDeclarationUrl);
		}
	}
}
