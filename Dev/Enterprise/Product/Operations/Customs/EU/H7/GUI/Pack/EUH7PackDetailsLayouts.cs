using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7PackDetailsLayouts : IPanelLayoutProvider
	{
		public EUH7PackDetailsLayouts()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateLayout()
		{
			var builder = new EUH7PackDetailsControlLayoutBuilder<AsycudaPack>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.GoodsDescriptionTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.PackQtyCalcEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.PackUQDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.MarksAndNumbersTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium);

			builder.SetCaption(commonBag.GoodsDescriptionTextBox, x => Res.GetData("adbcff90-65b0-4be0-a5d8-bb9a1737f6cd", "Description (on Pack)", "Description (on Pack)", "Description (on Pack)", "Description of goods, as stipulated on the pack."));
			builder.SetCaption(commonBag.MarksAndNumbersTextBox, x => Res.GetData("ae5802ee-31d8-4e3b-82b2-ecd9064073a1", "Marks (on Pack)", "Marks (on Pack)", "Marks (on Pack)", "Free form description of the marks and numbers stipulated on the pack."));

			return builder.Build();
		}
	}
}
