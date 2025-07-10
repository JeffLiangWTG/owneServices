using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public partial class ImportBorderTransportMeansList
	{
		public static bool RequireInformation(ZString btm) => btm == Codes.Other;
	}
}
