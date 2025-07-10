using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public sealed class AdditionalInfoProvider : IReference
	{
		public static AdditionalInfoProvider NewOrNull(AdditionalInfo additionalInfo) => NewOrNull(null, additionalInfo);

		public static AdditionalInfoProvider NewOrNull(decimal? amount, AdditionalInfo additionalInfo) => additionalInfo == null ? null : new AdditionalInfoProvider(additionalInfo, amount);

		AdditionalInfoProvider(AdditionalInfo additionalInfo, decimal? amount)
		{
			this.additionalInfo = additionalInfo;
			this.amount = amount;
		}
		readonly AdditionalInfo additionalInfo;
		readonly ZDecimal? amount;

		public string FullType => additionalInfo.CSI_Code;

		public string Type => additionalInfo.CSI_Code.Left(4);

		public string Qualifier => additionalInfo.CSI_Code.SubstringSafe(4, 3).ValueOrNullIfEmpty();

		public string ReferenceNumber => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference) ? (string)additionalInfo.CSI_ReferenceNumber : null;

		public string Detail => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail) ? (string)additionalInfo.CSI_ReferenceNumber2 : null;

		public string Complement => additionalInfo.CSI_Description;

		public string Currency => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value) ? (string)additionalInfo.CSI_RX_NKCurrency : null;

		public decimal Amount => (amount ?? additionalInfo.CSI_Value).FormatDecimal(2);

		bool MapProperty(string attributeName)
		{
			bool result;
			if (additionalInfo.Declaration?.IsExport ?? false)
			{
				if (CusCodeType == null)
				{
					result = true; // not TRA or REF
				}
				else
				{
					result = CusCodeType.HasAttribute(attributeName);
				}
			}
			else
			{
				result = true; // Import
			}
			return result;
		}

		ZZRefCusCodeListCombined CusCodeType => CachedValueHelper.GetValue(ref cusCodeType, () =>
		{
			switch (additionalInfo.CSI_SubType)
			{
				case AdditionalDocTypeList.Codes.TransportDocuments:
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(additionalInfo.Factory, additionalInfo.CSI_Code, Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E, CargoWise.Types.ZDateTime.Today);
				case AdditionalDocTypeList.Codes.AdditionalReference:
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(additionalInfo.Factory, additionalInfo.CSI_Code, Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, CargoWise.Types.ZDateTime.Today);
				default:
					return null;
			}
		});
		CachedValue<ZZRefCusCodeListCombined> cusCodeType;
	}
}
