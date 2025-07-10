using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class EMCSInvoiceLineControlTest : TestCaseWithFactory
	{
		public void TestClassificationDetailsDynamicLayoutPanel()
		{
			using (var form = new ZForm(declaration))
			using (var control = new EMCSInvoiceLineControl())
			{
				control.SetDataBinding(declaration, nameof(declaration.FilteredInvoiceLines));
				form.Controls.Add(control);
				form.Show();

				DynamicLayoutPanelTest.AssertControlsOrder(control.ClassificationDetailsDynamicLayoutPanel,
						nameof(InvoiceLineDetailsControlBag.LineNoCalcEdit),
						nameof(InvoiceLineDetailsControlBag.IsMainPackCheckBox),
						nameof(InvoiceLineDetailsControlBag.WineDetailsSeparatorUserControl),
						nameof(InvoiceLineDetailsControlBag.ProductCodeFindBox),
						nameof(InvoiceLineDetailsControlBag.WineCategoryDropEdit),
						nameof(InvoiceLineDetailsControlBag.CustomsQuantityCalcDropEdit),
						nameof(InvoiceLineDetailsControlBag.DescriptionLongTextControl),
						nameof(InvoiceLineDetailsControlBag.GrowingZoneDropEdit),
						nameof(InvoiceLineDetailsControlBag.WeightCalcDropEdit),
						nameof(InvoiceLineDetailsControlBag.TariffCodeFindBox),
						nameof(InvoiceLineDetailsControlBag.WineCountryOriginCodeFindBox),
						nameof(InvoiceLineDetailsControlBag.NetWeightCalcDropEdit),
						nameof(InvoiceLineDetailsControlBag.ExciseProductCodeDropEdit),
						nameof(InvoiceLineDetailsControlBag.CommentsLongTextControl),
						nameof(InvoiceLineDetailsControlBag.AlcoholicStrengthUserControl),
						nameof(InvoiceLineDetailsControlBag.OriginLongTextControl),
						nameof(InvoiceLineDetailsControlBag.OperationCodesGroupBox),
						nameof(InvoiceLineDetailsControlBag.DegreePlatoCalcEdit),
						nameof(InvoiceLineDetailsControlBag.FiscalMarkUserControl),
						nameof(InvoiceLineDetailsControlBag.DensityCalcEdit),
						nameof(InvoiceLineDetailsControlBag.BrandNameTextBox),
						nameof(InvoiceLineDetailsControlBag.SizeOfProducerCalcEdit),
						nameof(InvoiceLineDetailsControlBag.MaturationPeriodOrAgeOfProductsWordWrappingTextBox));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
		}
		EMCSJobDeclaration declaration;
	}
}
