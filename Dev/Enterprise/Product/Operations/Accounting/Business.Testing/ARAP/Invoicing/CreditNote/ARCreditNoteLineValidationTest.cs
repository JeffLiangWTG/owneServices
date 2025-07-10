using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNoteLine))]
	public class ARCreditNoteLineValidationTest : InvoicingLineBaseValidationTest
	{
		[TestDate(2021, 06, 01)]
		public void TestCheckAL_AT_CheckIndiaGSTReversalAllowedPeriod_India_AmendFromAR()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
			AssertEquals("PreCondition", Core.Constants.CountryCodes.India, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
			AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

			var invoiceDateWillFail = new ZDate(2021, 03, 20);
			var invoiceDateWillPass = new ZDate(2021, 04, 01);

			var arCreditNoteAmendFromAR = (ARCreditNote)Factory.New(InvoiceType);
			arCreditNoteAmendFromAR.AH_OH = GSTRegisteredOrg.PK;
			arCreditNoteAmendFromAR.OriginalTransactionReference = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS).PK;

			var line = (ARCreditNoteLine)arCreditNoteAmendFromAR.Lines.AddNew();
			var tax = TestObjectCreator.CreateTaxRate("TAX1", "", 10);
			line.AL_AT = tax.PK;
			Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;
			CombineAssertions("only IntegratedGST(INT) and Rated(RAT) are GST target, amend from AR", () =>
			{
				foreach (var taxType in line.TaxRate.Lookups.Types.GetAllCodes())
				{
					line.TaxRate.AT_Type = taxType;
					if (IndiaGSTReversalHelper.CheckIsConstraintTaxID(line.TaxRate))
					{
						line.InvoiceBase.OriginalReferenceTransaction.AH_PostDate = invoiceDateWillFail;
						line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
						((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
						AssertHasError("Financial Year End:2021-03-31, Current Date:2021-06-01 ,it should have error due to over 2 month offset",
							line.AL_ATInfo,
							IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);

						line.InvoiceBase.OriginalReferenceTransaction.AH_PostDate = invoiceDateWillFail;
						line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 05, 01);
						((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
						AssertNoErrorContaining("Financial Year End:2021-03-31, Current Date:2021-05-01 ,it should pass due to within 2 month offset",
							line.AL_ATInfo,
							IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);

						line.InvoiceBase.OriginalReferenceTransaction.AH_PostDate = invoiceDateWillPass;
						line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
						((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
						AssertNoErrorContaining("Financial Year End:2022-03-31, Current Date:2021-06-01 ,it should pass due to within 2 month offset",
							line.AL_ATInfo,
							IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);
					}
					else
					{
						line.InvoiceBase.AH_OriginalInvoiceDate = invoiceDateWillFail;
						line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
						((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
						AssertNoErrorContaining($"Tax Type:{taxType} not need to be validated",
							line.AL_ATInfo,
							IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);
					}
				}
			});

			line.AL_AT = ZGuid.Empty;
			line.InvoiceBase.OriginalReferenceTransaction.AH_PostDate = invoiceDateWillFail;
			line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
			((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
			AssertNoErrorContaining("only validate when Tax Rate is not empty",
				line.AL_ATInfo,
				IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);
			Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = true;
			line.AL_AT = tax.PK;
			line.InvoiceBase.OriginalReferenceTransaction.AH_PostDate = invoiceDateWillFail;
			line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
			AssertNotErrorWithAnyTaxType("[Have Security]", line);
		}

		[TestDate(2021, 06, 01)]
		public void TestCheckAL_AT_CheckIndiaGSTReversalAllowedPeriod_India_NotAmendFromAR()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
			AssertEquals("PreCondition", Core.Constants.CountryCodes.India, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
			AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

			var invoiceDateWillFail = new ZDate(2021, 03, 20);
			var invoiceDateWillPass = new ZDate(2021, 04, 01);

			var arCreditNote = (ARCreditNote)Factory.New(InvoiceType);
			arCreditNote.AH_OH = GSTRegisteredOrg.PK;

			var line = (ARCreditNoteLine)arCreditNote.Lines.AddNew();
			var tax = TestObjectCreator.CreateTaxRate("TAX1", "", 10);
			line.AL_AT = tax.PK;

			Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;
			CombineAssertions("only IntegratedGST(INT) and Rated(RAT) are GST target, not amend from AR", () =>
			{
				foreach (var taxType in line.TaxRate.Lookups.Types.GetAllCodes())
				{
					line.TaxRate.AT_Type = taxType;
					if (IndiaGSTReversalHelper.CheckIsConstraintTaxID(line.TaxRate))
					{
						line.InvoiceBase.AH_OriginalInvoiceDate = invoiceDateWillFail;
						line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
						((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
						AssertHasError("Financial Year End:2021-03-31, Current Date:2021-06-01 ,it should have error due to over 2 month offset",
							line.AL_ATInfo,
							IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);

						line.InvoiceBase.AH_OriginalInvoiceDate = invoiceDateWillFail;
						line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 05, 01);
						((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
						AssertNoErrorContaining("Financial Year End:2021-03-31, Current Date:2021-05-01 ,it should pass due to within 2 month offset",
							line.AL_ATInfo,
							IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);

						line.InvoiceBase.AH_OriginalInvoiceDate = invoiceDateWillPass;
						line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
						((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
						AssertNoErrorContaining("Financial Year End:2022-03-31, Current Date:2021-06-01 ,it should pass due to within 2 month offset",
							line.AL_ATInfo,
							IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);
					}
					else
					{
						line.InvoiceBase.AH_OriginalInvoiceDate = invoiceDateWillFail;
						line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
						((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
						AssertNoErrorContaining($"Tax Type:{taxType} not need to be validated",
							line.AL_ATInfo,
							IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);
					}
				}
			});

			line.AL_AT = ZGuid.Empty;
			line.InvoiceBase.AH_OriginalInvoiceDate = invoiceDateWillFail;
			line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
			((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
			AssertNoErrorContaining("Should not have error due to Tax Rate is empty",
				line.AL_ATInfo,
				IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);

			Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = true;
			line.AL_AT = tax.PK;
			line.InvoiceBase.AH_OriginalInvoiceDate = invoiceDateWillFail;
			line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
			AssertNotErrorWithAnyTaxType("[Have Security]", line);
		}

		[TestDate(2021, 06, 01)]
		public void TestCheckAL_AT_CheckIndiaGSTReversalAllowedPeriod_NonIndia()
		{
			AssertNotEquals("PreCondition", "IN", Env.CurrentCompany.Country.Code);

			var invoiceDateWillFail = new ZDate(2021, 03, 20);

			AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
			AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AT = TestObjectCreator.CreateTaxRate("TAX1", "", 10).PK;

			Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;
			line.InvoiceBase.AH_OriginalInvoiceDate = invoiceDateWillFail;
			line.InvoiceBase.AH_PostDate = new ZDateTime(2021, 06, 01);
			AssertNotErrorWithAnyTaxType("[Country not India]", line);
		}

		void AssertNotErrorWithAnyTaxType(string comment, InvoicingLineBase line)
		{
			CombineAssertions($"{comment}Should not have error in any tax rate type", () =>
			{
				foreach (var taxType in line.TaxRate.Lookups.Types.GetAllCodes())
				{
					line.TaxRate.AT_Type = taxType;

					((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
					AssertNoWarnings(line.AL_ATInfo);
				}
			});
		}

		public override void TestCheckAL_JHWithUXml_JobErrorMessages()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.GenericCharge = TestObjectCreator.FRT.PK;

			line.AL_JH = ZGuid.Empty;
			line.Validation.ValidateAL_JH();
			Assert(!line.AL_JHInfo.HasErrors());

			line.AL_JH = ZGuid.Invalid;
			line.Validation.ValidateAL_JH();
			Assert(!line.AL_JHInfo.HasErrors());
		}

		public override void TestCheckAL_JHForSelectedGLTypeCharge()
		{
			ForwardingShipment testShipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			ZQuery testFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, "P&L");
			testFilter.AddToFilter(ViewGenericChargeSchema.VC_DisallowDirectPosting, false);
			GenericCharge.GenericCharge testGenericCharge = Factory.LoadTop1(typeof(GenericCharge.GenericCharge), testFilter) as GenericCharge.GenericCharge;
			line.GenericCharge = testGenericCharge.PK;

			line.AL_JH = ZGuid.Empty;
			((CreditNoteLineValidation)line.Validation).CheckAL_JH_ForTestOnly();
			Assert(!line.AL_JHInfo.HasErrors());

			line.AL_JH = testShipment.PK;
			((CreditNoteLineValidation)line.Validation).CheckAL_JH_ForTestOnly();
			Assert(!line.AL_JHInfo.HasErrors());
		}

		public override void TestCheckAL_JHForSelectedCharge()
		{
			ForwardingShipment testShipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;

			ZQuery testFilter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, "MRG");
			AccChargeCode chargeCode = Factory.LoadTop1(typeof(AccChargeCode), testFilter) as AccChargeCode;
			chargeCode.AC_DepartmentFilterList = "ALL";

			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.GenericCharge = chargeCode.PK;
			((CreditNoteLineValidation)line.Validation).CheckAL_JH_ForTestOnly();
			Assert(!line.AL_JHInfo.HasError("You must select a job for this charge code."));

			line.AL_JH = testShipment.PK;
			((CreditNoteLineValidation)line.Validation).ValidateAL_JH();
			Assert(!line.AL_JHInfo.HasError("You must select a job for this charge code."));
		}

		public override void TestCheckAL_JHErrorMessage()
		{
			Assert(true);
		}

		public override void TestCheckAL_ATWhenRegisteredCompanyAndNonRegisteredDebtor()
		{
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			base.TestCheckAL_ATWhenRegisteredCompanyAndNonRegisteredDebtor();
		}

		protected override Type InvoiceLineType
		{
			get { return typeof(ARCreditNoteLine); }
		}

		protected override Type InvoiceType
		{
			get { return typeof(ARCreditNote); }
		}

		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(ARCreditNote);
		}
	}
}
