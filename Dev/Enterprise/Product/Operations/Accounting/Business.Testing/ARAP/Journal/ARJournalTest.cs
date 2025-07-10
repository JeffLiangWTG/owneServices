using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	[TestedType(typeof(ARJournal))]
	public class ARJournalTest : JournalTest
	{
		#region Implementation

		Guid WhtAccountPK;
		Guid MatchingAccountPK;

		protected override void SetUp()
		{
			base.SetUp();

			WhtAccountPK = Factory.NewWithValidTestData<AccGLHeader>().PK.ToGuid();
			MatchingAccountPK = Factory.NewWithValidTestData<AccGLHeader>().PK.ToGuid();

			AccountingConfigurationRegistry.Instance.WHTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WhtAccountPK);
			AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MatchingAccountPK);
		}

		#endregion

		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<ARJournal>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestDefaults()
		{
			AssertEquals("Description should default to AR JOURNAL",
				"AR JOURNAL", Header.AH_Desc);
			AssertEquals("AR Journal should default to CR",
				DebitCreditDataEntry.CR, ((ARJournal)Header).DebitCreditSign);
		}

		public void TestWHTAccount()
		{
			ARJournal aRJnl = Factory.NewWithValidTestData<ARJournal>();
			AssertEquals(WhtAccountPK, aRJnl.WHTAccount_ForTestOnly);
		}

		public void TestMatchingAccount()
		{
			ARJournal aRJnl = Factory.NewWithValidTestData<ARJournal>();
			AssertEquals(MatchingAccountPK, aRJnl.MatchingAccount_ForTestOnly);
		}

		public void TestReversingARJournal()
		{
			ARJournal aRJnl = Factory.NewWithValidTestData<ARJournal>();
			aRJnl.DebitCreditSign = DebitCreditDataEntry.DR; // DB values are negative whatever the amounts are set to
			aRJnl.AH_OSExTaxAmount = 390M;
			aRJnl.AH_LocalExTaxAmount = 390M;

			Factory.Save();

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			ARJournal loadedARJnl = loadingFactory.Load<ARJournal>(aRJnl.PK);

			ReversingBase journalReverser = new ReversingFactory().NewReversing(loadedARJnl);
			journalReverser.Reverse();
			loadingFactory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, aRJnl.PK);
			ARJournal revARJnl = loadingFactory.LoadTop1<ARJournal>(filter);
			AssertNotNull("There should be a reversing ARJnl", revARJnl);
			AssertEquals("The reversing Journal should have OSTotal = 390", 390M, revARJnl.AH_OSTotal);
			AssertEquals("The reversing Journal should have InvoiceAmount = 390", 390M, revARJnl.AH_InvoiceAmount);
		}

		public void TestGenerateReverseTransaction_BadDebt()
		{
			ZGuid expectedGenericCharge = ZGuid.NewZGuid();
			ZGuid badDebtGenericCharge = (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			Journal.AH_AG = expectedGenericCharge;

			Journal.GenerateReverseTransaction(true);

			AssertEquals("Reversing Invoice should have GenericCharge the same as in source", expectedGenericCharge, ((Journal)Journal.ReverseTransaction).AH_AG);

			IBadDebtWritingOff badDebt = Journal as IBadDebtWritingOff;
			if (badDebt != null)
			{
				badDebt.IsWritingOff = true;
			}

			Journal.GenerateReverseTransaction(true);

			if (badDebt != null)
			{
				AssertEquals("Reversing Invoice should have Bad Debt GenericCharge", badDebtGenericCharge, ((Journal)Journal.ReverseTransaction).AH_AG);

				if (Journal.ReverseTransaction is IBadDebtWritingOff)
				{
					AssertEquals("Reversing Invoice should have IsWrittingOff the same as in source", badDebt.IsWritingOff, ((IBadDebtWritingOff)Journal.ReverseTransaction).IsWritingOff);
				}
			}
			else
			{
				AssertEquals("Reversing Invoice should have GenericCharge the same as in source", expectedGenericCharge, ((Journal)Journal.ReverseTransaction).AH_AG);
			}
		}

		public void TestBedDebtWritingOffARJournal()
		{
			ARJournal aRJnl = Factory.NewWithValidTestData<ARJournal>();
			aRJnl.DebitCreditSign = DebitCreditDataEntry.DR; // DB values are negative whatever the amounts are set to
			aRJnl.AH_OSExTaxAmount = 390M;
			aRJnl.AH_LocalExTaxAmount = 390M;

			Factory.Save();

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			ARJournal loadedARJnl = loadingFactory.Load<ARJournal>(aRJnl.PK);

			BadDebtWritingOffFactory writingOffFactory = new BadDebtWritingOffFactory();
			ReversingBase journalReverser = writingOffFactory.NewWritingOff(loadedARJnl);
			journalReverser.Reverse();
			loadingFactory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, aRJnl.PK);
			ARJournal revARJnl = loadingFactory.LoadTop1<ARJournal>(filter);
			AssertNotNull("There should be a reversing ARJnl", revARJnl);
			AssertEquals("The reversing Journal should have OSTotal = 390", 390M, revARJnl.AH_OSTotal);
			AssertEquals("The reversing Journal should have InvoiceAmount = 390", 390M, revARJnl.AH_InvoiceAmount);
			AssertEquals("The reversing Journal should have DebitCreditSign = DR", DebitCreditDataEntry.CR, revARJnl.DebitCreditSign);
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "RJN", ((IDocManagerSupport)Header).DocManagerInfo.DocManagerCode);
		}

		public void TestTransactionAttachedToActiveBatchDoesNotAllowReversing()
		{
			var aRJnl = Factory.NewWithValidTestData<ARJournal>();
			aRJnl.DebitCreditSign = DebitCreditDataEntry.CR;
			aRJnl.AH_OSExTaxAmount = 390M;
			aRJnl.AH_LocalExTaxAmount = 390M;
			aRJnl.AH_AgreedPaymentMethodOverride = "CRQ";

			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			var batch = Factory.New<AccCollectionBatch>();
			batch.ACB_TotalAmount = 390M;
			batch.ACB_AB = bank.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";

			var order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDate.Today;
			order.ACO_OH_Debtor = TestObjectCreator.AALSHI.PK;
			order.ACO_OrderNumber = "000001";
			order.IncludeInBatch = true;
			order.ACO_Amount = 390M;

			var orderline = Factory.New<AccCollectionOrderLine>();
			orderline.AOL_ACO = order.PK;
			orderline.AOL_AH = aRJnl.PK;
			orderline.AOL_IsCancelled = false;
			orderline.IncludeInOrder = true;

			Factory.Save();

			var loadingFactory = new BusinessObjectFactory();
			var loadedARJnl = loadingFactory.Load<ARJournal>(aRJnl.PK);

			var journalReverser = new ReversingFactory().NewReversing(loadedARJnl);

			Assert("Should never allow reverse on transaction attached to active batch.", !journalReverser.CanReverseTransaction);
			AssertEquals("Can't reverse error", journalReverser.TransactionAttachedToCollectionBatchErrorMessage_ForTestOnly, journalReverser.GenerateCantReverseErrorMessage_ForTestOnly());
		}
	}
}
