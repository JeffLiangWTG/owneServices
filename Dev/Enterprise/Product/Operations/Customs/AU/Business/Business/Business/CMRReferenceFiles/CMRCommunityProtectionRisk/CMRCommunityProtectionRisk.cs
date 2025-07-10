
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCommunityProtectionRisk : AutoCMRCommunityProtectionRisk
	{
		public CMRCommunityProtectionRisk(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRCommunityProtectionRisk New(BusinessObjectFactory factory)
		{
			return factory.New<CMRCommunityProtectionRisk>();
		}

		public static CMRCommunityProtectionRisk[] Load(ICPQALineAttachee lineAttachee)
		{
			CMRCommunityProtectionRiskCollection result = new CMRCommunityProtectionRiskCollection(lineAttachee.Factory);

			if (lineAttachee.SelectionDate.IsValid)
			{
				CMRCommunityProtectionProfile[] profiles = GetMatchingProfiles(lineAttachee);
				if (profiles.Length > 0)
				{
					ArrayList identifierResults = new ArrayList();
					foreach (CMRCommunityProtectionProfile profile in profiles)
					{
						ZQuery filter = new ZQuery(CMRCommunityProtectionRiskSchema.CK_Identifier, profile.CP_CommunityProtectionRiskIdentifier);
						identifierResults.AddRange(lineAttachee.Factory.Load(typeof(CMRCommunityProtectionRisk), filter));
					}

					CMRCommunityProtectionRiskCollection identifierMatches = new CMRCommunityProtectionRiskCollection(lineAttachee.Factory);
					identifierMatches.AddRange((BusinessObject[])identifierResults.ToArray(typeof(BusinessObject)));

					ZQuery dateFilter = new ZQuery(CMRCommunityProtectionRiskSchema.CK_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, lineAttachee.SelectionDate);
					ZQuery endDateFilter = new ZQuery(CMRCommunityProtectionRiskSchema.CK_EndDate, SQLComparisonOperator.Equal, null);
					endDateFilter.AddToFilter(JoinCondition.Or, CMRCommunityProtectionRiskSchema.CK_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, lineAttachee.SelectionDate);
					dateFilter.AddToFilter(endDateFilter, JoinCondition.And);

					return (CMRCommunityProtectionRisk[])identifierMatches.Find(dateFilter);
				}
			}
			return (CMRCommunityProtectionRisk[])result.ToArray(typeof(CMRCommunityProtectionRisk));
		}

		public static CMRCommunityProtectionRisk[] Load(CMRLodgementQuestion lodgementQuestion)
		{
			ZQuery filter = new ZQuery(CMRCommunityProtectionRiskSchema.CK_LodgementQuestionIdentifier, SQLComparisonOperator.Equal, lodgementQuestion.CQ_LodgementQuestionIdentifier);
			return (CMRCommunityProtectionRisk[])lodgementQuestion.Factory.Load(typeof(CMRCommunityProtectionRisk), filter);
		}

		#region Implementation

		internal static CMRCommunityProtectionProfile[] GetMatchingProfiles(ICPQALineAttachee lineAttachee)
		{
			CPQuestionKeys cPQuestionKey = lineAttachee.CPQuestionKey;

			ZString filterString = cPQuestionKey.TariffNumber.Replace(".", "").Replace(" ", "");

			const int minimumLength = 2;
			int maximumLength = Math.Min(filterString.Length, 8);

			List<CMRCommunityProtectionProfile> result = new List<CMRCommunityProtectionProfile>();

			if (maximumLength >= minimumLength)
			{
				string[] tariffBits = new string[maximumLength - minimumLength + 1];
				for (int length = minimumLength; length <= maximumLength; length++)
				{
					tariffBits[length - minimumLength] = filterString.Left(length);
				}

				ZQuery filter = new ZQuery(CMRCommunityProtectionProfileSchema.CP_TariffClassificationNumberfield, tariffBits);
				BusinessObject[] candidates = lineAttachee.Factory.Load(typeof(CMRCommunityProtectionProfile), filter);

				foreach (CMRCommunityProtectionProfile profile in candidates)
				{
					if (profile.IsMatchingOtherThanTariff(cPQuestionKey))
					{
						CMRCommunityProtectionProfileCollection profilesMatchingNotInTariffAndStat = new CMRCommunityProtectionProfileCollection(lineAttachee.Factory);
						profilesMatchingNotInTariffAndStat.Load(GetCPProfileTariffWithNotFilter(cPQuestionKey.TariffNumber, cPQuestionKey.StatCode));

						if (!profilesMatchingNotInTariffAndStat.Contains(profile.CP_CommunityProtectionRiskIdentifier))
						{
							result.Add(profile);
						}
					}
				}
			}

			return result.ToArray();
		}

		#region GetCPProfileTariffWithNotFilter

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public static ZQuery GetCPProfileTariffWithNotFilter(ZString tariffNumber, ZString statCode)
		{
			ZQuery result = new ZQuery();
			ZString filterString = tariffNumber.Replace(".", "");

			if (filterString == "")
			{
				result.IsNoResultQuery = true;
			}
			else
			{
				while (filterString.Length > 0) // Complex criteria cannot be written as an IN statement
				{
					ZQuery statFilter = new ZQuery(CMRCommunityProtectionProfileSchema.CP_StatisticalClassificationCodefield, SQLComparisonOperator.Equal, CMRCommunityProtectionProfile.NotConstants + statCode);
					statFilter.AddToFilter(JoinCondition.Or, CMRCommunityProtectionProfileSchema.CP_StatisticalClassificationCodefield, SQLComparisonOperator.Equal, CMRCommunityProtectionProfile.NotConstants);
					statFilter.AddToFilter(JoinCondition.Or, CMRCommunityProtectionProfileSchema.CP_StatisticalClassificationCodefield, SQLComparisonOperator.Equal, ZString.Empty);

					ZQuery tariffStatFilter = new ZQuery(CMRCommunityProtectionProfileSchema.CP_TariffClassificationNumberfield, SQLComparisonOperator.Equal, CMRCommunityProtectionProfile.NotConstants + filterString);
					tariffStatFilter.AddToFilter(statFilter, JoinCondition.And);

					result.AddToFilter(tariffStatFilter, JoinCondition.Or);
					filterString = filterString.Substring(0, filterString.Length - 1);
				}
			}
			return result;
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CMRCommunityProtectionRiskFetchStrategy(this);
		}

		class CMRCommunityProtectionRiskFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public CMRCommunityProtectionRiskFetchStrategy(CMRCommunityProtectionRisk risk)
				: base(risk)
			{
			}

			CMRCommunityProtectionRisk risk
			{
				get { return BusinessObject as CMRCommunityProtectionRisk; }
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(CMRLodgementQuestionSchema.CQ_LodgementQuestionIdentifier, risk.CK_LodgementQuestionIdentifier);
			}
		}

		#endregion
	}
}
