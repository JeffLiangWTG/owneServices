using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.GUI.PlugIn;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	[TestedType(typeof(ImportSupplierHeaderUserControl))]
	class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestGetPreviousDocumentsUserControlType()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				var previousDocumentsUserControlType = typeof(ImportSupplierHeaderUserControl).GetMethod("GetPreviousDocumentsUserControlType", BindingFlags.Instance | BindingFlags.NonPublic);
				AssertEquals(typeof(PreviousDocumentsUserControl), previousDocumentsUserControlType.Invoke(control, System.Array.Empty<object>()));
			}
		}

		public void TestJZ_ValuationCodeColumnAvailable()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm())
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.SetDataBinding(dec, "");
				control.JobDeclaration = dec;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();
				var valuationCodeColumn = (ZDropEditColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_ValuationCode);
				AssertEquals(true, valuationCodeColumn.IsUnavailable);
			}
		}

		public void TestJZ_ValuationCodeDropEditVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm())
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.SetDataBinding(dec, "");
				control.JobDeclaration = dec;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();

				var valuationCodeDropEdit = control.FindSingle<ZDropEdit>("JZ_ValuationCodeDropEdit");
				AssertEquals(false, valuationCodeDropEdit.Visible);
			}
		}

		public void TestBaseGroupChargesGridColumns()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm())
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.SetDataBinding(dec, "");
				control.JobDeclaration = dec;
				form.Controls.Add(control);
				control.InitializeGridLayout();
				form.Show();
				var isSystemCalculatedColumn = (ZCheckBoxColumnStyleInfo)control.BaseGroupChargesGrid.GetColumnStyle(GroupInvoiceCharge.Schema.IsSystemCalculated);
				AssertNotNull(isSystemCalculatedColumn);
			}
		}

		public void TestSupplierColumnNotMandatoryNorVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm())
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.SetDataBinding(dec, "");
				control.JobDeclaration = dec;
				form.Controls.Add(control);
				form.Show();
				var supplierColumn = (ZGuidFindBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OH_Supplier);
				AssertEquals(true, supplierColumn.IsUnavailable);
			}
		}

		public void TestConsigneeColumnIsVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm())
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.SetDataBinding(dec, "");
				control.JobDeclaration = dec;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();
				var consigneeColumn = (ZGuidFindBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.ConsigneeOrgPK);
				AssertEquals(true, consigneeColumn.IsVisible);
			}
		}

		public void TestConsigneeAddressColumnIsVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm())
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.SetDataBinding(dec, "");
				control.JobDeclaration = dec;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();
				var consigneeAddressColumn = (ZGuidDropEditColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress);
				AssertEquals(true, consigneeAddressColumn.IsVisible);
			}
		}

		public void TestControls()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.Invoices.AddNew();

			using (var form = new ZForm())
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.SetDataBinding(dec, "");
				control.JobDeclaration = dec;
				form.Controls.Add(control);
				form.Show();

				control.Controls.Find("SupportingDocumentsTabPage", true).First().Show();
				var supportingDocument = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
				AssertEquals(typeof(SupportingDocumentsUserControl), supportingDocument.UserControlType);

				var tabPage = (ZTabPage)control.Controls.Find("NationalAdditionalCodeTabPage", true).FirstOrDefault();
				AssertNull(tabPage);

				control.Controls.Find("InvoiceChargesTabPage", true).First().Show();
				var button = control.Controls.Find("InvoiceChargesCalculateFreightButton", true).First() as ZButton;
				button.PerformClick();
				AssertType<CalculateFreightForm>(ZFormModaliser.ActiveForm);
			}
		}

		public void TestValuationMethodDropEditIsVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.Invoices.AddNew();

			using (var form = new ZForm())
			using (var testUserControl = new ImportSupplierHeaderUserControl())
			{
				form.Controls.Add(testUserControl);
				testUserControl.JobDeclaration = dec;
				testUserControl.SetDataBinding(dec, "");
				form.Show();

				AssertEquals("A ValuationMode field should show in the invoice header form", true, testUserControl.ValuationMethodDropEdit.Visible);
			}
		}

		public void TestVisibleChangeUCC6()
		{
			AssertVisibleChangeUCC6(true);
			AssertVisibleChangeUCC6(false);
		}

		void AssertVisibleChangeUCC6(bool isUCC6)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_ApplicationCode = isUCC6 ? Business.DeclarationApplicationCodeList.Codes.DeltaIE : Business.DeclarationApplicationCodeList.Codes.DeltaG;
			dec.Invoices.AddNew();

			using (var form = new ZForm())
			{
				using (var testUserControl = new ImportSupplierHeaderUserControl())
				{
					form.Controls.Add(testUserControl);
					testUserControl.JobDeclaration = dec;
					testUserControl.SetDataBinding(dec, "");
					form.Show();

					var agreedPlaceCodeFindBox = testUserControl.FindSingle<ZCodeFindBox>("AgreedPlaceCodeFindBox");
					AssertEquals("AgreedPlaceCodeFindBox visible", isUCC6, agreedPlaceCodeFindBox.Visible);
					var additionalTermsTextBox = testUserControl.FindSingle<ZTextBox>("JZ_AdditionalTermsTextBox");
					AssertEquals("AgreedPlaceCodeFindBox visible", isUCC6, additionalTermsTextBox.Visible);
					var incotermCountryDropEdit = testUserControl.FindSingle<ZCodeFindBox>("ZG_IncotermCountryCodeFindBox");
					AssertEquals("AgreedPlaceCodeFindBox visible", isUCC6, incotermCountryDropEdit.Visible);
					var agreedPlaceCodeDropEdit = testUserControl.AgreedPlaceCodeDropEdit;
					AssertEquals("AgreedPlaceCodeDropEdit visible", !isUCC6, agreedPlaceCodeDropEdit.Visible);
				}
			}
		}

		public void TestIsSystemCalculatedColumn()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.Invoices.AddNew();

			using (var form = new ZForm())
			using (var testUserControl = new ImportSupplierHeaderUserControl())
			{
				form.Controls.Add(testUserControl);
				testUserControl.JobDeclaration = dec;
				testUserControl.SetDataBinding(dec, "");
				testUserControl.InitializeGridLayout();
				form.Show();

				var grid = testUserControl.InvoiceChargesGrid;
				var systemColumn = grid.GetColumnStyle(InvoiceCharge.Schema.IsSystemCalculated);
				AssertEquals("IsSystemCalculated should be visible.", true, systemColumn.IsVisible);
			}
		}

		public void TestGetAdditionalInfosUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaG;
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(declaration, "AdditionalInfoTabPage", "additionalInfosUserControl1", "[44] Special Mentions", typeof(AdditionalInfosUserControl));

			declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaIE;
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(declaration, "AdditionalInfoTabPage", "additionalInfosUserControl1", "[44] Additional Documents", typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid));
		}

		public void TestIncotermCodeLogic()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_AdditionalTerms = "AAA";
			invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
			invoiceHeader.JZ_IncoTermPlace = "SYD";
			invoiceHeader.ZG_IncotermCountry = "LV";
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			using (var form = new ZForm())
			{
				using (var testUserControl = new ImportSupplierHeaderUserControl())
				{
					form.Controls.Add(testUserControl);
					testUserControl.JobDeclaration = dec;
					testUserControl.SetDataBinding(dec, "");
					form.Show();
					var incoTermBoundDropDownEdit = testUserControl.FindSingle<ZDropEdit>("JZ_IncoTermBoundDropDownEdit");
					var agreedPlaceCodeFindBox = testUserControl.FindSingle<ZCodeFindBox>("AgreedPlaceCodeFindBox");
					var incoTermPlaceTextBox = testUserControl.FindSingle<ZTextBox>("JZ_IncoTermPlaceTextBox");
					var incotermCountryDropEdit = testUserControl.FindSingle<ZCodeFindBox>("ZG_IncotermCountryCodeFindBox");
					var additionalTermsTextBox = testUserControl.FindSingle<ZTextBox>("JZ_AdditionalTermsTextBox");

					AssertEquals("JZ_AdditionalTermsTextBox is empty when JZ_IncoTermBoundDropDownEdit is not 'XXX'", string.Empty, additionalTermsTextBox.Text);
					AssertEquals("JZ_AdditionalTermsTextBox is read only when JZ_IncoTermBoundDropDownEdit is not 'XXX'", true, additionalTermsTextBox.ReadOnly);
					AssertEquals("JZ_AdditionalTermsTextBox is always visible", true, additionalTermsTextBox.Visible);
					AssertEquals("JZ_IncoTermPlaceTextBox is not empty when AgreedPlaceCodeFindBox is empty", "SYD", incoTermPlaceTextBox.Text);
					AssertEquals("JZ_IncoTermPlaceTextBox is enabled when JZ_IncoTermBoundDropDownEdit is not 'XXX' and AgreedPlaceCodeFindBox is empty", false, incoTermPlaceTextBox.ReadOnly);
					AssertEquals("JZ_IncoTermPlaceTextBox is always visible", true, incoTermPlaceTextBox.Visible);
					AssertEquals("ZG_IncotermCountryCodeFindBox is not empty when AgreedPlaceCodeFindBox is empty", "LV", incotermCountryDropEdit.Text);
					AssertEquals("ZG_IncotermCountryCodeFindBox is enabled when JZ_IncoTermBoundDropDownEdit is not 'XXX' and AgreedPlaceCodeFindBox is empty", false, incotermCountryDropEdit.ReadOnly);
					AssertEquals("ZG_IncotermCountryCodeFindBox is always visible", true, incotermCountryDropEdit.Visible);

					invoiceHeader.ZG_AgreedPlaceCode = "FRCDG";
					AssertEquals("JZ_IncoTermPlaceTextBox is empty when ZG_AgreedPlaceCode becomes not empty", string.Empty, incoTermPlaceTextBox.Text);
					AssertEquals("JZ_IncoTermPlaceTextBox is read only when JZ_IncoTermBoundDropDownEdit is not 'XXX' and AgreedPlaceCodeFindBox is not empty", true, incoTermPlaceTextBox.ReadOnly);
					AssertEquals("JZ_IncoTermPlaceTextBox is always visible", true, incoTermPlaceTextBox.Visible);
					AssertEquals("ZG_IncotermCountryCodeFindBox is empty when ZG_AgreedPlaceCode becomes not empty", string.Empty, incotermCountryDropEdit.Text);
					AssertEquals("ZG_IncotermCountryCodeFindBox is read only when JZ_IncoTermBoundDropDownEdit is not 'XXX' and AgreedPlaceCodeFindBox is not empty", true, incotermCountryDropEdit.ReadOnly);
					AssertEquals("ZG_IncotermCountryCodeFindBox is always visible", true, incotermCountryDropEdit.Visible);

					invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
					AssertEquals("JZ_IncoTermPlaceTextBox is enabled when JZ_IncoTermBoundDropDownEdit is not 'XXX' and AgreedPlaceCodeFindBox is empty", false, incoTermPlaceTextBox.ReadOnly);
					AssertEquals("JZ_IncoTermPlaceTextBox is always visible", true, incoTermPlaceTextBox.Visible);
					AssertEquals("ZG_IncotermCountryCodeFindBox is enabled when JZ_IncoTermBoundDropDownEdit is not 'XXX' and AgreedPlaceCodeFindBox is empty", false, incotermCountryDropEdit.ReadOnly);
					AssertEquals("ZG_IncotermCountryCodeFindBox is always visible", true, incotermCountryDropEdit.Visible);

					invoiceHeader.JZ_IncoTermPlace = "SYD";
					AssertEquals("AgreedPlaceCodeFindBox is read only when JZ_IncoTermBoundDropDownEdit is not 'XXX' and JZ_IncoTermPlaceTextBox is not empty", true, agreedPlaceCodeFindBox.ReadOnly);
					AssertEquals("AgreedPlaceCodeFindBox is always visible", true, agreedPlaceCodeFindBox.Visible);

					invoiceHeader.JZ_IncoTermPlace = ZString.Empty;
					AssertEquals("AgreedPlaceCodeFindBox is enabled when JZ_IncoTermBoundDropDownEdit is not 'XXX' and JZ_IncoTermPlaceTextBox is empty", false, agreedPlaceCodeFindBox.ReadOnly);
					AssertEquals("AgreedPlaceCodeFindBox is always visible", true, agreedPlaceCodeFindBox.Visible);

					invoiceHeader.ZG_IncotermCountry = "LV";
					AssertEquals("AgreedPlaceCodeFindBox is read only when JZ_IncoTermBoundDropDownEdit is not 'XXX' and ZG_IncotermCountryCodeFindBox is not empty", true, agreedPlaceCodeFindBox.ReadOnly);
					AssertEquals("AgreedPlaceCodeFindBox is always visible", true, agreedPlaceCodeFindBox.Visible);

					invoiceHeader.ZG_IncotermCountry = ZString.Empty;
					AssertEquals("AgreedPlaceCodeFindBox is enabled when JZ_IncoTermBoundDropDownEdit is not 'XXX' and ZG_IncotermCountryCodeFindBox is empty", false, agreedPlaceCodeFindBox.ReadOnly);
					AssertEquals("AgreedPlaceCodeFindBox is always visible", true, agreedPlaceCodeFindBox.Visible);

					invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;

					AssertEquals("JZ_AdditionalTermsTextBox is enabled when JZ_IncoTermBoundDropDownEdit is 'XXX'", false, additionalTermsTextBox.ReadOnly);
					AssertEquals("JZ_AdditionalTermsTextBox is always visible", true, additionalTermsTextBox.Visible);
					AssertEquals("AgreedPlaceCodeFindBox is empty when JZ_IncoTermBoundDropDownEdit is 'XXX'", string.Empty, agreedPlaceCodeFindBox.Text);
					AssertEquals("AgreedPlaceCodeFindBox is read only when JZ_IncoTermBoundDropDownEdit is 'XXX'", true, agreedPlaceCodeFindBox.ReadOnly);
					AssertEquals("JZ_IncoTermPlaceTextBox is empty when JZ_IncoTermBoundDropDownEdit is 'XXX'", string.Empty, incoTermPlaceTextBox.Text);
					AssertEquals("JZ_IncoTermPlaceTextBox is read only when JZ_IncoTermBoundDropDownEdit is 'XXX'", true, incoTermPlaceTextBox.ReadOnly);
					AssertEquals("ZG_IncotermCountryCodeFindBox is empty when JZ_IncoTermBoundDropDownEdit is 'XXX'", string.Empty, incotermCountryDropEdit.Text);
					AssertEquals("ZG_IncotermCountryCodeFindBox is read only when JZ_IncoTermBoundDropDownEdit is 'XXX'", true, incotermCountryDropEdit.ReadOnly);
				}
			}
		}
	}
}
