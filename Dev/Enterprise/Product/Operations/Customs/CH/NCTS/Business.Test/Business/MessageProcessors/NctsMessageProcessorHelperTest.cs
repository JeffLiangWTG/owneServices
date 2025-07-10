using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsMessageProcessorHelperTest : MessageProcessorHelperTest
{
	protected override BusinessObject CreateJobCore(GlbCompany company)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		if (company != null)
		{
			nctsHeader.BH_GB = company.ActiveBranches.First().PK;
		}
		return nctsHeader;
	}
}
