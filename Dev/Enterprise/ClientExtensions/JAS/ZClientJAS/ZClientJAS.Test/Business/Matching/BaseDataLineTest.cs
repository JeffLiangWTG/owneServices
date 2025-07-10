using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Matching.Testing
{
	public class BaseDataLineTest : TestCaseWithFactory
	{
		protected void SetNettingCode(OrgHeader org, ZString nettingCode)
		{
			TestUNCAndUOCSetter.SetNettingCode(Factory, org, nettingCode);
			AssertEquals(nettingCode, org.CustomsCodes.GetUNC());
		}
	}
}
