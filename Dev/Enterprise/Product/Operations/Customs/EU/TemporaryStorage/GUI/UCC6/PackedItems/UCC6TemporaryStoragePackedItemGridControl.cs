using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStoragePackedItemGridControl : ZUserControl
	{
		public UCC6TemporaryStoragePackedItemGridControl()
		{
			InitializeComponent();
		}

		protected virtual void InitializeGridLayout()
		{
			var gridColumnLayoutProvider = LayoutProvider
				?.GetTemporaryStorageGridColumnLayoutProviderFactory()
				?.CreateTemporaryStorageGridColumnLayoutProviderForPackedItem() ?? new UCC6TemporaryStoragePackedItemGridColumnLayout();

			PackedItemGrid.ApplyGridColumnLayout(gridColumnLayoutProvider);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			temporaryStorageHeader = dataSource as TemporaryStorageHeader;

			base.SetDataBinding(dataSource, dataMember);

			SetTariffControlProperties();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (Header is not null)
			{
				InitializeGridLayout();
			}
		}

		TemporaryStorageHeader temporaryStorageHeader;

		TemporaryStorageHeader Header => CurrentDataItem as TemporaryStorageHeader;

		ITemporaryStorageLayoutProvider LayoutProvider => fLayoutProvider ??= TemporaryStorageLayoutProviderHelper.GetLayoutProvider(Header);
		ITemporaryStorageLayoutProvider fLayoutProvider;

		void SetTariffControlProperties()
		{
			var tariffColumnStyle = PackedItemGrid.ColumnStyles.ToArray().FirstOrDefault(x => x is Universal.GUI.TariffColumnStyleInfo) as Universal.GUI.TariffColumnStyleInfo;
			if (tariffColumnStyle != null)
			{
				tariffColumnStyle.GetDataGrouping = GetDataGroupingForUniversalTariff;
				tariffColumnStyle.GetTariffType = GetTariffTypeForUniversalTariff;
				tariffColumnStyle.GetEffectiveDate = GetEffectiveDateForUniversalTariff;
			}
		}

		ZString GetTariffTypeForUniversalTariff()
		{
			return Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff;
		}

		ZString GetDataGroupingForUniversalTariff()
		{
			return temporaryStorageHeader.DataGrouping;
		}

		ZDateTime GetEffectiveDateForUniversalTariff()
		{
			return ZDateTime.Today;
		}
	}
}
