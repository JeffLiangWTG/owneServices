using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningRuleCollection))]
	sealed class HVLVPreScreeningRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<HVLVPreScreeningRuleCollection>
	{
		public void TestSetDefaultValueWhenAddNewPreScreeningRule()
		{
			var ruleCollection = new HVLVPreScreeningRuleCollection();
			var rule = ruleCollection.AddNew();
			AssertEquals("SHP", rule.ModuleType);
			AssertEquals("NOE", rule.EmailNotificationType);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override HVLVPreScreeningRuleCollection GetCollectionToTest()
		{
			return new HVLVPreScreeningRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HVLVPreScreeningRule();
		}

		#endregion
	}
}
