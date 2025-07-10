using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.IT.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.H7.GUI;

public sealed class H7ApplicationGUIProvider : EU.H7.GUI.H7ApplicationGUIProvider
{
	public override Type ApplicationBusinessProviderType => typeof(H7ApplicationBusinessProvider);

	protected override IPanelLayoutProvider GetManifestLayoutCore() => new ITH7ManifestLayout();

	protected override IPanelLayoutProvider GetBillLayoutCore() => new ITH7BillLayouts();

	protected override IPanelLayoutProvider GetItemDetailsLayoutCore() => new ITH7ItemDetailsLayouts();

	public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

	protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header)
	{
		return Array.Empty<IAdditionalTabPage>();
	}

	protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
	{
		yield return new EUH7PackUserControl();
		yield return new EUH7ItemUserControl();
		yield return new AdditionalDocumentsUserControl();
		yield return new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringSupportingDocuments, RelatedDocumentsUserControlWithGrid.SupportingDocumentsTabSequence);
		yield return new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringPreviousDocuments, RelatedDocumentsUserControlWithGrid.PreviousDocumentsTabSequence);
	}

	protected override IEnumerable<IAdditionalTabPage> GetItemAdditionalTabPageUserControlCore()
	{
		yield return new EUH7ItemPacksUserControl();
		yield return new AdditionalDocumentsUserControl();
		yield return new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringSupportingDocuments, RelatedDocumentsUserControlWithGrid.SupportingDocumentsTabSequence);
		yield return new RelatedDocumentsUserControlWithGrid(RelatedDocumentsUserControlWithGrid.ResStringPreviousDocuments, RelatedDocumentsUserControlWithGrid.PreviousDocumentsTabSequence);
	}
}
