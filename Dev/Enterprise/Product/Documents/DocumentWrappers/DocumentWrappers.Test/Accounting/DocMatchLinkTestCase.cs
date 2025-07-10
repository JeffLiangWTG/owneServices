using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocMatchLink))]
	sealed class DocMatchLinkTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocMatchLink.New(MatchLink, Factory)
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			BusinessObject wrappedObject = Factory.New(typeof(TransactionMatchLink));
			return DocMatchLink.New((AccTransactionMatchLink)wrappedObject, Factory);
		}

		public void TestToString()
		{
			MatchLink.AP_MatchGroupNum = "M00001999";
			AssertEquals("M00001999", MatchLinkWrapper.ToString());
		}

		public void TestARInvoice()
		{
			AssertNull(MatchLinkWrapper.Invoice);

			APInvoice invoice = CreateTransactionHeader(ZArchitecture.Core.TransactionTypes.Payment);
			MatchLink.AP_AH = invoice.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(MatchLink);
			Factory.Save();
			AssertEquals("0001", MatchLinkWrapper.Invoice.TransactionNumber);
		}

		public void TestAmount()
		{
			AssertEquals(0M, MatchLinkWrapper.Amount);
			MatchLink.AP_Amount = -100M;
			AssertEquals(-100M, MatchLinkWrapper.Amount);
		}

		public void TestGSTRealised()
		{
			AssertEquals(0M, MatchLinkWrapper.GSTRealised);
			MatchLink.AP_GSTRealised = 0.10M;
			AssertEquals(0.10M, MatchLinkWrapper.GSTRealised);
		}

		public void TestMatchAmount()
		{
			AssertEquals(0M, MatchLinkWrapper.MatchAmount);
			MatchLink.AP_Amount = 1000M;
			AssertEquals(1000M, MatchLinkWrapper.MatchAmount);
			MatchLink.AP_Amount = -1000M;
			AssertEquals(-1000M, MatchLinkWrapper.MatchAmount);
		}

		public void TestMatchDate()
		{
			AssertEquals(ZDateTime.Empty, MatchLinkWrapper.MatchDate);
			MatchLink.AP_MatchDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, MatchLinkWrapper.MatchDate);
		}

		public void TestMatchGroupNum()
		{
			AssertEquals("", MatchLinkWrapper.MatchGroupNum);
			MatchLink.AP_MatchGroupNum = "Y09999991";
			AssertEquals("Y09999991", MatchLinkWrapper.MatchGroupNum);
		}

		public void TestMatchPeriod()
		{
			AssertEquals(0, MatchLinkWrapper.MatchPeriod);
			MatchLink.AP_MatchPeriod = 1;
			AssertEquals(1, MatchLinkWrapper.MatchPeriod);
		}

		public void TestReason()
		{
			AssertEquals("", MatchLinkWrapper.Reason.Trim());
			MatchLink.AP_Reason = "TST";
			AssertEquals("TST", MatchLinkWrapper.Reason);
		}

		public void TestCreatorName()
		{
			AssertEquals("Creator name", GlbStaff.CurrentUser.GS_FullName, MatchLinkWrapper.CreatorName);
		}

		public void TestOSMatchAmount()
		{
			AssertEquals(0M, MatchLinkWrapper.OSMatchAmount);

			ARInvoice invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", TestObjectCreator.USD, 0.7m, 700m, 0m, 1000m, 0m);
			MatchLink.AP_Amount = 1000M;
			MatchLink.AP_AH = invoice.PK;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = -1000M;

			AccTransactionMatchLink link = MatchGroup.AddNew();
			link.AP_AH = header.PK;
			link.AP_Amount = -1000M;

			invoice.AH_OutstandingAmount = 0M;
			TestObjectCreator.SetupMatchLinkMatchDate(MatchGroup);

			Factory.Save();
			AssertEquals(700M, MatchLinkWrapper.OSMatchAmount);
		}

		public void TestInvertedOSAmount()
		{
			AssertEquals(0M, MatchLinkWrapper.InvertedOSAmount);

			APInvoice invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "0001", TestObjectCreator.USD, 0.7m, 700m, 0m, 1000m, 0m);
			MatchLink.AP_Amount = -1000M;
			MatchLink.AP_AH = invoice.PK;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = 1000M;

			AccTransactionMatchLink link = MatchGroup.AddNew();
			link.AP_AH = header.PK;
			link.AP_Amount = 1000M;

			invoice.AH_OutstandingAmount = 0M;
			TestObjectCreator.SetupMatchLinkMatchDate(MatchGroup);

			Factory.Save();
			AssertEquals(700M, MatchLinkWrapper.InvertedOSAmount);
		}

		public void TestOSMatchAmountWithNewOSFeature()
		{
			AssertOSMatchAmount(true, 100M);
			AssertOSMatchAmount(false, 200M);

			void AssertOSMatchAmount(bool registryEnabled, ZDecimal expectValue)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEnabled);

				ARInvoice invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", TestObjectCreator.USD, 0.2m, 200m, 0m, 1000m, 0m);
				MatchLink.AP_Amount = 1000M;
				MatchLink.AP_AH = invoice.PK;
				MatchLink.AP_OSAmount = 100M;

				TransactionHeaderOSOutstandingAmountProvider.ForceToSetOutstandingAmounts(invoice, 200m, 100m, false);

				AssertEquals(expectValue, MatchLinkWrapper.OSMatchAmount);
			}
		}

		public void TestInvertedOSAmountWithNewOSFeature()
		{
			AssertWithInvertAmount(1);
			AssertWithInvertAmount(-1);

			void AssertWithInvertAmount(ZInt invertAmount)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				APInvoice invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "0001", TestObjectCreator.USD, 0.1m, 100m, 0m, invertAmount * 1000m, 0m);
				MatchLink.AP_Amount = -1000M;
				MatchLink.AP_AH = invoice.PK;
				MatchLink.AP_OSAmount = invertAmount * -100M;

				TransactionHeaderOSOutstandingAmountProvider.ForceToSetOutstandingAmounts(invoice, 200m, 100m, false);

				AssertEquals(100M, MatchLinkWrapper.InvertedOSAmount);
			}
		}

		public void TestInvertedOSAmountInvoice()
		{
			APInvoice invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "0001", TestObjectCreator.USD, 0.7m, 700m, 0m, 1000m, 0m);
			MatchLink.AP_Amount = -1000M;
			MatchLink.AP_AH = invoice.PK;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = 1000M;

			AccTransactionMatchLink link = MatchGroup.AddNew();
			link.AP_AH = header.PK;
			link.AP_Amount = 1000M;
			TestObjectCreator.SetupMatchLinkMatchDate(MatchGroup);

			invoice.AH_OutstandingAmount = 0M;

			Factory.Save();
			AssertEquals(700M, MatchLinkWrapper.InvertedOSAmount);
		}

		public void TestAmountReturnsNegativeValueForAPPayment()
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "0001", TestObjectCreator.USD, 2m, 400m, 0m, 200m, 0m);
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = ZDateTime.Today;
			var payment = TestObjectCreator.CreateAPPayment(1m, 0m, ZDateTime.Today, ZDateTime.Today, TestObjectCreator.TestOrganisation.PK, TestObjectCreator.AUDBankAccount.PK);
			payment.AH_InvoiceAmount = 200m;
			payment.AH_OSTotal = 200m;
			payment.AH_FullyPaidDate = ZDateTime.Today;
			MatchLink.AP_Amount = 200m;
			MatchLink.AP_AH = payment.PK;
			MatchLink.AP_MatchDate = ZDateTime.Today;

			var matchlink = MatchGroup.AddNew();
			matchlink.AP_AH = invoice.PK;
			matchlink.AP_Amount = -200m;
			matchlink.AP_MatchDate = ZDateTime.Today;

			Factory.Save();
			AssertEquals(-200m, MatchLinkWrapper.Amount);
		}

		protected override void SetUp()
		{
			MatchGroup = new TransactionMatchLinkGroup(Factory);
			MatchLink = MatchGroup.AddNew();
			MatchLinkWrapper = DocMatchLink.New(MatchLink, Factory);
			base.SetUp();
		}

		DocMatchLink MatchLinkWrapper;
		AccTransactionMatchLink MatchLink;
		TransactionMatchLinkGroup MatchGroup;
		APInvoice CreateTransactionHeader(ZString type)
		{
			var invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionType = type;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_TransactionNum = "0001";
			return invoice;
		}
	}
}
