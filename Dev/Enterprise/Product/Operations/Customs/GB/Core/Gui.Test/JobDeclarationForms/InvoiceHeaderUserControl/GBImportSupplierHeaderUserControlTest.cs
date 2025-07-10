using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	[TestedType(typeof(GBImportSupplierHeaderUserControl))]
	class GBImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<GBImportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestGridId()
		{
			using (var control = new GBImportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutLTt6BSgzSfuOqd/b9PPK5Q==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestColumnAndGridVisbilityForCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			using (var form = new ZForm())
			using (var userControl = new GBImportSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = dec;
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();

				var dutiableColumn = userControl.BaseGroupChargesGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == EU.Business.Declaration.InvoiceCharge.Schema.J7_IsDutiable);
				AssertNotNull("Dutiable value applicable column must be available", dutiableColumn);

				var vatApplyColumn = userControl.BaseGroupChargesGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == EU.Business.Declaration.InvoiceCharge.Schema.J7_IsGSTApplicable);
				Assert("VAT apply is not null or invisible", vatApplyColumn != null && vatApplyColumn.IsVisible);
			}

			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			using (var form = new ZForm())
			using (var userControl = new GBExportSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = dec;
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();

				Assert("Charges grids is visible for exports too", userControl.BaseGroupChargesGrid != null && userControl.BaseGroupChargesGrid.Visible);
			}
		}

		public void TestCaptionWithApplicationCodeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();

			using (var form = new ZForm(declaration))
			{
				declaration.JE_ApplicationCode = "CHF";
				using (var control = new GBImportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.JobDeclaration = declaration;

					AssertEquals("Supplier", control.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().Single(x => x.ColumnName == "JZ_OH_Supplier").Caption);
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
				using (var control = new GBImportSupplierHeaderUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.JobDeclaration = declaration;

					AssertEquals("[UCC 3/1] Exporter", control.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().Single(x => x.ColumnName == "JZ_OH_Supplier").Caption);
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

		public void TestCalculateDDPButton_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			using (var form = new ZForm())
			using (var userControl = new GBImportSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesCalculateDDPButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateDDPButton");

				CombineAssertions(() =>
				{
					AssertEquals("InvoiceChargesCalculateDDPButton Import Visible", true, invoiceChargesCalculateDDPButton.Visible);
					AssertEquals("InvoiceChargesCalculateDDPButton Caption", "Calculate DDP", invoiceChargesCalculateDDPButton.CaptionResourceString.Caption);
				});
			}
		}
	}
}
