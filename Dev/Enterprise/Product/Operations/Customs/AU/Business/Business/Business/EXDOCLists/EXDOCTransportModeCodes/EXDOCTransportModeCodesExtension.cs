namespace Enterprise.Customs.AU.Declaration.Business
{
	partial class EXDOCTransportModeCodes
	{
		public static string GetEXDOCCodeFromTransportModeCode(string transportModeCode)
		{
			string result;
			switch (transportModeCode)
			{
				case Core.Constants.TransportModes.Sea:
					result = Codes.Sea;
					break;
				case Core.Constants.TransportModes.Air:
					result = Codes.Air;
					break;
				case Core.Constants.TransportModes.Mail:
					result = Codes.Mail;
					break;
				default:
					result = string.Empty;
					break;
			}
			return result;
		}
	}
}
