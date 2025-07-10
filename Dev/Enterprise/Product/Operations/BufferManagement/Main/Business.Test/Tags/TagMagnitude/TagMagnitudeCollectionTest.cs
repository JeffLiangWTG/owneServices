using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagMagnitudeCollection))]
	class TagMagnitudeCollectionTest : ActiveBusinessObjectCollectionTestCase<TagMagnitudeCollection>
	{
		public void Test_ExclusiveDefinition_IncrementRunSequence()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BIR", "It's my birthday.", isExclusive: true);

			var magnitudeCollection = new TagMagnitudeCollection(definition);

			AssertEquals(0, magnitudeCollection.AddNew().TGM_RuleRunSequence);
			AssertEquals(1, magnitudeCollection.AddNew().TGM_RuleRunSequence);

			magnitudeCollection.AddNew().TGM_RuleRunSequence = 26;

			AssertEquals(27, magnitudeCollection.AddNew().TGM_RuleRunSequence);
		}

		public void Test_NonExclusiveDefinition_DontIncrementRunSequence()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "AED", "Alexanger Eadles");

			var magnitudeCollection = new TagMagnitudeCollection(definition);

			AssertEquals(0, magnitudeCollection.AddNew().TGM_RuleRunSequence);
			AssertEquals(0, magnitudeCollection.AddNew().TGM_RuleRunSequence);
			AssertEquals(0, magnitudeCollection.AddNew().TGM_RuleRunSequence);
		}

		public void TestAllowAddForSystemTags()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "NIN", "The ninja way");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "FIR", "Throw fireballs at them");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "BLA", "Blast them with an energy beam");

			var magnitudeCollection = (IBindingList)new TagMagnitudeCollection(definition);

			AssertEquals(true, magnitudeCollection.AllowNew);

			definition.TGD_IsSystem = true;

			AssertEquals(false, magnitudeCollection.AllowNew);
		}

		#region Implementation

		protected override TagMagnitudeCollection GetCollectionToTest()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			return new TagMagnitudeCollection(definition);
		}

		#endregion
	}
}
