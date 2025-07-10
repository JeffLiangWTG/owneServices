using CargoWise.Types;

namespace Enterprise.Customs.GB.Business
{
	public static class GbMessageStatusCalculator
	{
		public static ZString GetMessageAwaitingStatus(Customs.Business.CusdecMessageFunction how)
		{
			switch (how)
			{
				case Customs.Business.CusdecMessageFunction.New _:
				case Customs.Business.CusdecMessageFunction.Amended _:
				case Customs.Business.CusdecMessageFunction.Deleted _:
					return Enterprise.Customs.Common.EU.MessageStatusList.Codes.AwaitingResponse;
				default:
					return ZString.Empty;
			}
		}
	}
}
