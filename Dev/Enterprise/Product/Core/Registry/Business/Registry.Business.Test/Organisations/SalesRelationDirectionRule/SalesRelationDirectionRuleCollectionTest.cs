using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SalesRelationDirectionRuleCollection))]
	sealed class SalesRelationDirectionRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SalesRelationDirectionRuleCollection>
	{
		public void TestAffectedActivityTypes()
		{
			AssertEquals(6, SalesRelationDirectionRuleCollection.AffectedActivityTypes.Count());
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

		protected override SalesRelationDirectionRuleCollection GetCollectionToTest()
		{
			return new SalesRelationDirectionRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SalesRelationDirectionRule();
		}

		#endregion
	}
}
