using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CL.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(Business.ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new CLManifestLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new CLBillLayouts();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new CLBillPartiesLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override string[] GetPacksGridColumnsOrderCore()
		{
			return new string[]
			{
				AsycudaPack.Schema.APA_LineNo,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.APA_MarksAndNumbers,
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
				AsycudaPack.Schema.ContainerPK,
				UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
				UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
			};
		}

		protected override AsycudaItemSelectionDialog GetNewAsycudaItemSelectionDialogCore(MessageChooser messageChooser, string itemsType, string messageType)
		{
			if (messageChooser is Business.CLMessageChooser clMessageChooser)
			{
				return new CLBillsSelectionDialog(clMessageChooser, itemsType);
			}

			return base.GetNewAsycudaItemSelectionDialogCore(messageChooser, itemsType, messageType);
		}

		protected override IReadOnlyDictionary<bool, string[]> GetArrivalHeadersGridColumnAvailabilityCore()
		{
			return new Dictionary<bool, string[]>
			{
				{
					false, new[] { AsycudaArrivalHeader.Schema.ATH_ETAAtDischargePort, Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine.Schema.ATL_ABL_AsycudaBill }
				}
			};
		}

		protected override IReadOnlyDictionary<bool, string[]> GetArrivalLinesGridColumnAvailabilityCore()
		{
			return new Dictionary<bool, string[]>
			{
				{
					false, new[] { AsycudaArrivalLine.Schema.ATL_CargoStatus, AsycudaArrivalLine.Schema.ATL_Reference, AsycudaArrivalLine.Schema.ATL_BillNumber }
				}
			};
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>() { { false, new[] { AsycudaBill.Schema.ABL_BolType, } } };
		}

		protected override ZBool ArrivalLinesReadOnlyCore() => false;
	}
}
