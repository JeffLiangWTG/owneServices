using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Module
{
	public class BarcodeParsingController : BarcodeRuleSetController
	{
		#region Constructors

		public BarcodeParsingController()
			: this(false)
		{
		}

		public BarcodeParsingController(bool copyFromSystemTemplate)
		{
			CopyFromSystemTemplate = copyFromSystemTemplate;
		}

		readonly bool CopyFromSystemTemplate;

		#endregion

		protected override BarcodeRuleSet GetRuleSetFromRule(IBusiness rule) => rule is BarcodeRule barcodeRule ? barcodeRule.RuleSet : null;

		#region Form

		protected override void OnFormLoad(BarcodeRuleSetForm form, IBusiness businessEntity)
		{
			form.SelectRule(businessEntity as BarcodeRule);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.BarcodeParsing;
			}
		}
		#endregion

		#region Business Entity

		protected override void GetNewBusinessEntityInLocalFactoryCore(BarcodeRuleSet barcodeRuleSet)
		{
			if (CopyFromSystemTemplate)
			{
				var query = new ZQuery();
				query.AddToFilter(BarcodeRuleSetSchema.BRS_IsSystem, true);
				query.AddToFilter(BarcodeRuleSetSchema.BRS_Module, BarcodeModuleTypes.Codes.Warehouse);
				var systemRuleSets = Factory.Load<BarcodeRuleSet>(query);
				barcodeRuleSet.Rules.AddRange(systemRuleSets.SelectMany(s => s.Rules.Where(r => r.IsGS1).Select(r => r.Clone())));

				var numRules = (short)barcodeRuleSet.Rules.Count;
				var consumer = Factory.GetBarcodeParsingConsumerFromModuleCode(BarcodeModuleTypes.Codes.Warehouse);
				var targetFields = consumer.TargetFields;

				foreach (var gs1TargetField in consumer.GS1TargetFieldsToDefault)
				{
					var description = targetFields.GetDescriptionFromCode(gs1TargetField);
					CreateNewEmptyBarcodeRuleComponent(barcodeRuleSet, ++numRules, gs1TargetField, description);
				}
			}
		}

		static void CreateNewEmptyBarcodeRuleComponent(BarcodeRuleSet ruleSet, short ruleNumber, string targetField, string description)
		{
			var newRule = ruleSet.Rules.AddNew();
			newRule.BRU_Name = description;
			newRule.BRU_RuleNumber = ruleNumber;
			newRule.IsGS1 = true;
			newRule.IsPartialRule = true;

			var newComponent = newRule.Components.AddNew();
			newComponent.BRC_Sequence = 0;
			newComponent.BRC_TargetField = targetField;
		}

		#endregion

		#region ID

		public override ControllerID ID
		{
			get { return ControllerIDs.BarcodeParsing; }
		}

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BarcodeRule); }
		}

		#endregion
	}
}
