using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	[TestedType(typeof(GBExportSupplierHeaderUserControl))]
	class GBExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<GBExportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestCaptionWithApplicationCodeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoice = declaration.Invoices.AddNew();

			using (var form = new ZForm(declaration))
			{
				declaration.JE_ApplicationCode = "CHF";
				using (var control = new GBExportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.JobDeclaration = declaration;

					AssertEquals("[S29] Transport charges MoP", control.FindSingle<ZDropEdit>(x => x.Name == "TransportChargesMethodOfPaymentDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[22] Inv. Amount", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "JZ_InvoiceAmountBoundCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[24] Tran. Nature", control.FindSingle<ZDropEdit>(x => x.Name == "JZ_ValuationCodeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[44] Supporting Documents", control.FindSingle<ZTabPage>(x => x.Name == "SupportingDocumentsTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[40] Previous Documents", control.FindSingle<ZTabPage>(x => x.Name == "PreviousDocumentsTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[44] Additional Info", control.FindSingle<ZTabPage>(x => x.Name == "AdditionalInfoTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Incoterm", control.FindSingle<ZDropEdit>(x => x.Name == "JZ_IncoTermBoundDropDownEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Agreed Place", control.FindSingle<ZTextBox>(x => x.Name == "JZ_IncoTermPlaceTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}

			using (var form = new ZForm(declaration))
			{
				declaration.JE_ApplicationCode = "CDS";
				using (var control = new GBExportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.JobDeclaration = declaration;

					AssertEquals("[UCC 4/2] Transport charges MoP", control.FindSingle<ZDropEdit>(x => x.Name == "TransportChargesMethodOfPaymentDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 4/11] Inv. Amount", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "JZ_InvoiceAmountBoundCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 8/5] Tran. Nature", control.FindSingle<ZDropEdit>(x => x.Name == "JZ_ValuationCodeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 2/3 && 8/7] Supporting Documents", control.FindSingle<ZTabPage>(x => x.Name == "SupportingDocumentsTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 2/1] Previous Documents", control.FindSingle<ZTabPage>(x => x.Name == "PreviousDocumentsTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 2/2] Additional Info", control.FindSingle<ZTabPage>(x => x.Name == "AdditionalInfoTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 4/1] Incoterm", control.FindSingle<ZDropEdit>(x => x.Name == "JZ_IncoTermBoundDropDownEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Agreed Place", control.FindSingle<ZTextBox>(x => x.Name == "JZ_IncoTermPlaceTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
		}
	}
}
