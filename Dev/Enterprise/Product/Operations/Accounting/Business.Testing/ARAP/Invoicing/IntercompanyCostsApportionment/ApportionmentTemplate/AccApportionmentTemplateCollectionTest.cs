using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(AccApportionmentTemplateCollection))]
	public class AccApportionmentTemplateCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccApportionmentTemplateCollection(Factory);
		}
		protected override void SetUp()
		{
			base.SetUp();
			testTemplateCollection = (AccApportionmentTemplateCollection)GetCollectionToTest();
		}

		protected AccApportionmentTemplateCollection testTemplateCollection;

		#region Tests

		public void TestCreateAdditionalFilter()
		{
			AssertEquals("A0_GC", testTemplateCollection.CompleteFilter.LiteralTextSqlFormatted.Contains(GlbCompany.CurrentCompany.PK.ToString()), true);
		}

		#endregion
	}
}
