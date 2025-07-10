using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocTaxTransaction))]
	sealed class DocTaxTransactionTest : DocumentWrapperTestCase, IHaveTaxFrameworkTestObjectCreator
	{
		[TestDate(2020, 9, 2)]
		public void TestProperties()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();

			var taxAuthorityCode = "AUATO";
			var taxAuthorityDescription = "Australian Taxation Office";
			var taxAuthority = this.CreateTaxAuthority(taxAuthorityCode, taxAuthorityDescription);

			var taxSystemCode = "AUPER";
			var taxSystemDescription = "Perceptions tax";
			var taxSystem = this.CreateTaxSystem(taxSystemCode, taxSystemDescription);

			var taxDescription = "PERCEPTION tax";
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_TaxAuthorityCode = taxAuthorityCode;
			taxConfiguration.ETC_Description = taxDescription;

			var englishTaxMessage = "Perception tax reportable to ATO";
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			taxMessage.A9_EnglishMsg = englishTaxMessage;

			var taxSuperType = TaxSuperTypeList.Perceptions.Code;
			var osTaxAmount = 10M;
			var localTaxAmount = 20M;
			var osTaxBaseAmount = 100M;
			var localTaxBaseAmount = 200M;
			var taxNumerator = 1475;
			var taxDenominator = 1000;
			var taxAuthorityServiceCode = "200.89";
			var taxAuthorityServiceCodeDescription = "ARTICLE 98.7 2007";

			TaxTransaction.ATT_AH = invoice.PK;
			TaxTransaction.ATT_A9_TaxMessage = taxMessage.PK;
			TaxTransaction.ATT_OSTaxAmount = 0;     // Allows ATT_ETC value to be set
			TaxTransaction.ATT_LocalTaxAmount = 0;  // Allows ATT_ETC value to be set
			TaxTransaction.ATT_ETC = taxConfiguration.PK;
			TaxTransaction.ATT_TaxSuperType = taxSuperType;
			TaxTransaction.ATT_AffectsSourceTransactionTotal = true;
			TaxTransaction.ATT_OSTaxAmount = osTaxAmount;
			TaxTransaction.ATT_LocalTaxAmount = localTaxAmount;
			TaxTransaction.ATT_OSTaxBaseAmount = osTaxBaseAmount;
			TaxTransaction.ATT_LocalTaxBaseAmount = localTaxBaseAmount;
			TaxTransaction.ATT_RateNumerator = taxNumerator;
			TaxTransaction.ATT_RateDenominator = taxDenominator;
			TaxTransaction.ATT_RealisationDate = ZDate.Today;
			TaxTransaction.ATT_TaxSystemCode = taxSystemCode;
			TaxTransaction.ATT_TaxAuthorityServiceCode = taxAuthorityServiceCode;
			TaxTransaction.ATT_TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription;

			var docInvoice = DocARInvoice.New(invoice, Factory);
			var docTaxTransaction = DocTaxTransaction.New(TaxTransaction, Factory);

			var taxAuthorityCodeDescriptionPair = new CodeDescriptionPair(taxAuthorityCode, taxAuthorityDescription);
			var taxSystemCodeDescriptionPair = new CodeDescriptionPair(taxSystemCode, taxSystemDescription);

			TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper()
								.WithGetTaxAuthorities(Core.Constants.CountryCodes.Australia, null, new CodeDescriptionPairList { taxAuthorityCodeDescriptionPair })
								.WithGetTaxSystems(Core.Constants.CountryCodes.Australia, ZString.Empty, new CodeDescriptionPairList { taxSystemCodeDescriptionPair });

			AssertEquals("TaxSuperType", taxSuperType, docTaxTransaction.TaxSuperType);
			AssertEquals("IncludeInInvoiceAmount", true, docTaxTransaction.IncludeInInvoiceAmount);

			var multiplier = (invoice as ITransactionHeader).Multiplier;
			CombineAssertions(() =>
			{
				AssertEquals("OSAmount", multiplier * osTaxAmount, docTaxTransaction.OSAmount);
				AssertEquals("LocalAmount", multiplier * localTaxAmount, docTaxTransaction.LocalAmount);
				AssertEquals("OSBaseAmount", multiplier * osTaxBaseAmount, docTaxTransaction.OSBaseAmount);
				AssertEquals("LocalBaseAmount", multiplier * localTaxBaseAmount, docTaxTransaction.LocalBaseAmount);

				AssertEquals("TaxRate", "1.475%", docTaxTransaction.TaxRate);
				AssertEquals("IsRealized", true, docTaxTransaction.IsRealized);

				AssertEquals("TaxSystemCode", taxSystemCode, docTaxTransaction.TaxSystemCode);
				AssertEquals("TaxSystemDescription", taxSystemDescription, docTaxTransaction.TaxSystemDescription);
				AssertEquals("TaxAuthorityCode", taxAuthorityCode, docTaxTransaction.TaxAuthorityCode);
				AssertEquals("TaxAuthorityDescription", taxAuthorityDescription, docTaxTransaction.TaxAuthorityDescription);
				AssertEquals("TaxAuthorityServiceCode", taxAuthorityServiceCode, docTaxTransaction.TaxAuthorityServiceCode);
				AssertEquals("TaxAuthorityServiceCodeDescription", taxAuthorityServiceCodeDescription, docTaxTransaction.TaxAuthorityServiceCodeDescription);
				AssertEquals("TaxMessageText", englishTaxMessage, docTaxTransaction.TaxMessageText);
				AssertEquals("TaxConfigurationDescription", taxDescription, docTaxTransaction.TaxConfigurationDescription);
			});
		}

		public void TestTaxSystemsUsesCountryFromTaxTransactionCompany()
		{
			var loginCountry = Core.Constants.CountryCodes.Mexico;
			GlbCompany.CurrentCompany.SetCountry(loginCountry);

			var taxTransactionCountry = Core.Constants.CountryCodes.Australia;
			TestObjectCreator.NonCurrentCompany.SetCountry(taxTransactionCountry);

			var taxSystemAU = this.CreateTaxSystem("AUPER", "Australian Perceptions Tax", taxTransactionCountry);

			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_GC = TestObjectCreator.NonCurrentCompany.PK;
			taxTransaction.ATT_TaxSystemCode = taxSystemAU.Code;

			var docTaxTransaction = DocTaxTransaction.New(taxTransaction, Factory);

			var taxSystemCodeDescriptionPair = new CodeDescriptionPair("AUPER", "Australian Perceptions Tax");
			TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper().WithGetTaxSystems(Core.Constants.CountryCodes.Australia, ZString.Empty, new CodeDescriptionPairList { taxSystemCodeDescriptionPair });

			AssertEquals("TaxSystemCode", taxSystemAU.Code, docTaxTransaction.TaxSystemCode);
			AssertEquals("TaxSystemDescription", taxSystemAU.Name, docTaxTransaction.TaxSystemDescription);
		}

		public void TestTaxAuthoritiesUsesCountryFromTaxTransactionCompany()
		{
			var loginCountry = Core.Constants.CountryCodes.Mexico;
			GlbCompany.CurrentCompany.SetCountry(loginCountry);

			var taxTransactionCountry = Core.Constants.CountryCodes.Australia;
			TestObjectCreator.NonCurrentCompany.SetCountry(taxTransactionCountry);

			var taxAuthorityAU = this.CreateTaxAuthority("ATO", "Australian Taxation Office", taxTransactionCountry);

			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_TaxAuthorityCode = taxAuthorityAU.Code;
			taxConfiguration.ETC_Description = "Blah blah";

			var taxTransaction = Factory.New<AccTaxTransaction>();
			taxTransaction.ATT_GC = TestObjectCreator.NonCurrentCompany.PK;
			taxTransaction.ATT_ETC = taxConfiguration.PK;

			var docTaxTransaction = DocTaxTransaction.New(taxTransaction, Factory);

			var taxAuthorityCodeDesc_AU = new CodeDescriptionPair("ATO", "Australian Taxation Office");
			TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper().WithGetTaxAuthorities(Core.Constants.CountryCodes.Australia, null, new CodeDescriptionPairList { taxAuthorityCodeDesc_AU });

			AssertEquals("TaxAuthorityCode", taxAuthorityAU.Code, docTaxTransaction.TaxAuthorityCode);
			AssertEquals("TaxAuthorityDescription", taxAuthorityAU.Name, docTaxTransaction.TaxAuthorityDescription);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var docInvoice = DocARInvoice.New(invoice, Factory);
			return new DocumentWrapper[]
				{
					DocTaxTransaction.New(TaxTransaction, Factory)
				};
		}

		protected override void SetUp()
		{
			TaxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();

			base.SetUp();
		}

		AccTaxTransaction TaxTransaction;

		TaxFrameworkTestObjectCreator IHaveTaxFrameworkTestObjectCreator.TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
