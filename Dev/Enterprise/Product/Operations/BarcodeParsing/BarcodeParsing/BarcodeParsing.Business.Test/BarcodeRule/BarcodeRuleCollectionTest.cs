using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeRuleCollection))]
	class BarcodeRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<BarcodeRuleCollection>
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			var ruleSet = Factory.New<BarcodeRuleSet>();
			var collection = new BarcodeRuleCollection(ruleSet);
			AssertEquals(true, ((IBindingList)collection).AllowNew);

			ruleSet.BRS_IsSystem = true;
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		#endregion

		#region TestSetDefaultsForNewElement

		public void TestSetDefaultsForNewElement()
		{
			var collection = GetCollectionToTest();
			AssertEquals(0, collection.Count);

			var rule1 = collection.AddNew();
			AssertEquals(new ZShort(1), rule1.BRU_RuleNumber);

			var rule2 = collection.AddNew();
			AssertEquals(new ZShort(2), rule2.BRU_RuleNumber);
		}

		#endregion
	}
}
