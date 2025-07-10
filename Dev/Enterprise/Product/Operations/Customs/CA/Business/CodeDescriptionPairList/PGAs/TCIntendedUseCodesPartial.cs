using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	partial class TCIntendedUseCodes
	{
		public static ZString ConvertFromOGDCode(string code)
		{
			switch (code)
			{
				case ImportReasonCodes.Codes.Sale:
					return Codes.TC01;
				case ImportReasonCodes.Codes.Export:
					return Codes.TC02;
				case ImportReasonCodes.Codes.NotRegulated:
					return Codes.TC03;
				case ImportReasonCodes.Codes.Retread:
					return Codes.TC04;
				default:
					return string.Empty;
			}
		}
	}
}
