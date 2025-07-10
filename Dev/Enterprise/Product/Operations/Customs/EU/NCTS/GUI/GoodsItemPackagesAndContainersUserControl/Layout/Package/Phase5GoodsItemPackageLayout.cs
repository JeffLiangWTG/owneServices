using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemPackageLayout : IPanelLayoutProvider
	{
		public Phase5GoodsItemPackageLayout()
		{
			Layout = CreateGoodsItemPackageLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateGoodsItemPackageLayout()
		{
			var builder = new GoodsItemPackageLayoutBuilder<Business.NctsPackage>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.PackageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.NumberOfPackagesCalcEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.MarksAndNumbersTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
