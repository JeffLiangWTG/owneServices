using System;
using System.Drawing;
using System.Reflection;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

#if !WINZOR
namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CAImportInvoiceLineUserControl))]
	sealed class ImportInvoiceLineUserControlBasherTest : ImportCustomsUserControlBasherTest
	{
		public void TestGridColours()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).CA_OGDStatus = AVSStatusList.Codes.NotImport;
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).CA_OGDStatus = AVSStatusList.Codes.NotValidated;
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).CA_OGDStatus = AVSStatusList.Codes.Error;
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).CA_OGDStatus = AVSStatusList.Codes.Rejected;
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).CA_OGDStatus = AVSStatusList.Codes.Unknown;
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).CA_OGDStatus = AVSStatusList.Codes.InspectionRequired;
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).CA_OGDStatus = AVSStatusList.Codes.ReviewRequired;
			((JobComInvoiceLine)invoice.InvoiceLines.AddNew()).CA_OGDStatus = AVSStatusList.Codes.Blank;

			using (var form = new ZForm(declaration))
			using (var control = new CAImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				AssertEquals(Color.Red, GetGridRowColour(control.CustomsInvoiceLinesBoundGrid, control.CustomsInvoiceLinesBoundGrid.ListManager.List[0]));
				AssertEquals(Color.LightBlue, GetGridRowColour(control.CustomsInvoiceLinesBoundGrid, control.CustomsInvoiceLinesBoundGrid.ListManager.List[1]));
				AssertEquals(Color.LightBlue, GetGridRowColour(control.CustomsInvoiceLinesBoundGrid, control.CustomsInvoiceLinesBoundGrid.ListManager.List[2]));
				AssertEquals(Color.LightBlue, GetGridRowColour(control.CustomsInvoiceLinesBoundGrid, control.CustomsInvoiceLinesBoundGrid.ListManager.List[3]));
				AssertEquals(Color.LightBlue, GetGridRowColour(control.CustomsInvoiceLinesBoundGrid, control.CustomsInvoiceLinesBoundGrid.ListManager.List[4]));
				AssertEquals(Color.Yellow, GetGridRowColour(control.CustomsInvoiceLinesBoundGrid, control.CustomsInvoiceLinesBoundGrid.ListManager.List[5]));
				AssertEquals(Color.Yellow, GetGridRowColour(control.CustomsInvoiceLinesBoundGrid, control.CustomsInvoiceLinesBoundGrid.ListManager.List[6]));
				AssertEquals(Color.Empty, GetGridRowColour(control.CustomsInvoiceLinesBoundGrid, control.CustomsInvoiceLinesBoundGrid.ListManager.List[7]));
			}
		}

		Type userControlToBashType;
		protected override Type UserControlToBashType => userControlToBashType ?? (userControlToBashType = typeof(CAImportInvoiceLineUserControl));

		protected override Customs.Business.BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashing();
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			return declaration;
		}

		Color GetGridRowColour(ZGrid grid, object objectAtRow)
		{
			EventHandler<ColourDecidingEventArgs> handler = (EventHandler<ColourDecidingEventArgs>)typeof(ZGrid).GetField("ColourDeciding", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(grid);
			ColourDecidingEventArgs e = new ColourDecidingEventArgs(objectAtRow);
			handler(null, e);
			return e.Colour;
		}
	}
}
#endif
