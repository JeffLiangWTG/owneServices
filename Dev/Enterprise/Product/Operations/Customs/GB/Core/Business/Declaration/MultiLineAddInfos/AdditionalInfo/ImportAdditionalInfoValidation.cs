using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos
{
	public class ImportAdditionalInfoValidation : AdditionalInfoValidation
	{
		public ImportAdditionalInfoValidation(AdditionalInfo parent) : base(parent)
		{
		}

		#region CSI_Code

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = Parent;
			if (!parent.CSI_Code.IsEmpty)
			{
				if (parent is CusSupportingInfo parentInfo
					&& parentInfo.Parent is JobComInvoiceLine invoiceLine)
				{
					var csiCodes = GetAllCSI_CodesForInvoiceLine(invoiceLine);
					var applicableCSICodes = csiCodes.Select(c => c).Where(c => c.In(RiskingAIStatementCodeList));
					foreach (var csiCode in applicableCSICodes)
					{
						if (!CheckRuleForInvoiceLine(CheckCSI_Code_Rules, applicableCSICodes, invoiceLine, csiCode))
						{
							if (parent.CSI_Code.Equals(csiCode))
							{
								if (ExclusionRiskAIStatementErrors.TryGetValue(csiCode, out var errorMessage))
								{
									parent.CSI_CodeInfo.AddMessageError(errorMessage);
								}
							}
						}
					}
					if (parent.CSI_Code.Equals(GBCommonConstants.AdditonalInfoCodes.NIQUO) && (invoiceLine.Declaration?.JE_NorthernIrelandMode.Equals(NIModeList.Codes.MovementFromNiToGreatBritain) ?? false))
					{
						parent.CSI_CodeInfo.AddMessageError(Res.GetString("B7D5BAE1-E956-4668-9816-45AFA4BD3A3E", "NIQUO cannot be used on a job whose NI Protocol field is N2G"));
					}
					if (!CheckRuleForInvoiceLine(CheckFor_NIIMP_OR_NIDOMRule, csiCodes, invoiceLine, GBCommonConstants.AdditonalInfoCodes.NIIMP))
					{
						parent.CSI_CodeInfo.AddMessageError(ExclusionRiskAIStatementErrors[GBCommonConstants.AdditonalInfoCodes.NIIMP]);
					}
				}
			}

			#endregion
		}

		static bool CheckFor_NIIMP_OR_NIDOMRule(IEnumerable<ZString> applicableCSICodes, ZString csiCode)
		{
			bool ruleCheck = true;
			if (applicableCSICodes.Any(x => x.In(RiskingAIStatementCodeList)))
			{
				if (ExclusionRiskAIStatementRules.TryGetValue(csiCode, out var excludedAIStatementCodes_For_CSICode))
				{
					var optionalCSICodesFound = applicableCSICodes.Count(x => x.In(excludedAIStatementCodes_For_CSICode));
					var csiCodeOccurrences = applicableCSICodes.Count(x => x == csiCode);
					ruleCheck = (optionalCSICodesFound + csiCodeOccurrences) >= 1;
				}
			}
			return ruleCheck;
		}

		static bool CheckCSI_Code_Rules(IEnumerable<ZString> applicableCSICodes, ZString csiCode)
		{
			bool ruleCheck = true;
			if (ExclusionRiskAIStatementRules.TryGetValue(csiCode, out var excludedAIStatementCodes_For_CSICode))
			{
				ruleCheck = !applicableCSICodes.Any(x => x.In(excludedAIStatementCodes_For_CSICode));
				if (ruleCheck)
				{
					var optionalCSICodesFound = 0;
					foreach (var xCode in RiskingAIStatementCodeList.Except(excludedAIStatementCodes_For_CSICode))
					{
						var csiCodeOccurrences = applicableCSICodes.Count(x => x == xCode);
						if (!xCode.Equals(csiCode) && csiCodeOccurrences > 0)
						{
							optionalCSICodesFound++;
						}
						ruleCheck = optionalCSICodesFound <= 1 && csiCodeOccurrences <= 1;
						if (!ruleCheck)
						{
							break;
						}
					}
				}
			}
			return ruleCheck;
		}

		static bool CheckRuleForInvoiceLine(Func<IEnumerable<ZString>, ZString, bool> checkMethod, IEnumerable<ZString> applicableCSICodes, JobComInvoiceLine invoiceLine, string key)
		{
			var checkResult = checkMethod(applicableCSICodes, key);

			if (!checkResult)
			{
				if (ExclusionRiskAIStatementErrors.TryGetValue(key, out var errorMessage))
				{
					invoiceLine.AddRowMessageError(errorMessage);
				}
			}
			else
			{
				if (ExclusionRiskAIStatementErrors.TryGetValue(key, out var errorMessage))
				{
					invoiceLine.RemoveRowMessageError(errorMessage);
				}
			}

			return checkResult;
		}

		static List<ZString> GetAllCSI_CodesForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var result = new List<ZString>();

			AddAdditionalInfosCodesToList(result, invoiceLine?.AdditionalInfos);

			return result;
		}

		static void AddAdditionalInfosCodesToList(List<ZString> list, AdditionalInfoCollection additionalInfo)
		{
			if (additionalInfo != null)
			{
				list.AddRange(additionalInfo.Select(x => ((AdditionalInfo)x).CSI_Code).ToList());
			}
		}
	}
}

