using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.GB.EMCS.Business.PreValidateTraderInfo;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public static class EMCSPreValidateTraderInfoHelper
	{
		public static IEnumerable<TraderData> GetTraders(EU.EMCS.Business.EMCSJobDeclaration declaration)
		{
			if (GetTraderData(declaration.SupplierDocumentaryAddress, isConsignee: false, out TraderData consignor))
			{
				yield return consignor;
			}

			if (GetTraderData(declaration.ImporterDocumentaryAddress, isConsignee: true, out TraderData consignee))
			{
				yield return consignee;
			}

			if (GetWarehouseData(declaration.DispatchWarehouseDocumentaryAddress, out TraderData dispatchWarehouse))
			{
				yield return dispatchWarehouse;
			}

			if (GetWarehouseData(declaration.DestinationWarehouseDocumentaryAddress, out TraderData destinationWarehouse))
			{
				yield return destinationWarehouse;
			}
		}

		public static IEnumerable<string> GetProductCodes(EU.EMCS.Business.EMCSJobDeclaration declaration)
		{
			return declaration?.InvoiceLines.Cast<EMCSJobComInvoiceLine>()?.Select(il => il.ZG_ExciseProductCode.ToString())?.Distinct()?.OrderBy(c => c);
		}

		public static bool GetTraderData(JobDocAddress address, bool isConsignee, out TraderData trader)
		{
			trader = new TraderData()
			{
				TraderID = isConsignee ? PartyConsigneeProvider.NewOrNull(address)?.TraderId : PartyConsignorProvider.NewOrNull(address)?.TraderExciseNumber,
				TraderType = GetTraderType(address)
			};

			return trader.IsValid;
		}

		public static bool GetWarehouseData(JobDocAddress address, out TraderData warehouse)
		{
			warehouse = new TraderData();
			if (address != null)
			{
				if (address.E2_AddressOverride)
				{
					warehouse.TraderID = address.E2_GovRegNumType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber ? address.E2_GovRegNum.ToString() : string.Empty;
				}
				else
				{
					warehouse.TraderID = address.Organisation.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
				}
				warehouse.TraderType = GetTraderType(address);
			}
			return warehouse.IsValid;
		}

		static string GetTraderType(JobDocAddress address)
		{
			var traderType = string.Empty;
			if (address.E2_AddressOverride)
			{
				traderType = address.E2_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom ? Constants.TraderTypes.UK : Constants.TraderTypes.EU;
			}
			else
			{
				var cusCode = address.Organisation?.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
				if (cusCode != null)
				{
					traderType = cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedKingdom ? Constants.TraderTypes.UK : Constants.TraderTypes.EU;
				}
			}
			return traderType;
		}

		public static class Constants
		{
			public const string PreValidateTraderInfoWarningMessage = "Cannot proceed!\r\n - Please ensure at least one trader of type Consignor, Consignee, Dispatch Warehouse or Destination Warehouse has a valid type TEN registration number.\r\n - Please ensure at least one invoice line has a valid excise product code.";
			public const string PreValidateTraderInfoCaption = "Pre-Validate Trader";

			public static class TraderTypes
			{
				public const string UK = "UK Record";
				public const string EU = "EU Trader";
			}
		}
	}
}
