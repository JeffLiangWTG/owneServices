using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class CusGuaranteeRuleValidation : Customs.Business.CusGuaranteeRuleValidation
	{
		public CusGuaranteeRuleValidation(CusGuaranteeRule parent) : base(parent)
		{
		}

		protected new CusGuaranteeRule Parent => (CusGuaranteeRule)base.Parent;

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();

			CheckValidLiabilityApplicablePercentageValueEntered();
		}

		void CheckValidLiabilityApplicablePercentageValueEntered()
		{
			if (Parent.CPR_RuleCode == PermitRuleCodeList.Codes.LAP)
			{
				ListValidation.ErrorIfInvalidCode(EnterValidLiabilityApplicablePercentage, Parent.CPR_ValueFromInfo);
			}
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckValidateGuaranteeRules();
		}

		void CheckValidateGuaranteeRules()
		{
			var composedLiabilityRuleCodes = new HashSet<string>
			{
				PermitRuleCodeList.Codes.PCP,
				PermitRuleCodeList.Codes.PCV,
				PermitRuleCodeList.Codes.PCD
			};
			var parent = Parent;

			if (parent.CPR_RuleCode == PermitRuleCodeList.Codes.LAP || composedLiabilityRuleCodes.Contains(parent.CPR_RuleCode))
			{
				var existingRuleCodes = parent.GuaranteeHeader.CusGuaranteeRules.Select(rule => rule.CPR_RuleCode).ToList();

				if (existingRuleCodes.Contains(PermitRuleCodeList.Codes.LAP) && existingRuleCodes.Any(code => composedLiabilityRuleCodes.Contains(code)))
				{
					if (parent.CPR_RuleCode == PermitRuleCodeList.Codes.LAP)
					{
						parent.AddRowError(Res.GetString("53C4D5B8-C182-4AFC-B358-BE82F083DF11", "LAP rule cannot exist with PCP, PCV, or PCD."));
					}
					else
					{
						parent.AddRowError(Res.GetString("53C4D5B8-C182-4AFC-B358-BE82F083DF45", $"{parent.CPR_RuleCode} rule cannot exist with LAP."));
					}
				}
			}
		}

		static ResourceString EnterValidLiabilityApplicablePercentage => ResString.GetMultilingualString("53C4D5B8-C182-4AFC-B358-BE82F083DF53", "Enter a valid Liability Applicable Percentage.");
	}
}
