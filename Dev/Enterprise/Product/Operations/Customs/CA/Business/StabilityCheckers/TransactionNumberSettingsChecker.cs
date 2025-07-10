using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.StabilityChecker;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

[assembly: StabilityChecker("CA Transaction Number Settings Stability Checker", "CAT", typeof(Enterprise.Customs.CA.Business.StabilityCheckers.TransactionNumberSettingsChecker))]

namespace Enterprise.Customs.CA.Business.StabilityCheckers
{
	public class TransactionNumberSettingsChecker : IStabilityChecker
	{
		public StabilityResult[] Check()
		{
			var checkResults = new List<StabilityResult>();

			var branch = GlbBranch.GetFirstActiveBranch(Core.Constants.CountryCodes.Canada);
			if (branch != null)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var factory = new BusinessObjectFactory();
					var settingsBO = new TransactionNumberSettingBO(factory);
					var settingCollection = settingsBO.ExistingTransactionNumberSettingCollection;

					foreach (var setting in settingCollection.Cast<TransactionNumberSetting>())
					{
						CheckTransactionNumberSetting(checkResults, setting);
					}
				}
			}

			return checkResults.ToArray();
		}

		void CheckTransactionNumberSetting(List<StabilityResult> checkResults, TransactionNumberSetting setting)
		{
			setting.RunPreSaveValidation();
			if (!setting.HasRowMessageErrors && !setting.HasRowWarnings)
			{
				return;
			}
			string description = Res.GetString(
				"3CB9D564-FFED-41E6-A47D-B2183EB9A0C9",
				"Canada Transaction Number Settings {0} - has problems:{1}",
				setting.RangeName,
				FormatErrors(setting)
			);
			checkResults.Add(new StabilityResult(StabilityResultLevel.Warning, description, isUserRelatedNotification: true));
		}

		string FormatErrors(TransactionNumberSetting setting)
		{
			const int maxNotifications = 5;

			StringBuilder sb = new StringBuilder();
			var allNotifications = setting.RowErrors.Union(setting.RowWarnings).ToList();
			foreach (var notification in allNotifications.Take(maxNotifications))
			{
				sb.Append("\r\n\t");
				sb.Append(notification.Message);
			}

			if (allNotifications.Count > maxNotifications)
			{
				sb.Append(Res.GetString("E450A07E-35C6-4883-BD52-643A82AD9E76", "\r\n\t...\r\n\t({0} more problems)", allNotifications.Count - maxNotifications));
			}

			return sb.ToString();
		}
	}
}
