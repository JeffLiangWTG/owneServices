using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStoragePackedItemWithGridLayout : IPanelLayoutWithGridProvider
	{
		public UCC6TemporaryStoragePackedItemWithGridLayout()
		{
			PackedItemDetailLayout = CreateUCC6TemporaryStoragePackedItemLayout();
		}

		public Type GridUserControlType => typeof(UCC6TemporaryStoragePackedItemGridControl);

		PanelLayout PackedItemDetailLayout { get; }

		public PanelLayout Layout => PackedItemDetailLayout;

		PanelLayout CreateUCC6TemporaryStoragePackedItemLayout()
		{
			var builder = new UCC6TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SeqTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.TariffCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GoodDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CusCodeCodeFindBox, ControlWidthClass.Long);
			return builder.Build();
		}
	}
}
