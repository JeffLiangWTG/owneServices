using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	public class JobComInvoiceGroupHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetApplicableInsurance() => CombineAssertions(() =>
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var insurance1 = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance1.CCR_StartDate = new ZDateTime(2024, 08, 01).ToOffset();
			insurance1.CCR_EndDate = new ZDateTime(2024, 08, 31).ToOffset();
			insurance1.CCR_OH_Importer = importer.PK;
			insurance1.CCR_TransportMode = TransportTypeList.Codes.Air;

			var insurance2 = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance2.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance2.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance2.CCR_OH_Importer = importer.PK;
			insurance2.CCR_TransportMode = TransportTypeList.Codes.Air;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ValuationDate = new ZDate(2024, 5, 10);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2024, 8, 17);
			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2024, 8, 17);
			AssertEquals("Use invoice ValuationDate when all invoices have the same ValuationDate", insurance1, groupHeader.GetApplicableInsurance());

			invoice2.JZ_ValuationDateOverride = new ZDateTime(2024, 8, 10);
			AssertEquals("Use declaration ValuationDate when invoices have different ValuationDate", insurance2, groupHeader.GetApplicableInsurance());

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.JE_ValuationDate = new ZDate(2024, 5, 10);
			declaration2.JE_OH_Importer = importer.PK;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Air;

			var groupHeader2 = declaration2.JobComInvoiceGroupHeaders[0];
			invoice1 = groupHeader2.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2024, 8, 17);
			invoice2 = groupHeader2.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2024, 8, 17);
			AssertEquals("Insurance can only be got when IMP", null, groupHeader2.GetApplicableInsurance());

			invoice2.JZ_ValuationDateOverride = new ZDateTime(2024, 8, 10);
			AssertEquals("Insurance can only be got when IMP", null, groupHeader2.GetApplicableInsurance());
		});

		public void TestValueForDuty_CFR_CFR()
		{
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_Formula = "IF(VFD >= 100, 1000, IF(VFD >= 10, 100, 10))";
			insurance.CCR_BasedOn = IncoTerms.CostAndFreight;
			insurance.CCR_RX_NKCurrency = "AUD";
			insurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoice1.JZ_InvoiceAmount = 2100;
			invoice1.JZ_IncoTerm = IncoTerms.CostAndFreight;
			var oftCharge1 = invoice1.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge1.J7_Amount = 100m;

			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoice2.JZ_InvoiceAmount = 600;
			invoice2.JZ_IncoTerm = IncoTerms.CostAndFreight;
			var oftCharge2 = invoice2.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge2.J7_Amount = 100m;

			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2023, 5, 1), new ZDateTime(2023, 5, 31), 0.66m, helper.USDCurrency);
			var onsCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			AssertEquals("2100 + 909.09", 3009.09m, ((IUniversalRateCalcData)groupHeader).ValueForDuty);
		}

		public void TestValueForDuty_CFR_FOB()
		{
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_Formula = "IF(VFD >= 100, 1000, IF(VFD >= 10, 100, 10))";
			insurance.CCR_BasedOn = IncoTerms.CostAndFreight;
			insurance.CCR_RX_NKCurrency = "AUD";
			insurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_InvoiceAmount = 2000;
			invoice1.JZ_IncoTerm = IncoTerms.FreeOnBoard;

			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			invoice2.JZ_InvoiceAmount = 660;
			invoice2.JZ_IncoTerm = IncoTerms.FreeOnBoard;

			var oftCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge.J7_Amount = 300m;
			oftCharge.J7_RX_NKCurrency = "AUD";

			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2024, 5, 1), new ZDateTime(2024, 5, 31), 0.66m, helper.USDCurrency);
			declaration.ResumeApportionment();

			AssertEquals("invoice1 JZ_Calc_OFTInInvoiceCurrency", 200m, invoice1.JZ_Calc_OFTInInvoiceCurrency);
			AssertEquals("invoice2 JZ_Calc_OFTInInvoiceCurrency", 66m, invoice2.JZ_Calc_OFTInInvoiceCurrency);

			var onsCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			AssertEquals("2200 + 1100", 3300m, ((IUniversalRateCalcData)groupHeader).ValueForDuty);
		}

		public void TestValueForDuty_FOB_FOB()
		{
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_Formula = "IF(VFD >= 100, 1000, IF(VFD >= 10, 100, 10))";
			insurance.CCR_BasedOn = IncoTerms.FreeOnBoard;
			insurance.CCR_RX_NKCurrency = "AUD";
			insurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_InvoiceAmount = 2000;
			invoice1.JZ_IncoTerm = IncoTerms.FreeOnBoard;

			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			invoice2.JZ_InvoiceAmount = 660;
			invoice2.JZ_IncoTerm = IncoTerms.FreeOnBoard;

			var oftCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge.J7_Amount = 300m;
			oftCharge.J7_RX_NKCurrency = "AUD";

			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2024, 5, 1), new ZDateTime(2024, 5, 31), 0.66m, helper.USDCurrency);
			declaration.ResumeApportionment();

			AssertEquals("invoice1 JZ_Calc_OFTInInvoiceCurrency", 200m, invoice1.JZ_Calc_OFTInInvoiceCurrency);
			AssertEquals("invoice2 JZ_Calc_OFTInInvoiceCurrency", 66m, invoice2.JZ_Calc_OFTInInvoiceCurrency);

			var onsCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			AssertEquals("2000 + 1000", 3000m, ((IUniversalRateCalcData)groupHeader).ValueForDuty);
		}

		public void TestValueForDuty_FOB_CFR()
		{
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_Formula = "IF(VFD >= 100, 1000, IF(VFD >= 10, 100, 10))";
			insurance.CCR_BasedOn = IncoTerms.FreeOnBoard;
			insurance.CCR_RX_NKCurrency = "AUD";
			insurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoice1.JZ_InvoiceAmount = 2100;
			invoice1.JZ_IncoTerm = IncoTerms.CostAndFreight;
			var oftCharge1 = invoice1.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge1.J7_Amount = 100m;

			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoice2.JZ_InvoiceAmount = 600;
			invoice2.JZ_IncoTerm = IncoTerms.CostAndFreight;
			var oftCharge2 = invoice2.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge2.J7_Amount = 100m;

			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2024, 5, 1), new ZDateTime(2024, 5, 31), 0.66m, helper.USDCurrency);
			var onsCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			AssertEquals("2000 + 757.58", 2757.58m, ((IUniversalRateCalcData)groupHeader).ValueForDuty);
		}

		public void TestImportCMRUsesIncoTermAndCustomsChargeFactory()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("all group header's IncoTerm Factory type", typeof(EdificeIncoTermAndCustomsChargeFactory), declaration.JobComInvoiceGroupHeaders[0].IncoTermAndChargeFactory.GetType());

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("all group header's IncoTerm Factory type", typeof(Common.CommonIncoTermAndCustomsChargeFactory), declaration.JobComInvoiceGroupHeaders[0].IncoTermAndChargeFactory.GetType());
		}

		public void TestImportExportChangeAffectsChargeFactoryIncoTermFactory()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader allGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			AssertEquals("all group header's IncoTerm Factory type", typeof(Common.CommonIncoTermAndCustomsChargeFactory), allGroupHeader.IncoTermAndChargeFactory.GetType());

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("all group header's IncoTerm Factory type", typeof(EdificeIncoTermAndCustomsChargeFactory), allGroupHeader.IncoTermAndChargeFactory.GetType());
		}

		public void TestJZ_Calc_TNI()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader allGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoice1 = allGroupHeader.JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invoice2 = allGroupHeader.JobComInvoiceHeaders.AddNew();

			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 1000;
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";

			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice2.JZ_InvoiceAmount = 4000;
			invoice2.JZ_RX_NKInvoice_Currency = "AUD";

			allGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000, JobDeclaration.LocalCurrencyConstantCode);
			allGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, JobDeclaration.LocalCurrencyConstantCode);
			declaration.ResumeApportionment();
			AssertEquals("Invoice1 TNI", 300m, invoice1.JZ_Calc_TNI);
			AssertEquals("Invoice2 TNI", 1200m, invoice2.JZ_Calc_TNI);
			AssertEquals("Total TNI", 1500m, allGroupHeader.JZ_Calc_TNI);
		}

		public void TestClearApportionedChargeWhenIncotermChanged()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceGroupHeader allGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoiceHeader1 = allGroupHeader.JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invoiceHeader2 = allGroupHeader.JobComInvoiceHeaders.AddNew();

			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			invoiceHeader2.JZ_InvoiceAmount = 4000m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;

			allGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 500, JobDeclaration.LocalCurrencyConstantCode);
			declaration.ResumeApportionment();
			AssertEquals("PreCondition:InvoiceHeader1 doesn't have apportioned LandingCharges", 0, invoiceHeader1.GroupCharges.Count);
			AssertEquals("PreCondition:InvoiceHeader2 is apportioned LandingCharge", 500m, invoiceHeader2.GroupCharges[0].J7_Amount);

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			declaration.ResumeApportionment();
			AssertEquals("One group charge", 1, invoiceHeader1.GroupCharges.Count);
			AssertEquals("One group charge", 1, invoiceHeader2.GroupCharges.Count);
			AssertEquals("InvoiceHeader1 is apportioned LandingCharge", 100m, invoiceHeader1.GroupCharges[0].J7_Amount);
			AssertEquals("InvoiceHeader2 is apportioned LandingCharge", 400m, invoiceHeader2.GroupCharges[0].J7_Amount);

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.ResumeApportionment();
			AssertEquals("Group charge cleared", 0, invoiceHeader1.GroupCharges.Count);
			AssertEquals("Group charge Cleared", 0, invoiceHeader2.GroupCharges.Count);
		}

		public void TestJobComInvoiceGroupHeader()
		{
			JobDeclaration aJobDeclaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader allGroupHeader = aJobDeclaration.JobComInvoiceGroupHeaders[0];

			allGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			JobComInvoiceGroupHeader group1Header = allGroupHeader.JobComInvoiceGroupHeaders[0];
			allGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			JobComInvoiceGroupHeader group2Header = allGroupHeader.JobComInvoiceGroupHeaders[1];
			AssertEquals("Group Headers", 2, allGroupHeader.JobComInvoiceGroupHeaders.Count);

			group1Header.JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invHeader1 = group1Header.JobComInvoiceHeaders[0];
			AssertEquals("Referencing the Invoice Header from either group should return the same value", invHeader1, allGroupHeader.AllJobComInvoiceHeaders[0]);
			group1Header.JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invHeader2 = group1Header.JobComInvoiceHeaders[1];
			AssertEquals("Referencing the Invoice Header from either group should return the same value", invHeader2, allGroupHeader.AllJobComInvoiceHeaders[1]);

			group2Header.JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invHeader3 = group2Header.JobComInvoiceHeaders[0];
			AssertEquals("Referencing the Invoice Header from either group should return the same value", invHeader3, allGroupHeader.AllJobComInvoiceHeaders[2]);

			group2Header.JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invHeader4 = group2Header.JobComInvoiceHeaders[1];
			AssertEquals("Referencing the Invoice Header from either group should return the same value", invHeader4, allGroupHeader.AllJobComInvoiceHeaders[3]);

			group2Header.JobComInvoiceGroupHeaders.AddNew();
			JobComInvoiceGroupHeader group3Header = group2Header.JobComInvoiceGroupHeaders[0];

			group3Header.JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invHeader5 = group3Header.JobComInvoiceHeaders[0];
			AssertEquals("Referencing the Invoice Header from either group should return the same value", invHeader5, allGroupHeader.AllJobComInvoiceHeaders[4]);

			group3Header.JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invHeader6 = group3Header.JobComInvoiceHeaders[1];
			AssertEquals("Referencing the Invoice Header from either group should return the same value", invHeader6, allGroupHeader.AllJobComInvoiceHeaders[5]);

			allGroupHeader.JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader invHeader7 = allGroupHeader.AllJobComInvoiceHeaders[6];

			AssertNotNull("Unable to create", invHeader7);
		}

		/// <summary>
		/// Test that the complex hierarchy of Invoie Groups can be persisted
		///
		/// The Test Data Set should look like this
		///			JobDeclaration
		///				All Invoices Group				InvoiceAmount		Calculated Total
		///					Group 1												600
		///						Invoice 1					100
		///						Invoice 2					200
		///						Invoice 3					300
		///					Group 2												1500
		///						Invoice 4					400
		///						Invoice 5					500
		///						Invoice 6					600
		///					Group 3														8400
		///						Group 4											2400
		///							Invoice 7				700
		///							Invoice 8				800
		///							Invoice 9				900
		///						Group 5											3300
		///							Invoice 10				1000
		///							Invoice 11				1100
		///							Invoice 12				1200
		///						Invoice 13					1300
		///						Invoice 14					1400
		/// </summary>
		public void TestPersistComplexHierarchy()
		{
			ZGuid jobDeclarationPK = CreateHierarchicalJobDeclaration();
			var testJobDeclaration = Factory.Load<JobDeclaration>(jobDeclarationPK);
			testJobDeclaration.Reload();

			AssertEquals("Total number of groups", 6, testJobDeclaration.AllGroupHeaders.Count);
			AssertEquals("Top Groups", 1, testJobDeclaration.JobComInvoiceGroupHeaders.Count);

			JobComInvoiceGroupHeader topGroup = testJobDeclaration.JobComInvoiceGroupHeaders[0];
			AssertEquals("top group's children groups", 3, topGroup.JobComInvoiceGroupHeaders.Count);

			JobComInvoiceGroupHeader group1 = GetGroupHeader(topGroup.JobComInvoiceGroupHeaders, "Group 1");
			JobComInvoiceGroupHeader group2 = GetGroupHeader(topGroup.JobComInvoiceGroupHeaders, "Group 2");
			JobComInvoiceGroupHeader group3 = GetGroupHeader(topGroup.JobComInvoiceGroupHeaders, "Group 3");

			AssertEquals("Group 1 Invoices" + group1.JZ_InvoiceNumber, 3, group1.JobComInvoiceHeaders.Count);
			AssertEquals("Group 2 Invoices" + group2.JZ_InvoiceNumber, 3, group2.JobComInvoiceHeaders.Count);
			AssertEquals("Group 3 Invoices" + group3.JZ_InvoiceNumber, 2, group3.JobComInvoiceHeaders.Count);

			AssertNotNull("invoice1 belongs to group1", GetInvoice(group1.PK, "INVOICE 1"));
			AssertNotNull("invoice2 belongs to group1", GetInvoice(group1.PK, "INVOICE 2"));
			AssertNotNull("invoice3 belongs to group1", GetInvoice(group1.PK, "INVOICE 3"));

			AssertNotNull("invoice4 belongs to group2", GetInvoice(group2.PK, "INVOICE 4"));
			AssertNotNull("invoice5 belongs to group2", GetInvoice(group2.PK, "INVOICE 5"));
			AssertNotNull("invoice6 belongs to group2", GetInvoice(group2.PK, "INVOICE 6"));

			AssertNotNull("invoice13 belongs to group3", GetInvoice(group3.PK, "INVOICE 13"));
			AssertNotNull("invoice14 belongs to group3", GetInvoice(group3.PK, "INVOICE 14"));

			JobComInvoiceGroupHeader group4 = GetGroupHeader(group3.JobComInvoiceGroupHeaders, "Group 4");
			JobComInvoiceGroupHeader group5 = GetGroupHeader(group3.JobComInvoiceGroupHeaders, "Group 5");

			AssertEquals("Group 4 Invoices", 3, group4.JobComInvoiceHeaders.Count);
			AssertEquals("Group 5 Invoices", 3, group5.JobComInvoiceHeaders.Count);

			AssertNotNull("invoice7 belongs to group4", GetInvoice(group4.PK, "INVOICE 7"));
			AssertNotNull("invoice8 belongs to group4", GetInvoice(group4.PK, "INVOICE 8"));
			AssertNotNull("invoice9 belongs to group4", GetInvoice(group4.PK, "INVOICE 9"));

			AssertNotNull("invoice10 belongs to group5", GetInvoice(group5.PK, "INVOICE 10"));
			AssertNotNull("invoice11 belongs to group5", GetInvoice(group5.PK, "INVOICE 11"));
			AssertNotNull("invoice12 belongs to group5", GetInvoice(group5.PK, "INVOICE 12"));
		}

		JobComInvoiceGroupHeader GetGroupHeader(IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> coll, ZString invoiceNumber)
		{
			return (JobComInvoiceGroupHeader)coll.Find(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoiceNumber))[0];
		}

		JobComInvoiceHeader GetInvoice(ZGuid jZ_JZ_GroupInvoiceFK, ZString invoiceNumber)
		{
			ZQuery query = new ZQuery(JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK, jZ_JZ_GroupInvoiceFK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoiceNumber);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
			return Factory.LoadTop1<JobComInvoiceHeader>(query);
		}

		public void TestApportionJZ_OverseasInsurance()
		{
			JobComInvoiceGroupHeader group = GetTestGroup();

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, validCurr);
			group.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 600, currency.RX_Code);

			ZDecimal expectedHeader1Result = 600 * (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader2Result = 600 * (group.JobComInvoiceHeaders[1].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader3Result = 600 * (group.JobComInvoiceHeaders[2].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			group.JobDeclaration.ResumeApportionment();
			AssertEquals("Apportion JZ_OverseasInsurance", expectedHeader1Result, group.JobComInvoiceHeaders[0].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_OverseasInsurance", expectedHeader2Result, group.JobComInvoiceHeaders[1].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_OverseasInsurance", expectedHeader3Result, group.JobComInvoiceHeaders[2].GroupCharges[0].J7_Amount);
		}

		public void TestApportionJZ_OverseasFreight()
		{
			JobComInvoiceGroupHeader group = GetTestGroup();

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, validCurr);
			group.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 600, currency.RX_Code);

			ZDecimal expectedHeader1Result = 600 * (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader2Result = 600 * (group.JobComInvoiceHeaders[1].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader3Result = 600 * (group.JobComInvoiceHeaders[2].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			group.JobDeclaration.ResumeApportionment();
			AssertEquals("Apportion JZ_OverseasFreight", expectedHeader1Result, group.JobComInvoiceHeaders[0].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_OverseasFreight", expectedHeader2Result, group.JobComInvoiceHeaders[1].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_OverseasFreight", expectedHeader3Result, group.JobComInvoiceHeaders[2].GroupCharges[0].J7_Amount);
		}

		public void TestApportionJZ_ExWorks()
		{
			JobComInvoiceGroupHeader group = GetTestGroup();

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, validCurr);
			group.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 600, currency.RX_Code);

			ZDecimal expectedHeader1Result = 600 * (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader2Result = 600 * (group.JobComInvoiceHeaders[1].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader3Result = 600 * (group.JobComInvoiceHeaders[2].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			group.JobDeclaration.ResumeApportionment();
			AssertEquals("Apportion JZ_ExWorksAmount", expectedHeader1Result, group.JobComInvoiceHeaders[0].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_ExWorksAmount", expectedHeader2Result, group.JobComInvoiceHeaders[1].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_ExWorksAmount", expectedHeader3Result, group.JobComInvoiceHeaders[2].GroupCharges[0].J7_Amount);
		}

		public void TestApportionJZ_ForeignInlandFreight()
		{
			JobComInvoiceGroupHeader group = GetTestGroup();

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, validCurr);
			group.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 600, currency.RX_Code);

			ZDecimal expectedHeader1Result = 600 * (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader2Result = 600 * (group.JobComInvoiceHeaders[1].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader3Result = 600 * (group.JobComInvoiceHeaders[2].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			group.JobDeclaration.ResumeApportionment();
			AssertEquals("Apportion JZ_ForeignInlandFreight", expectedHeader1Result, group.JobComInvoiceHeaders[0].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_ForeignInlandFreight", expectedHeader2Result, group.JobComInvoiceHeaders[1].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_ForeignInlandFreight", expectedHeader3Result, group.JobComInvoiceHeaders[2].GroupCharges[0].J7_Amount);
		}

		public void TestApportionJZ_PackingCosts()
		{
			JobComInvoiceGroupHeader group = GetTestGroup();

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, validCurr);
			group.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 600, currency.RX_Code);

			ZDecimal expectedHeader1Result = 600 * (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader2Result = 600 * (group.JobComInvoiceHeaders[1].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader3Result = 600 * (group.JobComInvoiceHeaders[2].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			group.JobDeclaration.ResumeApportionment();
			AssertEquals("Apportion JZ_PackingCosts", expectedHeader1Result, group.JobComInvoiceHeaders[0].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_PackingCosts", expectedHeader2Result, group.JobComInvoiceHeaders[1].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_PackingCosts", expectedHeader3Result, group.JobComInvoiceHeaders[2].GroupCharges[0].J7_Amount);
		}

		public void TestApportionJZ_LandingCharges()
		{
			JobComInvoiceGroupHeader group = GetTestGroup();

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, validCurr);
			group.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 600, currency.RX_Code);
			//DDP
			ZDecimal expectedHeader1Result = 600 * (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			//DDP
			ZDecimal expectedHeader3Result = 600 * (group.JobComInvoiceHeaders[2].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			group.JobDeclaration.ResumeApportionment();
			AssertEquals("Apportion JZ_LandingCharges", expectedHeader1Result, group.JobComInvoiceHeaders[0].GroupCharges[0].J7_Amount);
			AssertEquals("LandingCharges not apportioned into CIF invoice", 0, group.JobComInvoiceHeaders[1].GroupCharges.Count);
			AssertEquals("Apportion JZ_LandingCharges", expectedHeader3Result, group.JobComInvoiceHeaders[2].GroupCharges[0].J7_Amount);
		}

		public void TestApportionJZ_Other1()
		{
			JobComInvoiceGroupHeader group = GetTestGroup();

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, validCurr);
			group.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600, currency.RX_Code);

			ZDecimal expectedHeader1Result = 600 * (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader2Result = 600 * (group.JobComInvoiceHeaders[1].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader3Result = 600 * (group.JobComInvoiceHeaders[2].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			group.JobDeclaration.ResumeApportionment();
			AssertEquals("Apportion JZ_Other1Charges", expectedHeader1Result, group.JobComInvoiceHeaders[0].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_Other1Charges", expectedHeader2Result, group.JobComInvoiceHeaders[1].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_Other1Charges", expectedHeader3Result, group.JobComInvoiceHeaders[2].GroupCharges[0].J7_Amount);
		}

		public void TestApportionJZ_Discount()
		{
			JobComInvoiceGroupHeader group = GetTestGroup();

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, validCurr);
			group.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 600, currency.RX_Code);

			ZDecimal expectedHeader1Result = 600 * (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader2Result = 600 * (group.JobComInvoiceHeaders[1].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader3Result = 600 * (group.JobComInvoiceHeaders[2].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			group.JobDeclaration.ResumeApportionment();
			AssertEquals("Apportion JZ_Discount", expectedHeader1Result, group.JobComInvoiceHeaders[0].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_Discount", expectedHeader2Result, group.JobComInvoiceHeaders[1].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_Discount", expectedHeader3Result, group.JobComInvoiceHeaders[2].GroupCharges[0].J7_Amount);
		}

		public void TestApportionJZ_Commission()
		{
			JobComInvoiceGroupHeader group = GetTestGroup();
			group.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, validCurr);
			group.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 600, currency.RX_Code);

			ZDecimal expectedHeader1Result = 600 * (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader2Result = 600 * (group.JobComInvoiceHeaders[1].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			ZDecimal expectedHeader3Result = 600 * (group.JobComInvoiceHeaders[2].JZ_InvoiceAmount / (group.JobComInvoiceHeaders[0].JZ_InvoiceAmount + group.JobComInvoiceHeaders[1].JZ_InvoiceAmount + group.JobComInvoiceHeaders[2].JZ_InvoiceAmount));
			group.JobDeclaration.ResumeApportionment();
			AssertEquals("Apportion JZ_Commission", expectedHeader1Result, group.JobComInvoiceHeaders[0].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_Commission", expectedHeader2Result, group.JobComInvoiceHeaders[1].GroupCharges[0].J7_Amount);
			AssertEquals("Apportion JZ_Commission", expectedHeader3Result, group.JobComInvoiceHeaders[2].GroupCharges[0].J7_Amount);
		}

		public void TestApportionedChargesUpdateOnInvoiceTotalsChanging()
		{
			JobDeclaration testDeclaration = CreateDeclarationForApportioning();

			apportionGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 240, JobDeclaration.LocalCurrencyConstantCode);
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 40.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 40.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 40.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 40.0m, apportionHeader1_1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 40.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 40.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);

			apportionHeader1_1.JZ_InvoiceAmount = 5000.0m;
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 120.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 24.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 24.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 24.0m, apportionHeader1_1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 24.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 24.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);

			apportionHeader1_1.JZ_InvoiceAmount = 1000.0m;
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 40.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 40.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 40.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 40.0m, apportionHeader1_1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 40.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 40.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);

			apportionHeader1_1_1.JZ_InvoiceAmount = 5000.0m;
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 24.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 24.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 24.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 120.0m, apportionHeader1_1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 24.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 24.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);
		}

		public void TestApportionedChargesUpdateOnDeletingAnInvoice()
		{
			JobDeclaration testDeclaration = CreateDeclarationForApportioning();
			apportionGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 240, JobDeclaration.LocalCurrencyConstantCode);
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 40.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 40.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 40.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 40.0m, apportionHeader1_1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 40.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 40.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);

			testDeclaration.Invoices.Delete(apportionHeader1_1_2);
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 48.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 48.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 48.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 48.0m, apportionHeader1_1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 48.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);
		}

		public void TestApportionedChargesUpdateOnAddingAnInvoice()
		{
			JobDeclaration testDeclaration = CreateDeclarationForApportioning();
			apportionGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 420, JobDeclaration.LocalCurrencyConstantCode);
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 70.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 70.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 70.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 70.0m, apportionHeader1_1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 70.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 70.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);

			JobComInvoiceHeader newInvoice = apportionGroup1_1.JobComInvoiceHeaders.AddNew();
			newInvoice.JZ_InvoiceAmount = 1000.0m;
			newInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 60.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 60.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 60.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 60.0m, apportionHeader1_1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 60.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 60.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on New Invoice", 60.0m, newInvoice.GroupCharges[0].J7_Amount);
		}

		public void TestApportionedChargesUpdateOnInvoiceChargesChanged()
		{
			JobDeclaration testDeclaration = CreateDeclarationForApportioning();
			BaseJobComInvHeaderCharge groupCharge = apportionGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 280, JobDeclaration.LocalCurrencyConstantCode);
			groupCharge.J7_IsIncludedInITOT = false;

			apportionHeader1_1_1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 30, JobDeclaration.LocalCurrencyConstantCode);
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 50.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 50.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 50.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 30.0m, apportionHeader1_1_1.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 50.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 50.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);

			apportionHeader1_1_1.Charges[0].J7_Amount = 80;
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 40.0m, apportionHeader1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 40.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 40.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 80.0m, apportionHeader1_1_1.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 40.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 40.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);

			apportionHeader1_1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 80, JobDeclaration.LocalCurrencyConstantCode);
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 80.0m, apportionHeader1_1.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 30.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 30.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 80.0m, apportionHeader1_1_1.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 30.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 30.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);

			apportionHeader1_1_1.Charges.RemoveAndDeleteAll();
			testDeclaration.ResumeApportionment();
			AssertZDecimalEquals("Apportioned Value on Header 1", 80.0m, apportionHeader1_1.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 40.0m, apportionHeader1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 40.0m, apportionHeader1_3.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 40.0m, apportionHeader1_1_1.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 40.0m, apportionHeader1_1_2.GroupCharges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 40.0m, apportionHeader1_1_3.GroupCharges[0].J7_Amount);
		}

		public void TestApportionedChargesWithNoAvailableInvoiceError()
		{
			JobDeclaration testDeclaration = CreateDeclarationForApportioning();
			testDeclaration.AutoCreateChargesBasedOnIncoTerm = false;
			apportionHeader1_1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20, JobDeclaration.LocalCurrencyConstantCode);
			apportionHeader1_2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20, JobDeclaration.LocalCurrencyConstantCode);
			apportionHeader1_3.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20, JobDeclaration.LocalCurrencyConstantCode);

			apportionHeader1_1_1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20, JobDeclaration.LocalCurrencyConstantCode);
			apportionHeader1_1_2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20, JobDeclaration.LocalCurrencyConstantCode);
			apportionHeader1_1_3.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20, JobDeclaration.LocalCurrencyConstantCode);

			apportionGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 420, JobDeclaration.LocalCurrencyConstantCode);

			AssertZDecimalEquals("Apportioned Value on Header 1", 20.0m, apportionHeader1_1.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 2", 20.0m, apportionHeader1_2.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 3", 20.0m, apportionHeader1_3.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.1", 20.0m, apportionHeader1_1_1.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.2", 20.0m, apportionHeader1_1_2.Charges[0].J7_Amount);
			AssertZDecimalEquals("Apportioned Value on Header 1.3", 20.0m, apportionHeader1_1_3.Charges[0].J7_Amount);

			apportionGroup1.RunPreSaveValidation();
			AssertEquals("Overseas Freight should have Error as no money was apportioned to any invoices", true, apportionGroup1.Charges[0].J7_ChargeTypeInfo.HasNotifications());
		}

		public void TestApportioningNegativeAmounts()
		{
			JobDeclaration testDeclaration = CreateDeclarationForApportioning();

			apportionHeader1_1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 120, JobDeclaration.LocalCurrencyConstantCode);
			apportionHeader1_2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 120, JobDeclaration.LocalCurrencyConstantCode);
			apportionHeader1_3.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 120, JobDeclaration.LocalCurrencyConstantCode);

			apportionHeader1_1_1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 120, JobDeclaration.LocalCurrencyConstantCode);
			apportionHeader1_1_2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 120, JobDeclaration.LocalCurrencyConstantCode);

			BaseJobComInvHeaderCharge charge = apportionGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 420, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsIncludedInITOT = false;

			AssertZDecimalEquals("120 x 5 - 420 = -180 is not going to be apportioned as it is negative", 0, apportionHeader1_1_3.GroupCharges.Count);
		}

		public void TestCommissionTypeAfterLoaded()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader header = testDec.JobComInvoiceGroupHeaders[0];
			header.JZ_CommissionType = JobComInvoiceHeader.CommissionType.Buying;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceGroupHeader loadedHeader = factory2.Load<JobComInvoiceGroupHeader>(header.PK);
			AssertEquals("Commission Type", JobComInvoiceHeader.CommissionType.Buying, loadedHeader.JZ_CommissionType);
		}

		public void TestAddInfoHasCommissionTypeAfterLoadedButIsHidden()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader header = testDec.JobComInvoiceGroupHeaders[0];
			header.JZ_CommissionType = JobComInvoiceHeader.CommissionType.Buying;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceGroupHeader loadedHeader = factory2.Load<JobComInvoiceGroupHeader>(header.PK);
			string commissionType = loadedHeader.AddInfo.ZA_CommissionType_Hidden;
			AssertEquals("Commission type should be in addinfo", "1", commissionType);
			AssertEquals("Add Info Line should be blank", "", loadedHeader.AddInfo.AddInfoLine);
		}

		public void TestAddInfo()
		{
			string testAddInfo = "DTY=23.24*AMB=DVTQPOC*LCTE=404*ICN=C23000H";
			JobComInvoiceGroupHeader group = Factory.New<JobComInvoiceGroupHeader>();
			group.JZ_AddInfo = testAddInfo;
			AssertEquals("Declaration AddInfo Object Field - Duty", 23.24m, group.AddInfo.ZA_DTY);
			AssertEquals("Declaration AddInfo Object Field - Amber Processing", "DVTQPOC", group.AddInfo.ZA_AMB);
			AssertEquals("Declaration AddInfo Object Field - Luxury Car Tax Exemption", "404", group.AddInfo.ZA_LCTE);
			AssertEquals("Declaration AddInfo Object Field - Import Credit Number", "C23000H", group.AddInfo.ZA_ICN);
			AssertEquals("Declaration AddInfo Object Field - Origin", "", group.AddInfo.ZA_ORG);
			AssertEquals("Declaration AddInfo Object Field - Invoice Spirit Strength", 0m, group.AddInfo.ZA_ISS);
		}

		public void TestGroupChargesHashTableLoadedWhenBizOLoaded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			JobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";

			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);
			declaration.ResumeApportionment();
			AssertEquals("Group Header should have an entry", 1, invoiceHeader.GroupCharges.Count);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration loaded = newFactory.Load<JobDeclaration>(declaration.PK);
			declaration.ResumeApportionment();
			AssertEquals("Loaded Header Overseas Freight should get apportioned", 1, loaded.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].GroupCharges.Count);
		}

		public void TestGroupChargeApportionedOnInvoicesWithDifferentCurrency()
		{
			RefCurrency nZDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			new ZTestHelper(Factory).SetExchangeRate(new ZDateTime(2003, 12, 1), new ZDateTime(2003, 12, 3), 0.7123m, uSDCurrency);
			new ZTestHelper(Factory).SetExchangeRate(new ZDateTime(2003, 12, 1), new ZDateTime(2003, 12, 3), 1.0123m, nZDCurrency);
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABABEU");

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "90093519530");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_AddInfo = "MergeBy_Hidden=TRF";
			testDec.JE_ContainerMode = Core.Constants.TransportModes.Air;
			testDec.JE_DeclarationReference = "B00001003";
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testDec.JE_ExportDate = new ZDateTime(2003, 12, 2);

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2003, 12, 2);

			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 50, "USD");

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testHeader1.JZ_InvoiceAmount = 100.0000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2003, 12, 2);
			testHeader1.JZ_InvoiceNumber = "1";
			testHeader1.JZ_RX_NKInvoice_Currency = "AUD";

			JobComInvoiceHeader testHeader2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testHeader2.JZ_InvoiceAmount = 100.0000m;
			testHeader2.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader2.JZ_InvoiceDate = new ZDateTime(2003, 12, 2);
			testHeader2.JZ_InvoiceNumber = "2";
			testHeader2.JZ_RX_NKInvoice_Currency = "NZD";

			ZDecimal totalInvoiceAmountInAU = 100m + 100 / 1.0123m;
			ZDecimal expectedFreightHeader1 = 50m * (100 / totalInvoiceAmountInAU);
			ZDecimal expectedFreightHeader2 = 50m * (100 / 1.0123m / totalInvoiceAmountInAU);
			testDec.ResumeApportionment();
			AssertEquals(expectedFreightHeader1, testHeader1.GroupCharges[0].J7_Amount, 0.05m);

			AssertEquals(expectedFreightHeader2, testHeader2.GroupCharges[0].J7_Amount, 0.05m);
		}

		public void TestDefaultCurrencyFromAboveRow()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header1 = jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;

			JobComInvoiceHeader header2 = jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals("Header2 currency defaulted", header1.JZ_RX_NKInvoice_Currency, header2.JZ_RX_NKInvoice_Currency);
		}

		public void TestCopyPersistentValuesFrom()
		{
			JobComInvoiceGroupHeader header = Factory.New<JobComInvoiceGroupHeader>();
			header.JZ_Weight = 100m;
			header.JZ_WeightUQ = "KG";
			JobComInvoiceGroupHeader secondHeader = Factory.New<JobComInvoiceGroupHeader>();
			secondHeader.CopyPersistentValuesFrom(header);
			AssertEquals(header.JZ_Weight, secondHeader.JZ_Weight);
			AssertEquals(header.JZ_WeightUQ, secondHeader.JZ_WeightUQ);
		}

		public void TestDateOfValuationComesFromOverride()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header1 = jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 17);
			AssertEquals("DateOfValuation", new ZDateTime(2005, 8, 17), jobDec.JobComInvoiceGroupHeaders[0].DateOfValuation);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			uSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			distributeByForExport = CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return declaration.JobComInvoiceGroupHeaders[0];
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		ZGuid CreateHierarchicalJobDeclaration()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			JobDeclaration testJobDeclaration = testFactory.New<JobDeclaration>();
			AddInvoiceGroupToGroup("Group 1", testJobDeclaration.JobComInvoiceGroupHeaders[0]);
			AddInvoiceGroupToGroup("Group 2", testJobDeclaration.JobComInvoiceGroupHeaders[0]);
			AddInvoiceGroupToGroup("Group 3", testJobDeclaration.JobComInvoiceGroupHeaders[0]);
			AddInvoiceGroupToGroup("Group 4", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2]);
			AddInvoiceGroupToGroup("Group 5", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2]);
			AddInvoiceToGroup("Invoice 1", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0]);
			AddInvoiceToGroup("Invoice 2", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0]);
			AddInvoiceToGroup("Invoice 3", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0]);

			AddInvoiceToGroup("Invoice 4", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[1]);
			AddInvoiceToGroup("Invoice 5", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[1]);
			AddInvoiceToGroup("Invoice 6", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[1]);

			AddInvoiceToGroup("Invoice 7", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2].JobComInvoiceGroupHeaders[0]);
			AddInvoiceToGroup("Invoice 8", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2].JobComInvoiceGroupHeaders[0]);
			AddInvoiceToGroup("Invoice 9", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2].JobComInvoiceGroupHeaders[0]);

			AddInvoiceToGroup("Invoice 10", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2].JobComInvoiceGroupHeaders[1]);
			AddInvoiceToGroup("Invoice 11", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2].JobComInvoiceGroupHeaders[1]);
			AddInvoiceToGroup("Invoice 12", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2].JobComInvoiceGroupHeaders[1]);

			AddInvoiceToGroup("Invoice 13", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2]);
			AddInvoiceToGroup("Invoice 14", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[2]);

			ZGuid result = testJobDeclaration.PK;

			testFactory.Save();
			return result;
		}

		void AddInvoiceGroupToGroup(string groupName, JobComInvoiceGroupHeader aHeader)
		{
			JobComInvoiceGroupHeader newGroupHeader = aHeader.JobComInvoiceGroupHeaders.AddNew();
			newGroupHeader.JZ_InvoiceNumber = groupName;
		}
		void AddInvoiceToGroup(string invoiceName, JobComInvoiceGroupHeader aHeader)
		{
			JobComInvoiceHeader newInvoice = aHeader.JobComInvoiceHeaders.AddNew();
			newInvoice.JZ_InvoiceNumber = invoiceName;
			newInvoice.JZ_InvoiceAmount = 100.0m;
		}

		ZString validCurr;
		JobComInvoiceGroupHeader GetTestGroup()
		{
			validCurr = JobDeclaration.LocalCurrencyConstantCode;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceGroupHeader group = declaration.JobComInvoiceGroupHeaders[0];
			group.JobComInvoiceHeaders.AddNew();
			group.JobComInvoiceHeaders[0].JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.LandedIntoStore;
			group.JobComInvoiceHeaders[0].JZ_InvoiceAmount = 2000.0m;
			group.JobComInvoiceHeaders[0].JZ_RX_NKInvoice_Currency = validCurr;

			group.JobComInvoiceHeaders.AddNew();
			group.JobComInvoiceHeaders[1].JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.CostInsuranceAndFreight;
			group.JobComInvoiceHeaders[1].JZ_InvoiceAmount = 5000.0m;
			group.JobComInvoiceHeaders[1].JZ_RX_NKInvoice_Currency = validCurr;

			group.JobComInvoiceHeaders.AddNew();
			group.JobComInvoiceHeaders[2].JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.LandedIntoStore;
			group.JobComInvoiceHeaders[2].JZ_InvoiceAmount = 3000.0m;
			group.JobComInvoiceHeaders[2].JZ_RX_NKInvoice_Currency = validCurr;
			return group;
		}

		JobComInvoiceGroupHeader apportionGroup1;
		JobComInvoiceGroupHeader apportionGroup2;
		JobComInvoiceGroupHeader apportionGroup1_1;
		JobComInvoiceHeader apportionHeader1_1;
		JobComInvoiceHeader apportionHeader1_2;
		JobComInvoiceHeader apportionHeader1_3;

		JobComInvoiceHeader apportionHeader2_1;
		JobComInvoiceHeader apportionHeader2_2;
		JobComInvoiceHeader apportionHeader2_3;

		JobComInvoiceHeader apportionHeader1_1_1;
		JobComInvoiceHeader apportionHeader1_1_2;
		JobComInvoiceHeader apportionHeader1_1_3;

		RefCurrency aUDCurrency;
		RefCurrency uSDCurrency;

		JobDeclaration CreateDeclarationForApportioning()
		{
			JobDeclaration result = Factory.New<JobDeclaration>();
			result.AutoCreateChargesBasedOnIncoTerm = false;
			apportionGroup1 = result.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			apportionGroup2 = result.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			apportionGroup1_1 = apportionGroup1.JobComInvoiceGroupHeaders.AddNew();

			apportionHeader1_1 = apportionGroup1.JobComInvoiceHeaders.AddNew();
			apportionHeader1_2 = apportionGroup1.JobComInvoiceHeaders.AddNew();
			apportionHeader1_3 = apportionGroup1.JobComInvoiceHeaders.AddNew();

			apportionHeader1_1_1 = apportionGroup1_1.JobComInvoiceHeaders.AddNew();
			apportionHeader1_1_2 = apportionGroup1_1.JobComInvoiceHeaders.AddNew();
			apportionHeader1_1_3 = apportionGroup1_1.JobComInvoiceHeaders.AddNew();

			apportionHeader2_1 = apportionGroup2.JobComInvoiceHeaders.AddNew();
			apportionHeader2_2 = apportionGroup2.JobComInvoiceHeaders.AddNew();
			apportionHeader2_3 = apportionGroup2.JobComInvoiceHeaders.AddNew();

			SetupHeader(apportionHeader1_1, 1000.0m);
			SetupHeader(apportionHeader1_2, 1000.0m);
			SetupHeader(apportionHeader1_3, 1000.0m);
			SetupHeader(apportionHeader1_1_1, 1000.0m);
			SetupHeader(apportionHeader1_1_2, 1000.0m);
			SetupHeader(apportionHeader1_1_3, 1000.0m);
			SetupHeader(apportionHeader2_1, 3000.0m);
			SetupHeader(apportionHeader2_2, 4000.0m);
			SetupHeader(apportionHeader2_3, 5000.0m);

			return result;
		}

		void SetupHeader(JobComInvoiceHeader header, ZDecimal invoiceTotal)
		{
			header.JZ_InvoiceAmount = invoiceTotal;
			header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
		}

		#endregion
	}
}
