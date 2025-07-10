namespace Enterprise.Customs.CA.Business
{
	partial class QueryMessageSubTypes
	{
		public static string Get3CharCode(string subType)
		{
			switch (subType)
			{
				case Codes.CLASSFILE:
					return QueryMessageSubType3CharCodes.Codes.CLASSFILE;
				case Codes.TARIFFCODE:
					return QueryMessageSubType3CharCodes.Codes.TARIFFCODE;
				case Codes.GSTFILE:
					return QueryMessageSubType3CharCodes.Codes.GSTFILE;
				case Codes.EXCISETAX:
					return QueryMessageSubType3CharCodes.Codes.EXCISETAX;
				case Codes.QRCLASSTAR:
					return QueryMessageSubType3CharCodes.Codes.QRCLASSTAR;
				case Codes.EXCHANGERATE:
					return QueryMessageSubType3CharCodes.Codes.EXCHANGERATE;
				case Codes.QREXCHANGE:
					return QueryMessageSubType3CharCodes.Codes.QREXCHANGE;
				case Codes.BROADCAST:
					return QueryMessageSubType3CharCodes.Codes.BROADCAST;
				default:
					return string.Empty;
			}
		}
	}
}
