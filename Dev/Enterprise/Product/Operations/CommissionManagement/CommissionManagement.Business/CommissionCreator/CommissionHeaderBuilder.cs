using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionHeaderBuilder
	{
		public CommissionHeaderBuilder(BusinessObjectFactory factory, CreateCommissionContext context, Func<ICommissionAgreementAndRates, bool> additionalPreCreateCheck = null)
		{
			this.Factory = factory;
			this.Context = context;
			this.AdditionalPreCreateCheck = additionalPreCreateCheck;
		}

		public readonly BusinessObjectFactory Factory;
		public readonly CreateCommissionContext Context;
		protected readonly Func<ICommissionAgreementAndRates, bool> AdditionalPreCreateCheck;

		public AccCommissionHeader[] Create(ICommissionHeaderBuildItem buildItem)
		{
			OnCreating(buildItem);
			return CreateCore(buildItem);
		}

		protected virtual AccCommissionHeader[] CreateCore(ICommissionHeaderBuildItem buildItem)
		{
			var result = new List<AccCommissionHeader>();
			var commissionTargetsToCreateFor = UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor(buildItem);
			foreach (var commissionTarget in commissionTargetsToCreateFor)
			{
				var commissionHeader = GetCommissionHeaderWithLineGroups(buildItem, commissionTarget);
				var commissionLinesHelper = new CommissionLinesHelper(regenerating: Context.RegeneratingCommissions);
				commissionLinesHelper.Add(commissionHeader, commissionTarget.Value);

				commissionHeader = Finalize(commissionHeader);
				if (commissionHeader != null)
				{
					result.Add(commissionHeader);
				}
			}

			return result.ToArray();
		}

		protected virtual void OnCreating(ICommissionHeaderBuildItem buildItem)
		{
		}

		#region Pre-Create Checks

		protected Dictionary<ZString, ICommissionAgreementAndRates> UpdateExistingCommissionsAndGetCommissionTargetsToCreateFor(ICommissionHeaderBuildItem buildItem)
		{
			var commissionTargetsToCreateFor = new Dictionary<ZString, ICommissionAgreementAndRates>(Context.AgreementAndRatesOverride ?? buildItem.TargetAgreementAndRates.ByStream);
			if (!Context.OverwriteOldValues && commissionTargetsToCreateFor.Count == 0)
			{
				return commissionTargetsToCreateFor;
			}

			if (!Context.RegeneratingCommissions)
			{
				var existingHeadersFilter = Context.GetCommissionHeaderStreamsFilter();
				var existingHeaders = buildItem.GetExistingCommissionHeaders(existingHeadersFilter);
				var existingCommissionHeadersByStream = existingHeaders.GroupBy(x => x.CH0_CommissionStream).ToDictionary(x => x.Key, y => y.ToArray());
				foreach (var keyValue in existingCommissionHeadersByStream)
				{
					var existingCommissionHeaderStream = keyValue.Key;
					var existingCommissionHeaders = keyValue.Value;

					ICommissionAgreementAndRates targetAgreementAndRatesForStream;
					if (!commissionTargetsToCreateFor.TryGetValue(existingCommissionHeaderStream, out targetAgreementAndRatesForStream))
					{
						targetAgreementAndRatesForStream = null;
					}

					if (targetAgreementAndRatesForStream == null)
					{
						// commission exists for this stream - but there shouldn't be any
						if (Context.OverwriteOldValues)
						{
							foreach (var commissionHeader in existingCommissionHeaders)
							{
								if (ShouldMarkOverriden(commissionHeader)) //unless the approved agreement isn't related to this header
								{
									commissionHeader.MarkAsOverriden();
								}
							}
						}
					}
					else if (Context.OverwriteOldValues)
					{
						if (!buildItem.Source.AH_IsCancelled && !MarkAllOldCommissionHeadersAsOverriden(existingCommissionHeaders, targetAgreementAndRatesForStream.CommissionAgreement, Context, buildItem))
						{
							// existing commission headers already exist for this commission stream, and at least one of them is already up to date. Therefore do not create any new commissions for this stream
							commissionTargetsToCreateFor.Remove(existingCommissionHeaderStream);
						}
					}
					else
					{
						// existing commission headers already exist for this commission stream, and overwrite old values not checked. Therefore do not create any new commissions for this stream
						commissionTargetsToCreateFor.Remove(existingCommissionHeaderStream);
					}
				}
			}

			foreach (var commissionTarget in commissionTargetsToCreateFor.ToArray())
			{
				var stream = commissionTarget.Key;
				var agreementAndRates = commissionTarget.Value;

				if (agreementAndRates == null
					|| (AdditionalPreCreateCheck != null && !AdditionalPreCreateCheck(agreementAndRates))
					|| (Context.OnlyCreateForAgreementsBeingApproved && !Context.PksOfAgreementsBeingApproved.Contains(agreementAndRates.CommissionAgreement.PK)))
				{
					commissionTargetsToCreateFor.Remove(stream);
				}
			}

			return commissionTargetsToCreateFor;
		}

		protected virtual bool ShouldMarkOverriden(AccCommissionHeader commissionHeader)
		{
			return Context.PksOfAgreementsBeingApproved == null || Context.PksOfAgreementsBeingApproved.Contains(commissionHeader.CH0_CA0);
		}

		bool MarkAllOldCommissionHeadersAsOverriden(AccCommissionHeader[] commissionHeaders, OrgCommissionAgreement newCommissionAgreement, CreateCommissionContext context, ICommissionHeaderBuildItem buildItem)
		{
			var allHeadersBelongToReversal = AllHeadersBelongToReversal(buildItem.Source.PK, buildItem.GroupingSource.PK, newCommissionAgreement.PK);

			foreach (var commissionHeader in commissionHeaders)
			{
				var commissionHeaderIsUpToDate =
					(newCommissionAgreement != null) &&
					(commissionHeader.CH0_CA0 == newCommissionAgreement.PK) &&
					(!commissionHeader.IsInDatabase || commissionHeader.CH0_SystemCreateTimeUtc >= newCommissionAgreement.CA0_LastApprovedDateUtc);
				if (commissionHeaderIsUpToDate && !allHeadersBelongToReversal)
				{
					return false;
				}

				if (context.OnlyCreateForAgreementsBeingApproved)
				{
					var coversDate = newCommissionAgreement == null
						|| newCommissionAgreement.EffectiveDate <= commissionHeader.CH0_CommissionDate
						|| newCommissionAgreement.EffectiveDate.IsEmpty;

					var oldOrNewAgreementBeingApproved =
						(context.PksOfAgreementsBeingApproved.Contains(commissionHeader.CH0_CA0)) ||
						(
							(
								newCommissionAgreement == null
								|| context.PksOfAgreementsBeingApproved.Contains(newCommissionAgreement.PK)
							)
							&& coversDate
						);

					if (!oldOrNewAgreementBeingApproved)
					{
						return false;
					}
				}
			}

			foreach (var commissionHeader in commissionHeaders)
			{
				commissionHeader.MarkAsOverriden(allHeadersBelongToReversal);
			}

			return true;
		}

		bool AllHeadersBelongToReversal(ZGuid transactionPK, ZGuid jobPK, ZGuid agreementPK)
		{
			var nonReversedHeadersQuerySql = FormattableString.Invariant($@"
CH0_PK IN
(
	SELECT CH0_PK
	FROM
		dbo.AccCommissionHeader
		JOIN dbo.AccCommissionLineGroup ON CLG_CH0 = CH0_PK
		JOIN dbo.AccCommissionLine AS Line ON CL0_ParentID = CLG_PK
	WHERE
		CL0_BelongsToGroup IS NULL
		AND
		CH0_GroupingSourceID = @JH_PK
		AND
		CH0_AH_Source = @AH_PK
		AND
		CH0_CA0 = @CA0_PK
		AND
		CL0_PK IN
		(
			SELECT COALESCE(CL0_BelongsToGroup, CL0_PK)
			FROM dbo.AccCommissionHeader
			INNER JOIN dbo.AccCommissionLineGroup ON CLG_CH0 = CH0_PK
			INNER JOIN dbo.AccCommissionLine ON CL0_ParentID = CLG_PK
			WHERE
				CH0_GroupingSourceID = @JH_PK
				AND
				CH0_AH_Source = @AH_PK
				AND
				CH0_CA0 = @CA0_PK
			GROUP BY COALESCE(CL0_BelongsToGroup, CL0_PK)
			HAVING COUNT(*) = 1
		)
	UNION
	SELECT CH0_PK
	FROM
		dbo.AccCommissionHeader
		JOIN dbo.AccCommissionLine AS Line ON CL0_ParentID = CH0_PK
	WHERE
		CL0_BelongsToGroup IS NULL
		AND
		CH0_GroupingSourceID = @JH_PK
		AND
		CH0_AH_Source = @AH_PK
		AND
		CH0_CA0 = @CA0_PK
		AND
		CL0_PK IN
		(
			SELECT COALESCE(CL0_BelongsToGroup, CL0_PK)
			FROM dbo.AccCommissionLine
			WHERE
				CH0_GroupingSourceID = @JH_PK
				AND
				CH0_AH_Source = @AH_PK
				AND
				CH0_CA0 = @CA0_PK	
			GROUP BY COALESCE(CL0_BelongsToGroup, CL0_PK)
			HAVING COUNT(*) = 1
		)
)");

			var paramList = new ZSqlParameterCollection();
			paramList.Add("@JH_PK", jobPK, JobHeaderSchema.PK);
			paramList.Add("@AH_PK", transactionPK, AccTransactionHeaderSchema.PK);
			paramList.Add("@CA0_PK", agreementPK, OrgCommissionAgreementSchema.PK);

			var nonReversedHeadersQuery = new ZDBOnlyQuery(typeof(AccCommissionHeader));
			nonReversedHeadersQuery.AddFilterAndZSQLParameterCollection(nonReversedHeadersQuerySql, paramList);

			var atleastOneReversedHeaderQuerySql = FormattableString.Invariant($@"
CL0_PK IN
(
	SELECT COALESCE(CL0_BelongsToGroup, CL0_PK)
	FROM dbo.AccCommissionHeader
	INNER JOIN dbo.AccCommissionLineGroup ON CLG_CH0 = CH0_PK
	INNER JOIN dbo.AccCommissionLine ON CL0_ParentID = CLG_PK
	WHERE
		CH0_GroupingSourceID = @JH_PK
		AND
		CH0_AH_Source = @AH_PK
		AND
		CH0_CA0 = @CA0_PK
	GROUP BY COALESCE(CL0_BelongsToGroup, CL0_PK)
	HAVING COUNT(*) = 2
	UNION
	SELECT COALESCE(CL0_BelongsToGroup, CL0_PK)
	FROM dbo.AccCommissionHeader
	INNER JOIN dbo.AccCommissionLine ON CL0_ParentID = CH0_PK
	WHERE
		CH0_GroupingSourceID = @JH_PK
		AND
		CH0_AH_Source = @AH_PK
		AND
		CH0_CA0 = @CA0_PK
	GROUP BY COALESCE(CL0_BelongsToGroup, CL0_PK)
	HAVING COUNT(*) = 2
)
");

			var atleastOneReversedHeaderQuery = new ZDBOnlyQuery(typeof(AccCommissionLine));
			atleastOneReversedHeaderQuery.AddFilterAndZSQLParameterCollection(atleastOneReversedHeaderQuerySql, paramList);

			return !Factory.Exists(typeof(AccCommissionHeader), nonReversedHeadersQuery) && Factory.Exists(typeof(AccCommissionLine), atleastOneReversedHeaderQuery);
		}

		#endregion

		#region CommissionHeader

		protected virtual AccCommissionHeader GetCommissionHeaderWithLineGroups(ICommissionHeaderBuildItem buildItem, KeyValuePair<ZString, ICommissionAgreementAndRates> commissionTarget)
		{
			return buildItem.NewCommissionHeaderWithLineGroups(commissionTarget);
		}

		#endregion

		#region Finalize

		protected AccCommissionHeader Finalize(AccCommissionHeader commissionHeader)
		{
			foreach (var lineGroup in commissionHeader.LineGroups.ToArray())
			{
				if (!lineGroup.Lines.Any())
				{
					lineGroup.Delete();
				}
			}

			if (!commissionHeader.Lines.Any() && !commissionHeader.LineGroups.Any())
			{
				commissionHeader.Delete();
				return null;
			}
			else
			{
				var agreement = commissionHeader.CommissionAgreement;
				if (agreement != null && agreement.CA0_FirstUsageDateUtc.IsEmpty)
				{
					agreement.CA0_FirstUsageDateUtc = ZDateTime.UtcNow;
				}

				return commissionHeader;
			}
		}

		#endregion
	}
}
