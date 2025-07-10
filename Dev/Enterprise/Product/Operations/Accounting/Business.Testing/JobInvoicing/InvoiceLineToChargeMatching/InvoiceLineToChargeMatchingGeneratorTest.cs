using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching;
using Enterprise.Accounting.Export;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing.InvoiceLineToChargeMatching
{
	class InvoiceLineToChargeMatchingGeneratorTest : TestCaseWithFactory
	{
		public void TestNoLineInUniversalXml()
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.OrganizationCode = TestObjectCreator.AALSHI.OH_Code;
			universalTransaction.OrganizationAddress = orgAddress;

			var universalXml = universalTransaction.Serialize();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Matching not performed as no line present", MatchingOutcome.MatchingNotPerformed, result.Outcome);
		}

		public void TestCreditorNotFound()
		{
			var chargeCode = TestObjectCreator.FRT;

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine("ABCDEF", "S001001", chargeCode.AC_Code, 100M, 10M);

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			AssertNull(universalTransactionWrapper.CreditorOrgHeader);
			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Matching not performed as org not found", MatchingOutcome.MatchingNotPerformed, result.Outcome);
		}

		public void TestJobNotFound()
		{
			var chargeCode = TestObjectCreator.FRT;

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine(TestObjectCreator.AALSHI.OH_Code, "S001001", chargeCode.AC_Code, 100M, 10M);

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			AssertEquals(1, universalTransactionWrapper.Lines.Count);
			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Matching not performed as local job not found", MatchingOutcome.NoMatchFound, result.Outcome);
		}

		public void TestLineAmountDoesNotMatchWithJobCharge_InvoiceAmountDoesNotMatch()
		{
			var creditor = TestObjectCreator.AALSHI;
			var chargeCode = TestObjectCreator.FRT;
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine(creditor.OH_Code, shipmentNumber, chargeCode.AC_Code, 100M, 0M);

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job, chargeCode, "charge line 1", TestObjectCreator.AUD, 120M, creditor, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("No match will be found accrual amount does not match invoice line amount", MatchingOutcome.NoMatchFound, result.Outcome);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_CrossLedgerInvoice()
		{
			var creditor = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI")?.OrgProxy;
			var chargeCode = TestObjectCreator.FRT;
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnJobRelatedARUniversalTransactionWithSingleLine(creditor.OH_Code, shipmentNumber, chargeCode.AC_Code, 100M, 10M);

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job, chargeCode, "charge line 1", TestObjectCreator.AUD, 110M, creditor, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: true);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Match found", MatchingOutcome.FullyMatched, result.Outcome);

			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestion should be found for the single line group", 1, bestSuggestions.Count);

			AssertEquals(OrgType.OriginalOrg, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge(creditorInXml: creditor, creditorInJobCharge: creditor, matchingOrgType: OrgType.OriginalOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_SettlementGroup()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge(creditorInXml: creditor, creditorInJobCharge: SettlementGroup, matchingOrgType: OrgType.SettlementGroupOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_ChildOrg()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge(creditorInXml: creditor, creditorInJobCharge: ChildOrganization, matchingOrgType: OrgType.ChildOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_NoAccrualOrg()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge(creditorInXml: creditor, creditorInJobCharge: null, matchingOrgType: OrgType.NoOrg);
		}

		void SetupAndAssertLineAmountMatchesWithJobCharge(OrgHeader creditorInXml, OrgHeader creditorInJobCharge, OrgType matchingOrgType)
		{
			var chargeCode = TestObjectCreator.FRT;
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine(creditorInXml.OH_Code, shipmentNumber, chargeCode.AC_Code, 100M, 10M);

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job, chargeCode, "charge line 1", TestObjectCreator.AUD, 110M, creditorInJobCharge, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);

			AssertEquals(MatchingOutcome.FullyMatched, result.Outcome);
			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestion should be found for the single line group", 1, bestSuggestions.Count);

			AssertEquals(matchingOrgType, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_LineHasNoTotalAmount()
		{
			var creditor = TestObjectCreator.AALSHI;
			var chargeCode = TestObjectCreator.FRT;
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine_WithoutTotalAmount(TestObjectCreator.AALSHI.OH_Code, shipmentNumber, chargeCode.AC_Code, 100M, 10M);

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job, chargeCode, "charge line 1", TestObjectCreator.AUD, 110M, creditor, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);

			AssertEquals(MatchingOutcome.FullyMatched, result.Outcome);
			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestion should be found for the single line group", 1, bestSuggestions.Count);

			AssertEquals(OrgType.OriginalOrg, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void TestCheckIfAllLinesContainInTarget()
		{
			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithMultipleLine(TestObjectCreator.AALSHI.OH_Code, "S001001", TestObjectCreator.FRT.AC_Code, TestObjectCreator.AUD.RX_Code, 100M, 0M
																															, "S001002", TestObjectCreator.RevenueNoTaxChargeCode.AC_Code, TestObjectCreator.USD.RX_Code, 200M, 0M);

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var lines = universalTransactionWrapper.Lines.Cast<UniversalTransactionLineWrapper>().ToList();
			var lineGroupProvider = new LineGroupingByJobAndChargeCodeProvider(lines);

			var lineGroups = lineGroupProvider.Groups.ToList();
			AssertEquals("Precondition", 2, lineGroups.Count);

			var targetLineGroupCollection = new List<InvoiceLineGroup>();
			targetLineGroupCollection.Add(lineGroups[0]);

			Assert("All lines not present in the Line Groups", !InvoiceLineGroup.CheckIfAllLinesContainInLineGroups(lines, targetLineGroupCollection));

			targetLineGroupCollection.Add(lineGroups[1]);

			Assert("All lines now present in the Line Groups", InvoiceLineGroup.CheckIfAllLinesContainInLineGroups(lines, lineGroups));
		}

		public void TestCheckIfAnyLinesContainInTarget()
		{
			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithMultipleLine(TestObjectCreator.AALSHI.OH_Code, "S001001", TestObjectCreator.FRT.AC_Code, TestObjectCreator.AUD.RX_Code, 100M, 0M
																															, "S001002", TestObjectCreator.RevenueNoTaxChargeCode.AC_Code, TestObjectCreator.USD.RX_Code, 200M, 0M);

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var lines = universalTransactionWrapper.Lines.Cast<UniversalTransactionLineWrapper>().ToList();
			var lineGroupProvider = new LineGroupingByJobAndChargeCodeProvider(lines);

			var lineGroups = lineGroupProvider.Groups.ToList();
			AssertEquals("Precondition", 2, lineGroups.Count);

			var targetLineGroupCollection = new List<InvoiceLineGroup>();
			Assert("No line not present in the Line Groups", !InvoiceLineGroup.CheckIfAnyLinesContainInLineGroups(lines, targetLineGroupCollection));

			targetLineGroupCollection.Add(lineGroups[0]);

			Assert("One line present in the Line Groups", InvoiceLineGroup.CheckIfAnyLinesContainInLineGroups(lines, targetLineGroupCollection));

			targetLineGroupCollection.Add(lineGroups[1]);

			Assert("both lines present in the Line Groups", InvoiceLineGroup.CheckIfAnyLinesContainInLineGroups(lines, lineGroups));
		}

		public void TestMatchingResult()
		{
			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithMultipleLine(TestObjectCreator.AALSHI.OH_Code, "S001001", TestObjectCreator.FRT.AC_Code, TestObjectCreator.AUD.RX_Code, 100M, 0M
																															, "S001002", TestObjectCreator.RevenueNoTaxChargeCode.AC_Code, TestObjectCreator.USD.RX_Code, 200M, 0M);

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var lines = universalTransactionWrapper.Lines.Cast<UniversalTransactionLineWrapper>().ToList();
			var lineGroupProvider = new LineGroupingByJobAndChargeCodeProvider(lines);

			var lineGroups = lineGroupProvider.Groups.ToList();
			AssertEquals("Precondition", 2, lineGroups.Count);

			var targetLineGroupCollection = new List<InvoiceLineGroup>();

			var result = new MatchingResult(targetLineGroupCollection, lines);
			AssertEquals(MatchingOutcome.NoMatchFound, result.Outcome);

			targetLineGroupCollection.Add(lineGroups[0]);

			result = new MatchingResult(targetLineGroupCollection, lines);
			AssertEquals(MatchingOutcome.PartiallyMatched, result.Outcome);

			targetLineGroupCollection.Add(lineGroups[1]);

			result = new MatchingResult(targetLineGroupCollection, lines);
			AssertEquals(MatchingOutcome.FullyMatched, result.Outcome);
		}

		public void TestMatchFound_MultipleInvoiceLines()
		{
			var creditor = TestObjectCreator.AALSHI;
			var shipment1Number = "S001001";
			var chargeCode1 = TestObjectCreator.FRT;
			var currency1 = TestObjectCreator.AUD;

			var shipment2Number = "S001002";
			var chargeCode2 = TestObjectCreator.RevenueNoTaxChargeCode;
			var currency2 = TestObjectCreator.USD;

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithMultipleLine(creditor.OH_Code, shipment1Number, chargeCode1.AC_Code, currency1.RX_Code, 100M, 0M
																															, shipment2Number, chargeCode2.AC_Code, currency2.RX_Code, 200M, 0M);

			var shipment1 = TestObjectCreator.CreateShipment(shipment1Number, false);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job1, chargeCode1, "charge line 1", currency1, 100M, creditor, null, null, 0M, null);

			var shipment2 = TestObjectCreator.CreateShipment(shipment2Number, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job2, chargeCode2, "charge line 1", currency2, 200M, creditor, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);

			AssertEquals(MatchingOutcome.FullyMatched, result.Outcome);

			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("2 suggestions should be found for each of the line groups", 2, bestSuggestions.Count);
			foreach (var suggestion in bestSuggestions)
			{
				AssertEquals(OrgType.OriginalOrg, suggestion.ChargeGroup.Key.OrgType);
			}
		}

		public void TestMatchFound_MultipleInvoiceLines_PartialMatch()
		{
			var creditor = TestObjectCreator.AALSHI;
			var shipment1Number = "S001001";
			var chargeCode1 = TestObjectCreator.FRT;
			var currency1 = TestObjectCreator.AUD;

			var shipment2Number = "S001002";
			var chargeCode2 = TestObjectCreator.RevenueNoTaxChargeCode;
			var currency2 = TestObjectCreator.USD;

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithMultipleLine(creditor.OH_Code, shipment1Number, chargeCode1.AC_Code, currency1.RX_Code, 100M, 0M
																															, shipment2Number, chargeCode2.AC_Code, currency2.RX_Code, 200M, 0M);

			var shipment1 = TestObjectCreator.CreateShipment(shipment1Number, false);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job1, chargeCode1, "charge line 1", currency1, 100M, creditor, null, null, 0M, null);

			var shipment2 = TestObjectCreator.CreateShipment(shipment2Number, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job2, chargeCode2, "charge line 1", currency2, 250M, creditor, null, null, 0M, null); //charge amount does not match charge amount in invoice line, in invoice line charge amount is 200

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);

			AssertEquals(MatchingOutcome.PartiallyMatched, result.Outcome);

			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestions should be found for each of the line groups", 1, bestSuggestions.Count);
			AssertEquals(OrgType.OriginalOrg, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_DifferentChargeCode()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_DifferentChargeCode(creditorInXml: creditor, creditorInJobCharge: creditor, matchingOrgType: OrgType.OriginalOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_DifferentChargeCode_SettlementGroup()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_DifferentChargeCode(creditorInXml: creditor, creditorInJobCharge: SettlementGroup, matchingOrgType: OrgType.SettlementGroupOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_DifferentChargeCode_ChildOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_DifferentChargeCode(creditorInXml: creditor, creditorInJobCharge: ChildOrganization, matchingOrgType: OrgType.ChildOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_DifferentChargeCode_NoOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_DifferentChargeCode(creditorInXml: creditor, creditorInJobCharge: null, matchingOrgType: OrgType.NoOrg);
		}

		void SetupAndAssertLineAmountMatchesWithJobCharge_DifferentChargeCode(OrgHeader creditorInXml, OrgHeader creditorInJobCharge, OrgType matchingOrgType)
		{
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine(creditorInXml.OH_Code, shipmentNumber, "BAF", 100M, 10M);

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "charge line 1", TestObjectCreator.AUD, 110M, creditorInJobCharge, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);

			AssertEquals("Match found", MatchingOutcome.FullyMatched, result.Outcome);
			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestion should be found for the single line group", 1, bestSuggestions.Count);

			AssertEquals(matchingOrgType, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_GroupOfAccruals()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals(creditorInXml: creditor, creditorInJobCharge: creditor, matchingOrgType: OrgType.OriginalOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_GroupOfAccruals_SettlementGroup()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals(creditorInXml: creditor, creditorInJobCharge: SettlementGroup, matchingOrgType: OrgType.SettlementGroupOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_GroupOfAccruals_ChildOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals(creditorInXml: creditor, creditorInJobCharge: ChildOrganization, matchingOrgType: OrgType.ChildOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_GroupOfAccruals_NoOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals(creditorInXml: creditor, creditorInJobCharge: null, matchingOrgType: OrgType.NoOrg);
		}

		void SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals(OrgHeader creditorInXml, OrgHeader creditorInJobCharge, OrgType matchingOrgType)
		{
			var chargeCode = TestObjectCreator.FRT;
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine(creditorInXml.OH_Code, shipmentNumber, chargeCode.AC_Code, 100M, 0M);

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			TestObjectCreator.CreateCharge(job, chargeCode, "charge line 1", TestObjectCreator.AUD, 50M, creditorInJobCharge, null, null, 0M, null);
			TestObjectCreator.CreateCharge(job, chargeCode, "charge line 2", TestObjectCreator.AUD, 50M, creditorInJobCharge, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);

			AssertEquals("Match found", MatchingOutcome.FullyMatched, result.Outcome);
			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestion should be found for the single line group", 1, bestSuggestions.Count);

			AssertEquals(matchingOrgType, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_GroupOfAccruals_IgnoringChargeCode()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals_IgnoringChargeCode(creditorInXml: creditor, credtiorInJobCharge: creditor, matchingOrgType: OrgType.OriginalOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_GroupOfAccruals_IgnoringChargeCode_SettlementGroup()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals_IgnoringChargeCode(creditorInXml: creditor, credtiorInJobCharge: SettlementGroup, matchingOrgType: OrgType.SettlementGroupOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_GroupOfAccruals_IgnoringChargeCode_ChildOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals_IgnoringChargeCode(creditorInXml: creditor, credtiorInJobCharge: ChildOrganization, matchingOrgType: OrgType.ChildOrg);
		}

		public void TestMatchFound_LineAmountMatchesWithJobCharge_GroupOfAccruals_IgnoringChargeCode_NoOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals_IgnoringChargeCode(creditorInXml: creditor, credtiorInJobCharge: null, matchingOrgType: OrgType.NoOrg);
		}

		void SetupAndAssertLineAmountMatchesWithJobCharge_GroupOfAccruals_IgnoringChargeCode(OrgHeader creditorInXml, OrgHeader credtiorInJobCharge, OrgType matchingOrgType)
		{
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine(creditorInXml.OH_Code, shipmentNumber, "FRT", 100M, 0M);

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "charge line 1", TestObjectCreator.AUD, 50M, credtiorInJobCharge, null, null, 0M, null);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, "charge line 2", TestObjectCreator.AUD, 50M, credtiorInJobCharge, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);

			AssertEquals("Match found", MatchingOutcome.FullyMatched, result.Outcome);
			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestion should be found for the single line group", 1, bestSuggestions.Count);

			AssertEquals(matchingOrgType, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void Test_ConsolInvoice_MatchingNotRun_LocalConsolNotFound()
		{
			var creditor = TestObjectCreator.AALSHI;
			var chargeCode = TestObjectCreator.FRT;

			var universalXml = CreateAndReturnConsolRelatedUniversalTransactionWithSingleShipment(creditor.OH_Code, "C001001", "S001001", chargeCode.AC_Code, 100M, 0M);

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Matching not performed as local consol not found", MatchingOutcome.NoMatchFound, result.Outcome);
		}

		public void Test_ConsolInvoice_MatchFound_ConsolNumberMentionedInXml_MatchedAgainstNonConsolShipment()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertConsolInvoice_MatchFound_ConsolNumberMentionedInXml_MatchedAgainstNonConsolShipment(creditorInXml: creditor, credtiorInJobCharge: creditor, matchingOrgType: OrgType.OriginalOrg);
		}

		public void Test_ConsolInvoice_MatchFound_ConsolNumberMentionedInXml_MatchedAgainstNonConsolShipment_SettlementGroup()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertConsolInvoice_MatchFound_ConsolNumberMentionedInXml_MatchedAgainstNonConsolShipment(creditorInXml: creditor, credtiorInJobCharge: SettlementGroup, matchingOrgType: OrgType.SettlementGroupOrg);
		}

		public void Test_ConsolInvoice_MatchFound_ConsolNumberMentionedInXml_MatchedAgainstNonConsolShipment_ChildOrg()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertConsolInvoice_MatchFound_ConsolNumberMentionedInXml_MatchedAgainstNonConsolShipment(creditorInXml: creditor, credtiorInJobCharge: ChildOrganization, matchingOrgType: OrgType.ChildOrg);
		}

		public void Test_ConsolInvoice_MatchFound_ConsolNumberMentionedInXml_MatchedAgainstNonConsolShipment_NoOrg()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertConsolInvoice_MatchFound_ConsolNumberMentionedInXml_MatchedAgainstNonConsolShipment(creditorInXml: creditor, credtiorInJobCharge: null, matchingOrgType: OrgType.NoOrg);
		}

		void SetupAndAssertConsolInvoice_MatchFound_ConsolNumberMentionedInXml_MatchedAgainstNonConsolShipment(OrgHeader creditorInXml, OrgHeader credtiorInJobCharge, OrgType matchingOrgType)
		{
			var chargeCode = TestObjectCreator.FRT;
			var consolNumber = "C001001";
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnConsolRelatedUniversalTransactionWithSingleShipment(creditorInXml.OH_Code, consolNumber, shipmentNumber, chargeCode.AC_Code, 100M, 0M);

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			shipment.JS_HouseBill = shipmentNumber;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			TestObjectCreator.CreateCharge(job, chargeCode, "charge line 1", TestObjectCreator.AUD, 100M, credtiorInJobCharge, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Matching found even though local shipment is non-consol related", MatchingOutcome.FullyMatched, result.Outcome);

			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestion should be found for the single line group", 1, bestSuggestions.Count);

			AssertEquals(matchingOrgType, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void Test_ConsolInvoice_MatchFound_ConsolNumberInXmlIsDifferent_ButShipmentNumberIsSame()
		{
			var creditor = TestObjectCreator.AALSHI;

			var chargeCode = TestObjectCreator.FRT;
			var consolNumber = "C001001";
			var consolNumberInXml = "C001002";
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnConsolRelatedUniversalTransactionWithSingleShipment(creditor.OH_Code, consolNumberInXml, shipmentNumber, chargeCode.AC_Code, 100M, 0M);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", consolNumber);
			consol.JK_MasterBillNum = consolNumber;
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, consol);
			shipment.JS_HouseBill = shipmentNumber;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var apportionemntListing = new ApportionmentListing(Factory, consol);
			var consolCost = apportionemntListing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_OH_Creditor = creditor.PK;
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_OSCostAmount = 100M;

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Match found as shipment number specified in xml is same as local shipment number (charge details match too) though consol number does not match", MatchingOutcome.FullyMatched, result.Outcome);
		}

		public void Test_ConsolInvoice_NoMatchFound_BothConsolAndShipmentNumberInXmlIsDifferent()
		{
			var creditor = TestObjectCreator.AALSHI;

			var chargeCode = TestObjectCreator.FRT;
			var consolNumber = "C001001";
			var consolNumberInXml = "C001002";
			var shipmentNumber = "S001001";
			var shipmentNumberInXml = "S001002";

			var universalXml = CreateAndReturnConsolRelatedUniversalTransactionWithSingleShipment(creditor.OH_Code, consolNumberInXml, shipmentNumberInXml, chargeCode.AC_Code, 100M, 0M);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", consolNumber);
			consol.JK_MasterBillNum = consolNumber;
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, consol);
			shipment.JS_HouseBill = shipmentNumber;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var apportionemntListing = new ApportionmentListing(Factory, consol);
			var consolCost = apportionemntListing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_OH_Creditor = creditor.PK;
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_OSCostAmount = 100M;

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("No Match as consol number and shipment number specified in xml is different to local consol number and shipment number though charge details do match", MatchingOutcome.NoMatchFound, result.Outcome);
		}

		public void Test_ConsolInvoice_MatchFound()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertConsolInvoice_MatchFound(creditorInXml: creditor, creditorInConsolCost: creditor, matchingOrgType: OrgType.OriginalOrg);
		}

		public void Test_ConsolInvoice_MatchFound_SettlementGroup()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertConsolInvoice_MatchFound(creditorInXml: creditor, creditorInConsolCost: SettlementGroup, matchingOrgType: OrgType.SettlementGroupOrg);
		}

		public void Test_ConsolInvoice_MatchFound_ChildOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertConsolInvoice_MatchFound(creditorInXml: creditor, creditorInConsolCost: ChildOrganization, matchingOrgType: OrgType.ChildOrg);
		}

		public void Test_ConsolInvoice_MatchFound_NoOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertConsolInvoice_MatchFound(creditorInXml: creditor, creditorInConsolCost: null, matchingOrgType: OrgType.NoOrg);
		}

		void SetupAndAssertConsolInvoice_MatchFound(OrgHeader creditorInXml, OrgHeader creditorInConsolCost, OrgType matchingOrgType)
		{
			var chargeCode = TestObjectCreator.FRT;
			var consolNumber = "C001001";
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnConsolRelatedUniversalTransactionWithSingleShipment(creditorInXml.OH_Code, consolNumber, shipmentNumber, chargeCode.AC_Code, 100M, 0M);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", consolNumber);
			consol.JK_MasterBillNum = consolNumber;
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, consol);
			shipment.JS_HouseBill = shipmentNumber;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var apportionemntListing = new ApportionmentListing(Factory, consol);
			var consolCost = apportionemntListing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_OH_Creditor = creditorInConsolCost != null ? creditorInConsolCost.PK : ZGuid.Empty;
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_OSCostAmount = 100M;

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Match found though charge code does not match", MatchingOutcome.FullyMatched, result.Outcome);

			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestion should be found for the single line group", 1, bestSuggestions.Count);

			AssertEquals(matchingOrgType, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void Test_ConsolInvoice_NoMatchFound_AmountDoesNotMatch()
		{
			var creditor = TestObjectCreator.AALSHI;
			var chargeCode = TestObjectCreator.FRT;
			var consolNumber = "C001001";
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnConsolRelatedUniversalTransactionWithSingleShipment(creditor.OH_Code, consolNumber, shipmentNumber, chargeCode.AC_Code, 100M, 0M);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", consolNumber);
			consol.JK_MasterBillNum = consolNumber;
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, consol);
			shipment.JS_HouseBill = shipmentNumber;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var apportionemntListing = new ApportionmentListing(Factory, consol);
			var consolCost = apportionemntListing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_OH_Creditor = creditor.PK;
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_OSCostAmount = 120M;

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Matching not found as amount does not match", MatchingOutcome.NoMatchFound, result.Outcome);
		}

		public void Test_NonConsolInvoice_MatchedAgainstConsolCost()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertNonConsolInvoice_MatchedAgainstConsolCost(creditorInXml: creditor, creditorInConsolCost: creditor);
		}

		public void Test_NonConsolInvoice_MatchedAgainstConsolCost_SettlemetGroup()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertNonConsolInvoice_MatchedAgainstConsolCost(creditorInXml: creditor, creditorInConsolCost: SettlementGroup);
		}

		public void Test_NonConsolInvoice_MatchedAgainstConsolCost_ChildOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertNonConsolInvoice_MatchedAgainstConsolCost(creditorInXml: creditor, creditorInConsolCost: ChildOrganization);
		}

		public void Test_NonConsolInvoice_MatchedAgainstConsolCost_NoOrganization()
		{
			var creditor = TestObjectCreator.AALSHI;
			SetupAndAssertNonConsolInvoice_MatchedAgainstConsolCost(creditorInXml: creditor, creditorInConsolCost: null);
		}

		void SetupAndAssertNonConsolInvoice_MatchedAgainstConsolCost(OrgHeader creditorInXml, OrgHeader creditorInConsolCost)
		{
			var chargeCode = TestObjectCreator.FRT;
			var consolNumber = "C001001";
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine(creditorInXml.OH_Code, shipmentNumber, chargeCode.AC_Code, 100M, 10M);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", consolNumber);
			consol.JK_MasterBillNum = consolNumber;
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, consol);
			shipment.JS_HouseBill = shipmentNumber;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var apportionemntListing = new ApportionmentListing(Factory, consol);
			var consolCost = apportionemntListing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			consolCost.E6_OH_Creditor = creditorInConsolCost != null ? creditorInConsolCost.PK : ZGuid.Empty;
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_OSCostAmount = 110M;

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Matching found even though in XML it is not consol related but it is matched against consol cost", MatchingOutcome.FullyMatched, result.Outcome);
		}

		public void TestMatchIsDoneBasedOnBestMatch()
		{
			var creditor = TestObjectCreator.AALSHI;
			var chargeCode = TestObjectCreator.FRT;
			var shipmentNumber = "S001001";

			var universalXml = CreateAndReturnJobRelatedUniversalTransactionWithSingleLine(creditor.OH_Code, shipmentNumber, chargeCode.AC_Code, 100M, 0M);

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber, false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "charge line 1", TestObjectCreator.AUD, 100M, creditor, null, null, 0M, null);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, "charge line 2", TestObjectCreator.AUD, 100M, SettlementGroup, null, null, 0M, null);
			TestObjectCreator.CreateCharge(job, chargeCode, "charge line 3", TestObjectCreator.AUD, 100M, ChildOrganization, null, null, 0M, null);

			Factory.Save();

			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			universalTransactionWrapper.Initialize(universalXml, isCrossLedgerImport: false);

			var result = InvoiceLineToChargeMatchingGenerator.GetMatchingResult(universalTransactionWrapper);
			AssertEquals("Match found", MatchingOutcome.FullyMatched, result.Outcome);

			var invoiceLineGroup = result.InvoiceLineGroupsWithSuggestions.First();
			AssertEquals(shipmentNumber, invoiceLineGroup.Key.JobNumber);
			AssertEquals(TestObjectCreator.AUD.RX_Code, invoiceLineGroup.Key.Currency);

			AssertEquals(-100M, invoiceLineGroup.TotalAmount);

			var bestSuggestions = result.GetBestMatchingSuggestions();
			AssertEquals("1 suggestion should be found for the single line group", 1, bestSuggestions.Count);

			AssertEquals(OrgType.OriginalOrg, bestSuggestions[0].ChargeGroup.Key.OrgType);
		}

		public void TestGetRecordedCharges()
		{
			var currency = TestObjectCreator.AUD;
			var creditor = TestObjectCreator.AALSHI;
			var settlementGroupOrg = TestObjectCreator.AALSHI.GetRelatedParty(RelatedPartyTypeList.Codes.APSettlementGroup, ZString.Empty);
			AssertNotNull(settlementGroupOrg);

			var childOrg = TestObjectCreator.LocalClient;
			childOrg.AddRelatedParty(creditor.PK, RelatedPartyTypeList.Codes.APSettlementGroup, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);

			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Job charge 1", currency, 100m, creditor, null, 0M, null);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, "Job charge 2", TestObjectCreator.USD, 200m, creditor, null, 0M, null);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Job charge 3", currency, 300m, settlementGroupOrg, null, 0M, null);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, "Job charge 3", currency, 400m, TestObjectCreator.LocalClient, null, 0M, null);

			Factory.Save();

			IEnumerable<RecordedCharges> charges = (IEnumerable<RecordedCharges>)typeof(InvoiceLineToChargeMatchingGenerator).GetMethod("GetRecordedCharges", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { new ZGuid[] { job.PK }, creditor.PK, new ZString[] { currency.RX_Code } });

			var chargesWithOriginalCreditor = charges.Where(x => x.OH_PK == creditor.PK).Cast<RecordedCharges>();
			AssertNotNull(chargesWithOriginalCreditor);
			AssertEquals(1, chargesWithOriginalCreditor.Count());
			AssertEquals(TestObjectCreator.CC3.AC_Code, chargesWithOriginalCreditor.First().AC_Code);
			AssertEquals(currency.RX_Code, chargesWithOriginalCreditor.First().Currency);
			AssertEquals(100M, chargesWithOriginalCreditor.First().TotalCostAmount);
			AssertEquals(OrgType.OriginalOrg, chargesWithOriginalCreditor.First().OrgType);

			var chargesWithSettlementGroup = charges.Where(x => x.OH_PK == settlementGroupOrg.PK).Cast<RecordedCharges>();
			AssertNotNull(chargesWithSettlementGroup);
			AssertEquals(1, chargesWithSettlementGroup.Count());
			AssertEquals(TestObjectCreator.CC3.AC_Code, chargesWithSettlementGroup.First().AC_Code);
			AssertEquals(currency.RX_Code, chargesWithSettlementGroup.First().Currency);
			AssertEquals(300M, chargesWithSettlementGroup.First().TotalCostAmount);
			AssertEquals(OrgType.SettlementGroupOrg, chargesWithSettlementGroup.First().OrgType);

			var chargesWithChildOrg = charges.Where(x => x.OH_PK == childOrg.PK).Cast<RecordedCharges>();
			AssertNotNull(chargesWithChildOrg);
			AssertEquals(1, chargesWithChildOrg.Count());
			AssertEquals(TestObjectCreator.CC4.AC_Code, chargesWithChildOrg.First().AC_Code);
			AssertEquals(currency.RX_Code, chargesWithChildOrg.First().Currency);
			AssertEquals(400M, chargesWithChildOrg.First().TotalCostAmount);
			AssertEquals(OrgType.ChildOrg, chargesWithChildOrg.First().OrgType);
		}

		public void TestGetRecordedChargeFromReader()
		{
			string sql = @"
				SELECT
					NULL AS JR_PK
					, NULL AS JR_JH
					, NULL AS JR_OH_CostAccount
					, NULL AS JR_OSCostAmt
					, NULL AS JR_OSCostGSTAmt
					, NULL AS JR_AT_CostGSTRate
					, NULL AS JR_CostTaxDate
					, NULL AS JR_IsCostTaxAmountOverridden
					, NULL AS JR_RX_NKCostCurrency
					, NULL AS JR_E6
					, NULL AS E6_OSCostAmount
					, NULL AS E6_OSGSTAmount
					, NULL AS E6_AT_TaxRate
					, NULL AS E6_TaxDate
					, NULL AS E6_IsTaxAmountOverridden
					, NULL AS AC_PK
					, NULL AS AC_Code
					, NULL AS OH_Code
					, NULL AS JH_GS_NKRepOps
					, NULL AS AL_SystemCreateUser
					, NULL AS OrgType
					, NULL AS JH_JobNum
					, NULL AS JK_UniqueConsignRef
				";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();

				var dict1 = new Dictionary<ZGuid, (ZDecimal costAmount, ZString currency, ZGuid taxRatePK, ZDate taxDate, ZGuid chargeCodePK, ZGuid jr_E6)>();
				var dict2 = new Dictionary<ZGuid, (ZDecimal costAmount, ZGuid taxRatePK, ZDate taxDate, ZString currency)>();
				RecordedCharges charge = (RecordedCharges)typeof(InvoiceLineToChargeMatchingGenerator).GetMethod("GetRecordedChargeFromReader", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { reader, dict1, dict2 });

				AssertEquals(Guid.Empty, charge.JR_PK);
				AssertEquals(Guid.Empty, charge.JR_JH);
				AssertEquals(Guid.Empty, charge.OH_PK);
				AssertEquals(decimal.Zero, charge.TotalCostAmount);
				AssertEquals(string.Empty, charge.Currency);
				AssertEquals(Guid.Empty, charge.E6_PK);
				AssertEquals(decimal.Zero, charge.ConsolCostAmount);
				AssertEquals(string.Empty, charge.AC_Code);
				AssertEquals(string.Empty, charge.OH_Code);
				AssertEquals(string.Empty, charge.JH_GS_NKRepOps);
				AssertEquals(string.Empty, charge.AL_SystemCreateUser);
				AssertEquals(default(OrgType), charge.OrgType);
				AssertEquals(string.Empty, charge.JH_JobNum);
				AssertEquals(string.Empty, charge.JK_UniqueConsignRef);
			}
		}

		string CreateAndReturnJobRelatedUniversalTransactionWithSingleLine_WithoutTotalAmount(ZString orgCode, ZString shipmentNumber, ZString chargeCode, ZDecimal osExTaxAmount, ZDecimal osTaxAmount)
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.OrganizationCode = orgCode;
			universalTransaction.OrganizationAddress = orgAddress;

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.Branch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code };
			universalLine.Department = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code };
			universalLine.ChargeCode = new ChargeCode { Code = chargeCode };
			universalLine.Description = "Some line text";
			universalLine.IsFinalCharge = true;
			universalLine.Sequence = 3;
			universalLine.OSCurrency = new Currency { Code = "AUD" };
			universalLine.OSAmount = -1 * osExTaxAmount;
			universalLine.OSGSTVATAmount = -1 * osTaxAmount;
			universalLine.Job = new EntityReference { Key = shipmentNumber, Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber);
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentNumber);
			universalTransaction.ShipmentCollection.Add(universalShipment);

			return universalTransaction.Serialize();
		}

		string CreateAndReturnJobRelatedUniversalTransactionWithSingleLine(ZString orgCode, ZString shipmentNumber, ZString chargeCode, ZDecimal osExTaxAmount, ZDecimal osTaxAmount)
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.OrganizationCode = orgCode;
			universalTransaction.OrganizationAddress = orgAddress;

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.Branch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code };
			universalLine.Department = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code };
			universalLine.ChargeCode = new ChargeCode { Code = chargeCode };
			universalLine.Description = "Some line text";
			universalLine.IsFinalCharge = true;
			universalLine.Sequence = 3;
			universalLine.OSCurrency = new Currency { Code = "AUD" };
			universalLine.OSAmount = -1 * osExTaxAmount;
			universalLine.OSGSTVATAmount = -1 * osTaxAmount;
			universalLine.OSTotalAmount = universalLine.OSAmount + universalLine.OSGSTVATAmount;
			universalLine.Job = new EntityReference { Key = shipmentNumber, Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber);
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentNumber);
			universalTransaction.ShipmentCollection.Add(universalShipment);

			return universalTransaction.Serialize();
		}

		string CreateAndReturnJobRelatedUniversalTransactionWithMultipleLine(ZString orgCode, ZString shipmentNumber1, ZString chargeCode1, ZString currency1, ZDecimal osExTaxAmount1, ZDecimal osTaxAmount1
																							, ZString shipmentNumber2, ZString chargeCode2, ZString currency2, ZDecimal osExTaxAmount2, ZDecimal osTaxAmount2)
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.OrganizationCode = orgCode;
			universalTransaction.OrganizationAddress = orgAddress;

			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine1.Branch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code };
			universalLine1.Department = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code };
			universalLine1.ChargeCode = new ChargeCode { Code = chargeCode1 };
			universalLine1.Description = "Some line text";
			universalLine1.IsFinalCharge = true;
			universalLine1.Sequence = 3;
			universalLine1.OSCurrency = new Currency { Code = currency1 };
			universalLine1.OSAmount = -1 * osExTaxAmount1;
			universalLine1.OSGSTVATAmount = -1 * osTaxAmount1;
			universalLine1.OSTotalAmount = universalLine1.OSAmount + universalLine1.OSGSTVATAmount;
			universalLine1.Job = new EntityReference { Key = shipmentNumber1, Type = AccountingDataTransferConstants.DataContextTypeString.Job };

			var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine2.Branch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code };
			universalLine2.Department = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code };
			universalLine2.ChargeCode = new ChargeCode { Code = chargeCode2 };
			universalLine2.Description = "Some line text";
			universalLine2.IsFinalCharge = true;
			universalLine2.Sequence = 4;
			universalLine2.OSCurrency = new Currency { Code = currency2 };
			universalLine2.OSAmount = -1 * osExTaxAmount2;
			universalLine2.OSGSTVATAmount = -1 * osTaxAmount2;
			universalLine2.OSTotalAmount = universalLine2.OSAmount + universalLine2.OSGSTVATAmount;
			universalLine2.Job = new EntityReference { Key = shipmentNumber2, Type = AccountingDataTransferConstants.DataContextTypeString.Job };

			universalTransaction.PostingJournalCollection.Add(universalLine1);
			universalTransaction.PostingJournalCollection.Add(universalLine2);

			var universalShipment1 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment1.DataContext = DataContextFactory.New();
			universalShipment1.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber1);
			universalShipment1.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentNumber1);
			universalTransaction.ShipmentCollection.Add(universalShipment1);

			var universalShipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment2.DataContext = DataContextFactory.New();
			universalShipment2.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber2);
			universalShipment2.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentNumber2);
			universalTransaction.ShipmentCollection.Add(universalShipment2);

			return universalTransaction.Serialize();
		}

		string CreateAndReturnJobRelatedARUniversalTransactionWithSingleLine(ZString orgCode, ZString shipmentNumber, ZString chargeCode, ZDecimal osExTaxAmount, ZDecimal osTaxAmount)
		{
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				OrganizationCode = orgCode
			};
			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code },
				Department = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code },
				ChargeCode = new ChargeCode { Code = chargeCode },
				Description = "Some line text",
				IsFinalCharge = true,
				Sequence = 3,
				OSCurrency = new Currency { Code = "AUD" },
				OSAmount = osExTaxAmount,
				OSGSTVATAmount = osTaxAmount,
				Job = new EntityReference { Key = shipmentNumber, Type = AccountingDataTransferConstants.DataContextTypeString.Job }
			};
			universalLine.OSTotalAmount = universalLine.OSAmount + universalLine.OSGSTVATAmount;

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber);
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentNumber);

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsReceivable,
				OSExGSTVATAmount = 100,
				OrganizationAddress = orgAddress,
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code, Country = Country.New(GlbCompany.CurrentCompany.Country) },
			};
			universalTransaction.SetShipmentCollection(() => new List<Shipment> { universalShipment });
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { universalLine });

			return universalTransaction.Serialize();
		}

		string CreateAndReturnConsolRelatedUniversalTransactionWithSingleShipment(ZString orgCode, ZString consolNumber, ZString shipmentNumber, ZString chargeCode, ZDecimal osExTaxAmount, ZDecimal osTaxAmount)
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			universalTransaction.JobInvoiceNumber = "Consol1/AA";

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.OrganizationCode = orgCode;
			universalTransaction.OrganizationAddress = orgAddress;

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.ChargeCode = new ChargeCode { Code = chargeCode };
			universalLine.IsFinalCharge = true;
			universalLine.OSCurrency = new Currency { Code = "AUD" };
			universalLine.OSAmount = -1 * osExTaxAmount;
			universalLine.OSGSTVATAmount = -1 * osTaxAmount;
			universalLine.OSTotalAmount = universalLine.OSAmount + universalLine.OSGSTVATAmount;
			universalLine.Job = new EntityReference { Key = shipmentNumber, Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalLine.CostSource = new EntityReference { Key = consolNumber, Type = nameof(DataContextType.ForwardingConsol) };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber);
			universalShipment.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentNumber);
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipmentNumber;
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalConsol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalConsol.DataContext = DataContextFactory.New();
			universalConsol.DataContext.AddDataSource(DataContextType.ForwardingConsol, consolNumber);
			universalConsol.TransportMode = new UniversalCodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
			universalTransaction.ShipmentCollection.Add(universalConsol);

			return universalTransaction.Serialize();
		}

		protected override void SetUp()
		{
			base.SetUp();

			SettlementGroup = TestObjectCreator.CreateOrgHeader("SETMENTGRP", true, false);
			ChildOrganization = TestObjectCreator.CreateOrgHeader("CHILDORG", true, false);

			TestObjectCreator.AALSHI.AddRelatedParty(SettlementGroup.PK, RelatedPartyTypeList.Codes.APSettlementGroup, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			ChildOrganization.AddRelatedParty(TestObjectCreator.AALSHI.PK, RelatedPartyTypeList.Codes.APSettlementGroup, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);

			Factory.Save();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		OrgHeader SettlementGroup;
		OrgHeader ChildOrganization;
	}
}
