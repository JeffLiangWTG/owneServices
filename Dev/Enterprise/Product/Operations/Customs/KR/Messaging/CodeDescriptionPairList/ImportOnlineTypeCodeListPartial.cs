using CargoWise.Types;
namespace Enterprise.Customs.KR.Messaging
{
	partial class ImportOnlineTypeCodeList
	{
		public static bool IsDistributorRequired(ZString value)
		{
			return value == ImportOnlineTypeCodeList.Codes.B || value == ImportOnlineTypeCodeList.Codes.C;
		}
	}
}
