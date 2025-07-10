using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.FR.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.H7.GUI
{
	public sealed class H7ApplicationGUIProvider : EU.H7.GUI.H7ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(H7ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new FRH7ManifestLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new FRH7BillLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return Array.Empty<IAdditionalTabPage>();
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new FRH7PackTabUserControl();
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
}
