using System;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public partial class DsdtSdFormatUserControl : ZUserControl
{
	public DsdtSdFormatUserControl(bool hasDsdtMrnNumberAsBindingMember, bool isGoToUrlButtonVisible)
	{
		InitializeComponent();
		SetupDsdtSdFormatTextBox(hasDsdtMrnNumberAsBindingMember);
		SetupGoToUrlButton(isGoToUrlButtonVisible);
	}

	void SetupDsdtSdFormatTextBox(bool hasDsdtMrnNumberAsBindingMember)
	{
		DsdtSdFormatNumberTextBox.ReadOnly = !hasDsdtMrnNumberAsBindingMember;

		if (hasDsdtMrnNumberAsBindingMember)
		{
			BindingSource.SetBindingMember(DsdtSdFormatNumberTextBox, nameof(TemporaryStorageHeader.DsdtMrnNumber));
		} else
		{
			BindingSource.SetBindingMember(DsdtSdFormatNumberTextBox, nameof(TemporaryStorageHeader.DsdtMrnNumberSdFormat));
		}
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		SetupGoToUrlButtonVisibility();
	}

	void SetupGoToUrlButtonVisibility()
	{
		if (!GoToUrlButton.Visible)
		{
			DsdtSdFormatNumberTextBox.Width = this.Width;
		}
	}

	void SetupGoToUrlButton(bool isGoToUrlButtonVisible)
	{
		GoToUrlButton.Visible = isGoToUrlButtonVisible;
		GoToUrlButton.FlatStyle = FlatStyle.Standard;
		GoToUrlButton.BackgroundImage = Icons.GetImage(IconTypes.Globe20x16);
	}

	void GoToUrlButton_Click(object sender, EventArgs e)
	{
		if (DataSource is TemporaryStorageHeader temporaryStorageHeader && !temporaryStorageHeader.DsdtSummaryDeclarationUrl.IsEmpty)
		{
			WebUrlLauncher.Launch(temporaryStorageHeader.DsdtSummaryDeclarationUrl);
		}
	}
}
