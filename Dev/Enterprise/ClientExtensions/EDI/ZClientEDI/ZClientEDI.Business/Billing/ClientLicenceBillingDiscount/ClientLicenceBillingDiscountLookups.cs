using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingDiscountLookups : AutoClientLicenceBillingDiscountLookups
	{
		public ClientLicenceBillingDiscountLookups(AutoClientLicenceBillingDiscount parent) : base(parent)
		{
		}

		protected new ClientLicenceBillingDiscount Parent
		{
			get { return (ClientLicenceBillingDiscount)base.Parent; }
		}

		#region System Codes

		public CodeDescriptionPairList SystemCodes
		{
			get { return GetSystemCodes(); }
		}

		public static CodeDescriptionPairList GetSystemCodes()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(BillingConstants.GetBillingSystemList());
			// Discounts not supported for...
			result.RemoveCode(BillingConstants.BillingSystem.Maintenance);
			result.RemoveCode(BillingConstants.BillingSystem.Fee);
			result.RemoveCode(BillingConstants.BillingSystem.STL);

			result.AddPairsIfNotExist(EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.GetPriceListCodeDescriptionPairList().OfType<ICodeDescription>());
			return result;
		}

		#endregion

		#region Discount Type

		public CodeDescriptionPairList DiscountTypes
		{
			get { return BillingConstants.GetDiscountTypeList(); }
		}

		#endregion

		#region Module Codes

		public ReadOnlyCodeDescriptionPairList ModuleCodeList
		{
			get
			{
				return Parent.L5_SystemCode == BillingConstants.BillingSystem.BorderWise
					? BillingConstants.BorderWise.GetCachedBorderWiseModuleList(Factory)
					: LicenceModuleList.Instance.Names;
			}
		}

		#endregion

		#region Discount Break Units

		public CodeDescriptionPairList DiscountBreakUnits
		{
			get { return BillingConstants.GetDiscountBreakUnitList(); }
		}

		#endregion

		#region Sub Codes

		public CodeDescriptionPairList SubCodes
		{
			get { return GetSubCodes(Parent.L5_SystemCode, Factory); }
		}

		internal static CodeDescriptionPairList GetSubCodes(ZString systemCode, BusinessObjectFactory factory)
		{
			if (systemCode == BillingConstants.BillingSystem.AirlineMessaging)
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("AD1", "BT");
				result.AddPair("AD2", "Delta");
				result.AddPair("AD3", "CCSJ");
				result.AddPair("AD4", "CCN");
				result.AddPair("AD5", "Descartes");
				result.AddPair("AD6", "GLSHK");
				result.AddPair("ADX", "Traxon");
				result.AddPair("ADU", "Other Provider");
				return result;
			}
			else if (systemCode == BillingConstants.BillingSystem.ClientMapping)
			{
				return factory.GetCachedValue("ClientLicenceBillingDiscountLookups.SubCodes.ClientMapping", () =>
				{
					var result = new CodeDescriptionPairList(EDIDataRegistry.Instance.ClientMappingBillingNames.Value);

					var query = new ZQuery(ClientLicencePriceItemSchema.L7_Code, BillingConstants.BillingSystem.ClientMapping);
					var priceItems = factory.Load<ClientLicencePriceItem>(query);
					var interfacesGroupCodes = priceItems.Select(x => x.L7_ParentCode).Distinct().ToDictionary(x => x);
					var interfacesGroupPriceItems = priceItems.Where(x => interfacesGroupCodes.ContainsKey(x.L7_Ref4));

					foreach (var priceItem in interfacesGroupPriceItems)
					{
						result.AddPairIfNotExist(priceItem.L7_Ref4, priceItem.L7_DescriptionLocalized);
					}

					return result;
				});
			}
			else if (systemCode == BillingConstants.BillingSystem.ABMCustoms)
			{
				return new ABMCustomsTransactionTypes();
			}
			else if (systemCode == BillingConstants.BillingSystem.OceanCarrierMessaging)
			{
				return OceanCarrierMessagingBillingSystem.GetCachedOceanCarrierMessageTypes(factory);
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}

		internal static List<string> GetSystemCodesThatHaveSubCodes()
		{
			return new List<string>()
				{
					BillingConstants.BillingSystem.AirlineMessaging,
					BillingConstants.BillingSystem.ClientMapping,
					BillingConstants.BillingSystem.ABMCustoms,
					BillingConstants.BillingSystem.OceanCarrierMessaging
				};
		}

		#endregion
	}
}

