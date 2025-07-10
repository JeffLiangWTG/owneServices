using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class EXPProductsCustomDetailsControlBag : ControlBag
	{
		EXPProductsCustomDetailsControlBag()
		{
			BrandNameTextBox = RegisterControl(nameof(EXPProductsCustomsDetailsUserControl.BrandNameTextBox));
			ModelTradeNameTextBox = RegisterControl(nameof(EXPProductsCustomsDetailsUserControl.ModelTradeNameTextBox));
			IngredientTextBox = RegisterControl(nameof(EXPProductsCustomsDetailsUserControl.IngredientTextBox));
			UsageCommentTextBox = RegisterControl(nameof(EXPProductsCustomsDetailsUserControl.UsageCommentTextBox));
			ClassificationDescriptionTextBox = RegisterControl(nameof(EXPProductsCustomsDetailsUserControl.ClassificationDescriptionTextBox));

			GoodsOriginCodeFindBox = RegisterControl(nameof(EXPProductsCustomsDetailsUserControl.GoodsOriginCodeFindBox));
			COOLabelLocationDropEdit = RegisterControl(nameof(EXPProductsCustomsDetailsUserControl.COOLabelLocationDropEdit));
		}

		public static EXPProductsCustomDetailsControlBag Instance => instance ?? (instance = new EXPProductsCustomDetailsControlBag());

		[ThreadStatic]
		static EXPProductsCustomDetailsControlBag instance;

		protected override Control CreateTemplate() => new EXPProductsCustomsDetailsUserControl();

		public ControlReference BrandNameTextBox { get; }
		public ControlReference ModelTradeNameTextBox { get; }
		public ControlReference IngredientTextBox { get; }
		public ControlReference UsageCommentTextBox { get; }
		public ControlReference ClassificationDescriptionTextBox { get; }

		public ControlReference GoodsOriginCodeFindBox { get; }
		public ControlReference COOLabelLocationDropEdit { get; }
	}
}
