using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicencePriceItem))]
	public class ClientLicencePriceItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			ClientLicencePriceItem priceItem = Factory.New<ClientLicencePriceItem>();
			AssertNull(priceItem.Parent);

			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceItem = priceHeader.Items.AddNew();
			AssertEquals("Parent", priceHeader, priceItem.Parent);
		}

		public void TestPropertiesReadOnly()
		{
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			ClientLicencePriceItem priceItem = Factory.New<ClientLicencePriceItem>();

			foreach (ZPropertyInfo propertyInfo in priceItem.ZPropertyInfoHash)
			{
				if (propertyInfo.Name == "L7_ExchangeRateGroupCode" || propertyInfo.Name == "L7_IsVolumeAdjustmentEligible")
				{
					AssertEquals(true, propertyInfo.ReadOnly);
				}
				else
				{
					AssertEquals(false, propertyInfo.ReadOnly);
				}
			}

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			priceItem = Factory.New<ClientLicencePriceItem>();
			foreach (ZPropertyInfo propertyInfo in priceItem.ZPropertyInfoHash)
			{
				AssertEquals(true, propertyInfo.ReadOnly);
			}
		}

		public void TestCodeDescription()
		{
			ClientLicencePriceItem item = Factory.New<ClientLicencePriceItem>();
			item.L7_Code = "COR";
			AssertEquals(LicenceModuleList.Instance.GetDescriptionFromCode("COR"), item.CodeDescription);

			item.L7_Code = "ACC";
			AssertEquals(LicenceModuleList.Instance.GetDescriptionFromCode("ACC"), item.CodeDescription);

			item.L7_Code = "DS1";
			AssertEquals(EDIDataRegistry.Instance.PriceItemDiscountTypes.Value.GetDescriptionFromCode("DS1"), item.CodeDescription);

			item.L7_Code = "DDC";
			AssertEquals(EDIDataRegistry.Instance.PriceItemDiscountTypes.Value.GetDescriptionFromCode("DDC"), item.CodeDescription);
		}

		public void TestParentCodeDescription()
		{
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item1 = priceHeader.Items.AddNew();
			var item2 = priceHeader.Items.AddNew();
			var item3 = priceHeader.Items.AddNew();
			var item4 = priceHeader.Items.AddNew();
			var testItem = priceHeader.Items.AddNew();

			item1.L7_Category = "ODM";
			item1.L7_Code = "FOO";
			item1.L7_Description = "Item ODM-FOO";

			item2.L7_Category = "ODM";
			item2.L7_Code = "BAR";
			item2.L7_Description = "Item ODM-BAR";

			item3.L7_Category = "STL";
			item3.L7_Code = "FOO";
			item3.L7_Description = "Item STL-FOO";

			item4.L7_Category = "STL";
			item4.L7_Code = "BAR";
			item4.L7_Description = "Item STL-BAR";

			testItem.L7_ParentCategory = "ODM";
			testItem.L7_ParentCode = "FOO";
			AssertEquals("Item ODM-FOO", testItem.ParentCodeDescription);

			testItem.L7_ParentCategory = "ODM";
			testItem.L7_ParentCode = "BAR";
			AssertEquals("Item ODM-BAR", testItem.ParentCodeDescription);

			testItem.L7_ParentCategory = "STL";
			testItem.L7_ParentCode = "BAR";
			AssertEquals("Item STL-BAR", testItem.ParentCodeDescription);

			testItem.L7_ParentCategory = "STL";
			testItem.L7_ParentCode = "FOO";
			AssertEquals("Item STL-FOO", testItem.ParentCodeDescription);
		}

		public void TestL7_FeeTypeDesc()
		{
			ClientLicencePriceItem item = Factory.New<ClientLicencePriceItem>();
			item.L7_FeeType = BillingConstants.FeeType.Included;
			AssertEquals(item.Lookups.FeeTypes.GetDescriptionFromCode(item.L7_FeeType), item.L7_FeeTypeDesc);

			item.L7_FeeType = BillingConstants.FeeType.Licence;
			AssertEquals(item.Lookups.FeeTypes.GetDescriptionFromCode(item.L7_FeeType), item.L7_FeeTypeDesc);
		}

		public void TestIsTransactional()
		{
			ClientLicencePriceItem item = Factory.New<ClientLicencePriceItem>();
			item.L7_FeeType = "XXX";
			AssertEquals(false, item.IsTransactional);

			item.L7_FeeType = BillingConstants.FeeType.NamedUser;
			AssertEquals(false, item.IsTransactional);

			item.L7_FeeType = BillingConstants.FeeType.Licence;
			AssertEquals(false, item.IsTransactional);

			item.L7_FeeType = BillingConstants.FeeType.Transactional;
			AssertEquals(true, item.IsTransactional);
		}

		public void TestIsTransactionalOneVolumeBreak()
		{
			ClientLicencePriceItem item = Factory.New<ClientLicencePriceItem>();
			item.L7_FeeType = "XXX";
			AssertEquals(false, item.IsTransactionalOneVolumeBreak);

			item.L7_FeeType = BillingConstants.FeeType.NamedUser;
			AssertEquals(false, item.IsTransactionalOneVolumeBreak);

			item.L7_FeeType = BillingConstants.FeeType.Licence;
			AssertEquals(false, item.IsTransactionalOneVolumeBreak);

			item.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			AssertEquals(true, item.IsTransactionalOneVolumeBreak);
		}

		public void TestIsOnDemandFeeType()
		{
			ClientLicencePriceItem item = Factory.New<ClientLicencePriceItem>();
			item.L7_FeeType = "XXX";
			AssertEquals(false, item.IsOnDemandFeeType);

			foreach (ICodeDescription feeType in BillingConstants.GetFeeTypeList())
			{
				item.L7_FeeType = feeType.Code;
				AssertEquals(BillingConstants.FeeType.IsOnDemand(feeType.Code), item.IsOnDemandFeeType);
			}
		}

		public void TestIsMaintenanceFeeType()
		{
			ClientLicencePriceItem item = Factory.New<ClientLicencePriceItem>();
			item.L7_FeeType = "XXX";
			AssertEquals(false, item.IsMaintenanceFeeType);

			foreach (ICodeDescription feeType in BillingConstants.GetFeeTypeList())
			{
				item.L7_FeeType = feeType.Code;
				AssertEquals(BillingConstants.FeeType.IsMaintenance(feeType.Code), item.IsMaintenanceFeeType);
			}
		}

		public void TestHasLicenceUnits()
		{
			var item1 = Factory.New<ClientLicencePriceItem>();
			item1.L7_LicenceUnits = 1;
			var item2 = Factory.New<ClientLicencePriceItem>();
			item2.L7_LicenceUnits = 0;

			AssertEquals(true, item1.HasLicenceUnits);
			AssertEquals(false, item2.HasLicenceUnits);
		}

		public void TestLogChanges()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDNYC";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RN_NKCountry = "US";
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			priceHeader.L6_LicenceEdition = "EXP";
			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Code = "ISF";
			priceItem.L7_Description = "ediImporterSecurityFiling";
			priceItem.L7_ParentCode = "COR";
			priceItem.L7_FeeType = "TPJ";
			priceItem.L7_Price = 2m;

			Factory.Save();

			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "PriceItem");
			StmALog[] logs = org.Logs.Find(query);
			AssertEquals("No log for new added price item", 0, logs.Length);

			priceItem.L7_Code = "";
			priceItem.L7_ParentCode = "";
			priceItem.L7_FeeType = "TPT";
			priceItem.L7_Price = 1.5m;

			Factory.Save();

			logs = org.Logs.Find(query);
			AssertEquals("Should be one log for price item changes", 1, logs.Length);

			string expected = "PriceItem[ediImporterSecurityF] | Code:ISF=> | Parent:COR=> | FeeType:TPJ=>TPT | Price:2=>1.5";
			AssertEquals(expected, logs[0].SL_Reference);
			AssertEquals(Events.EditedARecordCode, logs[0].SL_SE_NKEvent);
		}

		public void TestDescriptionIndentLevel()
		{
			ClientLicencePriceItem item = Factory.New<ClientLicencePriceItem>();
			item.L7_Description = "XXX";
			AssertEquals(0, item.DescriptionIndentLevel);

			item.L7_Description = "";
			AssertEquals(0, item.DescriptionIndentLevel);

			item.L7_Description = "    ";
			AssertEquals(0, item.DescriptionIndentLevel);

			item.L7_Description = "    A";
			AssertEquals(4, item.DescriptionIndentLevel);
		}

		public void TestPriceDescriptionForUniquePriceCodeLookup()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDNYC";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RN_NKCountry = "US";
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			priceHeader.L6_LicenceEdition = "EXP";
			var priceItem1 = priceHeader.Items.AddNew();
			priceItem1.L7_Code = "";
			priceItem1.L7_Description = "   Usage for ABC  ";
			priceItem1.L7_Order = 100;

			var priceItem2 = priceHeader.Items.AddNew();
			priceItem2.L7_Code = "ABC";
			priceItem2.L7_Description = "0 ~ 50";
			priceItem2.L7_UnitBreak = 0;
			priceItem2.L7_Order = 200;

			var priceItem3 = priceHeader.Items.AddNew();
			priceItem3.L7_Code = "ABC";
			priceItem3.L7_Description = "51 ~ 100";
			priceItem3.L7_UnitBreak = 50;
			priceItem3.L7_Order = 201;

			var priceItem4 = priceHeader.Items.AddNew();
			priceItem4.L7_Code = "ABC";
			priceItem4.L7_Description = "100+";
			priceItem4.L7_UnitBreak = 100;
			priceItem4.L7_Order = 202;

			var priceItem5 = priceHeader.Items.AddNew();
			priceItem5.L7_Code = "DEF";
			priceItem5.L7_Description = "   DESC:DEF    ";
			priceItem5.L7_UnitBreak = 0;
			priceItem5.L7_Order = 203;

			AssertEquals("Usage for ABC", priceItem2.PriceDescriptionForUniquePriceCodeLookup);
			AssertEquals("Usage for ABC", priceItem3.PriceDescriptionForUniquePriceCodeLookup);
			AssertEquals("Usage for ABC", priceItem4.PriceDescriptionForUniquePriceCodeLookup);
			AssertEquals("DESC:DEF", priceItem5.PriceDescriptionForUniquePriceCodeLookup);
		}

		public void TestDelete()
		{
			var item = Factory.New<ClientLicencePriceItem>();
			var rate1 = item.CurrencyRates.AddNew();
			var rate2 = item.CurrencyRates.AddNew();

			item.Delete();
			Assert(rate1.IsDeleted);
			Assert(rate2.IsDeleted);
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
			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Code = "ISF";
			priceItem.L7_Description = "ediImporterSecurityFiling";
			priceItem.L7_ParentCode = "COR";
			priceItem.L7_FeeType = "MFE";
			priceItem.L7_Price = 2m;

			var rate1 = priceItem.CurrencyRates.AddNew();
			rate1.PIR_Price = 1.23m;
			rate1.PIR_RX_NKCurrency = "AUD";
			var rate2 = priceItem.CurrencyRates.AddNew();
			rate2.PIR_Price = 4.56m;
			rate2.PIR_RX_NKCurrency = "GBP";

			Factory.Save();

			var newObj = priceItem.CreateNewCopy("C01", "Desc1");
			AssertEquals(false, priceItem.HasChanges);

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

		public void TestTranslatable()
		{
			var obj = Factory.New<ClientLicencePriceItem>();
			obj.L7_Description = "    A";
			obj.L7_ChargeBasis = "B";

			var resKey1 = obj.L7_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(obj, "    A").ResourceKey;
			var resKey2 = obj.L7_ChargeBasisInfo.CustomizableDataResourceStrings.GetMultilingualString(obj, "B").ResourceKey;
			AssertEquals("    A", obj.L7_DescriptionLocalized);
			AssertEquals("B", obj.L7_ChargeBasisMultilingual);

			using (Res.TemporarilySwitchLanguage("CHS"))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey1, new ResourceStringData(resKey1, "A1"));
				mockRes.Put(resKey2, new ResourceStringData(resKey2, "B1"));
				AssertEquals("    A1", obj.L7_DescriptionLocalized);
				AssertEquals("B1", obj.L7_ChargeBasisMultilingual);
			}
		}

		public void TestProductProperties()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDNYC";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RN_NKCountry = "US";
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			priceHeader.L6_LicenceEdition = "EXP";
			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Code = "ISF";
			priceItem.L7_Description = "ediImporterSecurityFiling";
			priceItem.L7_ParentCode = "COR";
			priceItem.L7_FeeType = "TPJ";
			priceItem.L7_Price = 2m;
			Factory.Save();

			AssertEquals("", priceItem.L7_ProductAvailability);
			AssertEquals("", priceItem.L7_ProductDisplayCategory);
			AssertEquals("", priceItem.ProductDisplayCategoryDescription);

			priceItem.L7_ProductAvailability = "Y";
			priceItem.L7_ProductDisplayCategory = "ACC";
			AssertEquals("Accounting and 3rd Party Services and Transactions (External Services)", priceItem.ProductDisplayCategoryDescription);
			Factory.Save();

			priceItem.L7_ProductAvailability = "N";
			priceItem.L7_ProductDisplayCategory = "BWP";
			AssertEquals("BorderWise", priceItem.ProductDisplayCategoryDescription);
			priceItem.ProductDisplayCategoryDescription = "eServices Pricing";
			AssertEquals("ESV", priceItem.L7_ProductDisplayCategory);
			priceItem.ProductDisplayCategoryDescription = "123 abc";
			AssertEquals("", priceItem.L7_ProductDisplayCategory);
			AssertEquals("", priceItem.ProductDisplayCategoryDescription);
			Factory.Save();

			AssertExceptionThrown<ZSaveException>(() =>
			{
				priceItem.L7_ProductAvailability = "@";
				Factory.Save();
			});
		}

		public override void TestSettingValueCallsRefreshBinding()
		{
			var categories = EDIDataRegistry.Instance.ProductDisplayCategories.DefaultValue;
			var cat1 = categories.AddNew();
			cat1.Code = "`a1";
			cat1.Description = (NoResString)"`a1";
			cat1.Bool = true;
			var cat2 = categories.AddNew();
			cat2.Code = "~a1";
			cat2.Description = (NoResString)"~a1";
			cat2.Bool = true;
			var cat3 = categories.AddNew();
			cat3.Code = "~ax";
			cat3.Description = (NoResString)"~a1~";
			cat3.Bool = true;

			using (EDIDataRegistry.Instance.ProductDisplayCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, categories))
			{
				base.TestSettingValueCallsRefreshBinding();
			}
		}

		public void TestProductDisplayCategoryDescriptionMaxLen()
		{
			var l7 = Factory.New<ClientLicencePriceItem>();
			AssertEquals(3, l7.L7_ProductDisplayCategoryInfo.MaxLength);
			AssertEquals(256, l7.ProductDisplayCategoryDescriptionInfo.MaxLength);
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(ClientLicencePriceItem), nameof(ClientLicencePriceItem.ProductDisplayCategoryDescription), false, r => r.MaxLength == 256);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			return licCompany.PriceHeaders.AddNew().Items.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(factory, "AAA");
			return organisation.LicCompany.PriceHeaders.AddNew().Items.AddNew();
		}

		#endregion
	}
}
