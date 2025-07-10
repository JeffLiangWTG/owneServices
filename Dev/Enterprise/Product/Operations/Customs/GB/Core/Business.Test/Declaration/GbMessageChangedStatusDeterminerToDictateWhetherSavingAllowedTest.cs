using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	public class GbMessageChangedStatusDeterminerToDictateWhetherSavingAllowedTest : TestCaseWithFactory
	{
		public void TestMakeMessagesOnThisBizoForComparisonReturnsEdifactString()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();

			var changedStatusDeterminer = new GbMessageChangedStatusDeterminerToDictateWhetherSavingAllowed(declaration);
			var result = changedStatusDeterminer.MakeMessagesOnThisBizoForComparison(declaration);
			AssertStartsWith("This is not an edifact message", "UNH+", result[0].EM_MessageText);
		}
	}
}


