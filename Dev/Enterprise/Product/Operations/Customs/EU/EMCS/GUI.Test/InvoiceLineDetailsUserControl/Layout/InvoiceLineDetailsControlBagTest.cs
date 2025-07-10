using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsControlBag))]
	sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineDetailsUserControl.CustomsQuantityCalcDropEdit);
				yield return nameof(InvoiceLineDetailsUserControl.SizeOfProducerCalcEdit);
				yield return nameof(InvoiceLineDetailsUserControl.DensityCalcEdit);
				yield return nameof(InvoiceLineDetailsUserControl.DegreePlatoCalcEdit);
				yield return nameof(InvoiceLineDetailsUserControl.NetWeightCalcDropEdit);
				yield return nameof(InvoiceLineDetailsUserControl.WeightCalcDropEdit);
				yield return nameof(InvoiceLineDetailsUserControl.OriginLongTextControl);
				yield return nameof(InvoiceLineDetailsUserControl.BrandNameTextBox);
				yield return nameof(InvoiceLineDetailsUserControl.FiscalMarkUserControl);
				yield return nameof(InvoiceLineDetailsUserControl.AlcoholicStrengthUserControl);
				yield return nameof(InvoiceLineDetailsUserControl.DescriptionLongTextControl);
				yield return nameof(InvoiceLineDetailsUserControl.LineNoCalcEdit);
				yield return nameof(InvoiceLineDetailsUserControl.ProductCodeFindBox);
				yield return nameof(InvoiceLineDetailsUserControl.ExciseProductCodeDropEdit);
				yield return nameof(InvoiceLineDetailsUserControl.TariffCodeFindBox);
				yield return nameof(InvoiceLineDetailsUserControl.IsMainPackCheckBox);
				yield return nameof(InvoiceLineDetailsUserControl.WineDetailsSeparatorUserControl);
				yield return nameof(InvoiceLineDetailsUserControl.CommentsLongTextControl);
				yield return nameof(InvoiceLineDetailsUserControl.WineCountryOriginCodeFindBox);
				yield return nameof(InvoiceLineDetailsUserControl.GrowingZoneDropEdit);
				yield return nameof(InvoiceLineDetailsUserControl.WineCategoryDropEdit);
				yield return nameof(InvoiceLineDetailsUserControl.OperationCodesGroupBox);
				yield return nameof(InvoiceLineDetailsUserControl.MaturationPeriodOrAgeOfProductsWordWrappingTextBox);
				yield return nameof(InvoiceLineDetailsUserControl.IndependentSmallProducersDeclarationWordWrappingTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
	}
}
