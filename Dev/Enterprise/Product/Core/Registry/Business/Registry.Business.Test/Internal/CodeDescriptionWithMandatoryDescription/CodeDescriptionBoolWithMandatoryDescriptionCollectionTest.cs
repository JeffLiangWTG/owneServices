using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithMandatoryDescriptionCollection))]
	sealed class CodeDescriptionBoolWithMandatoryDescriptionCollectionTest : CodeDescriptionBoolCollectionAbstractTest<CodeDescriptionBoolWithMandatoryDescriptionCollection>
	{
		#region Implementation

		public override void TestDefaultBoolForNewChild()
		{
			AssertEquals(true, new CodeDescriptionBoolWithMandatoryDescriptionCollection().AddNew().Bool);
		}

		protected override CodeDescriptionBoolWithMandatoryDescriptionCollection GetCollectionToTest()
		{
			return new CodeDescriptionBoolWithMandatoryDescriptionCollection();
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionBoolWithMandatoryDescription();
		}

		#endregion
	}
}
