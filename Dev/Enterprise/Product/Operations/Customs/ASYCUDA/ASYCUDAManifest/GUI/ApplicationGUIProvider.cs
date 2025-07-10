using System;
using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ASYCUDAManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDAManifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new MenuBuilder(header, mainForm);
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override IPanelLayoutProvider GetBillLayoutCore()
		{
			return new ASYCUDABillLayouts();
		}

		protected override IReadOnlyDictionary<string, bool> GetBillsGridColumnVisiblilityOnValueChangedCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var asycudaManifestHeader = header as AsycudaManifestHeader;
			var showExportGeneralManifest = asycudaManifestHeader != null && asycudaManifestHeader.ShowExportGeneralManifest;

			return new Dictionary<string, bool>()
			{
				{ AsycudaBill.Schema.SADOfficeCode, showExportGeneralManifest },
				{ AsycudaBill.Schema.SADRegistrationSerial, showExportGeneralManifest },
				{ AsycudaBill.Schema.SADRegistrationNumber, showExportGeneralManifest },
				{ AsycudaBill.Schema.SADRegistrationDate, showExportGeneralManifest }
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var sADOfficeCodeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			sADOfficeCodeDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.SADOfficeCode;
			sADOfficeCodeDropEditColumnStyleInfo.IsVisible = false;
			sADOfficeCodeDropEditColumnStyleInfo.IsUnavailable = true;
			sADOfficeCodeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return sADOfficeCodeDropEditColumnStyleInfo;

			var sADRegistrationSerialTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			sADRegistrationSerialTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.SADRegistrationSerial;
			sADRegistrationSerialTextBoxColumnStyleInfo.IsVisible = false;
			sADRegistrationSerialTextBoxColumnStyleInfo.IsUnavailable = true;
			sADRegistrationSerialTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return sADRegistrationSerialTextBoxColumnStyleInfo;

			var sADRegistrationNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			sADRegistrationNumberTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.SADRegistrationNumber;
			sADRegistrationNumberTextBoxColumnStyleInfo.IsVisible = false;
			sADRegistrationNumberTextBoxColumnStyleInfo.IsUnavailable = true;
			sADRegistrationNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return sADRegistrationNumberTextBoxColumnStyleInfo;

			var sADRegistrationDateDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			sADRegistrationDateDateEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.SADRegistrationDate;
			sADRegistrationDateDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			sADRegistrationDateDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			sADRegistrationDateDateEditColumnStyleInfo.IsVisible = false;
			sADRegistrationDateDateEditColumnStyleInfo.IsUnavailable = true;
			yield return sADRegistrationDateDateEditColumnStyleInfo;
		}
	}
}
