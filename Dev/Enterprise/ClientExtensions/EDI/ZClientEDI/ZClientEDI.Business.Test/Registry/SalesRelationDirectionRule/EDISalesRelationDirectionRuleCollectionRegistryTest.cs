using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(EDISalesRelationDirectionRuleCollection))]
	internal class EDISalesRelationDirectionRuleCollectionRegistryTest : RegistryBusinessObjectCollectionTemplateTestCase<EDISalesRelationDirectionRuleCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override EDISalesRelationDirectionRuleCollection GetCollectionToTest()
		{
			return new EDISalesRelationDirectionRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collection = new EDISalesRelationDirectionRuleCollection();
			var rule = collection.AddNew();

			return rule;
		}

		#endregion
	}
}
