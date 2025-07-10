namespace Enterprise.Customs.CN.Business
{
	public partial class CNTransportModeList
	{
		public static string GetCorrespondingTransportCode(string transportMode)
		{
			var transportCode = string.Empty;
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					transportCode = Codes.Air;
					break;
				case Core.Constants.TransportModes.Sea:
					transportCode = Codes.Sea;
					break;
				case Core.Constants.TransportModes.Mail:
					transportCode = Codes.Mail;
					break;
				case Core.Constants.TransportModes.Road:
					transportCode = Codes.Road;
					break;
				case Core.Constants.TransportModes.Rail:
					transportCode = Codes.Rail;
					break;
				case Core.Constants.TransportModes.FixedTransportInstallations:
					transportCode = Codes.FixedTransportInstallations;
					break;
				case TransportTypeList.Codes.PassengerCarried:
					transportCode = Codes.PassengerCarried;
					break;
			}
			return transportCode;
		}
	}
}
