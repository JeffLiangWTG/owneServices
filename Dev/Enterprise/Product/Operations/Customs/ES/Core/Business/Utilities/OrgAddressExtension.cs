using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public static class OrgAddressExtension
	{
		public static ZString GetEOROrNIFCode(this OrgAddress address)
		{
			var eorOrnifCode = address.GetEuIdentificationNumber(EconomicGroupList.Codes.EuropeanUnion);
			return eorOrnifCode.IsEmpty ? address.GetEuIdentificationNumber() : eorOrnifCode;
		}
	}
}
