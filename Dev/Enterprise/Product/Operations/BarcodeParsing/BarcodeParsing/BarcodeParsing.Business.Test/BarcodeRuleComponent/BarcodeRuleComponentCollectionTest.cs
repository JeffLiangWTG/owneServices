using System;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeRuleComponentCollection))]
	class BarcodeRuleComponentCollectionTest : ActiveBusinessObjectCollectionTestCase<BarcodeRuleComponentCollection>
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			var rule = Helper.CreateRule();
			var collection = new BarcodeRuleComponentCollection(rule);

			rule.IsPartialRule = false;
			AssertEquals("Full rules should always be able to add new Rule Components.", true, ((IBindingList)collection).AllowNew);

			rule.IsPartialRule = true;
			AssertEquals("Partial rules should be allowed to add one Rule Component.", true, ((IBindingList)collection).AllowNew);

			rule.IsPartialRule = false;
			Helper.CreateRuleComponent(rule);
			AssertEquals("Full rules should always be able to add new Rule Components.", true, ((IBindingList)collection).AllowNew);

			rule.IsPartialRule = true;
			AssertEquals("Partial rules should not be allowed to add more than one Rule Component.", false, ((IBindingList)collection).AllowNew);
		}

		#endregion

		#region TestSetDefaultsForNewElement

		public void TestSetDefaultsForNewElement()
		{
			var rule = Helper.CreateRule();
			var collection = new BarcodeRuleComponentCollection(rule);
			var component1 = collection.AddNew();
			AssertEquals(new ZShort(1), component1.BRC_Sequence);

			var component2 = collection.AddNew();
			AssertEquals(new ZShort(2), component2.BRC_Sequence);

			// clean-up
			component1.Delete();
			component2.Delete();

			rule.IsPartialRule = true;
			var component3 = collection.AddNew();
			AssertEquals(new ZShort(0), component3.BRC_Sequence);

			var component4 = collection.AddNew();
			AssertEquals(new ZShort(0), component4.BRC_Sequence);
		}

		#endregion

		#region TestSortingBySequence

		public void TestSortingBySequence()
		{
			var rule = Helper.CreateRule();
			var collection = new BarcodeRuleComponentCollection(rule);
			AssertEquals("Collection supports sorting.", true, ((IBindingList)collection).SupportsSorting);

			var component1 = collection.AddNew();
			var component2 = collection.AddNew();
			var component3 = collection.AddNew();
			AssertEquals((ZShort)1, component1.BRC_Sequence);
			AssertEquals((ZShort)2, component2.BRC_Sequence);
			AssertEquals((ZShort)3, component3.BRC_Sequence);
			AssertEquals(component1, collection[0]);
			AssertEquals(component2, collection[1]);
			AssertEquals(component3, collection[2]);

			component2.BRC_Sequence = 5;
			AssertEquals((ZShort)1, component1.BRC_Sequence);
			AssertEquals((ZShort)5, component2.BRC_Sequence);
			AssertEquals((ZShort)3, component3.BRC_Sequence);
			AssertEquals(component1, collection[0]);
			AssertEquals(component3, collection[1]);
			AssertEquals(component2, collection[2]);

			var component4 = collection.AddNew();
			component4.BRC_Sequence = 2;
			AssertEquals(component1, collection[0]);
			AssertEquals(component4, collection[1]);
			AssertEquals(component3, collection[2]);
			AssertEquals(component2, collection[3]);
		}

		#endregion

		#region Implementation

		protected override BarcodeRuleComponentCollection GetCollectionToTest()
		{
			return new BarcodeRuleComponentCollection(Helper.CreateRule());
		}

		protected override void SetUp()
		{
			base.SetUp();
			dummyBarcodeEnableDisposable = BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			dummyBarcodeEnableDisposable?.Dispose();
		}

		IDisposable dummyBarcodeEnableDisposable;

		BarcodeParsingTestHelper Helper
		{
			get { return helper ?? (helper = new BarcodeParsingTestHelper(Factory)); }
		}

		BarcodeParsingTestHelper helper;

		#endregion
	}
}
