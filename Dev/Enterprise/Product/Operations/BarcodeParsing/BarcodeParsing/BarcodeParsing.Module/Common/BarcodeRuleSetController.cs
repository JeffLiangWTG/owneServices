using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BarcodeParsing.Module
{
	public abstract class BarcodeRuleSetController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected BarcodeRuleSetController()
		{
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.BarcodeParsingDelete;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.BarcodeParsingEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.BarcodeParsingNew;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.BarcodeParsingView;

		#endregion

		#region Delete Rules

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects) =>
			DeleteRulesOnUserConfirmation(Res.GetString("2af5b8fe-f912-4ea8-8f0a-9452b0447712", "Are you sure you wish to delete the selected Rules?"), selectedBusinessObjects.ToArray());

		protected override void ShowDeleteFormAfterSecurityAndCanDeleteCheck(BusinessObject sourceEntity) =>
			DeleteRulesOnUserConfirmation(Res.GetString("afc2b3ee-1a57-411b-a3c4-9060d763e1cb", "Are you sure you wish to delete the selected Rule?"), sourceEntity);

		protected void DeleteRulesOnUserConfirmation(string userQuestion, params BusinessObject[] rules)
		{
			if (rules.Length > 0)
			{
				var rulesFactory = rules[0].Factory;
				var warningMessageResult = Globals.Message.Show(
					userQuestion,
					Res.GetString("a3b7f87d-0286-4ace-bd9f-77be1ce6ab41", "Deleting Rules"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);

				if (warningMessageResult == DialogResult.Yes)
				{
					DeleteRuleOrEmptyRuleSet(rules);
					try
					{
						rulesFactory.Save();
					}
					catch (ZCannotSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
					catch (ZSaveException saveEx)
					{
						ZExceptionReporting.HandleSaveException(saveEx);
					}
				}
			}
		}

		protected abstract BarcodeRuleSet GetRuleSetFromRule(IBusiness rule);

		void DeleteRuleOrEmptyRuleSet(params BusinessObject[] rules)
		{
			foreach (var rule in rules)
			{
				var ruleSet = GetRuleSetFromRule(rule);
				if (ruleSet != null && ruleSet.TotalRules == 1)
				{
					ruleSet.Delete();
				}
				else
				{
					rule.Delete();
				}
			}
		}

		#endregion

		#region Business Entity

		protected override Guid GetIDForBusinessEntity(IBusiness entity)
		{
			var ruleSet = GetRuleSetFromRule(entity);
			return ruleSet != null ? ruleSet.PK.ToGuid() : entity.Identifier.ToGuid();
		}

		protected sealed override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = Factory.New<BarcodeRuleSet>();
			GetNewBusinessEntityInLocalFactoryCore(result);
			return result;
		}

		protected virtual void GetNewBusinessEntityInLocalFactoryCore(BarcodeRuleSet barcodeRuleSet)
		{
		}

		#endregion

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var ruleSet = GetRuleSetFromRule(businessEntity) ?? (BarcodeRuleSet)businessEntity;
			var form = new BarcodeRuleSetForm(ruleSet);
			form.Load += (sender, e) => OnFormLoad(form, businessEntity);
			return form;
		}

		protected virtual void OnFormLoad(BarcodeRuleSetForm form, IBusiness businessEntity)
		{
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			SetCollectionForDefaultsAndValidation(new BarcodeRuleSetCollection(businessEntity.Factory));
			return base.ShowFormForNewEntityCore(businessEntity);
		}

		#endregion
	}
}
