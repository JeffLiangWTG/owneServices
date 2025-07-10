using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocLandedCostInput : DocBaseWrapper
	{
		internal DocLandedCostInput(LandCostInput lCInput, BusinessObjectFactory factoryToWrap)
			: base(lCInput, factoryToWrap)
		{
		}

		public static DocLandedCostInput New(LandCostInput lCInput, BusinessObjectFactory factoryForWrapper)
		{
			if (lCInput == null)
			{
				return null;
			}
			else
			{
				return new DocLandedCostInput(lCInput, factoryForWrapper);
			}
		}

		#region ZString Fields

		public ZString ChargeCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (LCInput.ChargeCode != null)
				{
					result = LCInput.ChargeCode.AC_Code;
				}
				return result;
			}
		}

		public ZString ChargeDescription
		{
			get { return LCInput.LI_ChargeDescription; }
		}

		public ZDecimal CostAmountInLocalCurrency
		{
			get { return RoundingHelper.Round(LCInput.CostAmountInLocalCurrency, LCInput.Header.LocalCurrencyDecimals); }
		}

		public ZDecimal LCGroup1Amount
		{
			get { return RoundingHelper.Round(LCInput.Group1AmountInLocalCurrency, LCInput.Header.LocalCurrencyDecimals); }
		}

		public ZDecimal LCGroup2Amount
		{
			get { return RoundingHelper.Round(LCInput.Group2AmountInLocalCurrency, LCInput.Header.LocalCurrencyDecimals); }
		}

		public ZDecimal LCGroup3Amount
		{
			get { return RoundingHelper.Round(LCInput.Group3AmountInLocalCurrency, LCInput.Header.LocalCurrencyDecimals); }
		}

		public ZDecimal LCGroup4Amount
		{
			get { return RoundingHelper.Round(LCInput.Group4AmountInLocalCurrency, LCInput.Header.LocalCurrencyDecimals); }
		}

		public ZDecimal LCGroup5Amount
		{
			get { return RoundingHelper.Round(LCInput.Group5AmountInLocalCurrency, LCInput.Header.LocalCurrencyDecimals); }
		}

		public ZDecimal LCGroup6Amount
		{
			get { return RoundingHelper.Round(LCInput.Group6AmountInLocalCurrency, LCInput.Header.LocalCurrencyDecimals); }
		}

		public ZDecimal LCGroupMiscAmount
		{
			get { return RoundingHelper.Round(LCInput.GroupMiscAmountInLocalCurrency, LCInput.Header.LocalCurrencyDecimals); }
		}

		public ZString DistributionBy
		{
			get { return LCInput.DistributionByDescription; }
		}

		public ZString DistributionLevel
		{
			get { return LCInput.Parent == null ? ZString.Empty : LCInput.Parent.UniqueCode; }
		}

		#endregion

		#region Implementation

		internal RoundingHelper RoundingHelper
		{
			get { return LCInput.Header.RoundingHelper; }
		}

		LandCostInput LCInput
		{
			get { return (LandCostInput)WrappedObject; }
		}

		#endregion
	}
}
