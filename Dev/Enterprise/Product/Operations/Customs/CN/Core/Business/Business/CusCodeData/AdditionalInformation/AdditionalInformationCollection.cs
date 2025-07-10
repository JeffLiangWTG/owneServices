using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CN.Business
{
	public class AdditionalInformationCollection : CusCodeDataCollection<AdditionalInformation>, IAdditionalElementCollection
	{
		public AdditionalInformationCollection(CusClassPartPivot parent)
			: base(parent, Constants.CusCodeDataTypes.Codes.AdditionalInformation)
		{
		}

		public AdditionalInformation this[string code]
		{
			get { return this.Cast<AdditionalInformation>().FirstOrDefault(x => x.CY_Code == code); }
		}

		public ZString GetValue(ZString code)
		{
			return this[code]?.CY_Data.Trim() ?? ZString.Empty;
		}

		public void SetValue(ZString code, ZString value)
		{
			var addInfo = this[code];
			var trimmedValue = value.Trim();
			if (addInfo == null && !trimmedValue.IsEmpty)
			{
				addInfo = AddNew();
				addInfo.CY_Code = code;
			}

			if (addInfo != null)
			{
				var oldValue = addInfo.CY_Data;
				if (oldValue != trimmedValue)
				{
					addInfo.CY_Data = trimmedValue.Left(addInfo.CY_DataInfo.MaxLength);
				}
			}
		}

		public void CleanByTariff(TariffView tariff, EnteringOrExiting isEnteringOrExiting)
		{
			var attributes = tariff?.GetSortedAdditionalInfoAttributes(isEnteringOrExiting)?.Select(x => x.ZZ3_Value) ?? new ZString[] { NameOfGoodsElementStrategy.AdditionalElementCode };

			foreach (var additionalInformation in this.Cast<AdditionalInformation>().Where(x => x.CY_Data.IsEmpty || !attributes.Any(attr => attr == x.CY_Code)).ToArray())
			{
				RemoveAndDelete(additionalInformation);
			}
		}
	}
}
