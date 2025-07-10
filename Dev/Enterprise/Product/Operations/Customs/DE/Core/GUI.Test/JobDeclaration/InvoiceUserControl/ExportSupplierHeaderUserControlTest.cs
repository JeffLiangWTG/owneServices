using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ExportSupplierHeaderUserControl))]
	sealed class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestSupportingInfoTabsVisible()
		{
			using (var frm = new ZForm(declaration))
			using (var userControl = new ExportSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				userControl.InitializeGridLayout();
				frm.Controls.Add(userControl);
				frm.Show();

				var invoiceTabControl = userControl.Controls.Find("InvoiceTabControl", true).FirstOrDefault() as ZTabControl;
				if (invoiceTabControl != null)
				{
					CombineAssertions(() =>
					{
						Assert("Should display additional info tab", invoiceTabControl.TabPages.ContainsKey("AdditionalInfoTabPage"));
						Assert("Should display supporting document tab", invoiceTabControl.TabPages.ContainsKey("SupportingDocumentsTabPage"));
						Assert("Should display previous document tab", invoiceTabControl.TabPages.ContainsKey("PreviousDocumentsTabPage"));
					});
				}
			}
		}

		public void TestAdditionalInfosTabPage()
		{
			using (var frm = new ZForm(declaration))
			using (var userControl = new ExportSupplierHeaderUserControl())
			{
				frm.Controls.Add(userControl);
				userControl.JobDeclaration = declaration;
				frm.Show();

				var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
				tabPage.Show();
				CombineAssertions(() =>
				{
					AssertEquals("AdditionalInfosTabPage visible", true, tabPage.TabVisible);
					AssertEquals("Caption", "[44] Additional Documents", tabPage.CaptionResourceString.Caption);
					var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("additionalInfosUserControl1");
					AssertEquals("UserControl type", typeof(AdditionalInfosUserControlWithGrid), foundUserControl.UserControlType);
				});
			}
		}

		public void TestPreviousDocumentsUserControlType()
		{
			using (var frm = new ZForm(declaration))
			using (var userControl = new ExportSupplierHeaderUserControl())
			{
				frm.Controls.Add(userControl);
				frm.Show();

				var tabPage = userControl.FindSingle<ZTabPage>("PreviousDocumentsTabPage");
				tabPage.Show();
				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("previousDocumentsUserControl1");
				AssertEquals(typeof(ExportSupplierHeaderPreviousDocumentsUserControl), foundUserControl.UserControlType);
			}
		}

		public void TestSupportingDocumentsUserControlType()
		{
			using (var frm = new ZForm(declaration))
			using (var userControl = new ExportSupplierHeaderUserControl())
			{
				frm.Controls.Add(userControl);
				frm.Show();

				var tabPage = userControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
				tabPage.Show();
				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("SupportingDocumentsUserControl");
				AssertEquals(typeof(ExportSupplierHeaderSupportingDocumentsUserControl), foundUserControl.UserControlType);
			}
		}

		public void TestJobComInvoiceHeadersBoundGrid_Columns()
		{
			using (var frm = new ZForm(declaration))
			using (var control = new ExportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				frm.Controls.Add(control);
				frm.Show();

				CombineAssertions(() =>
				{
					var grid = control.FindSingle<InvoiceModuleButtonGrid>("JobComInvoiceHeadersBoundGrid");
					var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();

					var incotermPlaceColumn = columns.Single(x => x.ColumnName == nameof(JobComInvoiceHeader.ZG_AgreedPlaceCode));
					AssertEquals("ZG_AgreedPlaceCode Column Full Decription", "Incoterm Place", incotermPlaceColumn.CaptionResourceString.FullDescription);
					AssertEquals("ZG_AgreedPlaceCode Column Caption", "Inc. Place", incotermPlaceColumn.CaptionResourceString.Caption);
					AssertEquals("ZG_AgreedPlaceCode Column Short Caption", "Place", incotermPlaceColumn.CaptionResourceString.ShortCaption);
					AssertEquals("ZG_AgreedPlaceCode Column Width", 80, incotermPlaceColumn.Width);
				});
			}
		}

		public void TestInvoiceChargesGrid_Columns()
		{
			using (var frm = new ZForm(declaration))
			using (var control = new ExportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				frm.Controls.Add(control);
				frm.Show();

				var grid = control.FindSingle<ZGrid>(nameof(control.InvoiceChargesGrid));
				var isDutiableColumnStyle = grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable);

				Assert(isDutiableColumnStyle.IsUnavailable);
			}
		}

		public void TestBaseGroupChargesGrid_Columns()
		{
			using (var frm = new ZForm(declaration))
			using (var control = new ExportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				frm.Controls.Add(control);
				frm.Show();

				var grid = control.FindSingle<ZGrid>(nameof(control.BaseGroupChargesGrid));
				var isDutiableColumnStyle = grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable);

				Assert(isDutiableColumnStyle.IsUnavailable);
			}
		}

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" }).Union(new[] { "FreeOfChargeCheckBox", "AgreedPlaceCodeFindBox", "TransportChargesMethodOfPaymentDropEdit" });

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
		}
		JobDeclaration declaration;
	}
}
