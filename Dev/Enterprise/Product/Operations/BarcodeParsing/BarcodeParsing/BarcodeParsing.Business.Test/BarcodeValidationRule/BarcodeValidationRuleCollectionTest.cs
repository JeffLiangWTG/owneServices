using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeValidationRuleCollection))]
	class BarcodeValidationRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<BarcodeValidationRuleCollection>
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			var ruleSet = Factory.New<BarcodeRuleSet>();
			var collection = new BarcodeValidationRuleCollection(ruleSet);
			AssertEquals(true, ((IBindingList)collection).AllowNew);

			ruleSet.BRS_IsSystem = true;
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		#endregion

		#region Implementation

		protected override BarcodeValidationRuleCollection GetCollectionToTest()
		{
			return new BarcodeValidationRuleCollection(Helper.CreateRuleSet());
		}

		BarcodeParsingTestHelper Helper
		{
			get { return helper ?? (helper = new BarcodeParsingTestHelper(Factory)); }
		}

		BarcodeParsingTestHelper helper;

		#endregion
	}
}
