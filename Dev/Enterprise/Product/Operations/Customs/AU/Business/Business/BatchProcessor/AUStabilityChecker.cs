using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.StabilityChecker;

[assembly: StabilityChecker("AU Declaration Business BatchProcessor Stability Checker", "AUC", typeof(Enterprise.Customs.AU.Declaration.Business.AUStabilityChecker))]
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUStabilityChecker : IStabilityChecker
	{
		#region IStabilityChecker Members

		public StabilityResult[] Check()
		{
			var finalResult = new List<StabilityResult>();
			var factory = new BusinessObjectFactory();
			var certificateChecker = new CertificateChecker(factory);
			StabilityResultLevel? customsKeyResult = null;

			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Australia))
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var message = string.Empty;
					var companyKeyValid = certificateChecker.CheckCompanyKey(out message, true);
					if (companyKeyValid != StabilityResultLevel.Healthy)
					{
						finalResult.Add(new StabilityResult(companyKeyValid, string.Format("{0} - {1}", GlbCompany.CurrentCompany.GC_Name, message), isUserRelatedNotification: true));
					}

					if (!customsKeyResult.HasValue)
					{
						customsKeyResult = certificateChecker.CheckCustomsKey(out message, true);
						if (customsKeyResult != StabilityResultLevel.Healthy)
						{
							finalResult.Add(new StabilityResult(customsKeyResult.Value, string.Format("{0} - {1}", GlbCompany.CurrentCompany.GC_Name, message), isUserRelatedNotification: true));
						}
					}
				}
			}

			return finalResult.ToArray();
		}

		#endregion
	}
}
