using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AlternateGLAccountWithAttributeSetCollection))]
	public class AlternateGLAccountWithAttributeSetCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AlternateGLAccountWithAttributeSetCollection>
	{
		public void TestAllowRemoveCore()
		{
			var testCollection = GetCollectionToTest();
			Assert("User should not be able to remove elements from this collection.", !testCollection.AllowRemove);
		}

		#region Implementation

		protected override AlternateGLAccountWithAttributeSetCollection GetCollectionToTest()
		{
			var alternateGLAccountWithAttributeSetDetails = new AlternateGLAccountWithAttributeSetDetails(GLHeader.PK, Chart.PK, 0, "BSH", "CSH", "KG");
			return new AlternateGLAccountWithAttributeSetCollection(alternateGLAccountWithAttributeSetDetails, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var alternateGLAccountWithAttributeSetDetails = new AlternateGLAccountWithAttributeSetDetails(GLHeader.PK, Chart.PK, 0, "BSH", "CSH", "KG");
			return new AlternateGLAccountWithAttributeSet(alternateGLAccountWithAttributeSetDetails, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var creator = new TestObjectCreator(Factory);
			Chart = creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			creator.CreateAccAlternateChartFormat(Chart, 1, "X", "2", ".");

			GLHeader = creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			Factory.Save();
		}

		AccAlternateChart Chart;
		AccGLHeader GLHeader;

		#endregion
	}
}
