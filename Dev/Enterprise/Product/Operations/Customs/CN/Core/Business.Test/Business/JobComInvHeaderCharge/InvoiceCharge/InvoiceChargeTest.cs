using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestIsJ7_IsDutiable_ReadOnly()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceCharge = invoiceHeader.Charges.AddNew();
			Assert(!invoiceCharge.J7_IsDutiableInfo.ReadOnly);
			Assert(!invoiceCharge.J7_IsGSTApplicableInfo.ReadOnly);
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			Assert(invoiceCharge.J7_IsDutiableInfo.ReadOnly);
			Assert(invoiceCharge.J7_IsGSTApplicableInfo.ReadOnly);
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			Assert(invoiceCharge.J7_IsDutiableInfo.ReadOnly);
			Assert(invoiceCharge.J7_IsGSTApplicableInfo.ReadOnly);
		}

		public override void TestReapportionAllChargesWhenChargeCodeChargeKeyChanged()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var testDec = Factory.New<JobDeclaration>();
				var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoice.Charges.AddNew();
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				invoice.JZ_InvoiceAmount = 1000;
				invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100);
				var groupHeader = invoice.Master as BaseJobComInvoiceGroupHeader;
				var lCH = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100, invoice.JobDeclaration.LocalCurrencyCode);
				lCH.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();
				AssertEquals("One apportioned Charge for Invoice", 1, invoice.GroupCharges.Count);
				var invoiceLCH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100);
				invoiceLCH.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();
				AssertEquals("No apportioned Charge for invoice", 0, invoice.GroupCharges.Count);
				invoice.Charges.RemoveAndDelete(invoiceLCH);
				testDec.ResumeApportionment();
				AssertEquals("One apportioned Charge for Invoice", 1, invoice.GroupCharges.Count);
			}
		}

		public void TestShouldResetDefaultIsIncludedInAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceCharge = invoiceHeader.Charges.AddNew();
			Assert("Precondition:", invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			Assert("J7_Calc_IsIncludedInInvoiceAmount should be reset", !invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Royalty;
			Assert("J7_Calc_IsIncludedInInvoiceAmount should be reset", !invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoiceCharge.J7_Amount = 10;
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			Assert("J7_Calc_IsIncludedInInvoiceAmount should NOT be reset", invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
		}

		public void TestShouldResetDefaultIsIncludedInITOT()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Assert("J7_IsIncludedInITOT for IMP/OFT should be false", !invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight).J7_IsIncludedInITOT);
			Assert("J7_IsIncludedInITOT for IMP/ONS should be false", !invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance).J7_IsIncludedInITOT);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("J7_IsIncludedInITOT for EXP/OFT should be true", invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight).J7_IsIncludedInITOT);
			Assert("J7_IsIncludedInITOT for EXP/ONS should be true", invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance).J7_IsIncludedInITOT);
		}

		public void TestAllowNonWesternEuropeanCharacterForChargeDescription()
		{
			var charge = Factory.New<InvoiceCharge>();
			Assert("AllowNonWesternEuropeanCharacterForChargeDescription should be true", charge.AllowNonWesternEuropeanCharacterForChargeDescription);
		}
	}
}
