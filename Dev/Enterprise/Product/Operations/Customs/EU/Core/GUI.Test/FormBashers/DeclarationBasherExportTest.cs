using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class DeclarationBasherExportTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public override void TestMinimumSizeNotTooBig()
		{
			var declaration = Factory.New<JobDeclaration>();
			var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);
			var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(900);
			using (var form = new JobDeclarationForm(declaration))
			{
				Assert("EU Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("EU Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}
		public override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.Export;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceheader = declaration.Invoices.AddNew();
			invoiceheader.InvoiceLines.AddNew();
			declaration.CusContainers.AddNew();
			declaration.Bills.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.MergedLines.AddNew();
			return declaration;
		}

		protected override Customs.Business.BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(Customs.Business.BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
		{
			var line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "ABC";
			return line;
		}
	}
}
