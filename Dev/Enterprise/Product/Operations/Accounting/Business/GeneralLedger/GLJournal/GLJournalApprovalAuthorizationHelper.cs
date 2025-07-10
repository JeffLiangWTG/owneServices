using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public static class GLJournalApprovalAuthorizationHelper
	{
		public static bool CheckLevelSecurityRights(GLJournal journal)
		{
			if (journal.IsNoteJournal)
			{
				return true;
			}

			var requiredCheckPoint = RequiredSecurityCheckPoint(journal);
			var securityProvider = SecurityOverrideProviderSource.Get(journal).Provider;
			var shouldCreateApprovalWithoutUsersConfirm = ShouldCreateApprovalWithoutUsersConfirm(journal) && securityProvider is ISecurityOverrideProviderWithApprovalRequest;
			if (IsUserHasRightButNotAllowedToApproveOwnJournal(requiredCheckPoint) && securityProvider is Enterprise.Integration.Security.IInteractiveSecurityOverrideProvider)
			{
				if (shouldCreateApprovalWithoutUsersConfirm)
				{
					return false;
				}

				var securitySertificate = securityProvider.PromptForTemporaryAccess(requiredCheckPoint);
				return securitySertificate != null && securitySertificate.IsAllowed;
			}
			else if (IsUserHasRightButNotAllowedToApproveOwnJournal(requiredCheckPoint) && securityProvider is Enterprise.Integration.Security.INonInteractiveSecurityOverrideProvider && securityProvider is ISecurityOverrideProviderWithApprovalRequest securityOverrideProviderWithApprovalRequest && securityOverrideProviderWithApprovalRequest.ShouldApprovalRequestBeCreated)
			{
				return false;
			}
			else
			{
				if (shouldCreateApprovalWithoutUsersConfirm)
				{
					return requiredCheckPoint.IsAllowed;
				}

				return securityProvider.SecurityCertificates[requiredCheckPoint].IsAllowed;
			}
		}

		public static bool ShouldCreateApprovalWithoutUsersConfirm(GLJournal journal)
		{
			return !journal.IsNoteJournal &&
				IsGLJournalThresholdSetToAny(journal) && !AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.Value;
		}

		public static bool IsUserHasRightButNotAllowedToApproveOwnJournal(SecurityCheckpoint requiredCheckPoint)
		{
			return requiredCheckPoint != Env.Security.None && requiredCheckPoint.IsAllowed && !AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.Value;
		}

		public static SecurityCheckpoint RequiredSecurityCheckPoint(GLJournal journal)
		{
			object cachedData = null;

			return RequiredSecurityCheckPointByCachedData(journal, ref cachedData);
		}

		public static SecurityCheckpoint RequiredSecurityCheckPointByCachedData(GLJournalLine line, object cachedData)
		{
			if (line.IsNoteJournal)
			{
				return Env.Security.None;
			}

			var authorisationRequirementSettings = new List<PaymentThreeLevelAuthorisationSettings>();
			if (line.AL_AG.IsValid && line.AL_GB.IsValid)
			{
				var glHeaderPK = line.AL_AG;
				var cachedDataCasted = CastCacheData(cachedData);
				if (cachedDataCasted != null)
				{
					if (cachedDataCasted.Item1)
					{
						return cachedDataCasted.Item4;
					}

					var cachedAuthorisationRequirementSettingForGroppedLineList = cachedDataCasted.Item3;
					foreach (var item in cachedAuthorisationRequirementSettingForGroppedLineList)
					{
						var cachedGroupedLine = item.Item1;
						var cachedAuthorisationRequirementSetting = item.Item2;
						if (cachedGroupedLine.glHeaderPK == glHeaderPK)
						{
							authorisationRequirementSettings.Add(cachedAuthorisationRequirementSetting);
						}
					}
				}
			}

			return FindMaximumSecurityCheckpoint(authorisationRequirementSettings.ToArray());
		}

		public static SecurityCheckpoint RequiredSecurityCheckPointByCachedData(GLJournal journal, ref object cachedData)
		{
			if (journal.IsNoteJournal)
			{
				return Env.Security.None;
			}

			var cachedDataCasted = CastCacheData(cachedData);
			var isExitingJournal = journal.IsInDatabase;
			if (cachedDataCasted == null
				|| cachedDataCasted.Item1 != IsGLJournalThresholdSetToAny(journal)
				|| cachedDataCasted.Item2 != isExitingJournal)
			{
				var anySecurityCheckPoint = RequiredSecurityCheckPointWhenAny(journal);
				if (anySecurityCheckPoint == null)
				{
					var filter = new ZQuery(AccGLHeaderSchema.PK, journal.GLJournalLines.Select(x => ((GLJournalLine)x).AL_AG).Distinct());
					var glHeaders = journal.Factory.Load<AccGLHeader>(filter);

					var linesGroupedByAccountOnly = from GLJournalLine line in journal.GLJournalLines
													where line.AL_AG.IsValid && line.AL_GB.IsValid
													group line by new
													{
														glHeaderPK = line.AL_AG,
													} into g
													let groupGLHeader = glHeaders.FirstOrDefault(s => (s.PK == g.Key.glHeaderPK))
													select new GroupedLine
													{
														glHeaderPK = g.Key.glHeaderPK,
														reportSection = groupGLHeader != null ? groupGLHeader.AG_Column : ZString.Empty,
														totalLineAmount = Math.Abs(g.Sum(x => x.AL_LineAmount))
													};

					var authorisationRequirementSettingForGroppedLineList = linesGroupedByAccountOnly.Select(groupedLine => Tuple.Create(groupedLine, FindRequiredAuthorisationRequirementSetting(groupedLine, journal))).ToArray();

					cachedData = Tuple.Create(false, isExitingJournal, authorisationRequirementSettingForGroppedLineList, (SecurityCheckpoint)null);
				}
				else
				{
					cachedData = Tuple.Create(true, isExitingJournal, (Tuple<GroupedLine, PaymentThreeLevelAuthorisationSettings>[])null, anySecurityCheckPoint);
					return anySecurityCheckPoint;
				}
				cachedDataCasted = CastCacheData(cachedData);
			}

			return cachedDataCasted.Item1
				? cachedDataCasted.Item4
				: FindMaximumSecurityCheckpoint(cachedDataCasted.Item3.Select(x => x.Item2).ToArray());
		}

		static bool IsGLJournalThresholdSetToAny(GLJournal journal)
		{
			var thresholdSetup = GetJournalThresholdSetup(journal);

			return thresholdSetup != null
				&& thresholdSetup.Count > 0
				&& thresholdSetup[0].Type == Enterprise.Accounting.Registry.Business.GLJournalApprovalThreshold.TypeCodes.AnyChanges;
		}

		static GLJournalApprovalThresholdCollection GetJournalThresholdSetup(GLJournal journal)
		{
			return journal.IsInDatabase
				? AccountingConfigurationRegistry.Instance.ExistingGLJournalApprovalThresholdSetup.Value
				: AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.Value;
		}

		static SecurityCheckpoint RequiredSecurityCheckPointWhenAny(GLJournal journal)
		{
			return IsGLJournalThresholdSetToAny(journal)
				? Env.Security.GeneralLedgerJournal_FirstApproval
				: null;
		}

		static Tuple<bool, bool, Tuple<GroupedLine, PaymentThreeLevelAuthorisationSettings>[], SecurityCheckpoint> CastCacheData(object cachedData)
		{
			var result = cachedData as Tuple<bool, bool, Tuple<GroupedLine, PaymentThreeLevelAuthorisationSettings>[], SecurityCheckpoint>;
			if (cachedData != null)
			{
				Argument.NotNull(result, "cachedData", "Type of cache should not be exposed outside this helper as this is internal data. However correct type should be passed if it's not null. It have to be value returned from RequiredSecurityCheckPointByCachedData method for journal.");
			}
			return result;
		}

		static PaymentThreeLevelAuthorisationSettings FindRequiredAuthorisationRequirementSetting(GroupedLine groupedLine, GLJournal journal)
		{
			PaymentThreeLevelAuthorisationSettings result = null;

			var registryValue = GetJournalThresholdSetup(journal);

			if (registryValue.Count > 0)
			{
				var thresholdCollectionByCompanyAndBranch = registryValue.Cast<GLJournalApprovalThreshold>();
				var matchedThresholdRow = thresholdCollectionByCompanyAndBranch.FirstOrDefault(x => x.GLAccount == groupedLine.glHeaderPK) // find by GL Account first
										?? thresholdCollectionByCompanyAndBranch.FirstOrDefault(x => x.ReportSection == groupedLine.reportSection) // fall back to report section
										?? thresholdCollectionByCompanyAndBranch.FirstOrDefault(x => x.Type == GLJournalApprovalThreshold.TypeCodes.All); // fall back to type "ALL"

				if (matchedThresholdRow != null)
				{
					result = matchedThresholdRow.AuthorisationSettings.Cast<PaymentThreeLevelAuthorisationSettings>().GetAuthorisationRequired(groupedLine.totalLineAmount);
				}
			}

			return result;
		}

		static SecurityCheckpoint FindMaximumSecurityCheckpoint(PaymentThreeLevelAuthorisationSettings[] authorisationRequirementSettings)
		{
			PaymentThreeLevelAuthorisationSettings maxAuthorisationRequirementSetting = null;
			foreach (var authorisationRequirementSetting in authorisationRequirementSettings)
			{
				if (authorisationRequirementSetting != null)
				{
					var maxWeight = maxAuthorisationRequirementSetting == null ? -1 : maxAuthorisationRequirementSetting.GetAuthorisationRequirementWeight(maxAuthorisationRequirementSetting.AuthorisationRequirement);
					var currentWeight = authorisationRequirementSetting.GetAuthorisationRequirementWeight(authorisationRequirementSetting.AuthorisationRequirement);
					if (currentWeight > maxWeight)
					{
						maxAuthorisationRequirementSetting = authorisationRequirementSetting;
					}
					if (maxAuthorisationRequirementSetting.AuthorisationRequirement == PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly)
					{
						break;
					}
				}
			}

			var checkpoint = Env.Security.None;
			if (maxAuthorisationRequirementSetting != null)
			{
				switch (maxAuthorisationRequirementSetting.AuthorisationRequirement)
				{
					case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly:
						checkpoint = Env.Security.GeneralLedgerJournal_FirstApproval;
						break;
					case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly:
						checkpoint = Env.Security.GeneralLedgerJournal_SecondApproval;
						break;
					case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly:
						checkpoint = Env.Security.GeneralLedgerJournal_ThirdApproval;
						break;
				}
			}

			return checkpoint;
		}

		struct GroupedLine
		{
			public ZGuid glHeaderPK;
			public ZString reportSection;
			public ZDecimal totalLineAmount;
		}
	}
}
