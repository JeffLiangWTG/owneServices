using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class ImportPreviousDocumentList
	{
		public static string MapToCustomsCode(ZString code)
		{
			switch (code)
			{
				case Codes.DE:
					return "DE";
				case Codes.DI:
					return "DI";
				case Codes.DEU:
					return "DUE";
				case Codes.DIU:
					return "DUIMP";
				default:
					return string.Empty;
			}
		}
	}
}
