//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBarcodeRuleValidation
//
//    This class should be used for overriding validation in AutoBarcodeRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleValidation : AutoBarcodeRuleValidation
	{
		public BarcodeRuleValidation(AutoBarcodeRule parent)
			: base(parent)
		{
		}

		new BarcodeRule Parent
		{
			get { return (BarcodeRule)base.Parent; }
		}

		// persistent

		#region CheckBRU_Name

		protected override void CheckBRU_Name()
		{
			base.CheckBRU_Name();
			MandatoryValidation.CheckEntered(Parent.BRU_NameInfo);
		}

		#endregion

		#region CheckBRU_RuleNumber

		protected override void CheckBRU_RuleNumber()
		{
			base.CheckBRU_RuleNumber();

			MandatoryValidation.CheckNotNegative(Parent.BRU_RuleNumberInfo);
			MandatoryValidation.CheckNotZero(Parent.BRU_RuleNumberInfo);
			CheckRuleNumberIsUnique();
		}

		void CheckRuleNumberIsUnique()
		{
			if (!Parent.BRU_RuleNumberInfo.HasErrors())
			{
				var ruleSet = Parent.RuleSet;
				if (ruleSet != null)
				{
					foreach (var rule in ruleSet.Rules)
					{
						if (rule.PK != Parent.PK && rule.BRU_RuleNumber == Parent.BRU_RuleNumber)
						{
							Parent.BRU_RuleNumberInfo.AddError(Res.GetString("5730a63f-871d-48f4-8ebe-19bc75555b5b", "Rule No. must be unique."));
							break;
						}
					}
				}
			}
		}

		#endregion

		#region CheckBRU_Terminator

		protected override void CheckBRU_Terminator()
		{
			base.CheckBRU_Terminator();
			CheckTerminatorEntered();
		}

		void CheckTerminatorEntered()
		{
			if (!Parent.BRU_TerminatorInfo.HasErrors()
				&& Parent.BRU_Terminator == BarcodeRule.EmptyTerminatorWithQuotes
				// Allow an empty terminator if all components are fixed length (last component on a full rule can be variable length)
				&& (Parent.IsPartialRule ? !Parent.IsAllComponentsFixedLength : !Parent.IsAllComponentsExceptLastFixedLength))
			{
				Parent.BRU_TerminatorInfo.AddError(Res.GetString("beae09b7-bf0b-4990-ad3b-00884afefa99", "Enter a Terminator or make all Components (except the last for Full rules) have Fixed Length."));
			}
		}

		#endregion

		#region CheckBRU_TerminatorIsWesternEuropean

		protected override void CheckBRU_TerminatorIsWesternEuropean()
		{
			// allow non-western european for the GS1 terminator
			if (!Parent.IsGS1 || Parent.BRU_Terminator != BarcodeRule.GS1Terminator)
			{
				base.CheckBRU_TerminatorIsWesternEuropean();
			}
		}

		#endregion

		// calculated

		#region ValidateTerminatorType

		public void ValidateTerminatorType()
		{
			ValidateCalculatedProperty(Parent.TerminatorTypeInfo);
		}

		protected void CheckTerminatorType()
		{
			MandatoryValidation.CheckEntered(Parent.TerminatorTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TerminatorTypeInfo);
		}

		#endregion

		#region ValidateIsPartialRule

		public void ValidateIsPartialRule()
		{
			ValidateCalculatedProperty(Parent.IsPartialRuleInfo);
		}

		protected void CheckIsPartialRule()
		{
			if (Parent.IsPartialRule && Parent.Components.Count > 1)
			{
				Parent.IsPartialRuleInfo.AddError(Res.GetString("a1414359-0334-4d1a-9cad-85959d60f1a1", "Partial Rules can only have one Component."));
			}
		}

		#endregion

		//

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateAtLeastOneComponentExists();
			ValidateTerminatorType();
			ValidateIsPartialRule();
		}

		void ValidateAtLeastOneComponentExists()
		{
			if (Parent.Components.Count == 0)
			{
				Parent.AddRowError(Res.GetString("46859d27-c88f-4113-98ba-dd5e1e574b8c", "A Rule requires at least one Component."));
			}
			else
			{
				Parent.ClearRowNotifications();
			}
		}

		#endregion
	}
}
