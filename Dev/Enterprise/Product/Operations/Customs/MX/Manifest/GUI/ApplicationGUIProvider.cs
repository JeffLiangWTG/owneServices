using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new MXManifestlLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new MXBillLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override AsycudaItemSelectionDialog GetNewAsycudaItemSelectionDialogCore(ASYCUDA.Business.MessageChooser messageChooser, string itemsType, string messageType)
		{
			if (messageChooser is MXMessageChooser mxMessageChooser)
			{
				return new BillsSelectionDialog(mxMessageChooser, itemsType);
			}

			return base.GetNewAsycudaItemSelectionDialogCore(messageChooser, itemsType, messageType);
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>() { { false, new[] { AsycudaBill.Schema.ABL_BolType, } } };
		}

		protected override IReadOnlyDictionary<bool, string[]> GetPacksGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>()
			{
				{ header.IsSea, new[] { UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue, UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue } }
			};
		}

		public override bool ShouldCheckPacksGridColumnAvailability() => true;
	}
}
