using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
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
	[TestedType(typeof(DocTaxTransactionLinePivot))]
	sealed class DocTaxTransactionLinePivotTest : DocumentWrapperTestCase, IHaveTaxFrameworkTestObjectCreator
	{
		[TestDate(2021, 10, 11)]
		public void TestLinkedTaxTransactionAndLine()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			#region Tax System Setup

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

			#endregion

			TaxTransaction.ATT_AH = invoice.PK;
			TaxTransaction.ATT_OSTaxAmount = TaxTransaction.ATT_LocalTaxAmount = ZDecimal.Zero; // Amounts clean up to be able to set other properties
			TaxTransaction.ATT_A9_TaxMessage = taxMessage.PK;
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

			var transactionLine = (InvoicingLineBase)invoice.Lines.AddNew();
			transactionLine.FillWithValidTestData();
			transactionLine.AL_Desc = "A Line Description";
			transactionLine.AL_LineAmount = localTaxBaseAmount;
			transactionLine.AL_GSTVAT = localTaxAmount;
			transactionLine.AL_RX_NKTransactionCurrency = "USD";
			transactionLine.AL_ExchangeRate = 0.5m;
			transactionLine.AL_OSExTaxAmount = osTaxBaseAmount;
			transactionLine.AL_OSTaxAmount = osTaxAmount;
			var taxableLine = (ITaxableTransactionLine)TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(transactionLine);
			TaxTransactionLinePivot.LinkLine(taxableLine);

			var docInvoice = DocARInvoice.New(invoice, Factory);
			var docTaxTransactionLinePivot = DocTaxTransactionLinePivot.New(TaxTransactionLinePivot, Factory);

			var taxAuthorityCodeDescriptionPair = new CodeDescriptionPair(taxAuthorityCode, taxAuthorityDescription);
			var taxSystemCodeDescriptionPair = new CodeDescriptionPair(taxSystemCode, taxSystemDescription);

			TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper()
								.WithGetTaxAuthorities(Core.Constants.CountryCodes.Australia, null, new CodeDescriptionPairList { taxAuthorityCodeDescriptionPair })
								.WithGetTaxSystems(Core.Constants.CountryCodes.Australia, ZString.Empty, new CodeDescriptionPairList { taxSystemCodeDescriptionPair });

			var docTaxTransaction = docTaxTransactionLinePivot.TaxTransaction;
			var docTransactionLine = docTaxTransactionLinePivot.Line;
			AssertNotNull("TaxTransaction", docTaxTransaction);
			AssertNotNull("Line", docTransactionLine);

			var multiplier = (invoice as ITransactionHeader).Multiplier;
			CombineAssertions(() =>
			{
				AssertEquals("TaxSuperType", taxSuperType, docTaxTransaction.TaxSuperType);
				AssertEquals("IncludeInInvoiceAmount", true, docTaxTransaction.IncludeInInvoiceAmount);

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

				AssertEquals("Description", "A Line Description", docTransactionLine.Description);
				AssertEquals("Currency.Code", "USD", docTransactionLine.Currency?.Code);
				AssertEquals("OSExTaxAmount", multiplier * osTaxBaseAmount, docTransactionLine.OSExTaxAmount);
				AssertEquals("OSTaxAmount", multiplier * osTaxAmount, docTransactionLine.OSTaxAmount);
				AssertEquals("LineAmount", multiplier * localTaxBaseAmount, docTransactionLine.LineAmount);
				AssertEquals("GSTVAT", multiplier * localTaxAmount, docTransactionLine.GSTVAT);
			});
		}

		public override DocumentWrapper[] GetDocumentWrappers()
			=> new DocumentWrapper[]
				{
					DocTaxTransactionLinePivot.New(TaxTransactionLinePivot, Factory)
				};

		protected override void SetUp()
		{
			TaxTransactionLinePivot = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			TaxTransaction = TaxTransactionLinePivot.TaxTransaction;
			_ = TaxTransactionLinePivot.TransactionLine;

			base.SetUp();
		}

		AccTaxTransaction TaxTransaction;
		AccTaxRecordTransactionLinePivot TaxTransactionLinePivot;

		TaxFrameworkTestObjectCreator IHaveTaxFrameworkTestObjectCreator.TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
