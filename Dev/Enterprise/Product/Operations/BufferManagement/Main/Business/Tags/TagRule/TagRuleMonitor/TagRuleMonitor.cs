using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class TagRuleMonitor : BMServiceTaskProcessor
	{
		public TagRuleMonitor(ILogger logger)
			: base(logger, new ServiceTaskFactoryProviderWrapper(logger, TagMonitorServiceTask.Code))
		{
		}

		int churnLimit;
		int outputLimit;
		ZDateTime startTime;
		ZDateTime endTime;
		BusinessObjectFactory factory;

		#region ProcessHeaderProcessor Overrides

		protected override bool IsSufficientWorkflowManagementModeEnabled => BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled;

#if DEBUG

		public void Process()
		{
			Process(CancellationToken.None);
		}

#endif

		public override void ProcessCore(CancellationToken token)
		{
			var depth = BMSRegistry.Instance.TagRuleChurnDetectionDepth.Value;
			churnLimit = BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value;
			outputLimit = BMSRegistry.Instance.TagRuleLogOutputLimit.Value;

			endTime = ZDateTime.UtcNow;
			startTime = endTime.AddMinutes(-depth);

			factory = new BusinessObjectFactory();

			var fightingRuleIncidents = GetFightingRuleSets().GroupBy(r => r.RulesPKsCommaSeparated);
			var rulesToDeactivate = new Dictionary<TagRule, string>();

			int incidentCounter = 0;
			if (!fightingRuleIncidents.Any())
			{
				Logger.Log(LogType.Debug, "No fighting rules found."); // Error messages for logging and reporting should be in English
			}
			else
			{
				Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "{0} fighting tag rule incident(s) found:", fightingRuleIncidents.Count())); // Error messages for logging and reporting should be in English

				foreach (var ruleGroup in fightingRuleIncidents)
				{
					token.ThrowIfCancellationRequested();
					Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Incident {0}", ++incidentCounter)); // Error messages for logging and reporting should be in English

					var messageBuilder = new ZStringBuilder();

					var rulesPKs = ruleGroup.Key.Split(',').Select(s => new ZGuid(s.Trim()));
					var ruleDecisions = rulesPKs.Select(pk => new FightingRuleDecision(pk, factory.Load<TagRule>(pk))).ToArray();

					using (GetDisposableEnvironment(ruleDecisions))
					{
						MakeDecisionOnIncidentAndLog(ruleDecisions, ruleGroup, messageBuilder);

						var fightingDetails = messageBuilder.ToStringWithNewLineBetweenAppends();
						Logger.Log(LogType.Information, fightingDetails);

						var ruleDecisionsForDeactivation = ruleDecisions.Where(d => d.ToDeactivate).ToArray();

						if (ruleDecisionsForDeactivation.Any())
						{
							TagRule.NotifyOnTagRulesFighting(fightingDetails);

							foreach (var decision in ruleDecisionsForDeactivation)
							{
								token.ThrowIfCancellationRequested();
								if (!rulesToDeactivate.ContainsKey(decision.Rule))
								{
									var notifyMessageBuilder = new ZStringBuilder();
									notifyMessageBuilder.AppendFormat((NoResString)"Rule [{0}] was deactivated in order to prevent tag fighting.", decision.Rule.TGR_Name); // Error messages for logging and reporting should be in English
									notifyMessageBuilder.Append(fightingDetails);
									rulesToDeactivate.Add(decision.Rule, notifyMessageBuilder.ToStringWithNewLineBetweenAppends());
								}
							}
						}
					}
				}

				if (rulesToDeactivate.Count == 0)
				{
					Logger.Log(LogType.Information, "No fighting rules have been deactivated."); // Error messages for logging and reporting should be in English
				}
				else
				{
					foreach (var rule in rulesToDeactivate)
					{
						token.ThrowIfCancellationRequested();

						using (DisposableEnvironment.ForBranch(rule.Key.TGR_GB_Branch.ToGuid()))
						{
							rule.Key.LogFailureOnRuleAndMaybeDisableRule(rule.Value, notifyByEmail: false);
						}
					}

					var info = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} fighting rule(s) have been deactivated", rulesToDeactivate.Count); // Error messages for logging and reporting should be in English
					Logger.Log(LogType.Information, info);
				}
			}
		}

		static IDisposable GetDisposableEnvironment(FightingRuleDecision[] ruleDecisions)
		{
			var firstRule = ruleDecisions.FirstOrDefault(rd => rd.Rule != null)?.Rule;

			return firstRule == null
				? new BMSServiceTaskHelper().GetTemporaryEnvironmentForServiceTaskBranch()
				: DisposableEnvironment.ForBranch(firstRule.TGR_GB_Branch.ToGuid());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<FightingRulesSet> GetFightingRuleSets()
		{
			var fightingRuleSets = new List<FightingRulesSet>();
			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT TargetTagable, TargetTable, RulePKs, Total FROM dbo.GetFightingTagRules('{0}', '{1}', {2}, {3})", // SQL command
								startTime.SqlFormat, endTime.SqlFormat, churnLimit, outputLimit);
			var command = Db.Connection.Command(sql); // Direct SQL is required to increase performance
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var targetTagable = new ZGuid(reader[0]);
					var targetTable = (string)reader[1];
					var rulePKs = (string)reader[2];
					var total = (int)reader[3];
					try
					{
						fightingRuleSets.Add(new FightingRulesSet(targetTagable, targetTable, rulePKs, total));
					}
					catch (ZTypeValueException)
					{
						Logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Wrong Guid in the SL_Reference field of StmALog: SL_Parent = {0}, SL_Table = {1}", targetTagable, targetTable));
					}
				}
			}
			return fightingRuleSets;
		}

		void MakeDecisionOnIncidentAndLog(FightingRuleDecision[] ruleDecisions, IEnumerable<FightingRulesSet> items, ZStringBuilder messageBuilder)
		{
			if (ruleDecisions.Length == 1)
			{
				messageBuilder.Append((NoResString)"Suspicious tag rule activity:"); // Error messages for logging and reporting should be in English

				var decision = ruleDecisions[0];

				if (!CheckRuleWasDeletedOrDeactivated(decision, messageBuilder))
				{
					if (decision.Rule.TGR_ActionType == "ARM")
					{
						messageBuilder.AppendFormat((NoResString)" - {0} [{1}] of the ARM type is probably fighting with itself and to be deactivated in order to prevent tag fighting.", decision.Rule.TGR_IsSystem ? (NoResString)"System rule" : (NoResString)"Rule", decision.Rule.TGR_Name); // Error messages for logging and reporting should be in English

						ListItemsToFight(items, messageBuilder);
						decision.ToDeactivate = true;
					}
					else
					{
						messageBuilder.AppendFormat((NoResString)" - Rule [{0}] triggered several times due to manual tagging/untagging and does not need to be deactivated.", decision.Rule.TGR_Name); // Error messages for logging and reporting should be in English
						ListItemsToFight(items, messageBuilder);
						decision.ToDeactivate = false;
					}
				}
			}
			else
			{
				messageBuilder.Append((NoResString)"The following tag rules were fighting:"); // Error messages for logging and reporting should be in English

				var nonSystemRulesPresent = ruleDecisions.Any(d => d.Rule != null && !d.Rule.TGR_IsSystem);
				foreach (var decision in ruleDecisions)
				{
					if (!CheckRuleWasDeletedOrDeactivated(decision, messageBuilder))
					{
						if (decision.Rule.TGR_IsSystem && nonSystemRulesPresent)
						{
							messageBuilder.AppendFormat((NoResString)" - System rule [{0}] will stay active.", decision.Rule.TGR_Name); // Error messages for logging and reporting should be in English
							decision.ToDeactivate = false;
						}
						else if (decision.Rule.TGR_IsSystem && !nonSystemRulesPresent)
						{
							messageBuilder.AppendFormat((NoResString)" - System rule [{0}] is to be deactivated in order to prevent tag fighting.", decision.Rule.TGR_Name); // Error messages for logging and reporting should be in English
							decision.ToDeactivate = true;
						}
						else
						{
							messageBuilder.AppendFormat((NoResString)" - Rule [{0}] is to be deactivated in order to prevent tag fighting.", decision.Rule.TGR_Name); // Error messages for logging and reporting should be in English
							decision.ToDeactivate = true;
						}
					}
				}

				ListItemsToFight(items, messageBuilder);
			}
		}

		bool CheckRuleWasDeletedOrDeactivated(FightingRuleDecision decision, ZStringBuilder messageBuilder)
		{
			if (decision.Rule == null)
			{
				messageBuilder.AppendFormat((NoResString)" - Rule [PK: {0}] was deleted in the interim between the tag fighting and scanning.", decision.RulePk.ToString()); // Error messages for logging and reporting should be in English
				decision.ToDeactivate = false;
				return true;
			}
			else if (!decision.Rule.TGR_IsActive)
			{
				messageBuilder.AppendFormat((NoResString)" - Rule [{0}] is already deactivated.", decision.Rule.TGR_Name); // Error messages for logging and reporting should be in English
				decision.ToDeactivate = false;
				return true;
			}
			return false;
		}

		void ListItemsToFight(IEnumerable<FightingRulesSet> items, ZStringBuilder messageBuilder)
		{
			int itemsToFightTotal = items.First().Total;
			if (itemsToFightTotal <= items.Count())
			{
				messageBuilder.AppendFormat((NoResString)"{0} item(s) caused the tag fighting:", itemsToFightTotal.ToString(CultureInfo.InvariantCulture)); // Error messages for logging and reporting should be in English
			}
			else
			{
				messageBuilder.AppendFormat((NoResString)"{0} item(s) caused the tag fighting (only first {1} items are shown):", itemsToFightTotal.ToString(CultureInfo.InvariantCulture), outputLimit.ToString(CultureInfo.InvariantCulture)); // Error messages for logging and reporting should be in English
			}
			int itemsCounter = 0;
			foreach (var itemToFight in items)
			{
				itemsCounter++;
				var tagable = StmALog.GetMaster(itemToFight.TargetTagable, itemToFight.TargetTable, factory);
				if (tagable == null)
				{
					messageBuilder.AppendFormat((NoResString)" - Item {0}: deleted in the interim between the tag fighting and monitoring", itemsCounter.ToString(CultureInfo.InvariantCulture)); // Error messages for logging and reporting should be in English
				}
				else
				{
					var tagableWithCode = tagable as ICodeDescription;
					messageBuilder.AppendFormat((NoResString)" - Item {0}: Code = {1}, Description = {2}", itemsCounter.ToString(CultureInfo.InvariantCulture), tagableWithCode.Code, tagableWithCode.Description); // Error messages for logging and reporting should be in English
				}
			}

			if (itemsToFightTotal > items.Count())
			{
				messageBuilder.Append("........................"); // Error messages for logging and reporting should be in English
			}
		}

		#endregion

		#region FightingRulesSet

		class FightingRulesSet
		{
			public ZGuid TargetTagable;
			public string TargetTable;
			public string RulesPKsCommaSeparated;
			public int Total;

			public FightingRulesSet(ZGuid targetTagable, string targetTable, string rulesPKs, int total)
			{
				this.TargetTagable = targetTagable;
				this.TargetTable = targetTable;
				this.RulesPKsCommaSeparated = rulesPKs;
				this.Total = total;
			}
		}

		class FightingRuleDecision
		{
			public FightingRuleDecision(ZGuid rulePk, TagRule rule)
			{
				RulePk = rulePk;
				Rule = rule;
			}

			public ZGuid RulePk { get; }
			public TagRule Rule { get; }
			public bool ToDeactivate { get; set; }
		}

		#endregion

	}
}
