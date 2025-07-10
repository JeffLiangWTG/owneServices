using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	public abstract class JobComInvApportionedChargeCollectionTest<TCollection, TBizObj> : SubsetBusinessObjectCollectionTestCase<JobComInvApportionedChargeCollection<TBizObj>, TBizObj>
		where TCollection : IJobComInvApportionedChargeCollection<TBizObj>
		where TBizObj : JobComInvCharge
	{
		[ExpectNoExceptions]
		public void TestAddNewAMMVCharge()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<TestDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				NUnit.Framework.Assert.That(invoiceLine.Charges.AddNew().IsAMMV(), Is.EqualTo(false));
				NUnit.Framework.Assert.That(invoiceLine.ApportionedCharges.AddNew().IsAMMV(), Is.EqualTo(false));
				NUnit.Framework.Assert.That(invoiceLine.ApportionedCharges.AddNewAMMVCharge().IsAMMV(), Is.EqualTo(true));
				NUnit.Framework.Assert.That(invoiceLine.Charges.AddNewAMMVCharge().IsAMMV(), Is.EqualTo(true));
			});
		}

		[ExpectNoExceptions]
		public void TestClearApportionedCharges()
		{
			var invoice = Factory.New<TestInvoice>();
			var collection = invoice.ApportionedCharges;

			JobComInvCharge apportionedOFT = collection.AddNew();
			apportionedOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			apportionedOFT.J7_Amount = 100m;
			apportionedOFT.J7_RX_NKCurrency = "AUD";
			apportionedOFT.J7_DistributeBy = ChargeDistributeByList.Codes.Volume;
			apportionedOFT.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;

			JobComInvCharge apportionedCOM = collection.AddNew();
			apportionedCOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			apportionedCOM.J7_Percentage = 10m;
			apportionedCOM.J7_DistributeBy = ChargeDistributeByList.Codes.Volume;
			apportionedCOM.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;

			collection.ClearApportionedCharges();

			NUnit.Framework.Assert.That(apportionedOFT.J7_ChargeType, Is.EqualTo(CustomsChargeTypeList.Codes.OverseasFreight).Using(CustomComparers.TypeComparison), "Charge type should stay");
			NUnit.Framework.Assert.That(apportionedOFT.J7_Amount, Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "Amount should be cleared");
			NUnit.Framework.Assert.That(apportionedOFT.J7_RX_NKCurrency, Is.EqualTo("").Using(CustomComparers.TypeComparison), "Currency should be cleared");
			NUnit.Framework.Assert.That(apportionedOFT.J7_DistributeBy, Is.EqualTo(ChargeDistributeByList.Codes.Volume).Using(CustomComparers.TypeComparison), "Distribute by should stay");
			NUnit.Framework.Assert.That(apportionedOFT.J7_FullOrPartialApportionment, Is.EqualTo(ApportionmentTypeList.Codes.FullApportionment).Using(CustomComparers.TypeComparison), "Apportionment type should stay");

			NUnit.Framework.Assert.That(apportionedCOM.J7_ChargeType, Is.EqualTo(CustomsChargeTypeList.Codes.Commission).Using(CustomComparers.TypeComparison), "Charge type should stay");
			NUnit.Framework.Assert.That(apportionedCOM.J7_Percentage, Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "Percentage should be cleared");
			NUnit.Framework.Assert.That(apportionedCOM.J7_DistributeBy, Is.EqualTo(ChargeDistributeByList.Codes.Volume).Using(CustomComparers.TypeComparison), "Distribute by should stay");
			NUnit.Framework.Assert.That(apportionedCOM.J7_FullOrPartialApportionment, Is.EqualTo(ApportionmentTypeList.Codes.FullApportionment).Using(CustomComparers.TypeComparison), "Apportionment type should stay");
		}

		[ExpectNoExceptions]
		public void TestDeleteEmptyApportionedCharges()
		{
			var invoice = Factory.New<TestInvoice>();
			var collection = invoice.ApportionedCharges;

			JobComInvCharge chargeWithCurrency = collection.AddNew();
			chargeWithCurrency.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			chargeWithCurrency.J7_Amount = 0m;
			chargeWithCurrency.J7_RX_NKCurrency = "AUD";

			JobComInvCharge chargeWithPercentage = collection.AddNew();
			chargeWithPercentage.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			chargeWithPercentage.J7_Percentage = 10m;

			JobComInvCharge emptyONS = collection.AddNew();
			emptyONS.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;

			NUnit.Framework.Assert.That(chargeWithCurrency.IsEmpty, Is.EqualTo(false), "IsEmpty for ChargeWithCurrency");
			NUnit.Framework.Assert.That(chargeWithPercentage.IsEmpty, Is.EqualTo(false), "IsEmpty for ChargeWithPercentage");
			NUnit.Framework.Assert.That(emptyONS.IsEmpty, Is.EqualTo(true), "EmptyONS is empty");

			collection.DeleteEmptyApportionedCharges();

			NUnit.Framework.Assert.That(chargeWithCurrency.IsDeleted, Is.EqualTo(false), "ChargeWithCurrency should not be deleted");
			NUnit.Framework.Assert.That(collection.Contains(chargeWithCurrency), Is.EqualTo(true), "ChargeWithCurrency should be in the collection");
			NUnit.Framework.Assert.That(chargeWithPercentage.IsDeleted, Is.EqualTo(false), "ChargeWithPercentage should not be deleted");
			NUnit.Framework.Assert.That(collection.Contains(chargeWithPercentage), Is.EqualTo(true), "ChargeWithPercentage should be in the collection");
			NUnit.Framework.Assert.That(emptyONS.IsDeleted, Is.EqualTo(true), "EmptyONS should be deleted");
			NUnit.Framework.Assert.That(collection.Contains(emptyONS), Is.EqualTo(false), "Collection should not have empty ONS");
		}

		[ExpectNoExceptions]
		public void TestUpdatePrepaidCollect()
		{
			var invoice = Factory.New<TestInvoice>();
			var collection = invoice.ApportionedCharges;

			JobComInvCharge charge1 = collection.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge1.J7_PrepaidCollect = "";

			JobComInvCharge charge2 = collection.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge2.J7_PrepaidCollect = "";

			collection.UpdatePrepaidCollect(charge2.ChargeKey, "PPD");
			NUnit.Framework.Assert.That(charge1.J7_PrepaidCollect, Is.EqualTo("").Using(CustomComparers.TypeComparison), "Charge1' prepaid collect stays the same");
			NUnit.Framework.Assert.That(charge2.J7_PrepaidCollect, Is.EqualTo("PPD").Using(CustomComparers.TypeComparison), "Charge2 is set");
		}

		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			var invoice = Factory.New<TestInvoice>();
			var collection = invoice.ApportionedCharges;

			NUnit.Framework.Assert.That(collection.AllowNew, Is.EqualTo(false), "Allow new");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValuesForNewChild()
		{
			var invoice = Factory.New<TestInvoice>();
			var collection = invoice.ApportionedCharges;

			JobComInvCharge apportionedOFT = collection.AddNew();
			NUnit.Framework.Assert.That(apportionedOFT.J7_IsApportionedCharge, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsApportioned is set to true");
		}
	}

	[TestedType(typeof(JobComInvApportionedChargeCollection<JobComInvCharge>))]
	class JobComInvApportionedChargeCollectionTest : JobComInvApportionedChargeCollectionTest<JobComInvApportionedChargeCollection<JobComInvCharge>, JobComInvCharge>
	{
		protected override JobComInvApportionedChargeCollection<JobComInvCharge> GetCollectionToTest()
		{
			return new JobComInvApportionedChargeCollection<JobComInvCharge>(Factory.New<TestInvoice>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobComInvChargeForTest>();
		}

		class JobComInvChargeForTest : JobComInvCharge
		{
			public JobComInvChargeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}
}
