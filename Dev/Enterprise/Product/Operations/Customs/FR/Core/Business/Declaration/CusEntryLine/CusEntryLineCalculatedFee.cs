using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineCalculatedFee : NonPersistentBusinessObject
	{
		public CusEntryLineCalculatedFee(BusinessObjectFactory factory, ZString chargeType, Money money, bool isLineLevel = true)
		{
			this.factory = factory;
			ChargeType = chargeType;
			Money = money;
			IsLineLevel = isLineLevel;
		}
		readonly BusinessObjectFactory factory;

		public ZString Category
		{
			get
			{
				if (category.IsEmpty)
				{
					category = IsAggregatedTransportChargeType
					? Res.GetString("9b862968-e6f4-40e4-8882-810c28ab2b83", "Transport")
					: Res.GetString("5ad54ba6-ac4f-4b21-a3f2-d5fad19b9f57", "Other Fees");
				}

				return category;
			}
			set
			{
				category = value;
			}
		}
		ZString category;

		public bool IsLineLevel { get; set; }
		public ZString ChargeType { get; set; }
		public Money Money { get; set; }
		public ZDecimal Amount => Money.Amount;
		public ZString Currency => Money.Currency?.Code ?? ZString.Empty;
		public ZString CustomsCode
		{
			get
			{
				if (IsAggregatedTransportChargeType)
				{
					return ChargeType;
				}
				else
				{
					return ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.France, RefCusMapTypeList.Codes.ChargeCode, ChargeType, ZDateTime.Today);
				}
			}
		}

		bool IsAggregatedTransportChargeType => ChargeType.ToString().EqualsAny(UniversalReferenceConstants.RefCusCodeList.ChargeType.AK, UniversalReferenceConstants.RefCusCodeList.ChargeType.BA, UniversalReferenceConstants.RefCusCodeList.ChargeType.CA);
	}
}
