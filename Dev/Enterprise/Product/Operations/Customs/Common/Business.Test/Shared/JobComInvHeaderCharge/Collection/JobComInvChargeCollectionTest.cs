using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	sealed class ChargeCollectionBaseOnlyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAddCharge()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();

			IJobComInvChargeCollection<JobComInvCharge> collection = declaration.Invoices.AddNew().Charges;

			ApportionChargeKey testKey = new ApportionChargeKey("ADD"
				, true
				, true
				, "PAA"
				, GroupIsIncludedInLinesOptionList.Codes.No
				, GroupIsIncludedInLinesOptionList.Codes.No
				, "VAL"
				, 10m
				, false
				, "Additional Charges"
				, false
				, false);

			collection.AddCharge(testKey, 100m, "USD");

			NUnit.Framework.Assert.That(collection.Count, Is.EqualTo(1), "one charge should have been added");
			NUnit.Framework.Assert.That(collection[0].J7_ChargeType, Is.EqualTo("ADD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_IsDutiable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_IsGSTApplicable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_FullOrPartialApportionment, Is.EqualTo("PAA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_IsNotIncludedInInvoice, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_DistributeBy, Is.EqualTo("VAL").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_Percentage, Is.EqualTo(10m).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(collection[0].J7_Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Amount");
			NUnit.Framework.Assert.That(collection[0].J7_RX_NKCurrency, Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "Currency");

			collection.AddCharge(testKey, 200m, "USD");
			NUnit.Framework.Assert.That(collection.Count, Is.EqualTo(1), "Still one charge");
			NUnit.Framework.Assert.That(collection[0].J7_ChargeType, Is.EqualTo("ADD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_IsDutiable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_IsGSTApplicable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_FullOrPartialApportionment, Is.EqualTo("PAA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_IsNotIncludedInInvoice, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_DistributeBy, Is.EqualTo("VAL").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection[0].J7_Percentage, Is.EqualTo(10m).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(collection[0].J7_Amount, Is.EqualTo(300m).Using(CustomComparers.TypeComparison), "Amount");
			NUnit.Framework.Assert.That(collection[0].J7_RX_NKCurrency, Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "Currency");

			testKey = new ApportionChargeKey("AAA"
				, true
				, true
				, "PAA"
				, GroupIsIncludedInLinesOptionList.Codes.No
				, GroupIsIncludedInLinesOptionList.Codes.Yes
				, "VAL"
				, 0m
				, false
				, "TEST"
				, false
				, false);

			collection.AddCharge(testKey, 100m, "USD");
			NUnit.Framework.Assert.That(collection[0].J7_Amount, Is.EqualTo(300m).Using(CustomComparers.TypeComparison), "Amount");
			NUnit.Framework.Assert.That(collection[0].J7_RX_NKCurrency, Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "Currency");

			NUnit.Framework.Assert.That(collection[1].J7_Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Amount");
			NUnit.Framework.Assert.That(collection[1].J7_RX_NKCurrency, Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "Currency");
		}
	}

	public abstract class ChargeCollectionTest<TCollection, TObject> : SubsetBusinessObjectCollectionTestCase<TCollection, TObject>
		where TCollection : JobComInvChargeCollection<TObject>
		where TObject : JobComInvCharge
	{
		[ExpectNoExceptions]
		public void TestRemovingValidChargeMarksApportionmentDirty()
		{
			var declaration = Factory.New<TestDeclaration>();
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is not dirty yet");

			var invoice = declaration.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();
			invoice.Charges.RemoveFromRelationship(charge);
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is not dirty yet");

			invoice.Charges.Add(charge);
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = "AUD";
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(true), "Apportionment is dirty now");

			charge.J7_ChargeType = "";
			charge.J7_Amount = 0m;
			charge.J7_RX_NKCurrency = "";
			declaration.ApportionmentDirty = false;
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is not dirty");
			invoice.Charges.RemoveFromRelationship(charge);
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is not dirty as the charge removed is not valid");
		}

		[ExpectNoExceptions]
		public void TestGetDistributeBy()
		{
			var declaration = Factory.New<TestDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var charge1 = invoice.Charges.AddNew();
			var charge2 = invoice.Charges.AddNew();

			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge1.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge2.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;

			var collection = (TestChargeCollection)invoice.Charges;

			NUnit.Framework.Assert.That(collection.GetDistributeBy(charge1.ChargeKey), Is.EqualTo(ChargeDistributeByList.Codes.Value), "Distribute-by for OFT");
			NUnit.Framework.Assert.That(collection.GetDistributeBy(charge2.ChargeKey), Is.EqualTo(ChargeDistributeByList.Codes.Weight), "Distribute-by for ONS");
			NUnit.Framework.Assert.That(collection.GetDistributeBy(new ChargeCodeChargeKey("XXX", false, true, false)), Is.EqualTo(""), "Distribute-by for Non-Existent");

			NUnit.Framework.Assert.That(collection.GetDistributeBy(charge1.ApportionChargeKey), Is.EqualTo(ChargeDistributeByList.Codes.Value), "Distribute-by for OFT");
			NUnit.Framework.Assert.That(collection.GetDistributeBy(charge2.ApportionChargeKey), Is.EqualTo(ChargeDistributeByList.Codes.Weight), "Distribute-by for ONS");
			NUnit.Framework.Assert.That(collection.GetDistributeBy(new ApportionChargeKey("XXX", false, true, ApportionmentTypeList.Codes.FullApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.Yes, ChargeDistributeByList.Codes.Value, 0m, false, "", false, false)), Is.EqualTo(""), "Distribute-by for Non-Existent");
		}

		[ExpectNoExceptions]
		public void TestHasChargesDistributedByOtherThanValue()
		{
			var declaration = Factory.New<TestDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var charge1 = invoice.Charges.AddNew();
			var charge2 = invoice.Charges.AddNew();

			var collection = (TestChargeCollection)invoice.Charges;

			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;

			NUnit.Framework.Assert.That(charge1.J7_DistributeBy, Is.EqualTo(ChargeDistributeByList.Codes.Value).Using(CustomComparers.TypeComparison), "By default it is value");
			NUnit.Framework.Assert.That(charge2.J7_DistributeBy, Is.EqualTo(ChargeDistributeByList.Codes.Value).Using(CustomComparers.TypeComparison), "By default it is value");

			NUnit.Framework.Assert.That(collection.HasChargesDistributedByOtherThanValue(), Is.EqualTo(false), "HasChargesDistributedByOtherThanValue");

			charge2.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			NUnit.Framework.Assert.That(collection.HasChargesDistributedByOtherThanValue(), Is.EqualTo(true), "HasChargesDistributedByOtherThanValue");
		}
	}

	[TestedType(typeof(JobComInvChargeCollection<TestCharge>))]
	public class ChargeCollectionTest : ChargeCollectionTest<JobComInvChargeCollection<TestCharge>, TestCharge>
	{
		protected override JobComInvChargeCollection<TestCharge> GetCollectionToTest()
		{
			return new TestChargeCollection(Factory.New<TestInvoice>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TestCharge>();
		}
	}
}

