using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentWrappers.Testing.Accounting;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocARInvoiceLineCollection))]
	sealed class DocARInvoiceLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocARInvoiceLineCollection>
	{
		public void TestResetMultiplierTo1()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Desc = "ChargeCode Description";

			DocBatchARInvoiceLineTransactionLine line1 = DocBatchARInvoiceLineTransactionLine.New(Factory.New<ARInvoiceLine>(), Factory);
			DocBatchARInvoiceLineTransactionLine line2 = DocBatchARInvoiceLineTransactionLine.New(Factory.New<ARInvoiceLine>(), Factory);
			line1.ValueMultiplier = -1;
			line2.ValueMultiplier = -1;

			DocARInvoiceLineCollection collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(line1);
			collection.Add(line2);

			collection.ResetMultiplierTo1();

			AssertEquals(1, line1.ValueMultiplier);
			AssertEquals(1, line2.ValueMultiplier);
		}

		public void TestCombinedGroupDescription()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Desc = "ChargeCode Description";

			DocARInvoiceLine line1 = CreateInvoiceLineWithWrapper("Some Description", chargeCode);
			DocARInvoiceLine line2 = CreateInvoiceLineWithWrapper("Some Description", chargeCode);
			DocARInvoiceLine line3 = CreateInvoiceLineWithWrapper("Some Description", chargeCode);
			DocARInvoiceLine line4 = CreateInvoiceLineWithWrapper("Some Other Description", chargeCode);

			DocARInvoiceLineCollection collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(line1);
			collection.Add(line2);
			collection.Add(line3);
			AssertEquals("Invoice line descs are identical so use it", "Some Description", collection.CombinedGroupDescription);
			AssertEquals("Use charge code desc", "ChargeCode Description", collection.CombinedGroupDescriptionWithChargeAndGLAccount);

			collection.Add(line4);
			AssertEquals("Invoice line descs are no longer all matching - so fallback to charge code description", "ChargeCode Description", collection.CombinedGroupDescription);
			AssertEquals("Use charge code desc", "ChargeCode Description", collection.CombinedGroupDescriptionWithChargeAndGLAccount);

			AccGLHeader glAccount = Factory.NewWithValidTestData<AccGLHeader>();
			glAccount.AG_Description = "GL Account Description";

			DocARInvoiceLine line5 = CreateInvoiceLineWithWrapper("Some Account Description", glAccount);
			DocARInvoiceLine line6 = CreateInvoiceLineWithWrapper("Some Account Description", glAccount);
			DocARInvoiceLine line7 = CreateInvoiceLineWithWrapper("Some Account Description", glAccount);
			DocARInvoiceLine line8 = CreateInvoiceLineWithWrapper("Some other Account Description", glAccount);

			collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(line5);
			collection.Add(line6);
			collection.Add(line7);
			AssertEquals("Invoice line descs are identical so use it", "Some Account Description", collection.CombinedGroupDescription);
			AssertEquals("Use GL Account desc", "GL Account Description", collection.CombinedGroupDescriptionWithChargeAndGLAccount);

			collection.Add(line8);
			AssertEquals("Invoice line descs are no longer all matching - so fallback to account description", "GL Account Description", collection.CombinedGroupDescription);
			AssertEquals("Use GL Account desc", "GL Account Description", collection.CombinedGroupDescriptionWithChargeAndGLAccount);
		}

		public void TestCombinedGroupDescriptionWithEnableLocalChargeCodeDescriptionDefaultRegsitry()
		{
			var defaultChargeCodeDescription = "My Test Charge Code";
			var appendedLineDescription = "My Test Charge Code and some appended text";
			var overriddenLineDescription = "This is a very different charge code description";
			var chargeCodeDescriptionInGerman = "Mein Testgebührencode";

			var testObjectCreator = new TestObjectCreator(Factory);
			var chargeCode = testObjectCreator.CreateChargeCode("ABC");
			chargeCode.AC_Desc = defaultChargeCodeDescription;
			chargeCode.AC_LocalLanguageDescription = "This is my local language description";

			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("AR0001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var invoiceLine1 = testObjectCreator.CreateARInvoiceLine(invoice, null, chargeCode, testObjectCreator.AUD, 1m, defaultChargeCodeDescription, 10m);
			var invoiceLine2 = testObjectCreator.CreateARInvoiceLine(invoice, null, chargeCode, testObjectCreator.AUD, 1m, defaultChargeCodeDescription, 20m);
			var invoiceLine3 = testObjectCreator.CreateARInvoiceLine(invoice, null, chargeCode, testObjectCreator.AUD, 1m, appendedLineDescription, 30m);
			var invoiceLine4 = testObjectCreator.CreateARInvoiceLine(invoice, null, chargeCode, testObjectCreator.AUD, 1m, appendedLineDescription, 40m);
			var invoiceLine5 = testObjectCreator.CreateARInvoiceLine(invoice, null, chargeCode, testObjectCreator.AUD, 1m, overriddenLineDescription, 50m);
			var invoiceLine6 = testObjectCreator.CreateARInvoiceLine(invoice, null, chargeCode, testObjectCreator.AUD, 1m, overriddenLineDescription, 60m);
			Factory.Save();

			var docARInvoiceline1 = DocARInvoiceLine.New(invoiceLine1, Factory);
			var docARInvoiceline2 = DocARInvoiceLine.New(invoiceLine2, Factory);
			var docARInvoiceline3 = DocARInvoiceLine.New(invoiceLine3, Factory);
			var docARInvoiceline4 = DocARInvoiceLine.New(invoiceLine4, Factory);
			var docARInvoiceline5 = DocARInvoiceLine.New(invoiceLine5, Factory);
			var docARInvoiceline6 = DocARInvoiceLine.New(invoiceLine6, Factory);

			var resKey = chargeCode.AC_DescInfo.CustomizableDataResourceStrings.GetMultilingualString(chargeCode, defaultChargeCodeDescription).ResourceKey;

			//AllLinesHaveDefaultChargeDescription
			var collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(docARInvoiceline1);
			collection.Add(docARInvoiceline2);
			AssertCombinedGroupDescription(false, chargeCodeDescriptionInGerman, collection, resKey);
			AssertCombinedGroupDescription(true, defaultChargeCodeDescription, collection, resKey);

			//AllLinesHaveTheSameAppendedDescription
			collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(docARInvoiceline3);
			collection.Add(docARInvoiceline4);
			AssertCombinedGroupDescription(false, chargeCodeDescriptionInGerman + appendedLineDescription.Substring(defaultChargeCodeDescription.Length), collection, resKey);
			AssertCombinedGroupDescription(true, appendedLineDescription, collection, resKey);

			//SomeLineDescriptionsAreModifiedAndNotAllDescriptionsAreSame
			collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(docARInvoiceline1);
			collection.Add(docARInvoiceline3);
			collection.Add(docARInvoiceline5);
			AssertCombinedGroupDescription(false, chargeCodeDescriptionInGerman, collection, resKey);
			AssertCombinedGroupDescription(true, chargeCode.AC_LocalLanguageDescription, collection, resKey);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				AssertEquals("ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors is false", false, AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);
				AssertCombinedGroupDescription(true, chargeCodeDescriptionInGerman, collection, resKey);

				using (AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors is true", true, AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);
					AssertCombinedGroupDescription(true, chargeCode.AC_LocalLanguageDescription, collection, resKey);
				}
			}

			//AllLineDescriptionsAreModifiedAndAllDescriptionsAreSame
			collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(docARInvoiceline5);
			collection.Add(docARInvoiceline6);
			AssertCombinedGroupDescription(false, overriddenLineDescription, collection, resKey);
			AssertCombinedGroupDescription(true, overriddenLineDescription, collection, resKey);
		}

		void AssertCombinedGroupDescription(bool isRegistryOn, string expectedCombinedDescription, DocARInvoiceLineCollection lineCollection, string key)
		{
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryOn))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(key, new ResourceStringData(key, "Mein Testgebührencode"));
				AssertEquals(expectedCombinedDescription, lineCollection.CombinedGroupDescription);
			}
		}

		public void TestCombinedGroupDescriptionForGLAccount()
		{
			var defaultGLAccountDescription = "My Test GL Account";
			var overriddenLineDescription = "This is a very different GL account description";
			var glAccountDescriptionInGerman = "Mein Test-Hauptbuchkonto";

			var testObjectCreator = new TestObjectCreator(Factory);
			var glAccount = testObjectCreator.CreateGLHeader("TestGLAcc");
			glAccount.AG_Description = defaultGLAccountDescription;
			var resKey = glAccount.AG_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(glAccount, defaultGLAccountDescription).ResourceKey;

			var line1 = CreateInvoiceLineWithWrapper(defaultGLAccountDescription, glAccount);
			var line2 = CreateInvoiceLineWithWrapper(defaultGLAccountDescription, glAccount);
			var line3 = CreateInvoiceLineWithWrapper(overriddenLineDescription, glAccount);
			var line4 = CreateInvoiceLineWithWrapper(overriddenLineDescription, glAccount);

			//AllLinesHaveDefaultChargeDescription
			var collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(line1);
			collection.Add(line2);
			AssertCombinedGroupDescriptionForGLAccount(glAccountDescriptionInGerman, collection, resKey);

			//SomeLineDescriptionsAreModifiedAndNotAllDescriptionsAreSame
			collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(line1);
			collection.Add(line3);
			AssertCombinedGroupDescriptionForGLAccount(glAccountDescriptionInGerman, collection, resKey);

			//AllLineDescriptionsAreModifiedAndAllDescriptionsAreSame
			collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(line3);
			collection.Add(line4);
			AssertCombinedGroupDescriptionForGLAccount(overriddenLineDescription, collection, resKey);
		}

		void AssertCombinedGroupDescriptionForGLAccount(string expectedCombinedDescription, DocARInvoiceLineCollection lineCollection, string key)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(key, new ResourceStringData(key, "Mein Test-Hauptbuchkonto"));
				AssertEquals(expectedCombinedDescription, lineCollection.CombinedGroupDescription);
			}
		}

		public void TestCombinedGroupDescriptionWithNotSystemDefinedCountryDeleted()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "XX";
			country.RN_Desc = "Test XX country";

			var defaultChargeCodeDescription = "International Freight";
			var localLanguageDescription = "This is my local language description";

			var testObjectCreator = new TestObjectCreator(Factory);
			var chargeCode = testObjectCreator.FRT;
			chargeCode.AC_LocalLanguageDescription = localLanguageDescription;

			testObjectCreator.Debtor.OH_RL_NKClosestPort = "XXZZZ";

			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("AR0001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var invoiceLine = testObjectCreator.CreateARInvoiceLine(invoice, null, chargeCode, testObjectCreator.AUD, 1m, defaultChargeCodeDescription, 10m);
			Factory.Save();

			var docARInvoiceline = DocARInvoiceLine.New(invoiceLine, Factory);

			var collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(docARInvoiceline);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("XX"))
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				country.Delete();
				Factory.Save();

				AssertEquals("ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors is false", false, AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);
				AssertEquals("Default charge code description should display with no NULL Reference Exception thrown", defaultChargeCodeDescription, collection.CombinedGroupDescriptionWithChargeAndGLAccount);

				using (AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors is true", true, AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);
					AssertEquals("Local language description should display with no NULL Reference Exception thrown", localLanguageDescription, collection.CombinedGroupDescriptionWithChargeAndGLAccount);
				}
			}
		}

		public void TestIsAtLeastOneTaxRateSpecified()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGST", AccTaxRate.Types.Rated, 0);
				var otherRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, 0);

				DocARInvoiceLine line1 = CreateInvoiceLineWithTax(null);
				DocARInvoiceLine line2 = CreateInvoiceLineWithTax(null);
				DocARInvoiceLine line3 = CreateInvoiceLineWithTax(rate);

				DocARInvoiceLineCollection collection = new DocARInvoiceLineCollection(Factory);
				AssertEquals("Should be false for empty collection", false, collection.IsAtLeastOneTaxRateSpecified);
				collection.Add(line1);
				collection.Add(line2);
				AssertEquals("Should be false", false, collection.IsAtLeastOneTaxRateSpecified);

				collection.Add(line3);
				AssertEquals("Should be true", true, collection.IsAtLeastOneTaxRateSpecified);
			}
		}

		public void TestIsSameTaxRateForAllLines()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGST", AccTaxRate.Types.Rated, 0);
				var otherRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, 0);

				DocARInvoiceLine line1 = CreateInvoiceLineWithTax(rate);
				DocARInvoiceLine line2 = CreateInvoiceLineWithTax(rate);
				DocARInvoiceLine line3 = CreateInvoiceLineWithTax(rate);
				DocARInvoiceLine line4 = CreateInvoiceLineWithTax(otherRate);

				DocARInvoiceLineCollection collection = new DocARInvoiceLineCollection(Factory);
				AssertEquals("Should be false for empty collection", false, collection.IsSameTaxRateForAllLines);
				collection.Add(line1);
				collection.Add(line2);
				collection.Add(line3);
				AssertEquals("Should be true", true, collection.IsSameTaxRateForAllLines);

				collection.Add(line4);
				AssertEquals("Should be false", false, collection.IsSameTaxRateForAllLines);
			}
		}

		public void TestAsterisks()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			DocARBatchInvoiceTest.SetupForTestingInvoiceTaxMessages(invoice);
			DocARInvoiceLineCollection collection = new DocARInvoiceLineCollection(Factory);

			collection.Add(DocARInvoiceLine.New(invoice.Lines[0], Factory));
			collection.Add(DocARInvoiceLine.New(invoice.Lines[4], Factory));
			collection.Add(DocARInvoiceLine.New(invoice.Lines[2], Factory));
			collection.Add(DocARInvoiceLine.New(invoice.Lines[3], Factory));
			collection.Add(DocARInvoiceLine.New(invoice.Lines[1], Factory));

			AssertEquals(" *", collection[0].TaxRateAsterisks);
			AssertEquals(" *****", collection[1].TaxRateAsterisks);
			AssertEquals(" ***", collection[2].TaxRateAsterisks);
			AssertEquals(" ****", collection[3].TaxRateAsterisks);
			AssertEquals(" **", collection[4].TaxRateAsterisks);

			AssertEquals(" *,**,***,****,*****", collection.Asterisks);
		}

		public void TestFirstReportableTaxRate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGST", AccTaxRate.Types.Rated, 0);

				AccTaxRate notReportableRate = AccTaxRate.GetNOTREPORTTaxID(Factory, GlbCompany.CurrentCompany);
				DocARInvoiceLine line1 = CreateInvoiceLineWithTax(notReportableRate);
				DocARInvoiceLine line2 = CreateInvoiceLineWithTax(notReportableRate);
				DocARInvoiceLine line3 = CreateInvoiceLineWithTax(rate);

				DocARInvoiceLineCollection collection = new DocARInvoiceLineCollection(Factory);
				AssertNull("HasAtLeastOneReportableTaxRate should be null for empty collection", collection.FirstReportableTaxRate.Rate);
				collection.Add(line1);
				collection.Add(line2);
				AssertNull("FirstReportableTaxRate", collection.FirstReportableTaxRate.Rate);

				collection.Add(line3);
				AssertNotNull("FirstReportableTaxRate", collection.FirstReportableTaxRate);
				AssertEquals("FirstReportableTaxRate should wrap original rate", rate, collection.FirstReportableTaxRate.Rate.AccTaxRate);
			}
		}

		public void TestOnlyOneJob()
		{
			DocARInvoiceLineCollection collection = new DocARInvoiceLineCollection(Factory);
			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			AssertEquals("No Job on empty collection", false, collection.OnlyOneJob);

			DocARInvoiceLine line1 = CreateInvoiceLineWithJob(job1);
			collection.Add(line1);
			AssertEquals("Should be OneJob", true, collection.OnlyOneJob);

			DocARInvoiceLine line2 = CreateInvoiceLineWithJob(job1);
			collection.Add(line2);
			AssertEquals("Should be OneJob", true, collection.OnlyOneJob);

			DocARInvoiceLine line3 = CreateInvoiceLineWithJob(job2);
			DocARInvoiceLine line4 = CreateInvoiceLineWithJob(null);

			collection.Add(line3);
			AssertEquals("Not OneJob - contains line with different Job", false, collection.OnlyOneJob);

			collection.Remove(line3);
			AssertEquals("Should be OneJob", true, collection.OnlyOneJob);

			collection.Add(line4);
			AssertEquals("Not OneJob - contains line without Job", false, collection.OnlyOneJob);
		}

		public void TestOneCurrencyExchangeRate()
		{
			var defaultValue = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch
				(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All,
				 OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);

			defaultValue.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			defaultValue.InvoiceLineDisplayOption = "ROL";
			defaultValue.GroupOrSubtotalStyle = "CCD";
			defaultValue.InvoiceLineDisplayOption = "ALX";
			defaultValue.InvoicePostingStyle = "FIO";

			var registryCollection = new InvoiceRollupOrGroupCollection();
			registryCollection.Add(defaultValue);

			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCollection);

			var testObjectCreator = new TestObjectCreator(Factory);

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "USMIZ";
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_ParentID = shipment.PK;
			Factory.Save();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = testObjectCreator.AALSHI.PK;

			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.AL_JH = jobHeader.PK;
			line1.GenericCharge = testObjectCreator.CC3.PK;
			line1.AL_OSExTaxAmount = 100m;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_RX_NKSellCurrency = "UAH";
			charge1.JR_AL_ARLine = line1.PK;
			charge1.JR_OSSellAmt = 200;
			charge1.JR_OSSellExRate = 2m;

			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			line2.AL_JH = jobHeader.PK;
			line2.GenericCharge = testObjectCreator.CC2.PK;
			line2.AL_OSExTaxAmount = 10m;

			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_RX_NKSellCurrency = "UAH";
			charge2.JR_AL_ARLine = line2.PK;
			charge2.JR_OSSellAmt = 20;
			charge2.JR_OSSellExRate = 2m;

			InvoicingLineBase line3 = (InvoicingLineBase)invoice.Lines.AddNew();
			line3.GenericCharge = testObjectCreator.CC1.PK;
			line3.AL_JH = jobHeader.PK;
			line3.AL_OSExTaxAmount = 20m;

			JobCharge charge3 = Factory.NewWithValidTestData<JobCharge>();
			charge3.JR_RX_NKSellCurrency = "UAH";
			charge3.JR_AL_ARLine = line3.PK;
			charge3.JR_OSSellAmt = 60m;
			charge3.JR_OSSellExRate = 3m;

			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

			DocARInvoiceLineCollection collection = new DocARInvoiceLineCollection(Factory);
			collection.Add(DocARInvoiceLine.New(line1, Factory));
			collection.Add(DocARInvoiceLine.New(line2, Factory));
			AssertEquals("Should be 2m", 2m, collection.OneCurrencyExchangeRate);

			collection.Add(DocARInvoiceLine.New(line3, Factory));
			AssertEquals("Should be 2.1538m", 2.153846m, collection.OneCurrencyExchangeRate);  //(60+20+200)  / (100+10+20) = 2.153846154
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ARInvoiceLine invoiceLine = Factory.New<ARInvoiceLine>();
			return DocARInvoiceLine.New(invoiceLine, Factory);
		}

		protected override DocARInvoiceLineCollection GetCollectionToTest()
		{
			return DocARInvoiceLineCollection.New(Factory);
		}

		DocARInvoiceLine CreateInvoiceLineWithWrapper(string description, AccChargeCode chargeCode)
		{
			ARInvoiceLine invoiceLine = Factory.New<ARInvoiceLine>();
			invoiceLine.AL_AC = chargeCode.PK;
			invoiceLine.AL_Desc = description;
			return DocARInvoiceLine.New(invoiceLine, Factory);
		}

		DocARInvoiceLine CreateInvoiceLineWithWrapper(string description, AccGLHeader glHeader)
		{
			ARInvoiceLine invoiceLine = Factory.New<ARInvoiceLine>();
			invoiceLine.AL_AG = glHeader.PK;
			invoiceLine.AL_Desc = description;
			return DocARInvoiceLine.New(invoiceLine, Factory);
		}

		DocARInvoiceLine CreateInvoiceLineWithTax(AccTaxRate rate)
		{
			ARInvoiceLine invoiceLine = Factory.New<ARInvoiceLine>();
			invoiceLine.AL_AT = rate != null ? rate.PK : ZGuid.Empty;
			return DocARInvoiceLine.New(invoiceLine, Factory);
		}

		DocARInvoiceLine CreateInvoiceLineWithJob(JobHeader job)
		{
			ARInvoiceLine invoiceLine = Factory.New<ARInvoiceLine>();
			invoiceLine.AL_JH = job != null ? job.PK : ZGuid.Empty;
			return DocARInvoiceLine.New(invoiceLine, Factory);
		}

		#endregion
	}
}
