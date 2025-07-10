using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICompositeDutyRate : IFourRates
	{
		IFourRates AdditionalDutyRate { get; }//can be null
		ZString CalculationType { get; }
	}

	public static class DutyRateDescriptor
	{
		public static ZString GetDutyRateDescription(ICompositeDutyRate dutyRate)
		{
			ZStringBuilder result = new ZStringBuilder();
			if (dutyRate != null)
			{
				ZString calculationType = dutyRate.CalculationType;

				if (calculationType == Constants.DutyCalcTypes.Free)
				{
					result.Append(dutyRate.CalculationType);
				}
				else if (calculationType == Constants.DutyCalcTypes.Calc)
				{
					result.Append(DutyRateDescriptor.GetSingleDutyRateDescription(dutyRate));
				}
				else if (calculationType == Constants.DutyCalcTypes.Higher || calculationType == Constants.DutyCalcTypes.Lower)
				{
					IFourRates additionalDutyRate = dutyRate.AdditionalDutyRate;

					if (additionalDutyRate != null)
					{
						result.Append(calculationType);
						result.Append(" of ");
						result.Append(DutyRateDescriptor.GetSingleDutyRateDescription(dutyRate));
						result.Append(" or ");
						result.Append(DutyRateDescriptor.GetSingleDutyRateDescription(additionalDutyRate));
					}
					else
					{
						result.Append(DutyRateDescriptor.GetSingleDutyRateDescription(dutyRate));
					}
				}
				else if (calculationType == Constants.DutyCalcTypes.Info)
				{
					result.Append("Information Only");
				}
				else if (calculationType == Constants.DutyCalcTypes.InCalc)
				{
					result.Append("Incalculable, DTY in AddInfo Required");
				}
			}
			return result.ToString();
		}

		internal static ZString GetSingleDutyRateDescription(IFourRates dutyRate)
		{
			ZStringBuilder result = new ZStringBuilder();
			if (dutyRate != null)
			{
				if (dutyRate.CustomsRate != 0)
				{
					result.Append(dutyRate.CustomsRate + "%");
				}
				if (dutyRate.FirstQtyRate != 0)
				{
					if (!result.IsEmpty)
					{
						result.Append(" + ");
					}
					result.Append("$");
					result.Append(dutyRate.FirstQtyRate.ToString(5));
					result.Append("/");
					result.Append(dutyRate.FirstUQ);
				}
				if (dutyRate.SecondQtyRate != 0)
				{
					if (!result.IsEmpty)
					{
						result.Append(" + ");
					}
					result.Append("$");
					result.Append(dutyRate.SecondQtyRate.ToString(5));
					result.Append("/");
					result.Append(dutyRate.SecondUQ);
				}
				if (dutyRate.OtherDutyFactorRate != 0)
				{
					if (!result.IsEmpty)
					{
						result.Append(" + ");
					}
					result.Append(dutyRate.OtherDutyFactorRate + "%");
				}
			}
			return result.ToString();
		}
	}
}
