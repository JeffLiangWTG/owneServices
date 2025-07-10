using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class AmendingExtensionsTest : TestCaseWithFactory
	{
		public void TestCheckIsAmendingTransaction()
		{
			foreach (IAmending original in originalTransactions)
			{
				AssertEquals(string.Format("Original {0} should not be AmendingTransaction", original.HumanReadableName), false, original.CheckIsAmendingTransaction(true));
			}

			foreach (IAmending original in originalTransactions)
			{
				foreach (string amendingType in amendingTransactionTypes)
				{
					IAmending amending = original.GenerateAmendingTransaction(amendingType);
					AssertNotNull(string.Format("Should generate Amending transaction of {0} type for {1}", amendingType, original.HumanReadableName), amending);
					AssertEquals(string.Format("{0} should be referring {1} as OriginalTransaction", amending.HumanReadableName, original.HumanReadableName), original, amending.OriginalTransaction);
					Assert(string.Format("{0} should be AmendingTransaction for {1}", amending.HumanReadableName, original.HumanReadableName), amending.CheckIsAmendingTransaction(true));
				}
			}
		}

		public void TestCheckIsAmendingTransactionWhenAmendCreditNoteManually()
		{
			var transactions = new InvoicingBase[] { Factory.NewWithValidTestData<ARCreditNote>(), Factory.NewWithValidTestData<APCreditNote>() };
			foreach (var creditNote in transactions)
			{
				creditNote.AH_OriginalTransactionNum = "123456";
				AssertEquals(string.Format("Transaction {0} should be AmendingTransaction", creditNote.HumanReadableName), true, ((IAmending)creditNote).CheckIsAmendingTransaction(true));

				creditNote.AH_OriginalTransactionNum = ZString.Empty;
				creditNote.AH_OriginalInvoiceDate = new ZDate(2020, 08, 12);
				AssertEquals(string.Format("Transaction {0} should be AmendingTransaction", creditNote.HumanReadableName), true, ((IAmending)creditNote).CheckIsAmendingTransaction(true));

				creditNote.AH_OriginalInvoiceDate = ZDate.Empty;
				AssertEquals(string.Format("Transaction {0} should NOT be AmendingTransaction", creditNote.HumanReadableName), false, ((IAmending)creditNote).CheckIsAmendingTransaction(true));
			}
		}

		public void TestCheckInvoicingLineIsAmendingOriginalViaStrongReference()
		{
			var originalTransaction = Factory.NewWithValidTestData<ARInvoice>();
			var transactions = new InvoicingBase[] { Factory.NewWithValidTestData<ARCreditNote>(), Factory.NewWithValidTestData<APCreditNote>() };
			foreach (var creditNote in transactions)
			{
				Assert("Precondition", creditNote.AH_TransactionBelongsToGroup.IsEmpty);
				Assert("Precondition", creditNote.AH_OriginalTransactionNum.IsEmpty);
				Assert("Precondition", creditNote.AH_OriginalInvoiceDate.IsEmpty);

				var line = (InvoicingLineBase)creditNote.Lines.AddNew();

				creditNote.AH_OriginalTransactionNum = "123456";
				Assert("set AH_OriginalTransactionNum does not set strong reference", !line.IsAmendingOriginalViaStrongReference);

				creditNote.AH_OriginalInvoiceDate = new ZDate(2020, 08, 12);
				Assert("set AH_OriginalInvoiceDate does not set strong reference", !line.IsAmendingOriginalViaStrongReference);

				creditNote.AH_TransactionBelongsToGroup = originalTransaction.PK;
				Assert("set AH_TransactionBelongsToGroup does set strong reference", line.IsAmendingOriginalViaStrongReference);
			}
		}

		public void TestGetOriginalTransactionJobPKs()
		{
			foreach (InvoicingBase original in originalTransactions)
			{
				InvoicingLineBase line1 = (InvoicingLineBase)original.Lines.AddNew();
				JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				line1.AL_JH = job1.PK;

				InvoicingLineBase line2 = (InvoicingLineBase)original.Lines.AddNew();
				JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				line2.AL_JH = job2.PK;

				// Have to add the third line to cause recalculating Jobs
				InvoicingLineBase line3 = (InvoicingLineBase)original.Lines.AddNew();

				AssertEquals(string.Format("Original {0} should have two Jobs", original.HumanReadableName), 2, original.InvoiceDependentJobs.Count);

				foreach (string amendingType in amendingTransactionTypes)
				{
					IAmending amending = ((IAmending)original).GenerateAmendingTransaction(amendingType);
					AssertNotNull(string.Format("Should generate Amending transaction of {0} type for {1}", amendingType, original.HumanReadableName), amending);
					AssertEquals(string.Format("{0} should be referring {1} as OriginalTransaction", amending.HumanReadableName, original.HumanReadableName), original, amending.OriginalTransaction);
					AssertEquals(string.Format("OriginalTransactionJobPKs for {0} should contain two Job OKs from {1}", amending.HumanReadableName, original.HumanReadableName), 2, amending.GetOriginalTransactionJobPKs().Length);
					Assert(string.Format("Job1.PK from {0} should be in OriginalTransactionJobPKs for {1}", original.HumanReadableName, amending.HumanReadableName), amending.GetOriginalTransactionJobPKs().Contains(job1.PK));
					Assert(string.Format("Job2.PK from {0} should be in OriginalTransactionJobPKs for {1}", original.HumanReadableName, amending.HumanReadableName), amending.GetOriginalTransactionJobPKs().Contains(job2.PK));
				}
			}
		}

		public void TestGetOriginalTransactionAccountPK()
		{
			var originalAccountPKS = (from InvoicingBase original in originalTransactions select original.AH_OH).Distinct();
			Assert("The original transactions must have different AH_OHs", originalAccountPKS.Any());

			foreach (IAmending original in originalTransactions)
			{
				foreach (string amendingType in amendingTransactionTypes)
				{
					IAmending amending = original.GenerateAmendingTransaction(amendingType);
					AssertNotNull(string.Format("Should generate Amending transaction of {0} type for {1}", amendingType, original.HumanReadableName), amending);
					AssertEquals(string.Format("{0} should be referring {1} as OriginalTransaction", amending.HumanReadableName, original.HumanReadableName), original, amending.OriginalTransaction);
					AssertEquals(string.Format("{0} should have OriginalTransactionAccountPK from its original {1}", amending.HumanReadableName, original.HumanReadableName), ((InvoicingBase)original).AH_OH, amending.GetOriginalTransactionAccountPK());
				}
			}
		}

		public void TestCreditNotePopulateFromOriginalTransaction()
		{
			foreach (InvoicingBase original in originalTransactions)
			{
				var originalInvoice = original;
				originalInvoice.AH_RX_NKTransactionCurrency = "USD";
				originalInvoice.AH_ExchangeRate = 0.9m;
				AmendingTestHelper.PopulateOriginalTransaction(originalInvoice, creator, creator.GST1);
				original.Factory.Save();
				originalInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(originalInvoice, originalInvoice.InvoicingJob);

				var amending = (original as IAmending).GenerateAmendingTransaction<ARCreditNote>();
				var iAmending = amending as IAmending;

				AssertNotNull("IAmending", iAmending);
				iAmending.AmendingReasonCode = "TST";
				iAmending.AmendingReason = "Test";

				AssertNotNull(string.Format("{0} should be an Amending transaction for {0}", original.HumanReadableName), iAmending.IsAmendingTransaction);
				iAmending.PopulateFromOriginalTransaction();
				Factory.Save();

				AmendingTestHelper.AssertPopulatedTransaction(original, amending);
			}
		}

		public void TestInvoicePopulateFromOriginalTransactionWithItalianStampDuty()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.SetCountry(Core.Constants.CountryCodes.Italy);

			var taxRate = AccTaxRate.FindExistingTaxRate(Factory, "ART7", AccTaxRate.Types.Rated, Core.Constants.CountryCodes.Italy);

			var taxRateExempt = AccTaxRate.FindExistingTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, Core.Constants.CountryCodes.Italy);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxRate.PK.ToString());
			AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1.81m);
			AccountingConfigurationRegistry.Instance.StampDutyThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 77.47m);

			var stampDutyChargeCode = creator.CreateChargeCode("BOLLO", "Stamp Duty", "MRG", 1m, taxRateExempt, creator.WHTFREE1);
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, stampDutyChargeCode.PK.ToGuid());

			var italyOrg = creator.CreateOrgHeader("ITORG", false, true, "ITROM");
			var originalInvoice = Factory.NewWithValidTestData<ARInvoice>();
			originalInvoice.AH_OH = italyOrg.PK;
			AmendingTestHelper.PopulateOriginalTransaction(originalInvoice, creator, taxRate, stampDutyChargeCode, creator.USD); //adds two line to the original invoice
			AssertEquals("Before posting AR invoice no stamp duty charge is created. So number of lines in the original invoice should be 2", 2, originalInvoice.Lines.Count);

			Factory.Save();

			AssertEquals("Precondition: After posting AR invoice for Italy org in Country Italy stamp duty charge is created. So number of lines in the original invoice now should be 3", 3, originalInvoice.Lines.Count);

			var newFactory = new BusinessObjectFactory();
			var originalInvoiceInNewFactory = newFactory.Load<InvoicingBase>(originalInvoice.PK);

			IAmending original = originalInvoiceInNewFactory as IAmending;
			IAmending amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice);
			var amended = amending as InvoicingBase;
			AssertEquals("Stamp duty charge should not be copied from original invoice, hence number of lines in ameded invoice should be 2", 2, amended.Lines.Count);
			AssertNull("Amending invoice should not contain stamp duty charge code", amended.Lines.FindByPK(stampDutyChargeCode.PK));

			amended.Lines[0].AL_LocalExTaxAmount = 100M;
			amended.Lines[1].AL_LocalExTaxAmount = 10M;

			newFactory.Save();

			AssertEquals("After posting AR invoice for Italy org in Country Italy stamp duty charge is created should not be created once again as one stamp duty charge is already applied", 2, amended.Lines.Count);
		}

		public void TestInvoicePopulateFromOriginalTransactionWithRoundingLine()
		{
			var originalInvoice = Factory.NewWithValidTestData<ARInvoice>();

			AmendingTestHelper.PopulateOriginalTransaction(originalInvoice, creator);
			creator.CreateInvoiceLine(originalInvoice, creator.RevenueChargeCode.PK, 100M, creator.USD, 0.9M);

			Factory.Save();

			AssertEquals("Precondition: number of lines in the original invoice should be 3", 3, originalInvoice.Lines.Count);

			var original = originalInvoice as IAmending;
			var amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice) as InvoicingBase;
			AssertEquals("Before RoundingChargeCode is applied, number of amending lines should be 3", 3, amending.Lines.Count);

			AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.RevenueChargeCode.PK.ToGuid());
			amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice) as InvoicingBase;
			AssertEquals("After RoundingChargeCode is applied, number of amending lines should be 2", 2, amending.Lines.Count);

			AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.NonAccrualChargeCode.PK.ToGuid());
			amending = original.GenerateAmendingTransaction(TransactionTypes.Invoice) as InvoicingBase;
			AssertEquals("After RoundingChargeCode is applied with different charge code, number of amending lines should be 3", 3, amending.Lines.Count);
		}

		public void TestCreditNotePopulateFromOriginalTransaction_ConsolInvoice()
		{
			foreach (InvoicingBase original in originalTransactions)
			{
				ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();

				original.AH_RX_NKTransactionCurrency = "USD";
				original.AH_ExchangeRate = 0.9m;
				AmendingTestHelper.PopulateOriginalTransaction(original, creator);
				original.AH_JH = ZGuid.Empty;
				original.Factory.Save();
				original.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consol.JK_UniqueConsignRef, original.PK);

				var amending = (original as IAmending).GenerateAmendingTransaction<ARCreditNote>();
				IAmending iAmending = amending;
				AssertNotNull("IAmending", iAmending);
				iAmending.AmendingReasonCode = "TST";
				iAmending.AmendingReason = "Test";

				AssertNotNull(string.Format("{0} should be an Amending transaction for {0}", original.HumanReadableName), iAmending.IsAmendingTransaction);
				iAmending.PopulateFromOriginalTransaction();
				Factory.Save();

				AmendingTestHelper.AssertPopulatedTransaction(original, amending);
			}
		}

		public void TestInvoicePopulateFromOriginalTransaction()
		{
			foreach (InvoicingBase original in originalTransactions)
			{
				original.AH_RX_NKTransactionCurrency = "USD";
				original.AH_ExchangeRate = 0.9m;
				AmendingTestHelper.PopulateOriginalTransaction(original, creator, creator.GST1);
				original.UpdateAH_LocalExTaxAmount();
				original.Factory.Save();
				original.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(original, original.InvoicingJob);

				var amending = (original as IAmending).GenerateAmendingTransaction<ARInvoice>();

				var iAmending = amending as IAmending;
				AssertNotNull("IAmending", iAmending);
				AssertNotNull(string.Format("{0} should be an Amending transaction for {0}", original.HumanReadableName), iAmending.IsAmendingTransaction);

				Factory.Save();
				AmendingTestHelper.AssertPopulatedTransaction(original, amending);
			}
		}

		public void TestConsolidatedInvoiceRefGenerationWithConcurrency()
		{
			foreach (InvoicingBase original in originalTransactions)
			{
				original.AH_RX_NKTransactionCurrency = "USD";
				original.AH_ExchangeRate = 0.9m;
				AmendingTestHelper.PopulateOriginalTransaction(original, creator);
				original.Factory.Save();
				original.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(original, original.InvoicingJob);

				var amending = (original as IAmending).GenerateAmendingTransaction<ARInvoice>();

				var otherUserFactory = new BusinessObjectFactory();
				var amending2 = otherUserFactory.New<ARCreditNote>();
				amending2.AH_TransactionBelongsToGroup = original.PK;
				var iAmending2 = amending2 as IAmending;
				iAmending2.FlagAsCreatedAmending();
				iAmending2.PopulateFromOriginalTransaction();

				Factory.Save();
				otherUserFactory.Save();

				AssertNotEquals("Two distinct AH_ConsolidatedInvoiceRef values were created", amending.AH_ConsolidatedInvoiceRef, amending2.AH_ConsolidatedInvoiceRef);
				AmendingTestHelper.AssertPopulatedTransaction(original, amending);
			}
		}

		public void TestConsolidatedInvoiceRefGenerationWithConcurrency_ConsolLevelInvoice()
		{
			foreach (InvoicingBase original in originalTransactions)
			{
				ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();

				original.AH_RX_NKTransactionCurrency = "USD";
				original.AH_ExchangeRate = 0.9m;
				AmendingTestHelper.PopulateOriginalTransaction(original, creator);
				original.AH_JH = ZGuid.Empty;
				original.Factory.Save();
				original.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consol.JK_UniqueConsignRef, original.PK);

				var amending = (original as IAmending).GenerateAmendingTransaction<ARCreditNote>();

				var otherUserFactory = new BusinessObjectFactory();
				var amending2 = otherUserFactory.New<ARCreditNote>();
				amending2.AH_TransactionBelongsToGroup = original.PK;
				var iAmending2 = amending2 as IAmending;
				iAmending2.FlagAsCreatedAmending();
				iAmending2.PopulateFromOriginalTransaction();

				Factory.Save();
				otherUserFactory.Save();

				AssertNotEquals("Two distinct AH_ConsolidatedInvoiceRef values were created", amending.AH_ConsolidatedInvoiceRef, amending2.AH_ConsolidatedInvoiceRef);
				AmendingTestHelper.AssertPopulatedTransaction(original, amending);
			}
		}

		public void TestChargeWithCFXPopulateFromOriginalTransaction_AmendWithCreditNote()
		{
			AssertChargeWithCFXPopulateFromOriginalTransaction(typeof(ARCreditNote));
		}

		public void TestChargeWithCFXPopulateFromOriginalTransaction_AmendWithInvoice()
		{
			AssertChargeWithCFXPopulateFromOriginalTransaction(typeof(ARInvoice));
		}

		void AssertChargeWithCFXPopulateFromOriginalTransaction(Type amendingType)
		{
			foreach (IAmending original in originalTransactions)
			{
				var originalTransaction = original as InvoicingBase;
				AssertNotNull(originalTransaction);
				originalTransaction.AH_RX_NKTransactionCurrency = creator.USD.Code;
				originalTransaction.AH_ExchangeRate = 0.7125m;

				var shipment = creator.CreateShipment("S0001" + originalTransaction.AH_TransactionType);

				var job = creator.CreateJob(shipment, creator.ABIGAS, 0, creator.Agent, 0);
				originalTransaction.AH_OH = creator.ABIGAS.PK;
				originalTransaction.AH_JH = job.PK;
				originalTransaction.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(originalTransaction, originalTransaction.InvoicingJob);
				originalTransaction.AH_FullyPaidDate = ZDateTime.Empty;

				originalTransaction.Lines.RemoveAndDeleteAll();
				var line = creator.CreateInvoiceLine(TransactionLineTypes.Revenue, originalTransaction, job, creator.CC1, creator.USD, 0.7125m, "Line 1", 100m);
				var charge = creator.CreateJobCharge(line, job, creator.CC1, creator.USD);
				charge.JR_OH_SellAccount = creator.ABIGAS.PK;
				var exRateDEB = job.ExchangeRates[0];
				exRateDEB.JF_OH_Org = creator.ABIGAS.PK;
				exRateDEB.JF_CFXPercent = 5m;
				exRateDEB.JF_BaseRate = 0.75m;
				AssertEquals(0.7125m, exRateDEB.JF_SellRate);
				AssertEquals(0.7125m, charge.JR_OSSellExRate);

				original.Factory.Save();

				var amending = original.GenerateAmendingTransaction(amendingType);
				var iAmending = amending as IAmending;

				AssertNotNull("IAmending", iAmending);
				iAmending.AmendingReasonCode = "TST";
				iAmending.AmendingReason = "Test";

				AssertNotNull(string.Format("{0} should be an Amending transaction for {0}", original.HumanReadableName), iAmending.IsAmendingTransaction);
				iAmending.PopulateFromOriginalTransaction();

				if (original.GetType() == amendingType)
				{
					AssertEquals(1, amending.Lines.Count);
					amending.Lines[0].AL_OSExTaxAmount = 100m * amending.Lines[0].Multiplier_ForTestOnly; // To update the line's OSTotalSplitParts which is used by the IChargeCreator.CreateChargeFromJobRelatedRevenueLine
															   // method via AL_OSExTaxAmount_DBSigned
					amending.Lines[0].AL_LineAmount  = 140.35m;
				}
				
				Factory.Save();
				AssertEquals("Amending Transaction AH_ExchangeRate", 0.7125m, amending.AH_ExchangeRate);
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				AssertEquals(2, job.Charges.Count);
				var newCharge = job.Charges.Cast<Charge>().First(x => x.PK != charge.PK);
				AssertEquals(0.712504m, newCharge.JR_OSSellExRate);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);
		}

		TestObjectCreator creator;

		IEnumerable<IAmending> originalTransactions
		{
			get
			{
				yield return Factory.NewWithValidTestData<ARCreditNote>();
				yield return Factory.NewWithValidTestData<ARInvoice>();
			}
		}

		readonly string[] amendingTransactionTypes = new string[] { TransactionTypes.CreditNote, TransactionTypes.Invoice };
	}
}
