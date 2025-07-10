using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	internal sealed class BillingPricesControlTestCase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		public void TestTranslateTextElementsMenu()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var prices = BillingTestHelper.CreateStlPriceList(licCompany, "USR");
			prices.Items.First().L7_Description = "A";

			using (var form = new ZForm(licCompany))
			{
				form.ClientSize = new System.Drawing.Size(200, 400);
				var ctrl = new BillingPricesControl();
				form.Controls.Add(ctrl);

				form.Show();
				Application.DoEvents();

				var grid = ctrl.Controls.Find("PriceHeadersGrid", true).Single() as ZGrid;
				grid.Select(0);

				Form formCreated = null;
				var formCreatedHandler = new EventHandler(delegate(object sender, EventArgs args) { formCreated = sender as Form; });

				try
				{
					grid.ContextMenu.Popup += delegate
					{
						var menu = grid.ContextMenu.MenuItems.FindByText("Translate Text Elements");
						AssertNotNull(menu);

						ZForm.FormCreated += formCreatedHandler;
						menu.PerformClick();
						Application.DoEvents();
						grid.ContextMenu.Dispose();
					};

					grid.ContextMenu.Show(grid, grid.Location);
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}

				AssertNotNull(formCreated);
				AssertEquals("CustomizableDataTranslationForm", formCreated.Name);
				formCreated.Close();

				form.Close();
			}
		}

		#region DownloadAndConvertPriceItem

		public void TestDownloadAndConvertPriceItem()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var priceHeader = BillingTestHelper.CreateStlPriceList(licCompany, "USR");
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			var item1 = priceHeader.Items.First();
			item1.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item1.L7_Code = "BBB";
			item1.L7_Description = "A";
			item1.L7_Price = 2.01m;
			item1.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			var item2 = priceHeader.Items.AddNew();
			item2.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item2.L7_Code = "CCC";
			item2.L7_Description = "AB";
			item2.L7_Price = 4.01m;
			item2.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			exchangeRate1.RE_RX_NKExCurrency = "NZD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 1.087800;
			exchangeRate1.ExCurrency.RX_ISOSubUnitRatio = 100;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 0.674600;
			exchangeRate2.ExCurrency.RX_ISOSubUnitRatio = 1000;
			Factory.Save();

			using (var form = new ZForm(licCompany))
			{
				form.ClientSize = new System.Drawing.Size(200, 400);
				var ctrl = new BillingPricesControl();
				form.Controls.Add(ctrl);

				form.Show();
				Application.DoEvents();

				var priceHeadersGrid = ctrl.Controls.Find("PriceHeadersGrid", true).Single() as ZGrid;
				priceHeadersGrid.Select(0);
				var priceItemsGrid = ctrl.Controls.Find("PriceItemsGrid", true).Single() as ZGrid;
				priceItemsGrid.Select(0);
				priceItemsGrid.Select(1);

				var tempFilePath = Temp.GetTempFileNameWithExtension("xml");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				try
				{
					priceItemsGrid.ContextMenu.Popup += delegate
					{
						var menu = priceItemsGrid.ContextMenu.MenuItems.FindByText("Download and convert Price Item");
						AssertNotNull(menu);
						DeleteIfExists(tempFilePath);
						AssertEquals("Precondition", false, File.Exists(tempFilePath));

						menu.PerformClick();
						Application.DoEvents();
						priceItemsGrid.ContextMenu.Dispose();
					};

					priceItemsGrid.ContextMenu.Show(priceItemsGrid, priceItemsGrid.Location);
					AssertEquals(true, File.Exists(tempFilePath));
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertStartsWith("Should show saved file dialog", "The document was saved as ", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					DeleteIfExists(tempFilePath);
				}

				form.Close();
			}
		}

		public void TestDownloadAndConvertPriceItem_ChooseNo()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var priceHeader = BillingTestHelper.CreateStlPriceList(licCompany, "USR");
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_PricelistVersion = "V1";
			var item1 = priceHeader.Items.First();
			item1.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item1.L7_Code = "BBB";
			item1.L7_Description = "A";
			item1.L7_Price = 2.01m;
			item1.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			exchangeRate1.RE_RX_NKExCurrency = "NZD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 1.087800;
			exchangeRate1.ExCurrency.RX_ISOSubUnitRatio = 100;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 0.674600;
			exchangeRate2.ExCurrency.RX_ISOSubUnitRatio = 1000;
			Factory.Save();

			using (var form = new ZForm(licCompany))
			{
				form.ClientSize = new System.Drawing.Size(200, 400);
				var ctrl = new BillingPricesControl();
				form.Controls.Add(ctrl);

				form.Show();
				Application.DoEvents();

				var priceHeadersGrid = ctrl.Controls.Find("PriceHeadersGrid", true).Single() as ZGrid;
				priceHeadersGrid.Select(0);
				var priceItemsGrid = ctrl.Controls.Find("PriceItemsGrid", true).Single() as ZGrid;
				priceItemsGrid.Select(0);

				var tempFilePath = Temp.GetTempFileNameWithExtension("xml");
				DeleteIfExists(tempFilePath);
				
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				try
				{
					priceItemsGrid.ContextMenu.Popup += delegate
					{
						var menu = priceItemsGrid.ContextMenu.MenuItems.FindByText("Download and convert Price Item");
						AssertNotNull(menu);

						menu.PerformClick();
						Application.DoEvents();
						priceItemsGrid.ContextMenu.Dispose();
					};

					priceItemsGrid.ContextMenu.Show(priceItemsGrid, priceItemsGrid.Location);
					AssertEquals(false, File.Exists(tempFilePath));
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertStartsWith("Last message should have been continue prompt", FormattableString.Invariant($"You are going to convert Price Item(s) [CWN - BBB] from Pricelist [V1]. This pricelist will be valid from [{priceHeader.L6_ValidFrom.ToShortDateString()}]. Do you wish to proceed?"), UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					DeleteIfExists(tempFilePath);
				}

				form.Close();
			}
		}

		public void TestDownloadAndConvertPriceItem_NoRowsSelectedShouldReturnError()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var priceHeader = BillingTestHelper.CreateStlPriceList(licCompany, "USR");
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			var item1 = priceHeader.Items.First();
			item1.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item1.L7_Code = "BBB";
			item1.L7_Description = "A";
			item1.L7_Price = 2.01m;
			item1.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			exchangeRate1.RE_RX_NKExCurrency = "NZD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 1.087800;
			exchangeRate1.ExCurrency.RX_ISOSubUnitRatio = 100;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 0.674600;
			exchangeRate2.ExCurrency.RX_ISOSubUnitRatio = 1000;
			Factory.Save();

			using (var form = new ZForm(licCompany))
			{
				form.ClientSize = new System.Drawing.Size(200, 400);
				var ctrl = new BillingPricesControl();
				form.Controls.Add(ctrl);

				form.Show();
				Application.DoEvents();

				var priceHeadersGrid = ctrl.Controls.Find("PriceHeadersGrid", true).Single() as ZGrid;
				priceHeadersGrid.Select(0);
				var priceItemsGrid = ctrl.Controls.Find("PriceItemsGrid", true).Single() as ZGrid;

				priceItemsGrid.ContextMenu.Popup += delegate
				{
					var menu = priceItemsGrid.ContextMenu.MenuItems.FindByText("Download and convert Price Item");
					AssertNotNull(menu);

					menu.PerformClick();
					Application.DoEvents();
					priceItemsGrid.ContextMenu.Dispose();
				};

				priceItemsGrid.ContextMenu.Show(priceItemsGrid, priceItemsGrid.Location);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please select at least one row in the Price Items Grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.Close();
			}
		}

		public void TestDownloadAndConvertPriceItem_ValidationError()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var prices = BillingTestHelper.CreateStlPriceList(licCompany, "USR");
			var item1 = prices.Items.First();
			item1.L7_Description = "A";
			item1.L7_Price = 2.01m;
			item1.L7_ExchangeRateGroupCode = "ABC";

			using (var form = new ZForm(licCompany))
			{
				form.ClientSize = new System.Drawing.Size(200, 400);
				var ctrl = new BillingPricesControl();
				form.Controls.Add(ctrl);

				form.Show();
				Application.DoEvents();

				var priceHeadersGrid = ctrl.Controls.Find("PriceHeadersGrid", true).Single() as ZGrid;
				priceHeadersGrid.Select(0);
				var priceItemsGrid = ctrl.Controls.Find("PriceItemsGrid", true).Single() as ZGrid;
				priceItemsGrid.Select(0);

				priceItemsGrid.ContextMenu.Popup += delegate
				{
					var menu = priceItemsGrid.ContextMenu.MenuItems.FindByText("Download and convert Price Item");
					AssertNotNull(menu);

					menu.PerformClick();
					Application.DoEvents();
					priceItemsGrid.ContextMenu.Dispose();
				};

				priceItemsGrid.ContextMenu.Show(priceItemsGrid, priceItemsGrid.Location);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please save changes before generating the XML.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.Close();
			}
		}

		#endregion

		#region ExportDisbursementPricelist

		public void TestExportDisbursementPricelist()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var prices = BillingTestHelper.CreateStlPriceList(licCompany, "USR");
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			var item1 = prices.Items.First();
			item1.L7_Description = "A";
			item1.L7_Price = 2.01m;
			item1.L7_DisbursementDirection = "ALL";
			item1.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item1.L7_Code = "ABC";
			item1.L7_RX_NKCurrency = "AUD";
			Factory.Save();

			using (var form = new ZForm(licCompany))
			{
				form.ClientSize = new System.Drawing.Size(200, 400);
				var ctrl = new BillingPricesControl();
				form.Controls.Add(ctrl);

				form.Show();
				Application.DoEvents();

				var priceHeadersGrid = ctrl.Controls.Find("PriceHeadersGrid", true).Single() as ZGrid;
				priceHeadersGrid.Select(0);

				var tempFilePath = Temp.GetTempFileNameWithExtension("xml");

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;

				try
				{
					priceHeadersGrid.ContextMenu.Popup += delegate
					{
						var menu = priceHeadersGrid.ContextMenu.MenuItems.FindByText("Export Disbursement Pricelist");
						AssertNotNull(menu);
						DeleteIfExists(tempFilePath);
						AssertEquals("Precondition", false, File.Exists(tempFilePath));

						menu.PerformClick();
						Application.DoEvents();
						priceHeadersGrid.ContextMenu.Dispose();
					};

					priceHeadersGrid.ContextMenu.Show(priceHeadersGrid, priceHeadersGrid.Location);
					AssertEquals(true, File.Exists(tempFilePath));
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertStartsWith("Should show saved file dialog", "The document was saved as ", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					DeleteIfExists(tempFilePath);
				}

				form.Close();
			}
		}

		public void TestExportDisbursementPricelist_MultipleRowsSelectedShouldReturnError()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var prices = BillingTestHelper.CreateStlPriceList(licCompany, "USR");
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			var otherPriceHeader = BillingTestHelper.CreateStlPriceList(licCompany, "USR");

			AssertEquals("Precondition: Should have added two price headers", 2, licCompany.PriceHeaders.Count);

			var item1 = prices.Items.First();
			item1.L7_Description = "A";
			item1.L7_Price = 2.01m;
			item1.L7_DisbursementDirection = "ALL";
			item1.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item1.L7_Code = "ABC";
			item1.L7_RX_NKCurrency = "AUD";
			Factory.Save();

			using (var form = new ZForm(licCompany))
			{
				form.ClientSize = new System.Drawing.Size(200, 400);
				var ctrl = new BillingPricesControl();
				form.Controls.Add(ctrl);

				form.Show();
				Application.DoEvents();

				var priceHeadersGrid = ctrl.Controls.Find("PriceHeadersGrid", true).Single() as ZGrid;
				priceHeadersGrid.Select(0);
				priceHeadersGrid.Select(1);

				priceHeadersGrid.ContextMenu.Popup += delegate
				{
					var menu = priceHeadersGrid.ContextMenu.MenuItems.FindByText("Export Disbursement Pricelist");
					AssertNotNull(menu);

					menu.PerformClick();
					Application.DoEvents();
					priceHeadersGrid.ContextMenu.Dispose();
				};

				priceHeadersGrid.ContextMenu.Show(priceHeadersGrid, priceHeadersGrid.Location);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please select a single row in the Price Headers Grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.Close();
			}
		}

		public void TestExportDisbursementPricelist_ValidationError()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var prices = BillingTestHelper.CreateStlPriceList(licCompany, "USR");
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			var item1 = prices.Items.First();
			item1.L7_Description = "A";
			item1.L7_Price = 2.01m;
			item1.L7_DisbursementDirection = "ALL";
			item1.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item1.L7_Code = "ABC";
			item1.L7_RX_NKCurrency = "AUD";

			using (var form = new ZForm(licCompany))
			{
				form.ClientSize = new System.Drawing.Size(200, 400);
				var ctrl = new BillingPricesControl();
				form.Controls.Add(ctrl);

				form.Show();
				Application.DoEvents();

				var priceHeadersGrid = ctrl.Controls.Find("PriceHeadersGrid", true).Single() as ZGrid;
				priceHeadersGrid.Select(0);

				priceHeadersGrid.ContextMenu.Popup += delegate
				{
					var menu = priceHeadersGrid.ContextMenu.MenuItems.FindByText("Export Disbursement Pricelist");
					AssertNotNull(menu);

					menu.PerformClick();
					Application.DoEvents();
					priceHeadersGrid.ContextMenu.Dispose();
				};

				priceHeadersGrid.ContextMenu.Show(priceHeadersGrid, priceHeadersGrid.Location);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please save changes before generating the XML.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.Close();
			}
		}

		#endregion
	}
}
