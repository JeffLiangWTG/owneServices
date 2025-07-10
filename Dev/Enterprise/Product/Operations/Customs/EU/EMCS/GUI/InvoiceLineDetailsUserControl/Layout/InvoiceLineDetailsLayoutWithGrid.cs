using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public sealed class InvoiceLineDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public InvoiceLineDetailsLayoutWithGrid(ZString dataGroupingCode)
		{
			Layout = CreateInvoiceLineDetailsLayout(dataGroupingCode);
		}

		PanelLayout CreateInvoiceLineDetailsLayout(ZString dataGroupingCode)
		{
			var builder = new InvoiceLineDetailsLayoutBuilder<Business.EMCSJobComInvoiceLine>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.LineNoCalcEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.ProductCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.TariffCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ExciseProductCodeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.OriginLongTextControl, ControlWidthClass.Auto);
			builder.Add(commonBag.FiscalMarkUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.BrandNameTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.SizeOfProducerCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.MaturationPeriodOrAgeOfProductsWordWrappingTextBox, ControlWidthClass.Long);
			if (Business.EMCSVersionSwitchHelper.IsPhase4_1Enabled(dataGroupingCode))
			{
				builder.Add(commonBag.IndependentSmallProducersDeclarationWordWrappingTextBox, ControlWidthClass.Long);
			}

			builder.AddColumn();
			builder.Add(commonBag.IsMainPackCheckBox, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.AlcoholicStrengthUserControl, ControlWidthClass.Medium);
			builder.Add(commonBag.DegreePlatoCalcEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.DensityCalcEdit, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(commonBag.WineDetailsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.WineCategoryDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.GrowingZoneDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.WineCountryOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.CommentsLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.OperationCodesGroupBox, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}

		PanelLayout Layout { get; }

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(InvoiceLineGridUserControl);

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
