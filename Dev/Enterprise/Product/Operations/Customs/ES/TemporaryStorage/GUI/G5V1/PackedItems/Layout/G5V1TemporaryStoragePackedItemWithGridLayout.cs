using System;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStoragePackedItemWithGridLayout : IPanelLayoutWithGridProvider
	{
		public G5V1TemporaryStoragePackedItemWithGridLayout()
		{
			PackedItemDetailLayout = CreateUCC6TemporaryStoragePackedItemLayout();
		}

		public Type GridUserControlType => typeof(G5V1TemporaryStoragePackedItemGridControl);

		PanelLayout PackedItemDetailLayout { get; }

		public PanelLayout Layout => PackedItemDetailLayout;

		PanelLayout CreateUCC6TemporaryStoragePackedItemLayout()
		{
			var builder = new G5V1TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>();
			var commonBag = builder.CommonBag;
			var esBag = G5V1TemporaryStoragePackedItemDetailsControlBag.Instance;

			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(commonBag.SeqTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.TariffCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GoodDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CusCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(esBag.UCRTextBox, ControlWidthClass.Long);
			builder.Add(esBag.PresentationDateEdit, ControlWidthClass.Medium);
			builder.Add(esBag.MissingCheckBox, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsValueCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsSecondQuantityDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsThirdQuantityDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.DutiesAndTaxesLabel, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.DutiesAndTaxesGrid, ControlWidthClass.LongNoCaption);

			builder.SetVisibility(esBag.MissingCheckBox, i => i.IsMessageTypeG5V1Reception, i => i.AMA_MessageTypeInfo);

			builder.SetVisibility(commonBag.CountryOfOriginDropEdit, i => !i.IsMessageTypeLAM, i => i.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.CustomsValueCalcDropEdit, i => !i.IsMessageTypeLAM, i => i.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.SupplementaryUnitsCalcDropEdit, i => !i.IsMessageTypeLAM, i => i.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.CustomsSecondQuantityDropEdit, i => !i.IsMessageTypeLAM, i => i.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.CustomsThirdQuantityDropEdit, i => !i.IsMessageTypeLAM, i => i.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.AdditionalSupplementaryCodesUserControl, i => !i.IsMessageTypeLAM, i => i.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.DutiesAndTaxesLabel, i => !i.IsMessageTypeLAM, i => i.AMA_MessageTypeInfo);
			builder.SetVisibility(commonBag.DutiesAndTaxesGrid, i => !i.IsMessageTypeLAM, i => i.AMA_MessageTypeInfo);

			return builder.Build();
		}
	}
}
