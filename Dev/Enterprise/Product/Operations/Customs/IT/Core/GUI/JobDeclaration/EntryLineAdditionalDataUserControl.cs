using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI;

public partial class EntryLineAdditionalDataUserControl : EU.GUI.EntryLineAdditionalDataUserControl
{
	public EntryLineAdditionalDataUserControl()
	{
		InitializeComponent();
	}

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		HookDeclarationEvents();
		SetM2LinesTabPageVisibility();
	}

	protected override void Dispose(bool disposing)
	{
		UnhookDeclarationEvents();
		base.Dispose(disposing);
	}

	void HookDeclarationEvents() => AssignDeclarationValueChangeEvents(ann => ann.OnValueChanged += OnDeclarationValueChanged);

	void UnhookDeclarationEvents() => AssignDeclarationValueChangeEvents(ann => ann.OnValueChanged -= OnDeclarationValueChanged);

	void AssignDeclarationValueChangeEvents(Action<IInvoicesProviderValueChangedAnnouncer> action)
	{
		if (CurrentDataItem is IInvoicesProvider provider && provider.GetValueChangedAnnouncer() is IInvoicesProviderValueChangedAnnouncer announcer)
		{
			action(announcer);
		}
	}

	void OnDeclarationValueChanged(object sender, EventArgs e)
	{
		SetM2LinesTabPageVisibility();
	}

	void SetM2LinesTabPageVisibility()
	{
		if (CurrentDataItem is JobDeclaration declaration)
		{
			M2LinesTabPage.TabVisible = !declaration.IsUCC6;
		}
	}
}
