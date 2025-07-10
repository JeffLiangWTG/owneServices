
namespace Enterprise.Customs.BR.Business
{
	public partial class RiskChannelList
	{
		public static string GetRiskChannelValue(string brChannel)
		{
			switch (brChannel)
			{
				case Constants.RiskChannelTypes.Green:
					return RiskChannelList.Codes.Green;
				case Constants.RiskChannelTypes.Yellow:
					return RiskChannelList.Codes.Yellow;
				case Constants.RiskChannelTypes.Red:
					return RiskChannelList.Codes.Red;
				case Constants.RiskChannelTypes.Gray:
					return RiskChannelList.Codes.Gray;
				case Constants.RiskChannelTypes.Orange:
					return RiskChannelList.Codes.Orange;
			}
			return string.Empty;
		}
	}
}
