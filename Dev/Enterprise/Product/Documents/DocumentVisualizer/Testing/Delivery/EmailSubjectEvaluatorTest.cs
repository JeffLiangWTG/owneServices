using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class EmailSubjectEvaluatorTest : TestCaseWithFactory
	{
		public void TestTryCreateEmailSubject()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ZZZ";
			company.GC_Name = "ZZZ Comp";
			company.GC_RN_NKCountryCode = "US";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_BranchName = "Los Branchos";
			branch.GB_Code = "AAA";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var bizObj = Factory.New<DummyBusinessObject>();
				bizObj.Z0_Code = "AAA";

				const string emailSubjectMacro = "Code: <Z0_Code>, Company: <@env.Company.Name>";

				var evaluator = new EmailSubjectEvaluator();
				var hasEmailSubject = evaluator.TryCreateEmailSubject(bizObj, emailSubjectMacro, out var emailSubject);

				Assert("email subject has been created", hasEmailSubject);
				AssertEquals("matching filter", "Code: AAA, Company: ZZZ Comp", emailSubject);
			}
		}
	}
}
