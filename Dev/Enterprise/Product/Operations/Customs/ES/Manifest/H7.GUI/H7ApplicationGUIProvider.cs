using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public sealed class H7ApplicationGUIProvider : EU.H7.GUI.H7ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(H7ApplicationBusinessProvider);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new ESH7ManifestLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new ESH7BillLayout();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new ESH7BillPartiesLayout();

		protected override IPanelLayoutProvider GetItemDetailsLayoutCore() => new ESH7ItemsDetailsLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new ESH7PackTabUserControl();
			yield return new EUH7ItemUserControl();
			yield return new SupportingDocumentsUserControl();
			yield return new AdditionalDocumentsUserControl();
			yield return new PreviousDocumentsUserControl();
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			foreach (var columnInfo in base.GetBillsGridExtraColumnInfosCore())
			{
				if (columnInfo.ColumnName != AsycudaBill.Schema.MovementReferenceNumber)
				{
					yield return columnInfo;
				}
			}

			yield return new ZTextBoxColumnStyleInfo
			{
				ColumnName = nameof(AsycudaBill.Schema.DocumentationRequiredDescription),
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				IsVisible = false
			};

			yield return new ZTextBoxColumnStyleInfo
			{
				ColumnName = AsycudaBill.Schema.G3LocalReferenceNumber,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			};

			yield return new ZTextBoxColumnStyleInfo
			{
				ColumnName = AsycudaBill.Schema.G3MovementReferenceNumber,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			};

			yield return new ZTextBoxColumnStyleInfo
			{
				ColumnName = AsycudaBill.Schema.H7MovementReferenceNumber,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			};
		}

		protected override void CustomizeBillsGridCore(ZGridWithDynamicColumnHandler billsGrid)
		{
			base.CustomizeBillsGridCore(billsGrid);

			var needReplaceFieldNames = new[] { AsycudaBill.Schema.ABL_RN_NKConsigneeCountry, AsycudaBill.Schema.ABL_RN_NKShipperCountry, AsycudaBill.Schema.ABL_RN_NKSellerCountry };
			foreach (var needReplaceFieldName in needReplaceFieldNames)
			{
				billsGrid.ColumnStyles.Remove(billsGrid.GetColumnStyle(needReplaceFieldName));
				billsGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
				{
					ColumnName = needReplaceFieldName,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				});
			}
		}

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			yield return new ESH7MessagesUserControl();
		}

		protected override IEnumerable<IAdditionalTabPage> GetItemAdditionalTabPageUserControlCore()
		{
			yield return new EUH7ItemPacksUserControl();
		}
	}
}
