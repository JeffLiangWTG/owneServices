using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNLevelingRuleValidation : AutoBMNCNLevelingRuleValidation
	{
		public BMNCNLevelingRuleValidation(AutoBMNCNLevelingRule parent)
			: base(parent)
		{
		}

		new BMNCNLevelingRule Parent => (BMNCNLevelingRule)base.Parent;

		protected override void CheckBNR_Name()
		{
			base.CheckBNR_Name();

			MandatoryValidation.CheckEntered(Parent.BNR_NameInfo);

			if (Parent.Diagram != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.BNR_NameInfo, Parent.Diagram.LevelingRules);
			}
		}

		protected override void CheckBNR_Type()
		{
			base.CheckBNR_Type();

			MandatoryValidation.CheckEntered(Parent.BNR_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BNR_TypeInfo);
		}

		protected override void CheckBNR_RuleValue()
		{
			base.CheckBNR_RuleValue();

			CompareValidation.CheckGreaterThanOrEqualTo(Parent.BNR_RuleValueInfo, 1);
		}

		protected void CheckColorName()
		{
			MandatoryValidation.CheckEntered(Parent.ColorNameInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ColorNameInfo);
		}

		public void ValidateColorName()
		{
			ValidateCalculatedProperty(Parent.ColorNameInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateColorName();
		}
	}
}
