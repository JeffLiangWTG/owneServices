using System;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class EntryLineAdditionalDataUserControl : ZUserControl
{
	public EntryLineAdditionalDataUserControl()
	{
		InitializeComponent();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		ChangeControlsVisibility();
	}

	void ChangeControlsVisibility()
	{
		var declaration = (JobDeclaration)CurrentDataItem;
		TaxOrFeeTabPage.TabVisible = declaration?.IsImport ?? false;
	}
}
