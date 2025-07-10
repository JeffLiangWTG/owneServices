using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class LicenceUsageFilterValidation : ModuleFilterDateValidation
	{
		public LicenceUsageFilterValidation(LicenceUsageFilter parent)
			: base(parent)
		{
		}

		new LicenceUsageFilter Parent
		{
			get { return (LicenceUsageFilter)base.Parent; }
		}

		#region Validate Property1

		protected override void CheckProperty1()
		{
			base.CheckProperty1();
			if ((Parent.IsPropertySearchUsingSpecifiedDateRange || Parent.IsPropertySearchUsingSpecifiedDateTimeRange) && Parent.IsDateEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.Property1Info);
			}
		}

		#endregion

		#region Validate Property2

		protected override void CheckProperty2()
		{
			base.CheckProperty2();
			if ((Parent.IsPropertySearchUsingSpecifiedDateRange || Parent.IsPropertySearchUsingSpecifiedDateTimeRange) && Parent.IsDateEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.Property2Info);
			}
		}

		#endregion

		#region Validate HeaderCode

		public void ValidatePriceHeaderCode()
		{
			ValidateCalculatedProperty(Parent.PriceHeaderCodeInfo);
		}

		protected void CheckPriceHeaderCode()
		{
			if (Parent.IsBilledFilter)
			{
				MandatoryValidation.CheckEntered(Parent.PriceHeaderCodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.PriceHeaderCodeInfo);
			}
		}

		public void ValidatePriceItemCode()
		{
			ValidateCalculatedProperty(Parent.PriceItemCodeInfo);
		}

		protected void CheckPriceItemCode()
		{
			ListValidation.WarnIfInvalidCode(Parent.PriceItemCodeInfo);
		}

		#endregion

		#region Validate UsageCount

		public void ValidateUsageCount()
		{
			ValidateCalculatedProperty(Parent.UsageCountInfo);
		}

		protected void CheckUsageCount()
		{
			if (Parent.UsageCount < 0)
			{
				Parent.UsageCountInfo.AddError("Must be non-negative");
			}

			if (Parent.UsageCount == 0
				&& Parent.UsageCountComparisonOperator != LicenceUsageFilter.CountComparisonConstants.Exact
				&& Parent.UsageCountComparisonOperator != LicenceUsageFilter.CountComparisonConstants.IsBlank)
			{
				Parent.UsageCountInfo.AddError("Must be equal to or greater than 1");
			}
		}

		#endregion

		#region Validate UsageCountComparisonOperator

		public void ValidateUsageCountComparisonOperator()
		{
			ValidateCalculatedProperty(Parent.UsageCountComparisonOperatorInfo);
		}

		protected void CheckUsageCountComparisonOperator()
		{
			MandatoryValidation.CheckEntered(Parent.UsageCountComparisonOperatorInfo);
			ListValidation.ErrorIfInvalidCode(Parent.UsageCountComparisonOperatorInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePriceHeaderCode();
			ValidatePriceItemCode();
			ValidateUsageCount();
			ValidateUsageCountComparisonOperator();
		}
	}
}
