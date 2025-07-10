using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowTempOrgRequiredFieldCollection))]
	sealed class GlowTempOrgRequiredFieldCollectionTest : CodeDescriptionBoolCollectionAbstractTest<GlowTempOrgRequiredFieldCollection>
	{
		public void TestIsMandatoryIsSet()
		{
			var glowTempOrgRequiredFieldCollection = new GlowTempOrgRequiredFieldCollection();
			var glowTempOrgRequiredField1 = glowTempOrgRequiredFieldCollection.Add("AAA", (NoResString)"AAA Description", enabled: true, isMandatory: true);
			var glowTempOrgRequiredField2 = glowTempOrgRequiredFieldCollection.Add("BBB", (NoResString)"BBB Description", enabled: true, isMandatory: false);
			var glowTempOrgRequiredField3 = glowTempOrgRequiredFieldCollection.AddSystemDefined("CCC", (NoResString)"CCC Description", enabled: true, isMandatory: true);
			var glowTempOrgRequiredField4 = glowTempOrgRequiredFieldCollection.AddSystemDefined("DDD", (NoResString)"DDD Description", enabled: false, isMandatory: false);

			AssertEquals("Field 1", expected: true, glowTempOrgRequiredField1.IsMandatory);
			AssertEquals("Field 2", expected: false, glowTempOrgRequiredField2.IsMandatory);
			AssertEquals("Field 3", expected: true, glowTempOrgRequiredField3.IsMandatory);
			AssertEquals("Field 4", expected: false, glowTempOrgRequiredField4.IsMandatory);
		}

		public override void TestDefaultBoolForNewChild()
		{
			AssertEquals(false, new GlowTempOrgRequiredFieldCollection().AddNew().Bool);
		}

		#region Implementation

		protected override GlowTempOrgRequiredFieldCollection GetCollectionToTest()
		{
			return new GlowTempOrgRequiredFieldCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GlowTempOrgRequiredField();
		}

		#endregion
	}
}
