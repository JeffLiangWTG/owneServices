using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class GeneralRemarkProvider
	{
		public GeneralRemarkProvider(string generalRemarks)
		{
			GeneralRemarks = generalRemarks;
		}

		public ZString GeneralRemarks;
	}
}
