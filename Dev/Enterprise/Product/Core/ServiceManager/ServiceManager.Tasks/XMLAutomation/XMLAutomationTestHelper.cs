using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

#if DEBUG
namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	abstract class XMLAutomationTestHelper
	{
		public static void DeleteAllBranchesExceptCurrentBranch(GlbCompany company)
		{
			TestCaseHelper.ClearTable(TagRuleSchema.Constants.TableName);

			for (int i = company.Branches.Count - 1; i >= 0; i--)
			{
				if (company.Branches[i].PK != GlbBranch.CurrentBranch.PK)
				{
					company.Branches[i].Delete();
				}
			}
		}
	}
}
#endif
