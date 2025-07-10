using System;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BarcodeParsing.Module
{
	public class BarcodeValidationController : BarcodeRuleSetController
	{
		protected override BarcodeRuleSet GetRuleSetFromRule(IBusiness rule) => rule is BarcodeValidationRule barcodeValidationRule ? barcodeValidationRule.RuleSet : null;

		#region Form

		protected override void OnFormLoad(BarcodeRuleSetForm form, IBusiness businessEntity)
		{
			form.SelectValidationRulesTabPage();
			form.SelectRule(businessEntity as BarcodeValidationRule);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.BarcodeValidation;

		#endregion

		#region ID

		public override ControllerID ID => ControllerIDs.BarcodeValidation;

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject => typeof(BarcodeValidationRule);

		#endregion
	}
}
