using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.BR.Business.Constants;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryLine to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLine)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestImportLicenseNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var licEntryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			licEntryInstruction.CEI_JE = licDeclaration.PK;
			licEntryInstruction.CEI_Description = "TEST1";

			var licInvHeader = licDeclaration.Invoices.AddNew();
			var licInvLine = licInvHeader.InvoiceLines.AddNew();
			licInvLine.JI_CEI = licEntryInstruction.PK;

			var licEntryHeader = licDeclaration.CustomsEntryHeaders.AddNew();
			licEntryHeader.CH_CEI_Instruction = licEntryInstruction.PK;
			var licEntryLine = licEntryHeader.AllEntryLines.AddNew();
			licInvLine.JI_CL = licEntryLine.PK;
			licInvLine.ImportLicenseNumber = "TST_LIC";
			declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licEntryInstruction) });
			DoMerge(declaration);
			Factory.Save();

			var cusEntryHeader = declaration.CustomsEntryHeaders[0];
			var cusEntryLine = cusEntryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault();
			AssertEquals(licDeclaration.DeclarationNumber, cusEntryLine.ImportLicenseNumber);
		}

		public void TestTaxes()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000", 60m);
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 30m;
			invoiceLine.JI_CL = line.PK;

			var fee = line.Fees.AddOrUpdate(Enterprise.Customs.Business.ChargeTypesList.Codes.DTY, 74.88m);
			fee.CF_BaseValue = 200.05m;
			fee.CF_Rate = 5m;

			fee = line.Fees.AddOrUpdate(Constants.RateTypes.IPI, 25.88m);
			fee.CF_BaseValue = 200.05m;
			fee.CF_Rate = 6m;

			fee = line.Fees.AddOrUpdate(Constants.RateTypes.PIS, 21.88m);
			fee.CF_BaseValue = 200.05m;
			fee.CF_Rate = 7m;

			fee = line.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 87.88m);
			fee.CF_BaseValue = 200.05m;
			fee.CF_Rate = 4m;

			fee = line.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 12.15m);
			fee.CF_BaseValue = 200.05m;
			fee.CF_Rate = 2m;

			invoiceLine.DutyTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			invoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine.JI_Tariff = "09022000";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.Normal;

			CombineAssertions(() =>
			{
				AssertEquals("DutyChargePaybleAmount", 1.5m, line.DutyDueAmount);
				AssertEquals("DutyAgreementRate", 0m, line.DutyAgreementRate);
				AssertEquals("IPIChargePaybleAmount", 25.88m, line.IPIDueAmount);
				AssertEquals("PISChargePaybleAmount", 21.88m, line.PISDueAmount);
				AssertEquals("COFINSChargePaybleAmount", 87.88m, line.CofinsDueAmount);
				AssertEquals("AntidumpingChargePaybleAmount", 12.15m, line.AntidumpingDueAmount);
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.FreeTradeAgreement;
			AssertEquals("DutyAgreementRate", 5m, line.DutyAgreementRate);

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
			invoiceLine.DutyTaxRegime = TaxRegimeList.Codes.Reduction;
			invoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.Reduction;
			invoiceLine.IPITaxRegime = TaxRegimeList.Codes.Reduction;
			CombineAssertions(() =>
			{
				AssertEquals("DutyReducedRate", 5m, line.DutyReducedRate);
				AssertEquals("PISCofinsReducedRate", 7m, line.PISCofinsReducedRate);
				AssertEquals("IPIReducedRate", 0m, line.IPIReducedRate);
				AssertEquals("DutyReductionPercentage", 0m, line.DutyReductionPercentage);
				AssertEquals("PISReductionPercentage", 0m, line.PISCofinsReductionPercentage);
			});

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
			invoiceLine.DutyRateIsOverridden = true;
			invoiceLine.ReductionMarginRateValue = 50m;
			CombineAssertions(() =>
			{
				AssertEquals("DutyReducedRate", 0m, line.DutyReducedRate);
				AssertEquals("PISReducedRate", 7m, line.PISCofinsReducedRate);
				AssertEquals("IPIReducedRate", 0m, line.IPIReducedRate);
				AssertEquals("DutyReductionPercentage", 50m, line.DutyReductionPercentage);
				AssertEquals("PISReductionPercentage", 0m, line.PISCofinsReductionPercentage);
			});
		}

		public void TestIsActive()
		{
			var entryLine = Factory.New<CusEntryLine>();

			entryLine.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Deleted;
			Assert(!entryLine.IsActive);

			entryLine.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.DeletePending;
			Assert(!entryLine.IsActive);

			entryLine.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			Assert(entryLine.IsActive);

			entryLine.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
			Assert(entryLine.IsActive);

			entryLine.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;
			Assert(entryLine.IsActive);
		}

		public void TestCanBeLinkedUpByPivot()
		{
			var additionalLine = Factory.New<JobComInvoiceLine>();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = entryLine1.PK;
			var additional = entryLine1.AdditionalInvoiceLineLinks.AddNew();
			additional.BU_JI = additionalLine.PK;
			additional.BU_CL = entryLine1.PK;
			var collection = new InvoiceLinesForEntryLineCollection(entryLine1);
			collection.Load();
			AssertEquals(1, collection.Count);

			entryHeader.CH_MessageType = MessageTypeList.Codes.SUF;
			collection = new InvoiceLinesForEntryLineCollection(entryLine1);
			collection.Load();
			AssertEquals(2, collection.Count);
		}

		#region Implementation

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			if (declaration.JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMerge)
			{
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			}
		}

		#endregion

		#region Override

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		public override void TestMoneyInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			newCurrency.SetCustomsRate(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), RatesAreReciprocal ? 2m : 0.5m);

			var declaration = ImportJobDeclaration;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line2.JI_LinePrice = 200.0m;

			var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line1ONS.J7_Amount = 5.0m;
			var line1OFC = line1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect);
			line1OFC.J7_Amount = 5.0m;
			var line1OFP = line1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid);
			line1OFP.J7_Amount = 5.0m;
			var line2ONS = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line2ONS.J7_Amount = 10.0m;
			var line2OFC = line2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect);
			line2OFC.J7_Amount = 10.0m;
			var line2OFP = line2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid);
			line2OFP.J7_Amount = 10.0m;

			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("FOB", 600.0m, entryLine.FOBInLocalCurrency.Amount);
				AssertEquals("CIF", 690.0m, entryLine.CIFInLocalCurrency.Amount);
				AssertEquals("Overseas Freight", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
				AssertEquals("Overseas Insurance", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
				AssertEquals("T and I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
			});
		}

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		#endregion
	}
}
