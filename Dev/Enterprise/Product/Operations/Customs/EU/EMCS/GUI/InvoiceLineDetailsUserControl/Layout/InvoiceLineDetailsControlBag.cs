using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public sealed class InvoiceLineDetailsControlBag : ControlBag
	{
		public InvoiceLineDetailsControlBag()
		{
			CustomsQuantityCalcDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.CustomsQuantityCalcDropEdit));
			SizeOfProducerCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.SizeOfProducerCalcEdit));
			DensityCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.DensityCalcEdit));
			DegreePlatoCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.DegreePlatoCalcEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.NetWeightCalcDropEdit));
			WeightCalcDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.WeightCalcDropEdit));
			OriginLongTextControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.OriginLongTextControl));
			BrandNameTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.BrandNameTextBox));
			FiscalMarkUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.FiscalMarkUserControl));
			AlcoholicStrengthUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.AlcoholicStrengthUserControl));
			DescriptionLongTextControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.DescriptionLongTextControl));
			LineNoCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.LineNoCalcEdit));
			ProductCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ProductCodeFindBox));
			ExciseProductCodeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExciseProductCodeDropEdit));
			TariffCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.TariffCodeFindBox));
			IsMainPackCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.IsMainPackCheckBox));
			WineDetailsSeparatorUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.WineDetailsSeparatorUserControl));
			CommentsLongTextControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.CommentsLongTextControl));
			WineCountryOriginCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.WineCountryOriginCodeFindBox));
			GrowingZoneDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.GrowingZoneDropEdit));
			WineCategoryDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.WineCategoryDropEdit));
			OperationCodesGroupBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.OperationCodesGroupBox));
			MaturationPeriodOrAgeOfProductsWordWrappingTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.MaturationPeriodOrAgeOfProductsWordWrappingTextBox));
			IndependentSmallProducersDeclarationWordWrappingTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.IndependentSmallProducersDeclarationWordWrappingTextBox));
		}

		public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

		[ThreadStatic]
		static InvoiceLineDetailsControlBag instance;

		public ControlReference CustomsQuantityCalcDropEdit { get; }

		public ControlReference SizeOfProducerCalcEdit { get; }

		public ControlReference DensityCalcEdit { get; }

		public ControlReference DegreePlatoCalcEdit { get; }

		public ControlReference NetWeightCalcDropEdit { get; }

		public ControlReference WeightCalcDropEdit { get; }

		public ControlReference OriginLongTextControl { get; }

		public ControlReference BrandNameTextBox { get; }

		public ControlReference FiscalMarkUserControl { get; }

		public ControlReference AlcoholicStrengthUserControl { get; }

		public ControlReference DescriptionLongTextControl { get; }

		public ControlReference LineNoCalcEdit { get; }

		public ControlReference ProductCodeFindBox { get; }

		public ControlReference ExciseProductCodeDropEdit { get; }

		public ControlReference TariffCodeFindBox { get; }

		public ControlReference IsMainPackCheckBox { get; }

		public ControlReference WineDetailsSeparatorUserControl { get; }

		public ControlReference CommentsLongTextControl { get; }

		public ControlReference WineCountryOriginCodeFindBox { get; }

		public ControlReference GrowingZoneDropEdit { get; }

		public ControlReference WineCategoryDropEdit { get; }

		public ControlReference OperationCodesGroupBox { get; }

		public ControlReference MaturationPeriodOrAgeOfProductsWordWrappingTextBox { get; }

		public ControlReference IndependentSmallProducersDeclarationWordWrappingTextBox { get; }

		protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();
	}
}
