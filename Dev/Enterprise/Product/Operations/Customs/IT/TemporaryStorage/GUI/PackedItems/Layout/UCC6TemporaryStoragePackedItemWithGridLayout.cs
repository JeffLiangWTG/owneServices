using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class UCC6TemporaryStoragePackedItemWithGridLayout : IPanelLayoutWithGridProvider
{
	public UCC6TemporaryStoragePackedItemWithGridLayout()
	{
		PackedItemDetailLayout = CreateUCC6TemporaryStoragePackedItemLayout();
	}

	public Type GridUserControlType => typeof(EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemGridControl);

	PanelLayout PackedItemDetailLayout { get; }

	public PanelLayout Layout => PackedItemDetailLayout;

	PanelLayout CreateUCC6TemporaryStoragePackedItemLayout()
	{
		var builder = new UCC6TemporaryStoragePackedItemDetailsBuilder();
		var itBag = builder.CommonBag;

		var commonBag = EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsControlBag.Instance;
		builder.AddControlBag(commonBag);

		builder.AddColumn();
		builder.Add(commonBag.SeqTextBox, ControlWidthClass.Medium);
		builder.Add(commonBag.TariffCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.GoodDescriptionTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CustomsSecondQuantityDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CusCodeCodeFindBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(itBag.RegistrationNoTextBox, ControlWidthClass.Long);
		builder.Add(itBag.ReleaseDateEdit, ControlWidthClass.Long);

		return builder.Build();
	}
}
