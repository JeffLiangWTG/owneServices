using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMTagDefinitionFilterBusinessObject))]
	class BMTagDefinitionFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestTagDefinitionCode()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var definition1 = Factory.NewWithValidTestData<TagDefinition>();
			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			var definition3 = Factory.NewWithValidTestData<TagDefinition>();

			var mag1 = BMSTestHelper.CreateTagMagnitude(definition1, "WOW", "Nein");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition3, "WOW", "Bonjoir");

			Factory.Save();

			var bizo = new BMTagDefinitionFilterBusinessObject();
			var releaseGroupFilterStrip = (ModuleTextFilter)bizo["Magnitude Code"];
			releaseGroupFilterStrip.IsActive = true;
			releaseGroupFilterStrip.Property = "WOW";

			var definitions = Factory.Load<TagDefinition>(bizo.Filter);
			AssertEquals(2, definitions.Length);
			AssertCollectionContains(definition1, definitions);
			AssertCollectionContains(definition3, definitions);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BMTagDefinitionFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
