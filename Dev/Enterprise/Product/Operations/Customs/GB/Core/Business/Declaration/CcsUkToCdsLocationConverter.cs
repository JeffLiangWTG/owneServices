using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CcsUkToCdsLocationConverter
	{
		public CcsUkToCdsLocationConverter(ZString location)
		{
			this.location = location;
		}

		public CcsUkToCdsLocationConverter(ZString goodsLocation, ZString shed)
		{
			originalGoodsLocation = goodsLocation;
			location = goodsLocation + shed;
		}

		readonly ZString location;
		readonly ZString originalGoodsLocation;

		public ZString CalculateCdsLocation(BusinessObjectFactory factory, bool useSimpifiedLookup = false)
		{
			var attributeFilter = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CCSUK, JoinCondition.And, new[] { location }) };
			var list = ZZRefCusCodeListCombined.Loader.Load(factory, Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today, attributeFilter);
			var cusCode = list.FirstOrDefault();

			var result = Core.Constants.CountryCodes.UnitedKingdom;

			if (cusCode != null)
			{
				result += cusCode.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility) + cusCode.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.QUALF).PadRight(2) + cusCode.ZZD_Code;
				return result;
			}

			if (cusCode != null || !useSimpifiedLookup)
			{ return result; }
			var query = new ZQuery(ZZRefCusCodeListSchema.ZZD_Code, SQLComparisonOperator.EndsWith, location);
			list = ZZRefCusCodeListCombined.Loader.Load(factory, Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today, query);
			cusCode = list.FirstOrDefault();
			if (cusCode != null)
			{
				result += cusCode.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility) + cusCode.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.QUALF).PadRight(2) + cusCode.ZZD_Code;
				return result;
			}
			else
			{
				return originalGoodsLocation;
			}
		}
	}
}
