using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class PreferentialTariffTreatmentOnDestinationStrategy : CommonAdditionalElementStrategy
	{
		public static string AdditionalElementCode => "00069"; // 出口享惠情况

		public override bool ProvideList => true;

		public override bool IsMergeKey => true;

		public override ZString GetDefaultValue(EnteringOrExiting isEnteringOrExiting)
		{
			return isEnteringOrExiting == EnteringOrExiting.Entering ? PreferentialTariffTreatmentOnDestinationTypeList.Codes._3 : string.Empty;
		}

		public override ICodeDescriptionPairList GetList(BusinessObjectFactory factory, EnteringOrExiting enteringOrExiting)
		{
			ICodeDescriptionPairList result = null;
			switch (enteringOrExiting)
			{
				case EnteringOrExiting.Entering:
					result = factory.GetCachedValue("CNPreferentialTariffTreatmentOnDestinationTypeList_Entering", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(PreferentialTariffTreatmentOnDestinationTypeList.Codes._3, PreferentialTariffTreatmentOnDestinationTypeList.Descriptions._3);
						return list;
					});
					break;
				case EnteringOrExiting.Exiting:
					result = factory.GetCachedValue("CNPreferentialTariffTreatmentOnDestinationTypeList_Exiting", () =>
					{
						var list = new PreferentialTariffTreatmentOnDestinationTypeList();
						list.RemoveCode(PreferentialTariffTreatmentOnDestinationTypeList.Codes._3);
						return list;
					});
					break;
				default:
					result = factory.GetCachedValue<PreferentialTariffTreatmentOnDestinationTypeList>();
					break;
			}
			return result;
		}

		public override void ValidateAdditionalElement(IAdditionalInformationWrapperParent master, ZString elementValue, ZPropertyInfo propertyInfo)
		{
			var countryOfOrigin = master.CountryOfOrigin;

			if (elementValue == PreferentialTariffTreatmentOnDestinationTypeList.Codes._1 && !countryOfOrigin.IsEmpty && countryOfOrigin != Core.Constants.CountryCodes.China)
			{
				propertyInfo.AddNotification(Res.GetString("3846a184-0c9e-44bb-a566-9fd0f3b38b87", "{0} is only applicable for Goods originating in China.", PreferentialTariffTreatmentOnDestinationTypeList.Descriptions._1), master);
			}
		}
	}
}
