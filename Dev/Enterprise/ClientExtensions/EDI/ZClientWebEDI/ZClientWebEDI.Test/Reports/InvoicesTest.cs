using System;
using System.Linq;
using System.Reflection;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class InvoicesTest : ZPageTestCase
	{
		[TestDate(2025, 5, 1)]
		public void TestPageLoad()
		{
			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);
			var collection = new SystemProductCollection();
			var product = collection.AddNew("BOR", "BorderWise", true);
			var xxxMapping = product.ModuleMappings.AddNew("BOR", "BorderWise", "PA1", true);
			xxxMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var page = Page as InvoicesForTest;
			var methodInfo = typeof(ZPage).GetMethod("OnLoad", BindingFlags.NonPublic | BindingFlags.Instance);
			methodInfo.Invoke(page, new object[] { EventArgs.Empty });
			page.AppInstance.SiteUser.Login(Org.OH_Code, Contact.OC_Email, "1234");
			page.DoPageLoad();

			AssertEquals("2025,2024,2023,2022,2021,2020,2019,2018,2017,2016,2015", string.Join(",", page.YearDropDownList_Exposed.Items.OfType<ListItem>().Select(x => x.Text)));
			AssertEquals("All,CargoWise,BorderWise", string.Join(",", page.ProductDropDownList_Exposed.Items.OfType<ListItem>().Select(x => x.Text)));
			AssertEquals("ALL,STL,BOR", string.Join(",", page.ProductDropDownList_Exposed.Items.OfType<ListItem>().Select(x => x.Value)));

			AssertEquals(Invoice2025.PK, page.OutstandingInvoicesDataGridDataSource_Exposed.Single().PK);
			AssertEquals(0, page.HistoricalInvoicesDataGridDataSource_Exposed.Count);
			AssertEquals("2025", page.YearDropDownList_Exposed.Text);
			AssertEquals("All", page.ProductDropDownList_Exposed.Text);

			typeof(Invoices).GetField("outstandingInvoiceCollection", BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue(page, null);
			typeof(Invoices).GetField("hitoricalInvoiceCollection", BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue(page, null);
			page.YearDropDownList_Exposed.SelectedValue = "2024";
			page.ProductDropDownList_Exposed.SelectedValue = "BOR";
			page.DoPageLoad();
			AssertEquals(Invoice2024.PK, page.HistoricalInvoicesDataGridDataSource_Exposed.Single().PK);
			AssertEquals(0, page.OutstandingInvoicesDataGridDataSource_Exposed.Count);
		}

		OrgHeader Org;
		OrgContact Contact;
		ARInvoice Invoice2024;
		ARInvoice Invoice2025;

		protected override void SetUp()
		{
			base.SetUp();
			EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			var usageChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			Factory.Save();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			Org = licence.Company.Header;
			Org.OH_Code = "MEHMEH";
			Org.OH_FullName = "MEHMEH";

			Contact = Org.Contacts.AddNew();
			Contact.OC_ContactName = "CWSupport";
			Contact.OC_Email = "newuser@cargowise.com";
			Contact.OC_WebAccessEnabled = true;
			Contact.SetHashedPassword("1234");
			Factory.Save();

			Invoice2024 = BillingTestHelper.CreateInvoice(Factory, licence, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, 10, 1, "", 0);
			Invoice2024.AH_InvoiceDate = new ZDateTime(2024, 8, 31);
			Invoice2024.AH_InvoicePrinted = true;
			var usage2024 = BillingTestHelper.CreateChargeableUsage(Factory, "BOR", "WXA", new ZDateTime(2024, 8, 1), licence.ClientCompany, 1);
			usage2024.U1_AH_Invoice = Invoice2024.PK;

			var usage2025 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", new ZDateTime(2025, 1, 1), licence.ClientCompany, 1);
			Invoice2025 = BillingTestHelper.CreateInvoice(Factory, licence, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, 10, 1, "", 0);
			Invoice2025.AH_InvoiceDate = new ZDateTime(2025, 1, 31);
			Invoice2025.AH_InvoicePrinted = true;
			usage2025.U1_AH_Invoice = Invoice2025.PK;
			Factory.Save();

			TestConnection.ExecuteNonQuery($"UPDATE AccTransactionHeader SET AH_FullyPaidDate = '2024-9-15', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{Invoice2024.PK}';");
		}

		protected override ZPage GetNewZPage()
		{
			var page = new InvoicesForTest();
			return page;
		}

		class InvoicesForTest : Invoices
		{
			public InvoicesForTest()
			{
				Breadcrumb = new HtmlGenericControl();
				OutstandingInvoicesDataGrid = new ZDataGrid();
				HistoricalInvoicesDataGrid = new ZDataGrid();
				YearDropDownList = new ZDropDownList();
				ProductDropDownList = new ZDropDownList();
			}

			public void DoPageLoad()
			{
				try
				{
					base.Page_Load(null, EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException || ex is NullReferenceException)
					{
						throw;
					}
				}
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			public ZDataGrid OutstandingInvoicesDataGrid_Exposed => OutstandingInvoicesDataGrid;
			public ZDataGrid HistoricalInvoicesDataGrid_Exposed => HistoricalInvoicesDataGrid;
			public ZDropDownList YearDropDownList_Exposed => YearDropDownList;
			public ZDropDownList ProductDropDownList_Exposed => ProductDropDownList;
			public AccTransactionHeaderCollection OutstandingInvoicesDataGridDataSource_Exposed => OutstandingInvoicesDataGrid.DataSource as AccTransactionHeaderCollection;
			public AccTransactionHeaderCollection HistoricalInvoicesDataGridDataSource_Exposed => HistoricalInvoicesDataGrid.DataSource as AccTransactionHeaderCollection;
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}
