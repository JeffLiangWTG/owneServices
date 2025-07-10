using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.GUI.PlugIn;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	[TestedType(typeof(ExportSupplierHeaderUserControl))]
	class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestGetPreviousDocumentsUserControlType()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				var previousDocumentsUserControlType = typeof(ExportSupplierHeaderUserControl).GetMethod("GetPreviousDocumentsUserControlType", BindingFlags.Instance | BindingFlags.NonPublic);
				AssertEquals(typeof(PreviousDocumentsUserControl), previousDocumentsUserControlType.Invoke(control, System.Array.Empty<object>()));
			}
		}

		public void TestBaseGroupChargesGridColumns()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm())
			using (var control = new ExportSupplierHeaderUserControl())
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

		public void TestJZ_ValuationCodeColumnAvailable()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm())
			using (var control = new ExportSupplierHeaderUserControl())
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
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm())
			using (var control = new ExportSupplierHeaderUserControl())
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

		public void TestSupplierColumnNotMandatoryNorVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm())
			using (var control = new ExportSupplierHeaderUserControl())
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
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm())
			using (var control = new ExportSupplierHeaderUserControl())
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
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm())
			using (var control = new ExportSupplierHeaderUserControl())
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
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.Invoices.AddNew();

			using (var form = new ZForm())
			using (var control = new ExportSupplierHeaderUserControl())
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
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.Invoices.AddNew();

			using (var form = new ZForm())
			using (var testUserControl = new ExportSupplierHeaderUserControl())
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
				using (var testUserControl = new ExportSupplierHeaderUserControl())
				{
					form.Controls.Add(testUserControl);
					testUserControl.JobDeclaration = dec;
					testUserControl.SetDataBinding(dec, "");
					form.Show();

					var agreedPlaceCodeFindBox = testUserControl.FindSingle<ZCodeFindBox>("AgreedPlaceCodeFindBox");
					AssertEquals("AgreedPlaceCodeFindBox visible", isUCC6, agreedPlaceCodeFindBox.Visible);
					var agreedPlaceCodeDropEdit = testUserControl.AgreedPlaceCodeDropEdit;
					AssertEquals("AgreedPlaceCodeDropEdit visible", !isUCC6, agreedPlaceCodeDropEdit.Visible);
				}
			}
		}

		public void TestIsSystemCalculatedColumn()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.Invoices.AddNew();

			using (var form = new ZForm())
			using (var testUserControl = new ExportSupplierHeaderUserControl())
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

		public void TestAdditionalInfoTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ExportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfoTabPage", "additionalInfosUserControl1", "[44] Special Mentions", typeof(AdditionalInfosUserControl));
		}
	}
}
