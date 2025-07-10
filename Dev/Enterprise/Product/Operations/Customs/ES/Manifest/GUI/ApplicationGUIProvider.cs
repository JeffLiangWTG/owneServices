using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.GUI
{
	public sealed class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(Business.ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
			yield return new SupportingDocumentsUserControl();
		}

		protected override string[] GetPackedItemColumnsCore()
		{
			return new string[]
			{
				AsycudaPackedItem.Schema.API_FormattedTariff,
				AsycudaPackedItem.Schema.API_GoodsDescription,
				AsycudaPackedItem.Schema.API_CustomsQty,
				AsycudaPackedItem.Schema.API_CustomsUQ,
				AsycudaPackedItem.Schema.API_GrossWeight,
				AsycudaPackedItem.Schema.API_GrossWeightUQ,
				AsycudaPackedItem.Schema.API_NetWeight,
				AsycudaPackedItem.Schema.API_NetWeightUQ,
				AsycudaPackedItem.Schema.API_GoodsValue,
				AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency,
			};
		}
		protected override string[] GetPacksGridColumnsOrderCore() => PacksGridColumnsForESAllVisibleMandatory();

		protected override string[] GetPacksGridMandatoryColumnsCore() => PacksGridColumnsForESAllVisibleMandatory();

		string[] PacksGridColumnsForESAllVisibleMandatory()
		{
			return new string[]
			{
				AsycudaPack.Schema.APA_LineNo,
				AsycudaPack.Schema.APA_CommodityCode,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.ContainerPK,
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_MarksAndNumbers,
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
				UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
			};
		}
	}
}
