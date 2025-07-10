using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using Enterprise.Build.Database.Script.Public.Accounting.Balances;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class AccOrgBalancesClaimTotalTest : AccOrgBalancesTestCase
	{
		public void TestClaimTotal()
		{
			var claimStatusList = ObjectFactory.Get<IAccounting>().ClaimStatusCodeDescriptionPairList as CodeDescriptionPairList;

			var k = 0;
			var openStatusList = new List<string>() { "OPN", "WRK", "REJ", "CCR", "CCA" };
			foreach (CodeDescriptionPair claimStatus in claimStatusList)
			{
				if (openStatusList.Contains(claimStatus.Code))
				{
					TestClaimTotalWithOpenClaim("AR", claimStatus.Code, k.ToString(CultureInfo.InvariantCulture), k.ToString(CultureInfo.InvariantCulture), "AUD", 1M);
					k++;
					TestClaimTotalWithOpenClaim("AP", claimStatus.Code, k.ToString(CultureInfo.InvariantCulture), k.ToString(CultureInfo.InvariantCulture), "AUD", 1M);
					k++;
					TestClaimTotalWithOpenClaim("AR", claimStatus.Code, k.ToString(CultureInfo.InvariantCulture), k.ToString(CultureInfo.InvariantCulture), "CNY", 2M);
					k++;
					TestClaimTotalWithOpenClaim("AP", claimStatus.Code, k.ToString(CultureInfo.InvariantCulture), k.ToString(CultureInfo.InvariantCulture), "CNY", 2M);
					k++;
				}
				else
				{
					TestClaimTotalWithNonOpenClaim("AR", claimStatus.Code, k.ToString(CultureInfo.InvariantCulture), k.ToString(CultureInfo.InvariantCulture));
					k++;
					TestClaimTotalWithNonOpenClaim("AP", claimStatus.Code, k.ToString(CultureInfo.InvariantCulture), k.ToString(CultureInfo.InvariantCulture));
					k++;
				}
			}
		}
	}
}
