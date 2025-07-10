using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[UseSnapshotProtection]
	public abstract class NonTransactionedTest : TestCase
	{
		protected abstract Type InvoiceType { get; }

		[SuspendCriticalValidation]
		public void TestSaveExceptionDoesNotRetainMatchGroupNumber()
		{
			var factory = new BusinessObjectFactory();

			var invoice = (Invoice)factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OH = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ACAINT").PK;
			invoice.AH_GC = ZGuid.NewZGuid();
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;
			var line = (InvoiceLine)invoice.Lines.AddNew();
			line.FillWithValidTestData();
			line.AL_OSExTaxAmount = 10M;

			try
			{
				factory.Save();
			}
			catch (ZSaveException)
			{
				// we expect an exception because branch is empty
			}

			AssertEquals("Should be 1000", 1000, Env.NumberFountains.MatchNo.GetTodaysPeriodFountain().PeekPreliminary(factory));

			invoice.AH_GC = GlbCompany.CurrentCompany.PK;

			factory.Save();

			TransactionMatchLinkCollection matchLinkCollection = new TransactionMatchLinkCollection(factory);
			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice.PK);
			filter.AddToFilter(JoinCondition.Or, AccTransactionMatchLinkSchema.AP_AH, invoice.ReceiptPayment.PK);
			matchLinkCollection.Load(filter);
			AssertEquals("matchLinkCollection.Count", 2, matchLinkCollection.Count);
			AssertEquals("MatchGroupNum should be 1000", "M00001000", matchLinkCollection[0].AP_MatchGroupNum);
			AssertEquals("MatchGroupNum should be 1000", "M00001000", matchLinkCollection[1].AP_MatchGroupNum);
			AssertEquals("Should be 1001", 1001, Env.NumberFountains.MatchNo.GetTodaysPeriodFountain().PeekPreliminary(factory));
		}
	}

	public class ARInvoiceNonTransactionedTest : NonTransactionedTest
	{
		protected override Type InvoiceType
		{
			get { return typeof(ARInvoice); }
		}
	}

	public class APInvoiceNonTransactionedTest : NonTransactionedTest
	{
		protected override Type InvoiceType
		{
			get { return typeof(ARInvoice); }
		}
	}
}
