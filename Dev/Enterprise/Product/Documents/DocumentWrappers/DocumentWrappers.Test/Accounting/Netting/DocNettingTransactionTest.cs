using System;
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
	[TestedType(typeof(DocNettingTransaction))]
	sealed class DocNettingTransactionTest : DocumentWrapperTestCase
	{
		public void TestInvoiceNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber, docNettingTransaction => docNettingTransaction.Reference.InvoiceNumber);
		}

		public void TestJobInvoiceNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, docNettingTransaction => docNettingTransaction.Reference.JobInvoiceNumber);
		}

		public void TestShipmentNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.ShipmentNumber, docNettingTransaction => docNettingTransaction.Reference.ShipmentNumber);
		}

		public void TestConsolNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.ConsolNumber, docNettingTransaction => docNettingTransaction.Reference.ConsolNumber);
		}

		public void TestVesselVoyageNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.VesselVoyage, docNettingTransaction => docNettingTransaction.Reference.VesselVoyageNumber);
		}

		public void TestConsolContainerNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, docNettingTransaction => docNettingTransaction.Reference.ConsolContainerNumber);
		}

		public void TestMasterWayBillNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.MasterBill, docNettingTransaction => docNettingTransaction.Reference.MasterWayBillNumber);
		}

		public void TestBookingConfirmationReferenceNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, docNettingTransaction => docNettingTransaction.Reference.BookingConfirmationReferenceNumber);
		}

		public void TestAgentReferenceNumberIsPopulated()
		{
			TestTransactionReferenceField(AccountingConstants.TransactionReferenceTypes.AgentReference, docNettingTransaction => docNettingTransaction.Reference.AgentReferenceNumber);
		}

		void TestTransactionReferenceField(string referenceType, Func<DocNettingTransaction, ZString> propertyToTest)
		{
			SetTransactionReference(referenceType);

			Factory.Save();

			AssertNotNull(Statement.Transactions);
			var dbHitsBefore = Factory.DatabaseLoadCount;
			foreach (var transaction in Statement.Transactions)
			{
				var docNettingTransaction = DocNettingTransaction.New(transaction, Factory);
				AssertNotNull(docNettingTransaction.Reference);

				AssertReferences(transaction.TransactionPK, propertyToTest(docNettingTransaction));
			}
			var dbHitsAfter = Factory.DatabaseLoadCount;
			AssertEquals("There should be only one db hit to retrieve all the references", dbHitsBefore + 1, dbHitsAfter);
		}

		public void TestShipmentNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.ShipmentNumber, docNettingTransaction => docNettingTransaction.LineReference.ShipmentNumbers);
		}

		public void TestConsolNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.ConsolNumber, docNettingTransaction => docNettingTransaction.LineReference.ConsolNumbers);
		}

		public void TestVesselVoyageNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.VesselVoyage, docNettingTransaction => docNettingTransaction.LineReference.VesselVoyageNumbers);
		}

		public void TestConsolContainerNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, docNettingTransaction => docNettingTransaction.LineReference.ConsolContainerNumbers);
		}

		public void TestMasterWayBillNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.MasterBill, docNettingTransaction => docNettingTransaction.LineReference.MasterWayBillNumbers);
		}

		public void TestBookingConfirmationReferenceNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, docNettingTransaction => docNettingTransaction.LineReference.BookingConfirmationReferenceNumbers);
		}

		public void TestAgentReferenceNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.AgentReference, docNettingTransaction => docNettingTransaction.LineReference.AgentReferenceNumbers);
		}

		public void TestPackingReferenceNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.PackedContainer, docNettingTransaction => docNettingTransaction.LineReference.PackingReferenceNumbers);
		}

		public void TestHouseBillNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.HouseBill, docNettingTransaction => docNettingTransaction.LineReference.HouseBillNumbers);
		}

		public void TestOrderReferenceNumbersIsPopulated()
		{
			TestTransactionLineReferenceField(AccountingConstants.TransactionReferenceTypes.OrderReferences, docNettingTransaction => docNettingTransaction.LineReference.OrderReferenceNumbers);
		}

		void TestTransactionLineReferenceField(string referenceType, Func<DocNettingTransaction, ZString> propertyToTest)
		{
			SetTransactionLineReferences(referenceType);

			Factory.Save();

			AssertNotNull(Statement.Transactions);
			var dbHitsBefore = Factory.DatabaseLoadCount;
			foreach (var transaction in Statement.Transactions)
			{
				var docNettingTransaction = DocNettingTransaction.New(transaction, Factory);
				AssertNotNull(docNettingTransaction.Reference);
				AssertNotNull(docNettingTransaction.LineReference);

				AssertLineReferences(transaction.TransactionPK, propertyToTest(docNettingTransaction));
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
			//We do not allow any references for AP transactions for DocNettingTransaction
			else if (transactionPK == apTransaction1.PK)
			{
				AssertEquals(ZString.Empty, referenceValue);
			}
			else if (transactionPK == apTransaction2.PK)
			{
				AssertEquals(ZString.Empty, referenceValue);
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
			//We do not allow any references for AP transactions for DocNettingTransaction
			else if (transactionPK == apTransaction1.PK)
			{
				AssertEquals(ZString.Empty, referenceValue);
			}
			else if (transactionPK == apTransaction2.PK)
			{
				AssertEquals(ZString.Empty, referenceValue);
			}
			else if (transactionPK == apTransaction3.PK)
			{
				AssertEquals(ZString.Empty, referenceValue);
			}
		}

		void SetTransactionReference(string referenceType)
		{
			SetUpNettingCompaniesAndTransactions();
			NettingObjectCreator.AddNettingTransactionReference(arTransaction1, referenceType, "AR1");
			NettingObjectCreator.AddNettingTransactionReference(arTransaction2, referenceType, "AR2");

			NettingObjectCreator.AddNettingTransactionReference(apTransaction1, referenceType, "AP1");
			NettingObjectCreator.AddNettingTransactionReference(apTransaction2, referenceType, "AP2");
		}

		void SetTransactionLineReferences(string referenceType)
		{
			SetUpNettingCompaniesAndTransactions();
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
				DocNettingTransaction.New(NettingTransaction, Factory)
			};
		}

		NettingTransaction NettingTransaction;
		NettingStatement Statement;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);

			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var nettingSystem = NettingObjectCreator.CreateNettingSystem("code", "description", GlbCompany.CurrentCompany);
			var period = CreatePeriod(nettingSystem, "Current");

			Statement = new NettingCentreStatement(Factory, period.PK);
			NettingTransaction = new NettingTransaction(Statement);

			base.SetUp();
		}

		void SetUpNettingCompaniesAndTransactions()
		{
			var nettingSystem = Factory.LoadTop1<NettingSystem>(new ZQuery());
			var period = Factory.LoadTop1<NettingSystemPeriod>(new ZQuery());
			var org1 = NettingObjectCreator.CreateOrgHeader("AAAAA", true, true, "AUSYD");
			var org2 = NettingObjectCreator.CreateOrgHeader("BBBBB", true, true, "AUSYD");
			var org3 = NettingObjectCreator.CreateOrgHeader("CCCCC", true, true, "AUSYD");

			NettingObjectCreator.CreateNewCompany("CM1", org1);
			NettingObjectCreator.CreateNewCompany("CM2", org2);
			NettingObjectCreator.CreateNewCompany("CM3", org3);

			var participant1 = NettingObjectCreator.CreateNettingOrganisation(nettingSystem, org1, "HOM");
			var participant2 = NettingObjectCreator.CreateNettingOrganisation(nettingSystem, org2, "GRS");
			var participant3 = NettingObjectCreator.CreateNettingOrganisation(nettingSystem, org3, "FUL");

			SetupParticipant(participant1, "HOM");
			SetupParticipant(participant2, "GRS");
			SetupParticipant(participant3, "FUL");

			var matchPivot1 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "EUR 875", CurrencyCodes.EuropeanUnion, 875m);
			var matchPivot2 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "USD 400", CurrencyCodes.UnitedStates, 400m);
			var matchPivot3 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "EUR 350", CurrencyCodes.EuropeanUnion, 350m);

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
		}

		NettingSystemPeriod CreatePeriod(NettingSystem nettingSystem, ZString periodName)
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

			period.NSP_NS_NettingSystem = nettingSystem.PK;

			return period;
		}

		void SetupParticipant(NettingOrganisation participant, ZString nettingType)
		{
			participant.NSO_NettingType = nettingType;
			participant.NSO_RX_NKReportingCurrency = CurrencyCodes.UnitedKingdom;
			participant.NSO_RX_NKARSettlementCurrency = CurrencyCodes.EuropeanUnion;
			participant.NSO_RX_NKAPSettlementCurrency = CurrencyCodes.UnitedStates;
		}

		NettingObjectCreator NettingObjectCreator => nettingObjectCreator ?? (nettingObjectCreator = new NettingObjectCreator(Factory));
		NettingObjectCreator nettingObjectCreator;

		NettingReceivableTransaction arTransaction1, arTransaction2, arTransaction3;
		INettingTransactionLine arTransaction1Line1, arTransaction1Line2, arTransaction1Line3, arTransaction2Line1, arTransaction2Line2, arTransaction2Line3;
		NettingPayableTransaction apTransaction1, apTransaction2, apTransaction3;
		INettingTransactionLine apTransaction1Line1, apTransaction1Line2, apTransaction2Line1, apTransaction2Line2;
	}
}
