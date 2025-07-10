using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DutyCalculationManagerTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40LCL] * 10 + [20LCL] * 5 + [45LCL] * 2", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "202", "BORDEAUX BASSENS 2", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "108", "BORDEAUX BASSENS 3", "FRNTE", "FR005340");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "230", "BORDEAUX BASSENS 4", "FRLEH", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "395", "BORDEAUX BASSENS 5", "FRLEH", "FR004560");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR005340";
			declaration.JE_RL_NKPortOfArrival = "FRNTE";
			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;

			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var container20LCL = declaration.CusContainers.AddNew();
			container20LCL.CO_Weight = 999.1m;
			container20LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container40LCL = declaration.CusContainers.AddNew();
			container40LCL.CO_Weight = 2999.1m;
			container40LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container45LCL = declaration.CusContainers.AddNew();
			container45LCL.CO_Weight = 4999.1m;
			container45LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var empty45LCLContainer = declaration.CusContainers.AddNew();
			empty45LCLContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			var refContainer20LCL = Factory.New<RefContainer>();
			refContainer20LCL.RC_StorageClass = "20";
			refContainer20LCL.RC_Code = "C20LCL";
			container20LCL.CO_RC = refContainer20LCL.PK;

			var refContainer40LCL = Factory.New<RefContainer>();
			refContainer40LCL.RC_StorageClass = "40";
			refContainer40LCL.RC_Code = "C40LCL";
			container40LCL.CO_RC = refContainer40LCL.PK;

			var refContainer45LCL = Factory.New<RefContainer>();
			refContainer45LCL.RC_StorageClass = "45";
			refContainer45LCL.RC_Code = "C45LCL";
			container45LCL.CO_RC = refContainer45LCL.PK;

			empty45LCLContainer.CO_RC = refContainer45LCL.PK;

			Factory.Save();

			container20LCL.JobContainer.JC_RC = refContainer20LCL.PK;
			container40LCL.JobContainer.JC_RC = refContainer40LCL.PK;
			container45LCL.JobContainer.JC_RC = refContainer45LCL.PK;
			empty45LCLContainer.JobContainer.JC_RC = refContainer45LCL.PK;

			foreach (var container in declaration.CusContainers)
			{
				var containerInvoiceLine = invoiceLine.ContainersPivot.AddNew();
				containerInvoiceLine.C2_CO = container.PK;
			}

			Factory.Save();

			var charge1 = cusEntryHeader.Charges.AddNew("A001");

			AssertEquals("Prerequisite: en entry charge exists, it should be overwritten if able by calculation engine in next step.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to the unique port code 108. A fee can then be calculated, because a formula exists for port 108. Existing charge should be replaced by the new one.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)4, declaration.CustomsEntryHeaders[0].Charges[0].C1_ChargeAmount);

			declaration.JE_CustomsOffice = "FR004560";
			declaration.JE_RL_NKPortOfArrival = "FRLEH";
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to the multiple port codes (230 and 395). A fee can then be calculated, because a formula exists for port 395. Existing charge should be replaced by the new one.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)19, declaration.CustomsEntryHeaders[0].Charges[0].C1_ChargeAmount);

			declaration.JE_CustomsOffice = "FR000100";
			declaration.JE_RL_NKPortOfArrival = "FRBAS";
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to multiple port codes (010 and 202). But no harbour fee was found for those port codes. No fee should be created, and previous fee should have been deleted.", 0, declaration.CustomsEntryHeaders[0].Charges.Count);

			charge1.C1_MethodOfPayment = "MP1";
			charge1.C1_ChargeAmount = 5m;
			var charge2 = cusEntryHeader.Charges.AddNew("B001");
			charge2.C1_MethodOfPayment = "MP2";
			charge2.C1_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Additional;
			charge2.C1_ChargeAmount = 10m;
			var charge3 = cusEntryHeader.Charges.AddNew("C001");
			charge3.C1_MethodOfPayment = "MP3";
			charge3.C1_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Override;
			charge3.C1_ChargeAmount = 15m;
			var charge4 = cusEntryHeader.Charges.AddNew(UniversalReferenceConstants.RefCusRateCodes.P635);
			charge4.C1_MethodOfPayment = "MP4";
			charge4.C1_ChargeAmount = 20m;

			declaration.JE_CustomsOffice = "FR004560";
			declaration.JE_RL_NKPortOfArrival = "FRLEH";
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("Calculation leavesoverriden and additional charges unchanged, delete others and recreates a charge depending declaration values.", 3, declaration.CustomsEntryHeaders[0].Charges.Count);
			var charge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == "A001");
			AssertNull("A001 tax was removed when performing the calculation again, because it is not overriden nor added.", charge);
			charge = declaration.CustomsEntryHeaders[0].Charges.Cast<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == UniversalReferenceConstants.RefCusRateCodes.P635);
			AssertNotNull("The P635 charge was recalculated from elements in the declaration values.", charge);
			AssertEquals((ZDecimal)19, charge.C1_ChargeAmount);
			AssertEquals("MP4", charge.C1_MethodOfPayment);
		}

		public void TestCalculate_ShouldHaveChargesOnTheFirstEntry()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40LCL] * 10 + [20LCL] * 5 + [45LCL] * 2", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "202", "BORDEAUX BASSENS 2", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "108", "BORDEAUX BASSENS 3", "FRNTE", "FR005340");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "230", "BORDEAUX BASSENS 4", "FRLEH", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "395", "BORDEAUX BASSENS 5", "FRLEH", "FR004560");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR005340";
			declaration.JE_RL_NKPortOfArrival = "FRNTE";

			var cusEntryHeader1 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine1 = cusEntryHeader1.AllEntryLines.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceHeader1.JZ_InvoiceNumber = "A";

			var cusEntryHeader2 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine2 = cusEntryHeader2.AllEntryLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceHeader2.JZ_InvoiceNumber = "B";

			var container20LCL = declaration.CusContainers.AddNew();
			container20LCL.CO_Weight = 999.1m;
			container20LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container20LCL.CO_ContainerNumber = "CNT1";

			var container40LCL = declaration.CusContainers.AddNew();
			container40LCL.CO_Weight = 2999.1m;
			container40LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container40LCL.CO_ContainerNumber = "CNT2";

			var container1InvoiceLine1_1 = invoiceLine1.ContainersPivot.AddNew();
			container1InvoiceLine1_1.C2_CO = container20LCL.PK;

			var container1InvoiceLine1_2 = invoiceLine1.ContainersPivot.AddNew();
			container1InvoiceLine1_2.C2_CO = container40LCL.PK;

			var container1InvoiceLine2_1 = invoiceLine2.ContainersPivot.AddNew();
			container1InvoiceLine2_1.C2_CO = container20LCL.PK;

			var container1InvoiceLine2_2 = invoiceLine2.ContainersPivot.AddNew();
			container1InvoiceLine2_2.C2_CO = container40LCL.PK;
			Factory.Save();

			var refContainer20LCL = Factory.New<RefContainer>();
			refContainer20LCL.RC_StorageClass = "20";
			refContainer20LCL.RC_Code = "C20LCL";
			container20LCL.CO_RC = refContainer20LCL.PK;

			var refContainer40LCL = Factory.New<RefContainer>();
			refContainer40LCL.RC_StorageClass = "40";
			refContainer40LCL.RC_Code = "C40LCL";
			container40LCL.CO_RC = refContainer40LCL.PK;

			Factory.Save();

			container20LCL.JobContainer.JC_RC = refContainer20LCL.PK;
			container40LCL.JobContainer.JC_RC = refContainer40LCL.PK;

			Factory.Save();

			cusEntryHeader1.Charges.AddNew();
			cusEntryHeader2.Charges.AddNew();
			AssertEquals("Prerequisite: en entry charge exists, it should be overwritten if able by calculation engine in next step.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to the unique port code 108. A fee can then be calculated, because a formula exists for port 108. Existing charge should be replaced by the new one.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)2, declaration.CustomsEntryHeaders[0].Charges[0].C1_ChargeAmount);
			AssertEquals("Fee port code should be 108", "108", declaration.ChargePaymentOrDestinationID);

			declaration.JE_CustomsOffice = "FR000100";
			declaration.JE_RL_NKPortOfArrival = "FRBAS";
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to multiple port codes (010 and 202). But no harbour fee was found for those port codes. No fee should be created, and previous fee should have been deleted.", 0, declaration.CustomsEntryHeaders[0].Charges.Count);

			declaration.JE_CustomsOffice = "FR004560";
			declaration.JE_RL_NKPortOfArrival = "FRLEH";

			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to the multiple port codes (230 and 395). A fee can then be calculated, because a formula exists for port 395.", 1, cusEntryHeader1.Charges.Count);
			AssertEquals("Fee port code should be 395", "395", declaration.ChargePaymentOrDestinationID);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)15, cusEntryHeader1.Charges[0].C1_ChargeAmount);
		}

		public void TestOnlyCalculateFCLForFRFOSANDFRMRS()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "094", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[TFCL] * 1.1341", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "121", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[TFCL] * 1.1341", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "094", "UNLOCO: FRMRS, Customs Office: FR000100, THI: 094", "FRMRS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "121", "UNLOCO: FRFOS, Customs Office: FR000140, THI: 121", "FRFOS", "FR000140");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR000100";
			declaration.JE_RL_NKPortOfArrival = "FRMRS";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			var cusEntryHeader1 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine1 = cusEntryHeader1.AllEntryLines.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceHeader1.JZ_InvoiceNumber = "A";

			var cusEntryHeader2 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine2 = cusEntryHeader2.AllEntryLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceHeader2.JZ_InvoiceNumber = "B";

			var cusEntryHeader3 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine3 = cusEntryHeader3.AllEntryLines.AddNew();
			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceHeader3.JZ_InvoiceNumber = "C";

			var cusEntryHeader4 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine4 = cusEntryHeader4.AllEntryLines.AddNew();
			var invoiceHeader4 = declaration.Invoices.AddNew();
			var invoiceLine4 = invoiceHeader4.InvoiceLines.AddNew();
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceHeader4.JZ_InvoiceNumber = "D";

			var containerLCL1 = declaration.CusContainers.AddNew();
			containerLCL1.CO_Weight = 999.1m;
			containerLCL1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			containerLCL1.CO_ContainerNumber = "CNT1";

			var containerLCL2 = declaration.CusContainers.AddNew();
			containerLCL2.CO_Weight = 2999.1m;
			containerLCL2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			containerLCL2.CO_ContainerNumber = "CNT2";

			var containerFCL1 = declaration.CusContainers.AddNew();
			containerFCL1.CO_Weight = 15.600m;
			containerFCL1.CO_WeightUQ = "T";
			containerFCL1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			containerFCL1.CO_ContainerNumber = "CNT3";

			var containerFCL2 = declaration.CusContainers.AddNew();
			containerFCL2.CO_Weight = 2.22m;
			containerFCL2.CO_WeightUQ = "T";
			containerFCL2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			containerFCL2.CO_ContainerNumber = "CNT4";

			var container1InvoiceLine1_1 = invoiceLine1.ContainersPivot.AddNew();
			container1InvoiceLine1_1.C2_CO = containerLCL1.PK;

			var container1InvoiceLine1_2 = invoiceLine1.ContainersPivot.AddNew();
			container1InvoiceLine1_2.C2_CO = containerLCL2.PK;

			var container1InvoiceLine1_3 = invoiceLine1.ContainersPivot.AddNew();
			container1InvoiceLine1_3.C2_CO = containerFCL1.PK;

			var container1InvoiceLine1_4 = invoiceLine1.ContainersPivot.AddNew();
			container1InvoiceLine1_4.C2_CO = containerFCL2.PK;

			var container1InvoiceLine2_1 = invoiceLine2.ContainersPivot.AddNew();
			container1InvoiceLine2_1.C2_CO = containerLCL1.PK;

			var container1InvoiceLine2_2 = invoiceLine2.ContainersPivot.AddNew();
			container1InvoiceLine2_2.C2_CO = containerLCL2.PK;

			var container1InvoiceLine2_3 = invoiceLine2.ContainersPivot.AddNew();
			container1InvoiceLine2_3.C2_CO = containerFCL1.PK;

			var container1InvoiceLine2_4 = invoiceLine2.ContainersPivot.AddNew();
			container1InvoiceLine2_4.C2_CO = containerFCL2.PK;

			var container1InvoiceLine3_1 = invoiceLine3.ContainersPivot.AddNew();
			container1InvoiceLine3_1.C2_CO = containerLCL1.PK;

			var container1InvoiceLine3_2 = invoiceLine3.ContainersPivot.AddNew();
			container1InvoiceLine3_2.C2_CO = containerLCL2.PK;

			var container1InvoiceLine3_3 = invoiceLine3.ContainersPivot.AddNew();
			container1InvoiceLine3_3.C2_CO = containerFCL1.PK;

			var container1InvoiceLine3_4 = invoiceLine3.ContainersPivot.AddNew();
			container1InvoiceLine3_4.C2_CO = containerFCL2.PK;

			var container1InvoiceLine4_1 = invoiceLine4.ContainersPivot.AddNew();
			container1InvoiceLine4_1.C2_CO = containerLCL1.PK;

			var container1InvoiceLine4_2 = invoiceLine4.ContainersPivot.AddNew();
			container1InvoiceLine4_2.C2_CO = containerLCL2.PK;

			var container1InvoiceLine4_3 = invoiceLine4.ContainersPivot.AddNew();
			container1InvoiceLine4_3.C2_CO = containerFCL1.PK;

			var container1InvoiceLine4_4 = invoiceLine4.ContainersPivot.AddNew();
			container1InvoiceLine4_4.C2_CO = containerFCL2.PK;
			Factory.Save();

			var refContainer20LCL = Factory.New<RefContainer>();
			refContainer20LCL.RC_StorageClass = "20";
			refContainer20LCL.RC_Code = "C20LCL";
			containerLCL1.CO_RC = refContainer20LCL.PK;

			var refContainer40LCL = Factory.New<RefContainer>();
			refContainer40LCL.RC_StorageClass = "40";
			refContainer40LCL.RC_Code = "C40LCL";
			containerLCL2.CO_RC = refContainer40LCL.PK;

			var refContainer20FCL = Factory.New<RefContainer>();
			refContainer20FCL.RC_StorageClass = "20";
			refContainer20FCL.RC_Code = "C20FCL";
			containerFCL1.CO_RC = refContainer20FCL.PK;

			var refContainer40FCL = Factory.New<RefContainer>();
			refContainer40FCL.RC_StorageClass = "40";
			refContainer40FCL.RC_Code = "C40FCL";
			containerFCL2.CO_RC = refContainer40FCL.PK;

			Factory.Save();

			containerLCL1.JobContainer.JC_RC = refContainer20LCL.PK;
			containerLCL2.JobContainer.JC_RC = refContainer40LCL.PK;
			containerFCL1.JobContainer.JC_RC = refContainer20FCL.PK;
			containerFCL2.JobContainer.JC_RC = refContainer40FCL.PK;

			Factory.Save();

			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals(1, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[1].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[2].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[3].Charges.Count);

			AssertEquals("CustomsOffice + Port of loading leads to the unique port code 108. A fee can then be calculated, because a formula exists for port 108. Existing charge should be replaced by the new one.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)22, declaration.CustomsEntryHeaders[0].Charges[0].C1_ChargeAmount);
			AssertEquals("Fee port code should be 094", "094", declaration.ChargePaymentOrDestinationID);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals(1, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[1].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[2].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[3].Charges.Count);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)22, declaration.CustomsEntryHeaders[0].Charges[0].C1_ChargeAmount);

			declaration.JE_CustomsOffice = "FR000140";
			declaration.JE_RL_NKPortOfArrival = "FRFOS";
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals(1, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[1].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[2].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[3].Charges.Count);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)22, declaration.CustomsEntryHeaders[0].Charges[0].C1_ChargeAmount);

			containerFCL1.Delete();
			containerFCL2.Delete();
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals(0, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[1].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[2].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[3].Charges.Count);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals(0, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[1].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[2].Charges.Count);
			AssertEquals(0, declaration.CustomsEntryHeaders[3].Charges.Count);
		}

		public void TestCalculate_TwoEntriesWithSeparateContainers()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40LCL] * 10 + [20LCL] * 5 + [45LCL] * 2", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "202", "BORDEAUX BASSENS 2", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "108", "BORDEAUX BASSENS 3", "FRNTE", "FR005340");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "230", "BORDEAUX BASSENS 4", "FRLEH", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "395", "BORDEAUX BASSENS 5", "FRLEH", "FR004560");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR005340";
			declaration.JE_RL_NKPortOfArrival = "FRNTE";

			var cusEntryHeader1 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine1 = cusEntryHeader1.AllEntryLines.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceHeader1.JZ_InvoiceNumber = "A";

			var cusEntryHeader2 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine2 = cusEntryHeader2.AllEntryLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceHeader2.JZ_InvoiceNumber = "B";

			var container20LCL = declaration.CusContainers.AddNew();
			container20LCL.CO_Weight = 999.1m;
			container20LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container20LCL.CO_ContainerNumber = "CNT1";

			var container40LCL = declaration.CusContainers.AddNew();
			container40LCL.CO_Weight = 2999.1m;
			container40LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container40LCL.CO_ContainerNumber = "CNT2";

			var container1InvoiceLine1_1 = invoiceLine1.ContainersPivot.AddNew();
			container1InvoiceLine1_1.C2_CO = container20LCL.PK;

			var container1InvoiceLine2_1 = invoiceLine2.ContainersPivot.AddNew();
			container1InvoiceLine2_1.C2_CO = container40LCL.PK;
			Factory.Save();

			var refContainer20LCL = Factory.New<RefContainer>();
			refContainer20LCL.RC_StorageClass = "20";
			refContainer20LCL.RC_Code = "C20LCL";
			container20LCL.CO_RC = refContainer20LCL.PK;

			var refContainer40LCL = Factory.New<RefContainer>();
			refContainer40LCL.RC_StorageClass = "40";
			refContainer40LCL.RC_Code = "C40LCL";
			container40LCL.CO_RC = refContainer40LCL.PK;

			Factory.Save();

			container20LCL.JobContainer.JC_RC = refContainer20LCL.PK;
			container40LCL.JobContainer.JC_RC = refContainer40LCL.PK;

			Factory.Save();

			cusEntryHeader1.Charges.AddNew();
			cusEntryHeader2.Charges.AddNew();
			AssertEquals("Prerequisite: en entry charge exists, it should be overwritten if able by calculation engine in next step.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to the unique port code 108. A fee can then be calculated, because a formula exists for port 108. Existing charge should be replaced by the new one.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)2, declaration.CustomsEntryHeaders[0].Charges[0].C1_ChargeAmount);
			AssertEquals("Fee port code should be 108", "108", declaration.ChargePaymentOrDestinationID);

			declaration.JE_CustomsOffice = "FR000100";
			declaration.JE_RL_NKPortOfArrival = "FRBAS";
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to multiple port codes (010 and 202). But no harbour fee was found for those port codes. No fee should be created, and previous fee should have been deleted.", 0, declaration.CustomsEntryHeaders[0].Charges.Count);

			declaration.JE_CustomsOffice = "FR004560";
			declaration.JE_RL_NKPortOfArrival = "FRLEH";

			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to the multiple port codes (230 and 395). A fee can then be calculated, because a formula exists for port 395.", 1, cusEntryHeader1.Charges.Count);
			AssertEquals("Fee port code should be 395", "395", declaration.ChargePaymentOrDestinationID);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)15, cusEntryHeader1.Charges[0].C1_ChargeAmount);
		}

		public void TestCalculate_ContainerNotInInvoice()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40LCL] * 10 + [20LCL] * 5 + [45LCL] * 2", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "202", "BORDEAUX BASSENS 2", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "108", "BORDEAUX BASSENS 3", "FRNTE", "FR005340");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "230", "BORDEAUX BASSENS 4", "FRLEH", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "395", "BORDEAUX BASSENS 5", "FRLEH", "FR004560");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR005340";
			declaration.JE_RL_NKPortOfArrival = "FRNTE";

			var cusEntryHeader1 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine1 = cusEntryHeader1.AllEntryLines.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceHeader1.JZ_InvoiceNumber = "A";

			var cusEntryHeader2 = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine2 = cusEntryHeader2.AllEntryLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceHeader2.JZ_InvoiceNumber = "B";

			var container20LCL = declaration.CusContainers.AddNew();
			container20LCL.CO_Weight = 999.1m;
			container20LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container20LCL.CO_ContainerNumber = "CNT1";

			var container40LCL = declaration.CusContainers.AddNew();
			container40LCL.CO_Weight = 2999.1m;
			container40LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container40LCL.CO_ContainerNumber = "CNT2";

			var container40LCL_2 = declaration.CusContainers.AddNew();
			container40LCL_2.CO_Weight = 2999.1m;
			container40LCL_2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container40LCL_2.CO_ContainerNumber = "CNT2";

			var container1InvoiceLine1_1 = invoiceLine1.ContainersPivot.AddNew();
			container1InvoiceLine1_1.C2_CO = container20LCL.PK;

			var container1InvoiceLine2_1 = invoiceLine2.ContainersPivot.AddNew();
			container1InvoiceLine2_1.C2_CO = container40LCL.PK;
			Factory.Save();

			var refContainer20LCL = Factory.New<RefContainer>();
			refContainer20LCL.RC_StorageClass = "20";
			refContainer20LCL.RC_Code = "C20LCL";
			container20LCL.CO_RC = refContainer20LCL.PK;

			var refContainer40LCL = Factory.New<RefContainer>();
			refContainer40LCL.RC_StorageClass = "40";
			refContainer40LCL.RC_Code = "C40LCL";
			container40LCL.CO_RC = refContainer40LCL.PK;

			Factory.Save();

			container20LCL.JobContainer.JC_RC = refContainer20LCL.PK;
			container40LCL.JobContainer.JC_RC = refContainer40LCL.PK;

			Factory.Save();

			cusEntryHeader1.Charges.AddNew();
			cusEntryHeader2.Charges.AddNew();
			AssertEquals("Prerequisite: en entry charge exists, it should be overwritten if able by calculation engine in next step.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to the unique port code 108. A fee can then be calculated, because a formula exists for port 108. Existing charge should be replaced by the new one.", 1, declaration.CustomsEntryHeaders[0].Charges.Count);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)2, declaration.CustomsEntryHeaders[0].Charges[0].C1_ChargeAmount);
			AssertEquals("Fee port code should be 108", "108", declaration.ChargePaymentOrDestinationID);

			declaration.JE_CustomsOffice = "FR000100";
			declaration.JE_RL_NKPortOfArrival = "FRBAS";
			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to multiple port codes (010 and 202). But no harbour fee was found for those port codes. No fee should be created, and previous fee should have been deleted.", 0, declaration.CustomsEntryHeaders[0].Charges.Count);

			declaration.JE_CustomsOffice = "FR004560";
			declaration.JE_RL_NKPortOfArrival = "FRLEH";

			new HarbourFeeEntryHeaderCalculationManager(Factory).Calculate(declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>());
			AssertEquals("CustomsOffice + Port of loading leads to the multiple port codes (230 and 395). A fee can then be calculated, because a formula exists for port 395.", 1, cusEntryHeader1.Charges.Count);
			AssertEquals("Fee port code should be 395", "395", declaration.ChargePaymentOrDestinationID);
			AssertEquals("Formula used for calculation uses the harbour rate matching the port code.", (ZDecimal)15, cusEntryHeader1.Charges[0].C1_ChargeAmount);
		}
	}
}
