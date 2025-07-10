using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingClearingJournal))]
	sealed class DocNettingClearingJournalTest : DocumentWrapperTestCase
	{
		public void TestInvoiceNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber, docNettingClearingJournal => docNettingClearingJournal.Reference.InvoiceNumber);
		}

		public void TestJobInvoiceNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, docNettingClearingJournal => docNettingClearingJournal.Reference.JobInvoiceNumber);
		}

		public void TestShipmentNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.ShipmentNumber, docNettingClearingJournal => docNettingClearingJournal.Reference.ShipmentNumber);
		}

		public void TestConsolNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.ConsolNumber, docNettingClearingJournal => docNettingClearingJournal.Reference.ConsolNumber);
		}

		public void TestVesselVoyageNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.VesselVoyage, docNettingClearingJournal => docNettingClearingJournal.Reference.VesselVoyageNumber);
		}

		public void TestConsolContainerNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, docNettingClearingJournal => docNettingClearingJournal.Reference.ConsolContainerNumber);
		}

		public void TestMasterWayBillNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.MasterBill, docNettingClearingJournal => docNettingClearingJournal.Reference.MasterWayBillNumber);
		}

		public void TestBookingConfirmationReferenceNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, docNettingClearingJournal => docNettingClearingJournal.Reference.BookingConfirmationReferenceNumber);
		}

		public void TestAgentReferenceNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.AgentReference, docNettingClearingJournal => docNettingClearingJournal.Reference.AgentReferenceNumber);
		}

		void TestTransactionReferenceField(string referenceType, Func<DocNettingClearingJournal, ZString> propertyToTest)
		{
			SetTransactionReference(referenceType);

			Factory.Save();

			Assert(Statement.NettingClearingJournals.IsCountMoreThan(0));
			var dbHitsBefore = Factory.DatabaseLoadCount;
			foreach (var clearingJournal in Statement.NettingClearingJournals)
			{
				var docNettingClearingJournal = DocNettingClearingJournal.New(clearingJournal, Factory);
				AssertNotNull(docNettingClearingJournal.Reference);

				AssertReferences(clearingJournal.TransactionPK, propertyToTest(docNettingClearingJournal));
			}
			var dbHitsAfter = Factory.DatabaseLoadCount;
			AssertEquals("There should be only one db hit to retrieve all the references", dbHitsBefore + 1, dbHitsAfter);
		}

		public void TestShipmentNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.ShipmentNumber, docNettingClearingJournal => docNettingClearingJournal.LineReference.ShipmentNumbers);
		}

		public void TestConsolNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.ConsolNumber, docNettingClearingJournal => docNettingClearingJournal.LineReference.ConsolNumbers);
		}

		public void TestVesselVoyageNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.VesselVoyage, docNettingClearingJournal => docNettingClearingJournal.LineReference.VesselVoyageNumbers);
		}

		public void TestConsolContainerNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, docNettingClearingJournal => docNettingClearingJournal.LineReference.ConsolContainerNumbers);
		}

		public void TestMasterWayBillNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.MasterBill, docNettingClearingJournal => docNettingClearingJournal.LineReference.MasterWayBillNumbers);
		}

		public void TestBookingConfirmationReferenceNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, docNettingClearingJournal => docNettingClearingJournal.LineReference.BookingConfirmationReferenceNumbers);
		}

		public void TestAgentReferenceNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.AgentReference, docNettingClearingJournal => docNettingClearingJournal.LineReference.AgentReferenceNumbers);
		}

		public void TestPackingReferenceNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.PackedContainer, docNettingClearingJournal => docNettingClearingJournal.LineReference.PackingReferenceNumbers);
		}

		public void TestHouseBillNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.HouseBill, docNettingClearingJournal => docNettingClearingJournal.LineReference.HouseBillNumbers);
		}

		public void TestOrderReferenceNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.OrderReferences, docNettingClearingJournal => docNettingClearingJournal.LineReference.OrderReferenceNumbers);
		}

		void TestTransactionLineReferenceField(string referenceType, Func<DocNettingClearingJournal, ZString> propertyToTest)
		{
			SetTransactionLineReferences(referenceType);

			Factory.Save();

			Assert(Statement.NettingClearingJournals.IsCountMoreThan(0));
			var dbHitsBefore = Factory.DatabaseLoadCount;
			foreach (var clearingJournal in Statement.NettingClearingJournals)
			{
				var docNettingClearingJournal = DocNettingClearingJournal.New(clearingJournal, Factory);
				AssertNotNull(docNettingClearingJournal.Reference);
				AssertNotNull(docNettingClearingJournal.LineReference);

				AssertLineReferences(clearingJournal.TransactionPK, propertyToTest(docNettingClearingJournal));
			}
			var dbHitsAfter = Factory.DatabaseLoadCount;
			AssertEquals("There should be only one db hit to retrieve all the references", dbHitsBefore + 1, dbHitsAfter);
		}

		void AssertReferences(ZGuid transactionPK, ZString referenceValue)
		{
			if (transactionPK == arTransaction1.PK)
			{
				AssertEquals("AR1", referenceValue);
			}
			else if (transactionPK == arTransaction2.PK)
			{
				AssertEquals("AR2", referenceValue);
			}
			else if (transactionPK == arTransaction3.PK)
			{
				AssertEquals(ZString.Empty, referenceValue);
			}
			else if (transactionPK == apTransaction1.PK)
			{
				AssertEquals("AP1", referenceValue);
			}
			else if (transactionPK == apTransaction2.PK)
			{
				AssertEquals("AP2", referenceValue);
			}
			else if (transactionPK == apTransaction3.PK)
			{
				AssertEquals(ZString.Empty, referenceValue);
			}
		}

		void AssertLineReferences(ZGuid transactionPK, ZString referenceValue)
		{
			if (transactionPK == arTransaction1.PK)
			{
				AssertEquals("A1, A2, A3", referenceValue);
			}
			else if (transactionPK == arTransaction2.PK)
			{
				AssertEquals("A1, A2", referenceValue);
			}
			else if (transactionPK == arTransaction3.PK)
			{
				AssertEquals(ZString.Empty, referenceValue);
			}
			else if (transactionPK == apTransaction1.PK)
			{
				AssertEquals("A1, A2", referenceValue);
			}
			else if (transactionPK == apTransaction2.PK)
			{
				AssertEquals("A1, A2", referenceValue);
			}
			else if (transactionPK == apTransaction3.PK)
			{
				AssertEquals(ZString.Empty, referenceValue);
			}
		}

		void SetTransactionReference(string referenceType)
		{
			SetUpNettingTransactions();
			NettingObjectCreator.AddNettingTransactionReference(arTransaction1, referenceType, "AR1");
			NettingObjectCreator.AddNettingTransactionReference(arTransaction2, referenceType, "AR2");

			NettingObjectCreator.AddNettingTransactionReference(apTransaction1, referenceType, "AP1");
			NettingObjectCreator.AddNettingTransactionReference(apTransaction2, referenceType, "AP2");
		}

		void SetTransactionLineReferences(string referenceType)
		{
			SetUpNettingTransactions();
			NettingObjectCreator.AddNettingLineReference(arTransaction1Line1, referenceType, "A1");
			NettingObjectCreator.AddNettingLineReference(arTransaction1Line2, referenceType, "A2");
			NettingObjectCreator.AddNettingLineReference(arTransaction1Line3, referenceType, "A3");

			NettingObjectCreator.AddNettingLineReference(arTransaction2Line1, referenceType, "A1");
			NettingObjectCreator.AddNettingLineReference(arTransaction2Line2, referenceType, "A2");
			NettingObjectCreator.AddNettingLineReference(arTransaction2Line3, referenceType, "A1");

			NettingObjectCreator.AddNettingLineReference(apTransaction1Line1, referenceType, "A1");
			NettingObjectCreator.AddNettingLineReference(apTransaction1Line2, referenceType, "A2");

			NettingObjectCreator.AddNettingLineReference(apTransaction2Line1, referenceType, "A1");
			NettingObjectCreator.AddNettingLineReference(apTransaction2Line2, referenceType, "A2");
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocNettingClearingJournal.New(new NettingClearingJournal(Statement), Factory)
			};
		}

		NettingStatement Statement;
		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);

			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			participant1 = SetupParticipant("AAAAA", "CM1", "HOM");
			participant2 = SetupParticipant("BBBBB", "CM2", "GRS");
			Statement = new ParticipantStatement(Factory, Period.PK, "CM1", Enterprise.Accounting.Netting.StatementType.Trial);

			base.SetUp();
		}

		void SetUpNettingTransactions()
		{
			var nettingSystem = Factory.LoadTop1<NettingSystem>(new ZQuery());
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.Australia, "NET", 0.7503m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.UnitedStates, "NET", 1m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.UnitedKingdom, "NET", 1.2001m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.EuropeanUnion, "NET", 1.5m, Period);

			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.Australia, "IND", 0.7503m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.UnitedStates, "IND", 1m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.UnitedKingdom, "IND", 1.2001m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.EuropeanUnion, "IND", 1.5m, Period);

			var matchPivot1 = NettingObjectCreator.CreateFullMatchingNettingTransaction(Period, participant1, participant2, "EUR 875", CurrencyCodes.EuropeanUnion, 875m);
			var matchPivot2 = NettingObjectCreator.CreateFullMatchingNettingTransaction(Period, participant2, participant1, "USD 400", CurrencyCodes.UnitedStates, 400m);
			var matchPivot3 = NettingObjectCreator.CreateFullMatchingNettingTransaction(Period, participant2, participant1, "EUR 350", CurrencyCodes.EuropeanUnion, 350m);

			arTransaction1 = Factory.Load<NettingReceivableTransaction>(matchPivot1.NMP_NRT_ReceivableTransaction);
			apTransaction1 = Factory.Load<NettingPayableTransaction>(matchPivot1.NMP_NPT_PayableTransaction);

			arTransaction2 = Factory.Load<NettingReceivableTransaction>(matchPivot2.NMP_NRT_ReceivableTransaction);
			apTransaction2 = Factory.Load<NettingPayableTransaction>(matchPivot2.NMP_NPT_PayableTransaction);

			arTransaction3 = Factory.Load<NettingReceivableTransaction>(matchPivot3.NMP_NRT_ReceivableTransaction);
			apTransaction3 = Factory.Load<NettingPayableTransaction>(matchPivot3.NMP_NPT_PayableTransaction);

			arTransaction1Line1 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction1, "J1", 100M, CurrencyCodes.UnitedStates);
			arTransaction1Line2 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction1, "J2", 100M, CurrencyCodes.UnitedStates);
			arTransaction1Line3 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction1, "J3", 100M, CurrencyCodes.UnitedStates);

			arTransaction2Line1 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction2, "J1", 100M, CurrencyCodes.UnitedStates);
			arTransaction2Line2 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction2, "J2", 100M, CurrencyCodes.UnitedStates);
			arTransaction2Line3 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction2, "J3", 100M, CurrencyCodes.UnitedStates);

			apTransaction1Line1 = NettingObjectCreator.CreateNettingTransactionLine(apTransaction1, "J1", 100M, CurrencyCodes.UnitedStates);
			apTransaction1Line2 = NettingObjectCreator.CreateNettingTransactionLine(apTransaction1, "J2", 100M, CurrencyCodes.UnitedStates);

			apTransaction2Line1 = NettingObjectCreator.CreateNettingTransactionLine(apTransaction2, "J1", 100M, CurrencyCodes.UnitedStates);
			apTransaction2Line2 = NettingObjectCreator.CreateNettingTransactionLine(apTransaction2, "J2", 100M, CurrencyCodes.UnitedStates);
			Factory.Save();

			NettingObjectCreator.GenerateCalculationRecords(Period.PK.ToGuid());
		}

		NettingSystemPeriod CreatePeriod(ZString periodName)
		{
			var period = Factory.New<NettingSystemPeriod>();
			period.NSP_Period = periodName;
			period.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(-7);
			period.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(15);
			period.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(17);

			period.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(5);
			period.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(8);
			period.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(10);
			period.NSP_ValueDate = ZDate.Today.AddDays(12);
			period.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(12);

			period.NSP_NS_NettingSystem = NettingSystem.PK;

			return period;
		}

		NettingOrganisation SetupParticipant(ZString orgCode, ZString companyCode, ZString nettingType)
		{
			var orgProxy = NettingObjectCreator.CreateOrgHeader(orgCode, true, true, "AUSYD");
			NettingObjectCreator.CreateNewCompany(companyCode, orgProxy);

			var participant = NettingObjectCreator.CreateNettingOrganisation(NettingSystem, orgProxy, nettingType);
			participant.NSO_NettingType = nettingType;
			participant.NSO_RX_NKReportingCurrency = CurrencyCodes.UnitedKingdom;
			participant.NSO_RX_NKARSettlementCurrency = CurrencyCodes.EuropeanUnion;
			participant.NSO_RX_NKAPSettlementCurrency = CurrencyCodes.UnitedStates;
			return participant;
		}

		NettingObjectCreator NettingObjectCreator => nettingObjectCreator ?? (nettingObjectCreator = new NettingObjectCreator(Factory));
		NettingObjectCreator nettingObjectCreator;

		NettingSystem NettingSystem => nettingSystem ?? (nettingSystem = NettingObjectCreator.CreateNettingSystem("code", "desc", GlbCompany.CurrentCompany));
		NettingSystem nettingSystem;

		NettingSystemPeriod Period => period ?? (period = CreatePeriod("Current"));
		NettingSystemPeriod period;

		NettingOrganisation participant1, participant2;

		NettingReceivableTransaction arTransaction1, arTransaction2, arTransaction3;
		INettingTransactionLine arTransaction1Line1, arTransaction1Line2, arTransaction1Line3, arTransaction2Line1, arTransaction2Line2, arTransaction2Line3;
		NettingPayableTransaction apTransaction1, apTransaction2, apTransaction3;
		INettingTransactionLine apTransaction1Line1, apTransaction1Line2, apTransaction2Line1, apTransaction2Line2;
	}
}
