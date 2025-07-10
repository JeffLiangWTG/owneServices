namespace Enterprise.Customs.CA.Business
{
	using CargoWise.Types;
	using Enterprise.Customs.Common.CA;

	/// <summary>
	/// Pars the rate string sent in a message and determine exemption code, rate and rate type where appropriate.
	/// e.g.
	/// 5.0  is a % (V) rate
	/// 5.00 (or more decimals) is a specific rate (S)
	/// 50 with no decimals is an exemption code
	/// zero rate with amount then type X
	/// zero rate and zero amount type F
	/// </summary>
	public class ParsedRateCodeDetails
	{
		public ParsedRateCodeDetails(ZString rateString, ZDecimal amount)
		{
			this.Amount = amount;
			ParsRateString(rateString);
		}

		public ZString RateType { get; private set; }
		public ZString ExecemptionCode { get; private set; }
		public ZString Code { get; private set; }
		public ZDecimal Rate { get; private set; }
		public ZDecimal Amount { get; private set; }

		void ParsRateString(ZString rateString)
		{
			ZDecimal rate;
			ZDecimal.TryParse(rateString, out rate);
			Rate = rate;
			if (rate == 0)
			{
				RateType = Amount != ZDecimal.Zero ? RateTypes.Codes.AcceptX :
					(rateString.IsEmpty ? string.Empty : RateTypes.Codes.Free);
			}
			else
			{
				var rateStringParts = rateString.Trim().Split('.');
				if (rateStringParts.Length != 2)
				{
					Rate = ZDecimal.Zero;
					ExecemptionCode = rateString;
					RateType = RateTypes.Codes.Exempt;
				}
				else
				{
					RateType = rateStringParts[1].Length > 1 ? RateTypes.Codes.Specific : RateTypes.Codes.AdValorem;
				}
			}
		}
	}
}
