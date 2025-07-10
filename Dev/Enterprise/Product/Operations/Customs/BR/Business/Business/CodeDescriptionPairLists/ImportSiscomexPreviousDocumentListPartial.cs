using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class ImportSiscomexPreviousDocumentList
	{
		public static ZString MapToCustomsCode(ZString code)
		{
			switch (code)
			{
				case Codes.DI:
					return "2";
				case Codes.RE:
					return "3";
				default:
					return ZString.Empty;
			}
		}
	}
}
