using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection))]
	sealed class OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollectionTest : CodeDescriptionBoolCollectionAbstractTest<OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection>
	{
		#region Implementation

		public override void TestDefaultBoolForNewChild()
		{
			AssertEquals(true, new OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection().AddNew().Bool);
		}

		protected override OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection GetCollectionToTest()
		{
			return new OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OverrideImmuneCodeDescriptionBoolWithMandatoryDescription();
		}

		#endregion
	}
}
