using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class AsycudaPackedItemWithGridLayout : IPanelLayoutWithGridProvider
	{
		public AsycudaPackedItemWithGridLayout()
		{
			PackedItemDetailLayout = CreateAsycudaPackedItemLayout();
		}

		public Type GridUserControlType => typeof(AsycudaPackedItemGridControl);

		PanelLayout PackedItemDetailLayout { get; }

		public PanelLayout Layout => PackedItemDetailLayout;

		PanelLayout CreateAsycudaPackedItemLayout()
		{
			var builder = new AsycudaPackedItemDetailsBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SeqTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.TariffCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GoodDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.GrossWeightCalcEdit, ControlWidthClass.Long);
			builder.Add(commonBag.UQDropCodeBox, ControlWidthClass.Long);
			builder.Add(commonBag.PackStatusDropCodeBox, ControlWidthClass.Long);
			return builder.Build();
		}
	}
}
