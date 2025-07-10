using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EUNonLayoutExportSupplierHeaderUserControl))]
	sealed class EUNonLayoutExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<EUNonLayoutExportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestTransportChargesMethodOfPaymentDropEditVisibility()
		{
			const string transportChargesMethodOfPaymentControlName = "TransportChargesMethodOfPaymentDropEdit";
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();

			using (var form = new ZForm())
			using (var invoiceHeaderUserControl = new EUNonLayoutExportSupplierHeaderUserControl())
			{
				form.Controls.Add(invoiceHeaderUserControl);
				form.Show();
				invoiceHeaderUserControl.JobDeclaration = declaration;

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertControlVisibility(invoiceHeaderUserControl, transportChargesMethodOfPaymentControlName, true);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertControlVisibility(invoiceHeaderUserControl, transportChargesMethodOfPaymentControlName, false);
			}
		}

		public void TestJZ_InvoiceCurrLandedCostExRateCalcEditVisibility()
		{
			const string invoiceCurrLandedCostExRateControlName = "JZ_InvoiceCurrLandedCostExRateCalcEdit";
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();

			using (var form = new ZForm())
			using (var invoiceHeaderUserControl = new EUNonLayoutExportSupplierHeaderUserControl())
			{
				form.Controls.Add(invoiceHeaderUserControl);
				form.Show();
				invoiceHeaderUserControl.JobDeclaration = declaration;

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertControlVisibility(invoiceHeaderUserControl, invoiceCurrLandedCostExRateControlName, false);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertControlVisibility(invoiceHeaderUserControl, invoiceCurrLandedCostExRateControlName, true);
			}
		}

		void AssertControlVisibility(Control parentUserControl, ZString controlName, ZBool expectedResult)
		{
			var controlToCheck = parentUserControl.Controls.Find(controlName, true).FirstOrDefault();
			AssertNotNull(controlToCheck);
			AssertEquals(expectedResult, controlToCheck.Visible);
		}

		public void TestAgreedPlaceControlsVisibility_AgreedPlaceCodeSupport()
		{
			AssertAgreedPlaceControlsVisibility(true);
		}

		public void TestAgreedPlaceControlsVisibility_AgreedPlaceCodeNotSupport()
		{
			AssertAgreedPlaceControlsVisibility(false);
		}

		public void TestGroupInvoiceDropEditNotVisible()
		{
			using (var userControl = new EUNonLayoutExportSupplierHeaderUserControl())
			{
				var groupInvoiceDropEdit = userControl.FindSingleOrDefault<ZDropEdit>("GroupInvoiceDropEdit");
				AssertNotNull("GroupInvoiceDropEdit", groupInvoiceDropEdit);
				AssertEquals("GroupInvoiceDropEdit.Visible", false, groupInvoiceDropEdit.Visible);
			}
		}

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		void AssertAgreedPlaceControlsVisibility(bool agreedPlaceCodeSupport)
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, agreedPlaceCodeSupport))
			using (var form = new ZForm(declaration))
			using (var userControl = new EUNonLayoutExportSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
					AssertControlVisibility(userControl, "AgreedPlaceCodeFindBox", agreedPlaceCodeSupport);

					invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
					AssertControlVisibility(userControl, "AgreedPlaceCodeFindBox", false);
				});
			}
		}
	}
}
