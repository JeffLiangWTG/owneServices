using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class EUICS2PackedItemDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout PackedItemDetailsLayouts { get; }

		PanelLayout IPanelLayoutProvider.Layout => PackedItemDetailsLayouts;

		public EUICS2PackedItemDetailsLayouts()
		{
			PackedItemDetailsLayouts = CreatePackedItemDetailsLayouts();
		}

		PanelLayout CreatePackedItemDetailsLayouts()
		{
			var builder = new PackedItemDetailsLayoutBuilder<AsycudaPack>();
			var common = builder.CommonBag;
			var ics2ControlBag = EUICS2PackedItemDetailsControlBag.Instance;
			builder.AddControlBag(ics2ControlBag);

			builder.AddColumn();
			builder.Add(common.TariffFindBox, ControlWidthClass.Medium);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.GoodsOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(ics2ControlBag.CusCodeFindBox, ControlWidthClass.Auto);
			builder.Add(ics2ControlBag.PostalValueCalcFindBox, ControlWidthClass.Long);
			builder.SetVisibility(ics2ControlBag.PostalValueCalcFindBox, x => GetManifestHeader(x).IsPackedItemTypeOfGoodsAndGoodsValueEnabled, x => GetManifestHeader(x).IsPackedItemTypeOfGoodsAndGoodsValueEnabledInfo);
			builder.Add(ics2ControlBag.TypeOfGoodsDropEdit, ControlWidthClass.Long);
			builder.SetVisibility(ics2ControlBag.TypeOfGoodsDropEdit, x => GetManifestHeader(x).IsPackedItemTypeOfGoodsAndGoodsValueEnabled, x => GetManifestHeader(x).IsPackedItemTypeOfGoodsAndGoodsValueEnabledInfo);
			builder.AddColumn();
			builder.Add(common.CustomEntriesSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.CustomEntriesGrid, ControlWidthClass.Long);

			return builder.Build();
		}

		AsycudaManifestHeader GetManifestHeader(AsycudaPack pack) => manifestHeader ?? (manifestHeader = (AsycudaManifestHeader)pack.Bill.Header);
		AsycudaManifestHeader manifestHeader;
	}
}
