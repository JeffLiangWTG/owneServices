using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class CusAuthorizationUsageUpdater
	{
		public CusAuthorizationUsageUpdater(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
		}

		public void UpdateEntryInstructionAuthorizations()
		{
			foreach (var entryInstruction in jobDeclaration.CustomsEntryInstructions)
			{
				DeleteSystemGeneratedAuthorizations(entryInstruction);
				PopulateSystemGeneratedCusAuthorizations(entryInstruction);
			}
		}

		protected internal RuleConfiguration ForAuthorizationType(ZString authCode)
		{
			var result = new RuleConfiguration();
			rules.Add(authCode, result);
			return result;
		}

		void PopulateSystemGeneratedCusAuthorizations(CusEntryInstruction entryInstruction)
		{
			foreach (var ruleConfiguration in rules)
			{
				var codeExists = entryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(u => u.AGC_Code == ruleConfiguration.Key);
				if (!codeExists && ruleConfiguration.Value.Required(entryInstruction))
				{
					foreach (var (holder, number) in ruleConfiguration.Value.Calculate(entryInstruction))
					{
						if (!holder.IsEmpty || !number.IsEmpty)
						{
							AddCusAuthorizationUsage(entryInstruction, ruleConfiguration.Key, holder, number);
						}
					}
				}
			}
		}

		CusAuthorizationUsage AddCusAuthorizationUsage(CusEntryInstruction entryInstruction, ZString code, ZGuid holder, ZString number)
		{
			var newUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			newUsage.AGC_Code = code;
			newUsage.AGC_OH_Owner = holder;
			newUsage.AGC_IsSystemGenerated = true;
			if (!number.IsEmpty)
			{
				newUsage.EffectiveReferenceNumber = number;
			}
			return newUsage;
		}

		void DeleteSystemGeneratedAuthorizations(CusEntryInstruction entryInstruction)
		{
			var cusAuthorizationUsages = entryInstruction.CusAuthorizationUsages;
			foreach (var usage in cusAuthorizationUsages.Cast<CusAuthorizationUsage>().Where(usage => usage.AGC_IsSystemGenerated).ToArray())
			{
				cusAuthorizationUsages.RemoveAndDelete(usage);
			}
		}

		readonly Dictionary<ZString, RuleConfiguration> rules = new Dictionary<ZString, RuleConfiguration>();
		readonly JobDeclaration jobDeclaration;
	}
}
