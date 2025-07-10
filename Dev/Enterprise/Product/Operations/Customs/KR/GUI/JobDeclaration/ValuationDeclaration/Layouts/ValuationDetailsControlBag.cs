using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ValuationDetailsControlBag : ControlBag
	{
		public ValuationDetailsControlBag()
		{
			TariffFindBox = RegisterControl(nameof(ValuationDetailsUserControl.TariffFindBox));
			ProductCodeCodeFindBox = RegisterControl(nameof(ValuationDetailsUserControl.ProductCodeCodeFindBox));
			IngredientLongTextControl = RegisterControl(nameof(ValuationDetailsUserControl.IngredientLongTextControl));
			BrandNameTextBox = RegisterControl(nameof(ValuationDetailsUserControl.BrandNameTextBox));
			ModelTradeNameTextBox = RegisterControl(nameof(ValuationDetailsUserControl.ModelTradeNameTextBox));
			DescriptionLongTextControl = RegisterControl(nameof(ValuationDetailsUserControl.DescriptionLongTextControl));
		}

		public static ValuationDetailsControlBag Instance => instance ?? (instance = new ValuationDetailsControlBag());
		[ThreadStatic]
		static ValuationDetailsControlBag instance;

		protected override Control CreateTemplate() => new ValuationDetailsUserControl();

		public ControlReference TariffFindBox { get; }
		public ControlReference ProductCodeCodeFindBox { get; }
		public ControlReference IngredientLongTextControl { get; }
		public ControlReference BrandNameTextBox { get; }
		public ControlReference ModelTradeNameTextBox { get; }
		public ControlReference DescriptionLongTextControl { get; }
	}
}
