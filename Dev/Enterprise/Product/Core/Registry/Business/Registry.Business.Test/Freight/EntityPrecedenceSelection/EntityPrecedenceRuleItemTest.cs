using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EntityPrecedenceRuleItem))]
	sealed class EntityPrecedenceRuleItemTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestReadOnly()
		{
			var entityPrecedenceRuleItem = new EntityPrecedenceRuleItem();
			Assert(entityPrecedenceRuleItem.ReadOnly);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntityPrecedenceRuleItem();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (EntityPrecedenceRuleItem)GetNewBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
