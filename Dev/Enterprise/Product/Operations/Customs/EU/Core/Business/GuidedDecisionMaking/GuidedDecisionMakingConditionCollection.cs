using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingConditionCollection : NonPersistentBusinessObjectCollection<GuidedDecisionMakingCondition>
	{
		public GuidedDecisionMakingConditionCollection(GuidedDecisionMakingBasic gDMBasic)
			: base(gDMBasic.Factory)
		{
			this.gDMBasic = Argument.NotNull(gDMBasic, nameof(gDMBasic));
			Load();
		}

		readonly GuidedDecisionMakingBasic gDMBasic;

		public override void Load()
		{
			RemoveAndDeleteAll();

			if (!gDMBasic.DataGrouping.IsEmpty && gDMBasic.Tariff is TariffView tariff)
			{
				var criteria = gDMBasic.ConditionSelectionCriteria;
				var conditions = ConditionChecker.GetApplicableConditions(tariff.Factory, tariff, criteria)
																			.Where(x => x.ConditionClass.Equals(Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control)
																					|| x.ConditionClass.Equals(Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate)
																					|| x.ConditionClass.Equals(Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.VAT))
																			.OrderBy(x => x.ConditionType)
																			.ThenBy(x => x.ZX1_Comment);
				var tempExitedConditionDetails = new List<GuidedDecisionMakingConditionDetail>();
				var gDMCalcData = new ConditionCalcDataForGuidedDecisionMakingBasic(gDMBasic);
				foreach (var groupByConditionClass in conditions
								.GroupBy(x => new { x.ZX1_ZZZ_NKDataGrouping, x.ConditionClass })
								.OrderBy(x => x.Key.ConditionClass))
				{
					foreach (var groupByConditionType in groupByConditionClass
									.GroupBy(x => x.ConditionType)
									.OrderBy(x => x.Key))
					{
						var groupByLogicalAND = groupByConditionType.GroupBy(x => x.ZX1_LogicalANDWithinGroup).ToArray();
						if (groupByLogicalAND.All(andGroup => andGroup.Any(x => x.ShouldStop(null, gDMCalcData))))
						{
							foreach (var stoppingConditionGroup in groupByLogicalAND.OrderBy(x => x.Key))
							{
								foreach (var condition in stoppingConditionGroup.Where(x => x.ShouldStop(null, gDMCalcData))
											.OrderBy(x => x.ConditionTypeDescription).ThenBy(x => x.ZX1_Comment))
								{
									var conditionValues = condition.ConditionValues;
									if (conditionValues.Any(x => x.IsDocumentConditionValue))
									{
										var guidedDecisionMakingCondition = new GuidedDecisionMakingCondition(gDMBasic);
										guidedDecisionMakingCondition.ConditionType = condition.ConditionType;
										guidedDecisionMakingCondition.ConditionTypeDescription = condition.ConditionTypeDescription;
										guidedDecisionMakingCondition.ConditionSatisfactionType = condition.ZX1_Comment;
										guidedDecisionMakingCondition.InformationValue = ZString.Join(Res.GetString("9EC7F447-0CD4-4A64-B896-AAA0D729204C", " or "), conditionValues.Where(x => x.IsInformationConditionValue).Select(x => x.ZX3_Value).OrderBy(x => x).ToArray());

										foreach (var conditionValue in conditionValues.Where(x => x.IsDocumentConditionValue).OrderBy(x => x.ZX3_Value))
										{
											var detailCode = conditionValue.ZX3_Value;
											var logicalGroup = conditionValue.ZX3_LogicalORWithinGroup;
											var codeMatchedConditionDetail = tempExitedConditionDetails.FirstOrDefault(x => x.Code == detailCode && x.LogicalGroup == logicalGroup);
											if (codeMatchedConditionDetail != null)
											{
												guidedDecisionMakingCondition.ConditionDetails.Add(codeMatchedConditionDetail);
											}
											else
											{
												var detail = guidedDecisionMakingCondition.ConditionDetails.AddNew();
												tempExitedConditionDetails.Add(detail);
												detail.Code = detailCode;
												detail.Type = conditionValue.ValueType;
												detail.TypeDescription = conditionValue.ValueTypeDescription;
												detail.LogicalGroup = logicalGroup;
												var suppportDocument = gDMBasic.CapturedDocumentConditions?.Where(x => x.Code.Equals(detailCode)).FirstOrDefault();
												if (suppportDocument != null && !suppportDocument.Value.Code.IsEmpty)
												{
													detail.Reference = suppportDocument.Value.Reference;
													detail.DateOfIssue = suppportDocument.Value.DateOfIssue;
													detail.IsTicked = true;
												}
											}
										}

										if (guidedDecisionMakingCondition.ConditionDetails.Count == 1)
										{
											guidedDecisionMakingCondition.ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().First().IsTicked = true;
										}

										Add(guidedDecisionMakingCondition);
									}
								}
							}
						}
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new GuidedDecisionMakingCondition(gDMBasic);

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
