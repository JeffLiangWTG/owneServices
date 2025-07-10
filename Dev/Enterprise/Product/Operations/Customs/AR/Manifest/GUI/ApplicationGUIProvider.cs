using System;
using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetBillLayoutCore() => new ARBillLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			yield return new ZDateEditColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.ACN_ExpireDate,
				DateTimeFormat = ZDateTimePickerFormat.Short,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};

			yield return new ZTextBoxColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.ACN_ACEP,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145)
			};
		}

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new ARBillPartiesLayouts();

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new ARManifestLayouts();

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>() { { false, new[] { AsycudaBill.Schema.ABL_BolType, } } };
		}
	}
}
