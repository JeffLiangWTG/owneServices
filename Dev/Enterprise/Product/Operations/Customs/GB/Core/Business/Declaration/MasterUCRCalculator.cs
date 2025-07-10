using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class MasterUCRCalculator
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")] // it's really not that complicated
		public string Calculate(IConsolMessagingProvider messagingProvider)
		{
			this.dataSource = messagingProvider;
			BadgeCodeSetting badgeCodeSetting = BadgeCodeGetter.InstanceCachedFor(dataSource).GetFromBadgeCode(dataSource.CustomsProfile, dataSource.MessageType);

			if (badgeCodeSetting != null && !badgeCodeSetting.MasterUcrCalculationMode.IsEmpty)
			{
				if (Registry.MucrGenerationStyles.Codes.CSPExport == badgeCodeSetting.MasterUcrCalculationMode)
				{
					if (dataSource.JobReference.IsEmpty || badgeCodeSetting.CSPCode.IsEmpty)
					{
						return string.Empty;  // If we need the job reference but the job reference is empty, don't calculate.  Otherwise we get a half-baked mucr such as "GB/CUK1-" or "HBAC           00000001" etc.
					}
				}
				else if (Registry.MucrGenerationStyles.Codes.SeaConsol != badgeCodeSetting.MasterUcrCalculationMode
							&& Registry.MucrGenerationStyles.Codes.Courier != badgeCodeSetting.MasterUcrCalculationMode
							&& dataSource.MasterBill.IsEmpty)
				{
					return string.Empty;  // If we need the mawb but the mawb is empty, don't calculate.  Otherwise we get a half-baked mucr such as "GB/CUK1-" or "HBAC           00000001" etc. 
				}

				switch (badgeCodeSetting.MasterUcrCalculationMode)
				{
					case Registry.MucrGenerationStyles.Codes.None:
						return string.Empty;
					case Registry.MucrGenerationStyles.Codes.Air:
						return CalculateAir();
					case Registry.MucrGenerationStyles.Codes.Ccsuk:
						return CalculateCcsuk();
					case Registry.MucrGenerationStyles.Codes.GemsCcsuk:
						return CalculateGems();
					case Registry.MucrGenerationStyles.Codes.Eori:
						return CalculateFromTurn_BillNumber();
					case Registry.MucrGenerationStyles.Codes.SeaConsol:
						return CalculateFromTurn_ConsolNumber();
					case Registry.MucrGenerationStyles.Codes.CSPExport:
						return CalculateCSP(badgeCodeSetting.CSPCode);
					case Registry.MucrGenerationStyles.Codes.Courier:
						return CalculateCourier(badgeCodeSetting.CSPCode);
				}
			}
			return string.Empty;
		}

		string CalculateCourier(string csp)
		{
			if (csp == GatewayList.Codes.CNS_CUSDECOnly)
			{
				var siteId = dataSource.CourierSiteId;
				var carrier = dataSource.CourierCarrierCode;
				var courierReference = dataSource.CourierConsignmentReference;
				if (!new[] { siteId, carrier, courierReference }.Any(x => x.IsEmpty))
				{
					return siteId + carrier + courierReference;
				}
			}
			return ZString.Empty;
		}

		string CalculateCSP(ZString cspCode)
		{
			return string.Format(CultureInfo.InvariantCulture, "GB/{0}-{1}", cspCode, dataSource.JobReference);
		}

		string CalculateGems()
		{
			string srf = "";
			if (!dataSource.HouseSplitReference.IsEmpty && dataSource.IsInventoryControlledAirImport)  //CcsukInv files for gems only generated for imports, and not for IFW
			{
				srf = dataSource.HouseSplitReference;
			}
			return GetAirportPrefixFromSixCharShedCode(GetLocationOfGoodsCode(), dataSource.Factory)
					  + Get3CharShedCode()
					  + GetMawbAndHawbInUkFormat(dataSource.MasterBill, dataSource.HouseBill)
					  + srf;
		}

		ZString GetLocationOfGoodsCode()
		{
			FindGen51();
			return gen51 != null ? gen51.CSI_Description.SubstringSafe(0, 6) : dataSource.SubLocationOfGoods.Trim();
		}

		ZString Get3CharShedCode()
		{
			// If declaration has a Gen51 AI statement with the ETSF code in it, use that. Else use the regular location of goods. 
			FindGen51();
			return (gen51 != null ? gen51.CSI_Description.SubstringSafe(3, 3) : dataSource.SubLocationOfGoods.SubstringSafe(3, 3)).PadRight(3);
		}

		bool hasLookedForGen51Already;
		AdditionalInfo gen51;
		AdditionalInfo FindGen51()
		{
			if (!hasLookedForGen51Already && dataSource.FindGen51Statement)
			{
				gen51 = FindGen51Statement(dataSource);
				hasLookedForGen51Already = true;
			}
			return gen51;
		}

		public static AdditionalInfo FindGen51Statement(IConsolMessagingProvider consolMessagingProvider)
		{
			var addInfos = new List<AdditionalInfo>();
			foreach (JobComInvoiceLine invoiceLine in consolMessagingProvider.GetInvoiceLines())
			{
				addInfos.AddRange(invoiceLine.AdditionalInfos.OfType<AdditionalInfo>());
			}
			return addInfos.FirstOrDefault(a => a.CSI_Code == "GEN51");
		}

		public ZString GetAirportPrefixFromSixCharShedCode(ZString shedCode, BusinessObjectFactory fact)
		{
			Shed shed = null;
			if (shedCode.Length == 6)
			{
				shed = Shed.LoadByCode(fact, Core.Constants.CountryCodes.UnitedKingdom, shedCode);
			}
			var prefix = shed?.ACPCode ?? ZString.Empty;
			return prefix.IsEmpty ? (ZString)" " : prefix;
		}

		string CalculateAir()
		{
			var bill = dataSource.MasterBill;
			return bill.Length == 11 ? CalculateAirExports(bill) : string.Empty;
		}

		public static string CalculateAirExports(ZString masterBillNumber)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", "A", masterBillNumber.Replace("-", ""));
		}

		ZString GetMawbAndHawbInUkFormat(ZString mawb, ZString hawb)
		{
			string hawbFormatted = hawb.IsEmpty ? ZString.Empty.PadRight(8, ' ') : hawb.Right(8).PadLeft(8, '0');
			return mawb.PadRight(11) + hawbFormatted;
		}

		string CalculateCcsuk()
		{
			return CalculateInventorySystem("CUK1", CalculateGems());
		}

		string CalculateInventorySystem(string inventoryId, string reference)
		{
			return string.Format(CultureInfo.InvariantCulture, "GB/{0}-{1}", inventoryId, reference);
		}

		string CalculateFromTurn(ZString reference)
		{
			return CalculateFromTurn(dataSource.Declarant?.Header, reference);
		}

		static string CalculateFromTurn(OrgHeader declarant, ZString reference)
		{
			var result = string.Empty;
			if (declarant != null)
			{
				string turn = declarant.GetEuIdentificationNumber().Replace("GB", "");
				result = string.Format(CultureInfo.InvariantCulture, "GB/{0}-{1}", turn, reference);
			}
			return result;
		}

		string CalculateFromTurn_BillNumber()
		{
			string reference = GetMawbAndHawbInUkFormat(dataSource.MasterBill, dataSource.HouseBill);
			return CalculateFromTurn(reference);
		}

		string CalculateFromTurn_ConsolNumber()
		{
			string reference = dataSource.IsSea && dataSource.RelevantConsol != null ? dataSource.RelevantConsol.JK_UniqueConsignRef :
								GetMawbAndHawbInUkFormat(dataSource.MasterBill, dataSource.HouseBill);
			return CalculateFromTurn(reference);
		}

		public static string CalculateFromTurn_ConsolNumber(ForwardingConsol consol, OrgHeader declarant)
		{
			string reference = consol.JK_UniqueConsignRef;
			return CalculateFromTurn(declarant, reference);
		}
		IConsolMessagingProvider dataSource;
	}
}
