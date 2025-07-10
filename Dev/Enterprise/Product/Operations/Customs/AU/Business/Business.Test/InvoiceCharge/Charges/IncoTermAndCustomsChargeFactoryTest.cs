using System;
using CargoWise.IO;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllCharges()
		{
			var allCharges = incoTermAndChargeFactory.GetAllCharges();
			AssertEquals("There should be 12 charges", new AUChargeCodeList().Count, allCharges.Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(AUChargeCodeList.Codes.PackingCost, EdificeIncoTermAndCustomsChargeFactory.EdificePackingCost);
			AssertGetCharge(AUChargeCodeList.Codes.OverseasFreight, EdificeIncoTermAndCustomsChargeFactory.EdificeOverseasFreight);
			AssertGetCharge(AUChargeCodeList.Codes.OverseasInsurance, CustomsChargeCodeProvider.OverseasInsurance);
			AssertGetCharge(AUChargeCodeList.Codes.Discount, CustomsChargeCodeProvider.Discount);
			AssertGetCharge(AUChargeCodeList.Codes.BuyingCommission, EdificeIncoTermAndCustomsChargeFactory.EdificeBuyingCommission);
			AssertGetCharge(AUChargeCodeList.Codes.OtherCommission, EdificeIncoTermAndCustomsChargeFactory.EdificeOtherCommission);
			AssertGetCharge(AUChargeCodeList.Codes.ExWorks, CustomsChargeCodeProvider.ExWorks);
			AssertGetCharge(AUChargeCodeList.Codes.ForeignInlandFreight, CustomsChargeCodeProvider.ForeignInlandFreight);
			AssertGetCharge(AUChargeCodeList.Codes.LandingCharges, CustomsChargeCodeProvider.LandingCharges);
			AssertGetCharge(AUChargeCodeList.Codes.OtherCharges, CustomsChargeCodeProvider.OtherCharges);
			AssertGetCharge(AUChargeCodeList.Codes.AdditionCharge, CustomsChargeCodeProvider.AdditionCharge);
			AssertGetCharge(AUChargeCodeList.Codes.DeductionCharge, CustomsChargeCodeProvider.DeductionCharge);
		}

		public void TestITOTIncoTermForLISInvoicePacked()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			testInvoice.JZ_InvoiceAmount = 10000m;

			BaseJobComInvHeaderCharge lCH = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100);
			BaseJobComInvHeaderCharge oNS = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100);
			BaseJobComInvHeaderCharge oFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			BaseJobComInvHeaderCharge fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			oNS.J7_IsIncludedInITOT = true;
			oFT.J7_IsIncludedInITOT = true;
			fIFT.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT is CIF", Core.Constants.IncoTerms.CostInsuranceAndFreight, testInvoice.ITOTIncoTerm);

			oNS.J7_IsIncludedInITOT = false;
			oFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is FOB", Core.Constants.IncoTerms.FreeOnBoard, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is PAF", Core.Constants.IncoTerms.PackedAtFactory, testInvoice.ITOTIncoTerm);

			BaseJobComInvHeaderCharge pC = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			pC.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is UAF", Core.Constants.IncoTerms.UnpackedAtFactory, testInvoice.ITOTIncoTerm);

			pC.J7_IsIncludedInITOT = true;
			AssertEquals("Packing Costs is included in ITOT : ITOT is PAF", Core.Constants.IncoTerms.PackedAtFactory, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = true;
			AssertEquals("Foreign InLand Freight is included in ITOT : ITOT is FOB", Core.Constants.IncoTerms.FreeOnBoard, testInvoice.ITOTIncoTerm);

			oFT.J7_IsIncludedInITOT = true;
			AssertEquals("OverseasFreight is included in ITOT : ITOT is C&F", Core.Constants.IncoTerms.CostFreightWithAmpersand, testInvoice.ITOTIncoTerm);

			oFT.J7_IsIncludedInITOT = false;
			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("Insurance is included in ITOT : ITOT is C&I", Core.Constants.IncoTerms.CostAndInsurance, testInvoice.ITOTIncoTerm);

			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = true;
			lCH.J7_IsIncludedInITOT = true;
			AssertEquals("Landing charges is included in ITOT : ITOT is LIS", Core.Constants.IncoTerms.LandedIntoStore, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForLISInvoiceUnPacked()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			testInvoice.JZ_InvoiceAmount = 10000m;

			BaseJobComInvHeaderCharge pC = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge lCH = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100);
			BaseJobComInvHeaderCharge oNS = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100);
			BaseJobComInvHeaderCharge oFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			BaseJobComInvHeaderCharge fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			fIFT.J7_IsIncludedInITOT = true;
			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = true;

			AssertEquals("Lines are UCI", Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight, testInvoice.ITOTIncoTerm);

			oNS.J7_IsIncludedInITOT = false;
			oFT.J7_IsIncludedInITOT = false;
			AssertEquals("Lines are UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = false;
			AssertEquals("Lines are UAF", Core.Constants.IncoTerms.UnpackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForCIFInvoice()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testInvoice.JZ_InvoiceAmount = 10000m;

			BaseJobComInvHeaderCharge oNS = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100);
			BaseJobComInvHeaderCharge oFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			BaseJobComInvHeaderCharge fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = true;
			fIFT.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT is CIF", Core.Constants.IncoTerms.CostInsuranceAndFreight, testInvoice.ITOTIncoTerm);

			oFT.J7_IsIncludedInITOT = false;
			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT is FOB", Core.Constants.IncoTerms.CostAndInsurance, testInvoice.ITOTIncoTerm);

			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is FOB", Core.Constants.IncoTerms.CostFreightWithAmpersand, testInvoice.ITOTIncoTerm);

			oFT.J7_IsIncludedInITOT = false;
			oNS.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is FOB", Core.Constants.IncoTerms.FreeOnBoard, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is PAF", Core.Constants.IncoTerms.PackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForUnPackedCIFInvoice()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testInvoice.JZ_InvoiceAmount = 10000m;
			BaseJobComInvHeaderCharge pC = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			pC.J7_IsIncludedInITOT = false;//All lines are unpacked

			BaseJobComInvHeaderCharge oNS = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100);
			BaseJobComInvHeaderCharge oFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			BaseJobComInvHeaderCharge fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = true;
			fIFT.J7_IsIncludedInITOT = true;

			AssertEquals("ITOT is UCI", Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight, testInvoice.ITOTIncoTerm);

			oNS.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is UCF", Core.Constants.IncoTerms.UnpackedCostAndFreight, testInvoice.ITOTIncoTerm);

			oFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is UAF", Core.Constants.IncoTerms.UnpackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForC_FInvoice()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;
			testInvoice.JZ_InvoiceAmount = 10000m;

			var oFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			var fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			oFT.J7_IsIncludedInITOT = true;
			fIFT.J7_IsIncludedInITOT = true;

			AssertEquals("ITOT is C&F", Core.Constants.IncoTerms.CostFreightWithAmpersand, testInvoice.ITOTIncoTerm);

			oFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is FOB", Core.Constants.IncoTerms.FreeOnBoard, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is PAF", Core.Constants.IncoTerms.PackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForUnpackedC_FInvoice()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;
			testInvoice.JZ_InvoiceAmount = 10000m;

			BaseJobComInvHeaderCharge pC = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			pC.J7_IsIncludedInITOT = false;//All lines are unpacked

			BaseJobComInvHeaderCharge oFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			BaseJobComInvHeaderCharge fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			oFT.J7_IsIncludedInITOT = true;
			fIFT.J7_IsIncludedInITOT = true;

			AssertEquals("ITOT is UCF", Core.Constants.IncoTerms.UnpackedCostAndFreight, testInvoice.ITOTIncoTerm);

			oFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is UAF", Core.Constants.IncoTerms.UnpackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForC_IInvoice()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			testInvoice.JZ_InvoiceAmount = 10000m;

			BaseJobComInvHeaderCharge pC = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			pC.J7_IsIncludedInITOT = false;//All lines are unpacked

			BaseJobComInvHeaderCharge oNS = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100);
			BaseJobComInvHeaderCharge fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			fIFT.J7_IsIncludedInITOT = true;

			AssertEquals("ITOT is UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is UAF", Core.Constants.IncoTerms.UnpackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForFOBInvoice()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testInvoice.JZ_InvoiceAmount = 10000m;

			BaseJobComInvHeaderCharge fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			fIFT.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT is FOB", Core.Constants.IncoTerms.FreeOnBoard, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is PAF", Core.Constants.IncoTerms.PackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForUFBInvoice()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testInvoice.JZ_InvoiceAmount = 10000m;
			BaseJobComInvHeaderCharge pC = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			pC.J7_IsIncludedInITOT = false;//unpacked

			BaseJobComInvHeaderCharge fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			fIFT.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT is UFB", Core.Constants.IncoTerms.UnpackedFreeOnBoard, testInvoice.ITOTIncoTerm);

			fIFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is UAF", Core.Constants.IncoTerms.UnpackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForPAFInvoice()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			testInvoice.JZ_InvoiceAmount = 10000m;
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			BaseJobComInvHeaderCharge pC = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			pC.J7_IsIncludedInITOT = false;//unpacked
			AssertEquals("ITOT is UAF", Core.Constants.IncoTerms.UnpackedAtFactory, testInvoice.ITOTIncoTerm);

			pC.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT is PAF", Core.Constants.IncoTerms.PackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForUAFInvoice()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			testInvoice.JZ_InvoiceAmount = 10000m;
			AssertEquals("ITOT is UAF", Core.Constants.IncoTerms.UnpackedAtFactory, testInvoice.ITOTIncoTerm);
		}

		public void TestITOTIncoTermForCIFInvoiceWithZeroAmountAndCurrency()
		{
			testInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testInvoice.JZ_InvoiceAmount = 10000m;
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			BaseJobComInvHeaderCharge oNS = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100);
			BaseJobComInvHeaderCharge oFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			BaseJobComInvHeaderCharge fIFT = testInvoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = true;
			fIFT.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT is CIF", Core.Constants.IncoTerms.CostInsuranceAndFreight, testInvoice.ITOTIncoTerm);

			oNS.J7_Amount = 0;
			oNS.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			oNS.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is C&F", Core.Constants.IncoTerms.CostFreightWithAmpersand, testInvoice.ITOTIncoTerm);

			oFT.J7_Amount = 0;
			oFT.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			oFT.J7_IsIncludedInITOT = false;
			AssertEquals("ITOT is FOB", Core.Constants.IncoTerms.FreeOnBoard, testInvoice.ITOTIncoTerm);

			oNS.J7_Amount = 100;
			oNS.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT is C&I", Core.Constants.IncoTerms.CostAndInsurance, testInvoice.ITOTIncoTerm);
		}

		public override void TestGetAllIncoTerms()
		{
			AssertEquals("Count", 11, incoTermAndChargeFactory.GetAllIncoTerms().Length);
		}

		protected override string IncoTermAndCustomsChargeConfigurationFilename => embeddedResourceRetriever.SaveResourceToFile("Enterprise.Customs.AU.Declaration.Business.Testing.InvoiceCharge.IncoTerms.TestFile.IncoTermAndCustomsChargeConfiguration.csv");

		protected override Type GetCustomsChargeCodeProviderActualType() => typeof(EdificeIncoTermAndCustomsChargeFactory);

		protected override string GetCountryContext() => JobDeclaration.AUEdifice;

		JobDeclaration testDec;
		JobComInvoiceGroupHeader testGroupInvoice;
		JobComInvoiceHeader testInvoice;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testGroupInvoice = testDec.JobComInvoiceGroupHeaders[0];
			testInvoice = testGroupInvoice.JobComInvoiceHeaders.AddNew();
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
	}
}
