using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class RelevantDiagnosticCriteriaCollection : NonPersistentBusinessObjectCollection<RelevantDiagnosticCriteria>
	{
		public RelevantDiagnosticCriteriaCollection(TriageAssistBusinessObject parent) : base(parent.Factory)
		{
			Parent = parent;
		}

		public TriageAssistBusinessObject Parent { get; }

		public void LoadBySearchTerm()
		{
			try
			{
				using (SuspendCountChanged(null))
				{
					RemoveAll();

					if (!Parent.SearchTermTokens.Any() || (!Parent.ShouldSearchKeywords && !Parent.ShouldSearchDescription))
					{
						return;
					}

					var query = new ZDBOnlyQuery(typeof(IncidentDiagnosticCriteria));

					if (!Parent.Product.IsEmpty)
					{
						var subQueryIncidentTriage = new ZDBOnlySubQuery(typeof(IncidentTriage), IncidentTriageSchema.PK);
						subQueryIncidentTriage.AddToFilter(IncidentTriageSchema.IMT_Product, Parent.Product);
						subQueryIncidentTriage.AddToFilter(JoinCondition.Or, IncidentTriageSchema.IMT_Product, SQLComparisonOperator.Equal, ZString.Empty);

						var subQueryIncidentTriageDiagnosticCriteriaPivot = new ZDBOnlySubQuery(typeof(IncidentTriageDiagnosticCriteriaPivot), IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMD_DiagnosticCriteria);
						subQueryIncidentTriageDiagnosticCriteriaPivot.AddSubQuery(IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage, subQueryIncidentTriage, JoinCondition.And);

						query.AddSubQuery(subQueryIncidentTriageDiagnosticCriteriaPivot, JoinCondition.And);
					}

					var subQueryToken = new ZQuery();
					if (Parent.ShouldSearchKeywords)
					{
						AddTokenFilter(subQueryToken, IncidentDiagnosticCriteriaSchema.IMD_Keywords);
					}

					if (Parent.ShouldSearchDescription)
					{
						AddTokenFilter(subQueryToken, IncidentDiagnosticCriteriaSchema.IMD_Description);
					}

					query.AddToFilter(subQueryToken, JoinCondition.And);
					query.AddToFilter(IncidentDiagnosticCriteriaSchema.IMD_IsActive, true);
					var typeFilterCode = Parent.CriteriaTypeFilterCode;
					if (!typeFilterCode.IsEmpty)
					{
						query.AddToFilter(IncidentDiagnosticCriteriaSchema.IMD_Type, typeFilterCode);
					}

					var relevantCriteria = Parent.LoadRelevantCriteria(query);
					foreach (var item in relevantCriteria)
					{
						item.Sequence = Parent.SearchTermTokens.Sum(t => item.DiagnosticCriteria.IMD_Description.Contains(t, StringComparison.OrdinalIgnoreCase)
													|| item.DiagnosticCriteria.IMD_Keywords.Contains(t, StringComparison.OrdinalIgnoreCase) ? 1 : 0);
					}

					AddRange(relevantCriteria.OrderByDescending(x => x.Sequence).ThenBy(x => x.DiagnosticCriteria.IMD_Description));
				}
			}
			finally
			{
				OnCountChanged(null);
			}
		}

		void AddTokenFilter(ZQuery query, SchemaColumn schemaColumn)
		{
			switch (Parent.SearchTermOperator)
			{
				case TriageAssistBusinessObject.ComparisonConstants.Exact:
					{
						query.AddToFilter(JoinCondition.Or, schemaColumn, SQLComparisonOperator.Contains, Parent.SearchTerm.Trim('"'));
						break;
					}
				case TriageAssistBusinessObject.ComparisonConstants.ContainsAll:
					{
						AddTokenFilterCore(query, schemaColumn, JoinCondition.And);
						break;
					}
				default: //AnyMatch
					{
						AddTokenFilterCore(query, schemaColumn, JoinCondition.Or);
						break;
					}
			}
		}

		void AddTokenFilterCore(ZQuery query, SchemaColumn schemaColumn, JoinCondition keywordJoinCondition)
		{
			var prefix = ZGuid.NewZGuid().ToString().Replace("-", "");
			var sqlParameters = new ZSqlParameterCollection();
			var sql = new ZStringBuilder();

			sql.AppendLine(" ( ");
			foreach (var item in Parent.SearchTermTokens.Select((token, index) => new { token, index }))
			{
				if (item.index > 0)
				{
					sql.AppendLine(keywordJoinCondition == JoinCondition.And ? " AND " : " OR ");
				}

				var isLastToken = item.index == Parent.SearchTermTokens.Count() - 1;
				var parameterName = $"@P_{prefix}_{item.index}";
				var pattern = (isLastToken && !Parent.SearchTermEndsWithSpace) ? $"%[^a-zA-Z0-9]{item.token}%" //starts with..
																		: $"%[^a-zA-Z0-9]{item.token}[^a-zA-Z0-9]%"; //whole word match

				sqlParameters.Add(parameterName, pattern, schemaColumn);
				sql.AppendLine($"PATINDEX({parameterName}, ' ' + [{schemaColumn.Name}] + ' ') > 0");
			}
			sql.AppendLine(" ) ");

			query.AddFilterAndZSQLParameterCollection(sql.ToString(), sqlParameters, JoinCondition.Or);
		}

		public void LoadLinkedCriteria()
		{
			try
			{
				using (SuspendCountChanged(null))
				{
					RemoveAll();
					var pivots = Factory.Load<IncidentDiagnosticCriteriaPivot>(new ZQuery(IncidentDiagnosticCriteriaPivotSchema.IMV_ParentID, Parent.Parent.PK));
					var diagnosticCriteriaQuery = new ZQuery(IncidentDiagnosticCriteriaSchema.PK, pivots.Select(x => x.IMV_IMD_DiagnosticCriteria));
					diagnosticCriteriaQuery.AddToFilter(IncidentDiagnosticCriteriaSchema.IMD_IsActive, true);
					var relevantCriteria = Parent.LoadRelevantCriteria(diagnosticCriteriaQuery).OrderBy(x => x.DiagnosticCriteria.IMD_Description);

					foreach (var item in relevantCriteria)
					{
						using (item.SuspendSettingHasChanges())
						{
							var pivot = pivots.SingleOrDefault(x => x.IMV_IMD_DiagnosticCriteria == item.PK);
							switch (pivot.IMV_Status)
							{
								case IncidentDiagnosticCriteriaPivotStatusList.Codes.Confirmed:
									{
										item.Confirm = true;
										break;
									}
								case IncidentDiagnosticCriteriaPivotStatusList.Codes.Negate:
									{
										item.Negate = true;
										break;
									}
								case IncidentDiagnosticCriteriaPivotStatusList.Codes.Investigate:
									{
										item.Investigate = true;
										break;
									}
								default:
									{
										item.Confirm = item.Negate = item.Investigate = false;
										break;
									}
							}
						}
					}
					AddRange(relevantCriteria);
					SortByActionOption();
				}
			}
			finally
			{
				OnCountChanged(null);
			}
		}

		public void LoadSuggestedCriteria()
		{
			try
			{
				using (Parent.FilteredSuggestedCriteriaCollection.SuppressRebuild())
				using (Parent.FilteredSuggestedCriteriaCollection.SuspendCountChanged(null))
				using (SuspendCountChanged(null))
				{
					RemoveAll();
					var suggestedCriteriaPKs = Parent.TriageAssistTreeWrapperCollection.OfType<TriageAssistTreeTriageWrapper>()
						.Where(x => x.TriageStatus != TriageAssistTreeTriageWrapper.TriageNodeStatus.Excluded)
						.SelectMany(x => x.Triage.DiagnosticCriteriaPivots.Select(p => p.IMO_IMD_DiagnosticCriteria))
						.Except(Parent.LinkedCriteriaCollection.Select(x => x.PK));

					var diagnosticCriteriaQuery = new ZQuery(IncidentDiagnosticCriteriaSchema.PK, suggestedCriteriaPKs);
					diagnosticCriteriaQuery.AddToFilter(IncidentDiagnosticCriteriaSchema.IMD_IsActive, true);
					var suggestedCriteria = Parent.LoadRelevantCriteria(diagnosticCriteriaQuery)
								.OrderBy(x => x.DiagnosticCriteria.IMD_Description);

					//if the DC is linked to a focused triage node, then the DC is focused.
					var criteriaLinkedToFocusedTriageNode = Parent.TriageAssistTreeWrapperCollection.OfType<TriageAssistTreeTriageWrapper>()
						.Where(x => x.IsFocused)
						.SelectMany(x => x.Triage.DiagnosticCriteriaPivots.Select(p => p.IMO_IMD_DiagnosticCriteria))
						.ToHashSet();

					foreach (var criteria in suggestedCriteria)
					{
						criteria.IsFocused = criteriaLinkedToFocusedTriageNode.Contains(criteria.PK);
					}

					AddRange(suggestedCriteria.OrderBy(x => x.IsFocused ? 0 : 1));
				}
			}
			finally
			{
				OnCountChanged(null);
			}
		}

		public void SortByActionOption() => Sort(new CriteriaComparer());

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RelevantDiagnosticCriteria(Factory);
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override bool AllowSort => false;
	}
}
