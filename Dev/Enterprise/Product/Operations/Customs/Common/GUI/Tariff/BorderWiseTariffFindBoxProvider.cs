using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Common.GUI
{
	public static class BorderWiseTariffFindBoxProvider
	{
		public static IFindBoxPopup GetTariffFindBoxWrapper(BusinessObject businessObject, string bindingPropertyName, Func<IFindBoxPopup> defaultFindBoxPopup)
		{
			return CreateTariffFindBoxPopupWrapper(() => CreateTariffFindBoxWrapperForBorderWise(businessObject, bindingPropertyName), defaultFindBoxPopup);
		}

		public static IFindBoxPopup GetTariffFindBoxWrapper(string tariffType, Func<IFindBoxPopup> defaultFindBoxPopup)
		{
			return CreateTariffFindBoxPopupWrapper(() => CreateTariffFindBoxWrapperForBorderWise(tariffType), defaultFindBoxPopup);
		}

		public static IFindBoxPopup GetTariffFindBoxWrapper(string tariffType, string countryOverride, Func<IFindBoxPopup> defaultFindBoxPopup)
		{
			return CreateTariffFindBoxPopupWrapper(() => CreateTariffFindBoxWrapperForBorderWise(tariffType, countryOverride), defaultFindBoxPopup);
		}

		public static IFindBoxPopup GetTariffFindBoxWrapper(AdditionalDataForBorderWise additionalDataForBorderWise, Func<IFindBoxPopup> defaultFindBoxPopup)
		{
			return CreateTariffFindBoxPopupWrapper(() => CreateTariffFindBoxWrapperForBorderWise(additionalDataForBorderWise), defaultFindBoxPopup);
		}

		public static IFindBoxPopup GetTariffFindBoxWrapper(BorderWiseFilters borderWiseFilters, Func<IFindBoxPopup> defaultFindBoxPopup)
		{
			return CreateTariffFindBoxPopupWrapper(() => CreateTariffFindBoxWrapperForBorderWise(borderWiseFilters), defaultFindBoxPopup);
		}

		static IFindBoxPopup CreateTariffFindBoxPopupWrapper(Func<FindBoxWrapperForBorderWise> createFindBoxWrapperForBorderWise, Func<IFindBoxPopup> defaultFindBoxPopup)
		{
			if (Env.Registry.ExternalBorderComplianceTool == ExternalBorderComplianceToolList.Codes.BorderWiseWeb)
			{
				var findBoxWrapperForBorderWise = createFindBoxWrapperForBorderWise();
				if (findBoxWrapperForBorderWise.BorderWiseLauncher?.Launcher != null)
				{
					return findBoxWrapperForBorderWise;
				}
			}

			if (defaultFindBoxPopup != null)
			{
				return defaultFindBoxPopup();
			}

			return null;
		}

		static FindBoxWrapperForBorderWise CreateTariffFindBoxWrapperForBorderWise(AdditionalDataForBorderWise additionalDataForBorderWise)
		{
			return new FindBoxWrapperForBorderWise(additionalDataForBorderWise);
		}

		static FindBoxWrapperForBorderWise CreateTariffFindBoxWrapperForBorderWise(BusinessObject businessObject, string bindingPropertyName)
		{
			var additionalData = AdditionalDataForBorderWise.GetAdditionalDataFrom(businessObject, bindingPropertyName);
			return new FindBoxWrapperForBorderWise(additionalData);
		}

		static FindBoxWrapperForBorderWise CreateTariffFindBoxWrapperForBorderWise(string tariffType)
		{
			return new FindBoxWrapperForBorderWise(tariffType);
		}

		static FindBoxWrapperForBorderWise CreateTariffFindBoxWrapperForBorderWise(string tariffType, string countryOverride)
		{
			return new FindBoxWrapperForBorderWise(tariffType, countryOverride);
		}

		static FindBoxWrapperForBorderWise CreateTariffFindBoxWrapperForBorderWise(BorderWiseFilters borderWiseFilters)
		{
			return new FindBoxWrapperForBorderWise(borderWiseFilters);
		}
	}
}
