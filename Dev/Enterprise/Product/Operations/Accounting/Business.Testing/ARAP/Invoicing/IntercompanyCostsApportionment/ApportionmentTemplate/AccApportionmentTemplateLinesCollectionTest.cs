using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(AccApportionmentTemplateLinesCollection))]
	public class AccApportionmentTemplateLinesCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccApportionmentTemplateLinesCollection(Factory.New<AccApportionmentTemplate>(), Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testTemplateLinesCollection = (AccApportionmentTemplateLinesCollection)GetCollectionToTest();
		}

		protected AccApportionmentTemplateLinesCollection testTemplateLinesCollection;

		#endregion

		#region Tests

		public void TestSetDefaultsForNewChild()
		{
			AccApportionmentTemplate template = testTemplateLinesCollection.Master;
			AccApportionmentTemplateLines templateLines = testTemplateLinesCollection.AddNew();
			AssertEquals("Y0_A0", template.PK, templateLines.Y0_A0);
			AssertEquals("Y0_Description", template.DefaultDescription, templateLines.Y0_Description);
		}

		#endregion

	}
}
