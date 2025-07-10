using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	static class NCTSProviderHelpers
	{
		internal static string GetSecurity(ZString typeOfSecurity)
		{
			switch (typeOfSecurity)
			{
				case NctsTypeOfSecurityList.Codes.NON:
					return "0";
				case NctsTypeOfSecurityList.Codes.ENT:
					return "1";
				case NctsTypeOfSecurityList.Codes.EXI:
					return "2";
				case NctsTypeOfSecurityList.Codes.BTH:
					return "3";
				default:
					return null;
			}
		}

		internal static int FindGoodsItemWithMainPack(NctsPackage package)
		{
			var result = 0;
			if (package.Parent is NctsDepartureCargoDesc cargoDesc && cargoDesc.MoveHeaderOrBillParent is NctsBill bill)
			{
				result = bill.GoodsItems.FirstOrDefault(g => g.BY_IsMainPack && g.Packages.Cast<NctsPackage>().Any(p => p.B5_MarksAndNumbers == package.B5_MarksAndNumbers))?.BY_LineNo ?? 0;
			}
			return result;
		}

		internal static bool UnloadedStatusInNewMisDif(ZString unloadedStatus) => unloadedStatus.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS, NctsUnloadedStateList.Codes.DIF });

		internal static bool UnloadedStatusInNewMis(ZString unloadedStatus) => unloadedStatus.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS });

		internal static bool UnloadedStatusInNewDif(ZString unloadedStatus) => unloadedStatus.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF });

		internal static bool IsDEMRN(string input) => input != null && isDEMRN.IsMatch(input);

		internal static RefExchangeRate GetEffectiveExchangeRate(ZString currencyCode, BusinessObjectFactory factory)
		{
			return new RefExchangeRate.Loader(factory).GetEffectiveRateOn(ZDate.Today, currencyCode, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK);
		}

		static readonly Regex isDEMRN = new Regex(@"^\d{2}DE"); // starts with 2 digits, followed by "DE"
	}
}
