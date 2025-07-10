using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.MFI.Test
{
	abstract class MFIStatementBaseTest : StatementBaseTest
	{
		public void TestMFIStatementIsClientSpecific()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			AssertEquals("IsClientSpecific should be false", false, ((MFIStatement)Statement).StatementIsClientSpecificProperty);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			GlbCompany.CurrentCompany.GC_Code = "AKL";
			AssertEquals("IsClientSpecific should be true", true, ((MFIStatement)Statement).StatementIsClientSpecificProperty);
		}

		protected override BusinessObject GetNewBusinessObject() => new MFIStatement(GlbBranch.CurrentBranch);

		protected override ZBool ExpectedIsClientSpecific => MFIConstants.NZ.ClientSpecificCondition;
	}
}
