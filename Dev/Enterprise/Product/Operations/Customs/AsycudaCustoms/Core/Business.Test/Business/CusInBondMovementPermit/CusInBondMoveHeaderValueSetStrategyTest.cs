using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	public class CusInBondMoveHeaderValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSetDefaultValues_WhenPermitNumberMatched()
		{
			var dec1 = Factory.New<JobDeclaration>();
			var moveHeader1 = dec1.CusEntryInstruction.CusInBondPermitsHeaders.AddNew();
			moveHeader1.BM_Calc_PermitNumber = "PN001";
			moveHeader1.BM_Calc_IssueDate = ZDateTime.Today.AddDays(1);
			moveHeader1.BM_ArrivalDate = ZDateTime.Today.AddDays(2);
			moveHeader1.BM_Calc_ValidityDate = ZDateTime.Today.AddDays(3);
			Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			var instruction2 = dec2.CustomsEntryInstructions.AddNew();
			var moveHeader2 = instruction2.CusInBondPermitsHeaders.AddNew();
			moveHeader2.BM_Calc_PermitNumber = "PN001";
			AssertEquals("match number from different job in db", ZDateTime.Today.AddDays(1), moveHeader2.BM_Calc_IssueDate);
			AssertEquals("match number from different job in db", ZDateTime.Today.AddDays(2), moveHeader2.BM_ArrivalDate);
			AssertEquals("match number from different job in db", ZDateTime.Today.AddDays(3), moveHeader2.BM_Calc_ValidityDate);

			var instruction3 = dec2.CustomsEntryInstructions.AddNew();
			var moveHeader3 = instruction3.CusInBondPermitsHeaders.AddNew();
			moveHeader3.BM_Calc_PermitNumber = "PN002";
			moveHeader3.BM_Calc_IssueDate = ZDateTime.Today.AddDays(4);
			moveHeader3.BM_ArrivalDate = ZDateTime.Today.AddDays(5);
			moveHeader3.BM_Calc_ValidityDate = ZDateTime.Today.AddDays(6);

			var instruction4 = dec2.CustomsEntryInstructions.AddNew();
			var moveHeader4 = instruction4.CusInBondPermitsHeaders.AddNew();
			moveHeader4.BM_Calc_PermitNumber = "PN002";
			AssertEquals("matched number in current job", ZDateTime.Today.AddDays(4), moveHeader4.BM_Calc_IssueDate);
			AssertEquals("matched number in current job", ZDateTime.Today.AddDays(5), moveHeader4.BM_ArrivalDate);
			AssertEquals("matched number in current job", ZDateTime.Today.AddDays(6), moveHeader4.BM_Calc_ValidityDate);
		}
	}
}
