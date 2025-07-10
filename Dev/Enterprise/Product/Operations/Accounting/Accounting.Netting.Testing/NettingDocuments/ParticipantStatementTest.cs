using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(ParticipantStatement))]
	public class ParticipantStatementTest : NettingStatementTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ParticipantStatement(Factory, Period.PK);
		}

		protected override void AssertSignsForNetMovements()
		{
			ParticipantStatement bizO = (ParticipantStatement)GetNewBusinessObject();
			var participantReceivables = bizO.GetReceivableNettingMovements();
			var participantPayables = bizO.GetPayableNettingMovements();

			foreach (var item in participantReceivables)
			{
				Assert("Amount should be negetaive for all receivable movement for Participants", item.SignedMovementAmount < 0);
			}

			foreach (var item in participantPayables)
			{
				Assert("Amount should be positive for all receivable movement for Participants", item.SignedMovementAmount > 0);
			}
		}

		public void TestNettingOrganizationWithIncorrectDataSetup()
		{
			var companyOrgProxy = NettingObjectCreator.CreateOrgHeader("OrgCompany1", true, true);
			var company = NettingObjectCreator.CreateNewCompany("CC1", companyOrgProxy);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.NettingModeOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.NettingModeOption.CompanyLevel.Code);

			AssertNoExceptionThrown(() =>
			{
				new ParticipantStatement(Factory, Period.PK, "CM1", StatementType.Final);
				new ParticipantStatement(Factory, Period.PK, string.Empty, StatementType.Final);
			});

			AssertExceptionThrown<IncorrectDataSetupException>("Netting Participant not found. The Organization Proxy of company 'CC1' is not a Netting Participant. Kindly add the same as the participant and try again.",
				() => new ParticipantStatement(Factory, Period.PK, "CC1", StatementType.Final));

			AccountingConfigurationRegistry.Instance.NettingModeOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.NettingModeOption.OrganizationLevel.Code);

			AssertNoExceptionThrown(() =>
			{
				new ParticipantStatement(Factory, Period.PK, "CM1", StatementType.Final);
				new ParticipantStatement(Factory, Period.PK, string.Empty, StatementType.Final);
				new ParticipantStatement(Factory, Period.PK, "CC1", StatementType.Final);
			});
		}

		[TestDate(2018, 09, 15)]
		public void TestNettingClearingJournals()
		{
			AccountingConfigurationRegistry.Instance.NettingModeOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.NettingModeOption.OrganizationLevel.Code);

			var company1OrgProxy = NettingObjectCreator.CreateOrgHeader("OrgCompany1", true, true);
			var company1 = NettingObjectCreator.CreateNewCompany("CC1", company1OrgProxy);
			var branchOrgProxy = NettingObjectCreator.CreateOrgHeader("OrgBranch", true, true);
			var branch = NettingObjectCreator.CreateNewBranch(company1, "BR1");
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;
			var company2OrgProxy = NettingObjectCreator.CreateOrgHeader("OrgCompany2", true, true);
			var company2 = NettingObjectCreator.CreateNewCompany("CC2", company2OrgProxy);

			Factory.Save();

			var ns = NettingObjectCreator.CreateNettingSystem("NS1", "Test Netting System", GlbCompany.CurrentCompany);
			var period = NettingObjectCreator.CreateNettingPeriod(ns, "201801", new ZDateTime(2018, 09, 01), new ZDateTime(2018, 09, 30), new ZDateTime(2018, 10, 15), new ZDateTime(2018, 10, 10), new ZDateTime(2018, 10, 12), new ZDateTime(2018, 10, 13), new ZDate(2018, 09, 28));

			var nettingSystemParticipant = NettingObjectCreator.CreateNettingOrganisation(ns, GlbCompany.CurrentCompany.OrgProxy, "FUL");
			var participant1 = NettingObjectCreator.CreateNettingOrganisation(ns, company1OrgProxy, "FUL");
			var participant2 = NettingObjectCreator.CreateNettingOrganisation(ns, branchOrgProxy, "FUL");
			var participant3 = NettingObjectCreator.CreateNettingOrganisation(ns, company2OrgProxy, "FUL");

			var currencyCode = NettingObjectCreator.AUD.RX_Code;
			SetupParticipant(nettingSystemParticipant, "FUL", currencyCode, currencyCode, currencyCode);
			SetupParticipant(participant1, "FUL", currencyCode, currencyCode, currencyCode);
			SetupParticipant(participant2, "FUL", currencyCode, currencyCode, currencyCode);
			SetupParticipant(participant3, "FUL", currencyCode, currencyCode, currencyCode);

			Factory.Save();

			NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant3, "TRN001", currencyCode, 1000M);
			NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant3, "TRN002", currencyCode, 2000M);

			NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant3, participant1, "TRN003", currencyCode, 100M);
			NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant3, participant2, "TRN004", currencyCode, 200M);

			NettingObjectCreator.CreateNettingExchangeRate(ns, currencyCode, NettingExchangeRateType.Execution, 1m, period);
			Factory.Save();

			NettingObjectCreator.GenerateCalculationRecords(period.PK.ToGuid());

			var nettingStatement = new NettingCentreStatement(Factory, period.PK, StatementType.Final);
			var participants = nettingStatement.GetAllParticipants();
			AssertEquals("Participants are company based, so 2 participant should be found", 2, participants.Count());

			var participantStatement1 = new ParticipantStatement(Factory, period.PK, company1.GC_Code, StatementType.Final);
			var company1ClearingJournals = participantStatement1.NettingClearingJournals;
			AssertNotNull("Should contain entries for company1 org proxy", company1ClearingJournals.FirstOrDefault(x => x.OrgCode == company1.OrgProxy.OH_Code));
			AssertNotNull("Should contain entries for company1 branch org proxy", company1ClearingJournals.FirstOrDefault(x => x.OrgCode == company1.FirstActiveBranch.OrgProxy.OH_Code));
			AssertNotNull("Should also contain enties from company2 org proxy, this is for getting actual AP amount. Since our netting is receivable only we do not store the actual AP amount", company1ClearingJournals.FirstOrDefault(x => x.OrgCode == company2.OrgProxy.OH_Code));

			var participantStatement2 = new ParticipantStatement(Factory, period.PK, company2.GC_Code, StatementType.Final);
			var company2ClearingJournals = participantStatement2.NettingClearingJournals;
			AssertNotNull("Should contain entries for company2 org proxy", company2ClearingJournals.FirstOrDefault(x => x.OrgCode == company2.OrgProxy.OH_Code));
			AssertNotNull("Should also contain enties from company1 org proxy, this is for getting actual AP amount. Since our netting is receivable only we do not store the actual AP amount", company2ClearingJournals.FirstOrDefault(x => x.OrgCode == company1.OrgProxy.OH_Code));
			AssertNotNull("Should also contain enties from company1 branch org proxy, this is for getting actual AP amount. Since our netting is receivable only we do not store the actual AP amount", company2ClearingJournals.FirstOrDefault(x => x.OrgCode == company1.FirstActiveBranch.OrgProxy.OH_Code));
		}

		[TestDate(2019, 10, 20)]
		public void TestPartiallyMatchedAPTransactionNotIncludedInTrialStatement()
		{
			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NettingObjectCreator.GLHeader1.PK.ToGuid());

			var period1 = Period;
			var period2 = NettingObjectCreator.CreateNextNettingPeriod(period1, "201902");
			NettingObjectCreator.CreateNextNettingPeriod(period2, "201903");

			var issuerEHubID = "123456";
			participant1.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, issuerEHubID);

			var receipientEHubID = "234567";
			participant2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, receipientEHubID);

			var currency = NettingObjectCreator.AUD.RX_Code;

			var arInvoice1Reference = "1001001";
			var arTransaction = NettingObjectCreator.CreateNettingTransaction("AR", period1, participant1, participant2, arInvoice1Reference, currency, 100M);
			NettingObjectCreator.CreateNettingTransactionLine(arTransaction, "S001001", 100M, currency);

			var apReference = "AP1001001";
			var apTransaction = NettingObjectCreator.CreateNettingTransaction("AP", period1, participant1, participant2, apReference, currency, 200M);
			NettingObjectCreator.CreateNettingTransactionLine(apTransaction, "S001001", 100M, currency);
			NettingObjectCreator.CreateNettingTransactionLine(apTransaction, "S001002", 100M, currency);

			Factory.Save();
			NettingHelper.NettingMatchTransactions(CargoWise.Data.Db.Connection, period1.PK.ToGuid(), issuerEHubID, receipientEHubID, GlbCompany.CurrentCompany.PK.ToGuid(), "~BP");

			var newFactory = new BusinessObjectFactory();
			var arTransactionInNewFactory = newFactory.Load<NettingReceivableTransaction>(arTransaction.PK);
			var apTransactionInNewFactory = newFactory.Load<NettingPayableTransaction>(apTransaction.PK);

			var participant1CompanyCode = participant1.Organisation.CompanyProxies(true).FirstOrDefault().GC_Code;
			var participant2CompanyCode = participant2.Organisation.CompanyProxies(true).FirstOrDefault().GC_Code;

			var participant1Period1Trial = new ParticipantStatement(Factory, period1.PK.ToGuid(), participant1CompanyCode, StatementType.Trial);
			var participant2Period1Trial = new ParticipantStatement(Factory, period1.PK.ToGuid(), participant2CompanyCode, StatementType.Trial);

			AssertEquals("Precondition: AR transaction is matched fully", NettingTransactionApprovalStatus.Matched, arTransactionInNewFactory.ApprovalStatus);
			AssertEquals("Precondition: But AP transaction is still in approved status", NettingTransactionApprovalStatus.Approved, apTransactionInNewFactory.ApprovalStatus);

			CombineAssertions("Trial statements should not include the AR and AP transactions since AP is not matched yet", () =>
			{
				AssertEquals(false, participant1Period1Trial.Transactions.Any(x => x.MatchingTransactionReference == arInvoice1Reference));
				AssertEquals(false, participant2Period1Trial.Transactions.Any(x => x.MatchingTransactionReference == arInvoice1Reference));
			});

			var notificationBuffer = new ZArchitecture.NotificationBuffer();
			new NettingPeriodManager(period1).FinaliseNettingCycle(notificationBuffer);

			newFactory.ReloadAll<NettingReceivableTransaction>();
			newFactory.ReloadAll<NettingPayableTransaction>();
			CombineAssertions("Since AP is a candidate for moving to next cycle both AR and AP transactions have moved to next cycle", () =>
			{
				AssertEquals(period2.PK.ToGuid(), arTransactionInNewFactory.NRT_NSP_Period);
				AssertEquals(period2.PK.ToGuid(), apTransactionInNewFactory.NPT_NSP_Period);
			});

			var arInvoice2Reference = "1001002";
			var arTransaction1 = NettingObjectCreator.CreateNettingTransaction("AR", period2, participant1, participant2, arInvoice2Reference, currency, 100M);
			NettingObjectCreator.CreateNettingTransactionLine(arTransaction1, "S001002", 100M, currency);

			Factory.Save();
			NettingHelper.NettingMatchTransactions(CargoWise.Data.Db.Connection, period2.PK.ToGuid(), issuerEHubID, receipientEHubID, GlbCompany.CurrentCompany.PK.ToGuid(), "~BP");

			var arTransaction1InNewFactory = newFactory.Load<NettingReceivableTransaction>(arTransaction1.PK);

			newFactory.ReloadAll<NettingPayableTransaction>();
			AssertEquals("Precondition: AP transaction is now in matched status", NettingTransactionApprovalStatus.Matched, apTransactionInNewFactory.ApprovalStatus);

			var participant1Period2Trial = new ParticipantStatement(Factory, period2.PK.ToGuid(), participant1CompanyCode, StatementType.Trial);
			var participant2Period2Trial = new ParticipantStatement(Factory, period2.PK.ToGuid(), participant2CompanyCode, StatementType.Trial);

			CombineAssertions("Trial statements should now include the AR and AP transactions since all transactions are now matched", () =>
			{
				AssertEquals(true, participant1Period2Trial.Transactions.Any(x => x.MatchingTransactionReference == arInvoice1Reference));
				AssertEquals(true, participant2Period2Trial.Transactions.Any(x => x.MatchingTransactionReference == arInvoice1Reference));

				AssertEquals(true, participant1Period2Trial.Transactions.Any(x => x.MatchingTransactionReference == arInvoice2Reference));
				AssertEquals(true, participant2Period2Trial.Transactions.Any(x => x.MatchingTransactionReference == arInvoice2Reference));
			});

			NettingObjectCreator.NettingCopyIndicativeRatesToExecutionRates(period2);
			Factory.Save();

			notificationBuffer = new ZArchitecture.NotificationBuffer();
			new NettingPeriodManager(period2).FinaliseNettingCycle(notificationBuffer);

			Factory.Save();

			newFactory.ReloadAll<NettingReceivableTransaction>();
			newFactory.ReloadAll<NettingPayableTransaction>();

			CombineAssertions("All transactions should be settled now", () =>
			{
				AssertEquals(NettingTransactionApprovalStatus.Setteled, arTransactionInNewFactory.ApprovalStatus);
				AssertEquals(NettingTransactionApprovalStatus.Setteled, arTransaction1InNewFactory.ApprovalStatus);
				AssertEquals(NettingTransactionApprovalStatus.Setteled, apTransactionInNewFactory.ApprovalStatus);
			});

			var participant1FinalStatement = new ParticipantStatement(Factory, period2.PK.ToGuid(), participant1CompanyCode, StatementType.Final);
			var participant2FinalStatement = new ParticipantStatement(Factory, period2.PK.ToGuid(), participant2CompanyCode, StatementType.Final);

			CombineAssertions("Final statements should now include the AR and AP transactions since all transactions are now matched", () =>
			{
				AssertEquals(true, participant1FinalStatement.Transactions.Any(x => x.MatchingTransactionReference == arInvoice1Reference));
				AssertEquals(true, participant2FinalStatement.Transactions.Any(x => x.MatchingTransactionReference == arInvoice1Reference));

				AssertEquals(true, participant1FinalStatement.Transactions.Any(x => x.MatchingTransactionReference == arInvoice2Reference));
				AssertEquals(true, participant2FinalStatement.Transactions.Any(x => x.MatchingTransactionReference == arInvoice2Reference));
			});
		}
	}
}
