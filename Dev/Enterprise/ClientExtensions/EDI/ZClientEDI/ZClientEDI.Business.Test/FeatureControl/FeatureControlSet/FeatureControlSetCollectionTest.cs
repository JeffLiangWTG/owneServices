using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(FeatureControlSetCollection))]
	public class FeatureControlSetCollectionTest : ActiveBusinessObjectCollectionTestCase<FeatureControlSetCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<FeatureControlSet>();
		}
	}

	[TestedType(typeof(FeatureControlRuleNonDependentCollection))]
	public class FeatureControlRuleNonDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new FeatureControlRuleNonDependentCollection(Factory, new ZQuery());
		}
	}
}
