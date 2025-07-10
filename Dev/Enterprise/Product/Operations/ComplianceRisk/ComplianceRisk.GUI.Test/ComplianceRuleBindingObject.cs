using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(ComplianceRuleBindingObject))]
	class ComplianceRuleBindingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComplianceRuleBindingObject(new ComplianceRuleCollection(Factory));
		}
	}
}
