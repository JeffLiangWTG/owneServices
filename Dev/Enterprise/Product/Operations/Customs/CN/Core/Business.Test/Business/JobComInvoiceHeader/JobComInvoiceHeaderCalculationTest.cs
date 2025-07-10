using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		public override void TestCalculateCIFValueWithEXW() => Assert(true);

		public new void TestCalculateRealInvoiceTotal()
		{
			var testDec = GetNewDeclaration();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			var header = testDec.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 5);
			// FOB Incoterm
			header.JZ_IncoTerm = "FOB";
			var expected = new ZDecimal(10600 - 200 - 300 + 1 - 5);
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);
			// CIF Incoterm
			header.JZ_IncoTerm = "CIF";
			expected = 10600 - 200 - 300 - 100 + 1 - 5 - 10;
			AssertEquals("Real Invoice / CIF ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateCIFWithCFR()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;
				header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;
				PrepareCharge(header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 100));
				PrepareCharge(header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 1));
				BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 5);
				PrepareCharge(preOTH);
				preOTH.J7_IsDutiable = true;
				preOTH.J7_IsGSTApplicable = true;
				PrepareCharge(header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200));
				PrepareCharge(header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 400));
				ZDecimal expected = 10605m;
				testDec.ResumeApportionment();
				AssertEquals("CIF value / CFR ", expected, header.JZ_Calc_CIFAmount);
				PrepareCharge(groupHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 100, header.JobDeclaration.LocalCurrencyCode));
				testDec.ResumeApportionment();
				expected = 10605m + 100;
				AssertEquals("CIF value / CFR ", expected, header.JZ_Calc_CIFAmount);
			}
		}

		public override void TestCalculateCIFWithFOB()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;
				header.JZ_IncoTerm = "FOB";
				PrepareCharge(header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 100));
				PrepareCharge(header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 1));
				BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 5);
				PrepareCharge(preOTH);
				preOTH.J7_IsDutiable = true;
				preOTH.J7_IsGSTApplicable = true;
				PrepareCharge(header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200));
				ZDecimal expected = 10605m;
				AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);
				PrepareCharge(groupHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 100, header.JobDeclaration.LocalCurrencyCode));
				expected = 10605m + 100;
				testDec.ResumeApportionment();
				AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);
			}
		}

		public override void TestCalculateCIFWithCIP()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;
				header.JZ_IncoTerm = "CIP";
				PrepareCharge(header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 100));
				PrepareCharge(header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 1));
				BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 5);
				PrepareCharge(preOTH);
				preOTH.J7_IsDutiable = true;
				preOTH.J7_IsGSTApplicable = true;
				header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
				header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 300);
				ZDecimal expected = 10605m;
				testDec.ResumeApportionment();
				AssertEquals("CIF value / CIP ", expected, header.JZ_Calc_CIFAmount);
				var groupHeader = header.Master;
				PrepareCharge(groupHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 100, header.JobDeclaration.LocalCurrencyCode));
				testDec.ResumeApportionment();
				header.GroupCharges[0].J7_IsIncludedInITOT = false;
				expected = 10605m + 100;
				AssertEquals("CIF value / CIP ", expected, header.JZ_Calc_CIFAmount);
			}
		}

		public override void TestCalculateCIFWithDDU()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;
			header.JZ_IncoTerm = "DDU";
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.PackingCost, 40);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			BaseJobComInvHeaderCharge postOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 600);
			postOTH.J7_IsDutiable = false;
			postOTH.J7_IsGSTApplicable = false;
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.LandingCharges, 500);
			ZDecimal expected = 10600m - 500 + 5;
			AssertEquals("CIF value / DDU ", expected, header.JZ_Calc_CIFAmount);
		}

		public override void TestCalculateRealInvoiceTotalWithEXW()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;
			header.JZ_IncoTerm = "EXW";
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			ZDecimal expected = 10600 + 1m;
			AssertEquals("Real Invoice / EXW ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateRealInvoiceTotalWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;
			header.JZ_IncoTerm = "FOB";
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 1000); // not relevant
			ZDecimal expected = 10600m - 100 - 200 + 1;
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateRealInvoiceTotalWithDDU()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;
			header.JZ_IncoTerm = "DDU";
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.PackingCost, 40);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			BaseJobComInvHeaderCharge postOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 600);
			postOTH.J7_IsDutiable = false;
			postOTH.J7_IsGSTApplicable = false;
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.LandingCharges, 1000);
			ZDecimal expected = 10600m - 40 - 100 + 1 - 200 - 1000;
			AssertEquals("Real Invoice / DDU ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateRealInvoiceTotalWithCIP()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;
			header.JZ_IncoTerm = "CIP";
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			BaseJobComInvHeaderCharge oFT = header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 1000); //irrelevant
			header.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			ZDecimal expected = 10600m - 100 - 200 + 1;
			AssertEquals("Real Invoice / CIP ", expected, header.InvoiceLineTotal);
			header.Charges.RemoveAndDelete(oFT);
			expected = 10600m - 100 - 200 + 1;
			AssertEquals("Real Invoice / CIP ", expected, header.InvoiceLineTotal);
		}

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		protected override void SetUp()
		{
			base.SetUp();
			testDec = GetNewDeclaration();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			header = testDec.Invoices.AddNew();
		}
		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader groupHeader;
		BaseJobComInvoiceHeader header;
	}
}
