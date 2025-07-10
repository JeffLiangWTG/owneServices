using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(AccCashBasisVAT))]
	internal class AccCashBasisVATTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesAccAccCashBasisVAT()
		{
			var cashVATRecord = Factory.New<AccCashBasisVAT>();
			AssertNotNull("Company should not be null", cashVATRecord.Company);

			var localList = new List<string>
			{
				nameof(cashVATRecord.YC_TaxBaseAmount),
				nameof(cashVATRecord.YC_TaxAmount)
			};

			var tester = new DecimalPlacesAttributeTester(cashVATRecord, cashVATRecord.Company);
			tester.CheckLocalCurrency(localList, nameof(cashVATRecord.LocalDecimals));
		}

		public void TestSetDefaultValues()
		{
			var cashVATRecord = Factory.New<AccCashBasisVAT>();
			AssertEquals("YC_GC", GlbCompany.CurrentCompany.PK, cashVATRecord.YC_GC);
		}

		[SuspendCriticalValidation]
		public void TestDelete()
		{
			var cashVATRecord = Factory.New<AccCashBasisVAT>();
			cashVATRecord.Delete();
			Assert("Object which is not in db can be deleted.", cashVATRecord.IsDeleted);

			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			invoice.AH_FullyPaidDate = ZDateTime.Today;
			var line = invoice.Lines[0];
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			cashVATRecord = TestObjectCreator.CreateCashBasisVAT(invoice.Lines[0], -100, -10);
			Factory.Save();

			AssertExceptionThrown<NotSupportedException>("Object in db can't be deketed.", () => cashVATRecord.Delete());
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deletion is tested in TestDelete method", true);
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		protected override void SetUp()
		{
			TestObjectCreator.SetupCashBasisVAT();
		}

		protected override void TearDown()
		{
			base.TearDown();
			var currentCompany = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_IsGSTCashBasis = false;
			currentCompany.Factory.Save();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
