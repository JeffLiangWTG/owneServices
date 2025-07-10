using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class InvoiceLineDetailsLayoutWithGridTest_Phase4_1 : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.LineNoCalcEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.ProductCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.TariffCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.ExciseProductCodeDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.OriginLongTextControl, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.FiscalMarkUserControl, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.BrandNameTextBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.SizeOfProducerCalcEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.MaturationPeriodOrAgeOfProductsWordWrappingTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.IndependentSmallProducersDeclarationWordWrappingTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.IsMainPackCheckBox, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.AlcoholicStrengthUserControl, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.DegreePlatoCalcEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.DensityCalcEdit, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.WineDetailsSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (InvoiceLineDetailsControlBag.Instance.WineCategoryDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.GrowingZoneDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.WineCountryOriginCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.CommentsLongTextControl, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.OperationCodesGroupBox, ControlWidthClass.LongNoCaption);
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(InvoiceLineGridUserControl);

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new InvoiceLineDetailsLayoutBuilder<Business.EMCSJobComInvoiceLine>();

		protected override IPanelLayoutProvider GetNewPanelLayoutProvider() => new InvoiceLineDetailsLayoutWithGrid(GlbCompany.CurrentCompany.Country.Code);

		protected override void SetUp()
		{
			base.SetUp();
			temporaryFunctionality = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EMCS_Phase4_1, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true);
		}

		IDisposable temporaryFunctionality;

		protected override void TearDown()
		{
			temporaryFunctionality?.Dispose();
			base.TearDown();
		}
	}
}
