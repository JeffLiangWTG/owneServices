using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(PriceItemBulkCopyForm))]
	public class PriceItemBulkCopyFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PriceItemBulkCopyForm(null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public void TestCreateNewCopy()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDNYC";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RN_NKCountry = "US";
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			priceHeader.L6_LicenceEdition = "EXP";
			var parentCodeKey = new UsageCodeKey(BillingConstants.BillingSystem.ODM, "COR");
			BillingTestHelper.AddPriceItem(priceHeader, parentCodeKey, BillingConstants.FeeType.Transactional, 1m);
			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Category = BillingConstants.BillingSystem.ImporterSecurityFiling;
			priceItem.L7_Code = "ISF";
			priceItem.L7_Description = "MFE Test1";
			priceItem.L7_ParentCategory = parentCodeKey.Category;
			priceItem.L7_ParentCode = parentCodeKey.Code;
			priceItem.L7_FeeType = "MFE";
			priceItem.L7_Price = 2m;

			var rate1 = priceItem.CurrencyRates.AddNew();
			rate1.PIR_Price = 1.23m;
			rate1.PIR_RX_NKCurrency = "AUD";
			var rate2 = priceItem.CurrencyRates.AddNew();
			rate2.PIR_Price = 4.56m;
			rate2.PIR_RX_NKCurrency = "GBP";

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddUserResponse("Yes");
			UnitTestUserNotification.Instance.AddOKAnswer();

			using (var form = new PriceItemBulkCopyForm(new[] { priceItem.PK }))
			{
				var priceCodeTextBox = form.Controls.Find("PriceCodeTextBox", true)[0] as ZTextBox;
				var priceDescTextBox = form.Controls.Find("PriceDescTextBox", true)[0] as ZTextBox;
				var okButton = form.Controls.Find("OKButton", true)[0] as ZButton;

				form.Show();
				Application.DoEvents();

				priceCodeTextBox.Text = "C01";
				priceDescTextBox.Text = "Desc1";
				okButton.PerformClick();
			}

			AssertEquals("Prices have been created.", UnitTestUserNotification.Instance.LastMessage.Text);

			var newObj = new BusinessObjectFactory().Load<ClientLicencePriceItem>(new ZQuery(ClientLicencePriceItemSchema.L7_Code, "C01"))[0];

			foreach (var col in ClientLicencePriceItemSchema.All)
			{
				if (col != ClientLicencePriceItemSchema.PK
					&& col != ClientLicencePriceItemSchema.L7_Code
					&& col != ClientLicencePriceItemSchema.L7_Description
					&& col != ClientLicencePriceItemSchema.L7_Order)
				{
					AssertEquals(priceItem[col], newObj[col]);
				}
			}

			AssertEquals("C01", newObj.L7_Code);
			AssertEquals("Desc1", newObj.L7_Description);
			AssertEquals(priceItem.L7_Order + 1, newObj.L7_Order);

			AssertEquals(2, newObj.CurrencyRates.Count);
			var aud = newObj.CurrencyRates.First(x => x.PIR_RX_NKCurrency == "AUD");
			var gbp = newObj.CurrencyRates.First(x => x.PIR_RX_NKCurrency == "GBP");
			AssertNotNull(aud);
			AssertNotNull(gbp);

			foreach (var col in EdiPriceItemRateSchema.All)
			{
				if (col != EdiPriceItemRateSchema.PK
					&& col != EdiPriceItemRateSchema.PIR_L7)
				{
					AssertEquals(rate1[col], aud[col]);
					AssertEquals(rate2[col], gbp[col]);
				}
			}

			AssertEquals(newObj.PK, aud.PIR_L7);
			AssertEquals(newObj.PK, gbp.PIR_L7);
		}
	}
}
