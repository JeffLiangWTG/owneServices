using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(StaticControlCustomisationCollection))]
	class StaticControlCustomisationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StaticControlCustomisationCollection>
	{
		public void TestAddingLines_ShouldSetTopToMakeThemStackNicely()
		{
			var customisation = Factory.New<BMControlCustomisation>();

			var line1 = customisation.CustomisedControls.AddNew();
			line1.Top = 20;
			line1.Height = 20;

			var line2 = customisation.CustomisedControls.AddNew();
			AssertEquals(40, line2.Top);
			line2.Height = 40;

			var line3 = customisation.CustomisedControls.AddNew();
			AssertEquals(80, line3.Top);
		}

		protected override StaticControlCustomisationCollection GetCollectionToTest()
		{
			return Factory.New<BMControlCustomisation>().CustomisedControls;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StaticControlCustomisation(Factory.New<BMControlCustomisation>());
		}
	}
}
