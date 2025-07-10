using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(
				Factory.New<JobComInvoiceHeader>(),
				"CNJobComInvoiceHeader");
		}

		public void TestOnJZ_PaymentOfRoyaltyConfirmChanges()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			var charges = invoice.GroupHeader.Charges;
			AssertEquals("Ryt charge has not been added yet", 0, charges.Count);
			invoice.JZ_PaymentOfRoyaltyConfirm = ConfirmationTypeList.Codes.Yes;
			AssertEquals("Ryt charge should be added", 1, charges.Count);
			Assert("Ryt charge code should be added", charges.Cast<JobComInvCharge>().Last().J7_ChargeType == CustomsChargeTypeList.Codes.Royalty);
			invoice.JZ_PaymentOfRoyaltyConfirm = ConfirmationTypeList.Codes.No;
			AssertEquals("Should remain one charge code", 1, charges.Count);
			Assert("Ryt charge should remain", charges.Cast<JobComInvCharge>().Last().J7_ChargeType == CustomsChargeTypeList.Codes.Royalty);
			invoice.JZ_PaymentOfRoyaltyConfirm = ConfirmationTypeList.Codes.Yes;
			AssertEquals("Should remain one charge code", 1, charges.Count);
			Assert("Ryt charge should remain", charges.Cast<JobComInvCharge>().Last().J7_ChargeType == CustomsChargeTypeList.Codes.Royalty);
		}

		public void TestRefreshDefaultsWhenInvoiceAttachedToDeclarationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			Assert(invoiceLine1.JI_CEI != entryInstruction1.PK);
			Assert(invoiceLine2.JI_CEI != entryInstruction1.PK);
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_JE = declaration.PK;
			Assert(invoiceLine1.JI_CEI != entryInstruction1.PK);
			Assert(invoiceLine2.JI_CEI != entryInstruction1.PK);
			invoice.JZ_JE = ZGuid.Empty;
			declaration.CustomsEntryInstructions.Remove(entryInstruction2);
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_JE = declaration.PK;
			Assert(invoiceLine1.JI_CEI == entryInstruction1.PK);
			Assert(invoiceLine2.JI_CEI == entryInstruction1.PK);
		}

		public override void TestDefaultINCOFromSupplier()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			invoiceHeader.JZ_OH_Supplier = consignor.PK;
			AssertEquals("Defaulted Inco Term", "", invoiceHeader.JZ_IncoTerm);
			invoiceHeader.JZ_IncoTerm = "234";
			invoiceHeader.JZ_OH_Supplier = consignor.PK;
			AssertEquals("Overridden Inco Term", "234", invoiceHeader.JZ_IncoTerm);
		}

		public void TestJZ_IncoTerm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			var invoiceHeader = declaration.Invoices.AddNew();
			Assert(!invoiceHeader.JZ_IncoTermInfo.ReadOnly);
			AssertEquals(CNInvoiceHeaderIncoTermList.Codes.CIF, invoiceHeader.JZ_IncoTerm);
		}

		public void TestContractNumbers()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			AssertEquals(typeof(JobComInvoiceHeaderContractCollection), invoice.ContractNumbers.GetType());
			var ref1 = Factory.New<JobComInvoiceHeaderContract>();
			ref1.J2_ReferenceType = "CTR";
			ref1.J2_ReferenceNumber = "ABC";
			ref1.J2_JZ = invoice.PK;
			var ref2 = Factory.New<JobComInvoiceHeaderContract>();
			ref2.J2_ReferenceType = "CTR";
			ref2.J2_ReferenceNumber = "DEF";
			ref2.J2_JZ = invoice.PK;
			AssertEquals(2, invoice.ContractNumbers.Count);
			AssertEquals("ABC,DEF", invoice.ContractNumbersAsString);
			invoice.ContractNumbersAsString = "AAA,BBB,CCC,, , E   E E";
			AssertEquals(4, invoice.ContractNumbers.Count);
			AssertEquals("AAA,BBB,CCC,E   E E", invoice.ContractNumbersAsString);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestDefaultsForNewChild()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Should be 0", ConfirmationTypeList.Codes.No, invoice.JZ_SpecialRelationshipConfirm);
			AssertEquals("Should be 0", ConfirmationTypeList.Codes.No, invoice.JZ_PriceAffectConfirm);
			AssertEquals("Should be 0", ConfirmationTypeList.Codes.No, invoice.JZ_PaymentOfRoyaltyConfirm);
		}

		public void TestEffectiveMarksAndNumbers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MarksAndNumbers = "JE_MarksAndNumbers";
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("JE_MarksAndNumbers", invoice.EffectiveMarksAndNumbers);
			invoice.JZ_MarksAndNumbers = Core.Constants.ContainerMarking.NoMarks;
			AssertEquals(Core.Constants.ContainerMarking.NoMarks, invoice.EffectiveMarksAndNumbers);
			invoice.JZ_MarksAndNumbers = "TestMarksAndNumbers";
			AssertEquals("TestMarksAndNumbers", invoice.EffectiveMarksAndNumbers);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList = commonInvoice.ChargeTypeList;
			AssertNotNullOrEmpty("ChargeTypeList has 'RYT'", chargeTypeList.GetDescriptionFromCode("RYT"));
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList = commonInvoice.ChargeTypeList;
			AssertNullOrEmpty("ChargeTypeList not has 'RYT'", chargeTypeList.GetDescriptionFromCode("RYT"));
		}

		public void TestPricingConfirms()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();

			AssertEquals("Default value of JZ_Calc_FormulaPricingConfirm", ConfirmationTypeList.Codes.Uncertain, invoice.JZ_Calc_FormulaPricingConfirm);
			AssertEquals("Default value of JZ_Calc_TemporaryPricingConfirm", ConfirmationTypeList.Codes.Uncertain, invoice.JZ_Calc_TemporaryPricingConfirm);

			void SaveAndAssertPricingConfirms(string formulaConfirm, string temporaryConfirm, string valuationCode)
			{
				invoice.JZ_Calc_FormulaPricingConfirm = formulaConfirm;
				invoice.JZ_Calc_TemporaryPricingConfirm = temporaryConfirm;

				AssertEquals("JZ_Calc_FormulaPricingConfirm", formulaConfirm, invoice.JZ_Calc_FormulaPricingConfirm);
				AssertEquals("JZ_Calc_TemporaryPricingConfirm", temporaryConfirm, invoice.JZ_Calc_TemporaryPricingConfirm);
				AssertEquals("JZ_ValuationCode", valuationCode, invoice.JZ_ValuationCode);
			}

			SaveAndAssertPricingConfirms(ConfirmationTypeList.Codes.No, string.Empty, "0");
			SaveAndAssertPricingConfirms(string.Empty, ConfirmationTypeList.Codes.No, " 0");
			SaveAndAssertPricingConfirms(string.Empty, string.Empty, "");
			SaveAndAssertPricingConfirms("X", "X", "XX");
		}

		public void TestPricingConfirmInfos()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var temporaryConfirmInfo = invoice.JZ_Calc_TemporaryPricingConfirmInfo;
			var formulaConfirmInfo = invoice.JZ_Calc_FormulaPricingConfirmInfo;

			CombineAssertions(() =>
			{
				AssertEquals("MaxLengthAttribute", 1, temporaryConfirmInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
				AssertEquals("ResourceStringDataAttribute", "Temporary Pricing Confirm", temporaryConfirmInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
				AssertEquals("ResourceStringDataAttribute", "Lookups.ConfirmationTypeList", temporaryConfirmInfo.GetAttribute<ListAttribute>().ListDataSourceMember);

				AssertEquals("MaxLengthAttribute", 1, formulaConfirmInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
				AssertEquals("ResourceStringDataAttribute", "Formula Pricing Confirm", formulaConfirmInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
				AssertEquals("ResourceStringDataAttribute", "Lookups.ConfirmationTypeList", formulaConfirmInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			});
		}

		public void TestAnyLineHasFormulaPricingRecordNumber()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertEquals("AnyLineHasFormulaPricingRecordNumber should be false", false, invoice.AnyLineHasFormulaPricingRecordNumber);
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.FormulaPricingRecordNumber = ZString.Empty;
			AssertEquals("AnyLineHasFormulaPricingRecordNumber should be false", false, invoice.AnyLineHasFormulaPricingRecordNumber);
			invoiceLine.FormulaPricingRecordNumber = "999";
			AssertEquals("AnyLineHasFormulaPricingRecordNumber should be true", true, invoice.AnyLineHasFormulaPricingRecordNumber);
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals("AnyLineHasFormulaPricingRecordNumber should be true", true, invoice.AnyLineHasFormulaPricingRecordNumber);
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.China;

		protected override bool RatesAreReciprocal => true;

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);
	}
}
