using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using UConstants = Enterprise.Customs.CN.Business.Constants.UniversalReferenceConstants;

namespace Enterprise.Customs.CN.Business
{
	public static class TariffViewExtension
	{
		public static ZString InwardSupervisionConditions(this TariffView tariffView)
		{
			return tariffView.GetRequirementsForDisplay(UConstants.CusTariffAttributeName.ImportCUSRequirement);
		}

		public static ZString OutwardSupervisionConditions(this TariffView tariffView)
		{
			return tariffView.GetRequirementsForDisplay(UConstants.CusTariffAttributeName.ExportCUSRequirement);
		}

		public static bool HasSpecialCIQImportRequirement(this TariffView tariffView)
		{
			return tariffView?.GetRequirements(UConstants.CusTariffAttributeName.ImportCIQRequirement).Any(x => x != UConstants.CusTariffAttributeValue.StandardCIQImportRequirement) ?? false;
		}

		public static bool HasSpecialCIQExportRequirement(this TariffView tariffView)
		{
			return tariffView?.GetRequirements(UConstants.CusTariffAttributeName.ExportCIQRequirement).Any(x => x != UConstants.CusTariffAttributeValue.StandardCIQExportRequirement) ?? false;
		}

		public static bool HasExportCUSRequirementB(this TariffView tariffView)
		{
			return tariffView?.GetRequirements(UConstants.CusTariffAttributeName.ExportCUSRequirement).Contains(UConstants.CusTariffAttributeValue.CUSExportRequirementB) ?? false;
		}

		public static bool HasImportCUSRequirementA(this TariffView tariffView)
		{
			return tariffView?.GetRequirements(UConstants.CusTariffAttributeName.ImportCUSRequirement).Contains(UConstants.CusTariffAttributeValue.CUSImportRequirementA) ?? false;
		}

		public static bool HasImportCIQRequirementL(this TariffView tariffView)
		{
			return tariffView?.GetRequirements(UConstants.CusTariffAttributeName.ImportCIQRequirement).Contains(UConstants.CusTariffAttributeValue.CIQImportRequirementL) ?? false;
		}

		public static bool HasCommodityTypeMED(this TariffView tariffView)
		{
			return tariffView?.HasAttribute(UConstants.CusTariffAttributeName.CommodityType, UConstants.CusTariffAttributeValue.CommodityTypeMED) ?? false;
		}

		public static bool HasCommodityTypeCFCS(this TariffView tariffView)
		{
			return tariffView?.HasAttribute(UConstants.CusTariffAttributeName.CommodityType, UConstants.CusTariffAttributeValue.CommodityTypeCFCS) ?? false;
		}

		public static bool HasCommodityTypeUME(this TariffView tariffView)
		{
			return tariffView?.HasAttribute(UConstants.CusTariffAttributeName.CommodityType, UConstants.CusTariffAttributeValue.CommodityTypeUME) ?? false;
		}

		public static bool HasCommodityTypeATP(this TariffView tariffView)
		{
			return tariffView?.HasAttribute(UConstants.CusTariffAttributeName.CommodityType, UConstants.CusTariffAttributeValue.CommodityTypeATP) ?? false;
		}

		public static bool HasCommodityTypeDGC(this TariffView tariffView)
		{
			return tariffView?.HasAttribute(UConstants.CusTariffAttributeName.CommodityType, UConstants.CusTariffAttributeValue.CommodityTypeDGC) ?? false;
		}

		public static bool DoesNotSupportTSD(this TariffView tariffView)
		{
			return tariffView?.HasAttribute(UConstants.CusTariffAttributeName.SupportsTSD, UConstants.CusTariffAttributeValue.NotSupportsTSD) ?? false;
		}

		public static ZString CIQImportRequirements(this TariffView tariffView)
		{
			return tariffView.GetRequirementsForDisplay(UConstants.CusTariffAttributeName.ImportCIQRequirement);
		}

		public static ZString CIQExportRequirements(this TariffView tariffView)
		{
			return tariffView.GetRequirementsForDisplay(UConstants.CusTariffAttributeName.ExportCIQRequirement);
		}

		static char[] GetRequirements(this TariffView tariffView, string attributeName)
		{
			return tariffView?.Factory.GetCachedValue($"CN|TariffViewExtension|{attributeName}|{tariffView.PK}",
				() => tariffView.GetAttributes(attributeName).SelectMany(x => x.ZZ3_Value.ToString().Cast<char>()).Distinct().OrderBy(x => x).ToArray()) ?? Array.Empty<char>();
		}

		static ZString GetRequirementsForDisplay(this TariffView tariffView, string attributeName)
		{
			return string.Join(DelimiterForAttributeDisplay, tariffView.GetRequirements(attributeName));
		}

		const string DelimiterForAttributeDisplay = ", ";

		#region AdditionalInfo Attributes

		public static IEnumerable<TariffAttributeView> GetSortedAdditionalInfoAttributes(this TariffView tariffView, EnteringOrExiting isEnteringOrExiting)
		{
			return tariffView?.Attributes.Where(x => x.ZZ3_Name.StartsWith(UConstants.CusTariffAttributeName.AdditionalInfomation, StringComparison.OrdinalIgnoreCase))
				.OrderBy(x => x.ZZ3_Name).Where(x => AdditionalElementStrategyProvider.GetAdditionalElementStrategy(x.ZZ3_Value).IsApplicable(isEnteringOrExiting));
		}

		public static IEnumerable<TariffAttributeView> GetSortedGoodsSpecModelAttribues(this TariffView tariffView, EnteringOrExiting isEnteringOrExiting)
		{
			return tariffView?.GetSortedAdditionalInfoAttributes(isEnteringOrExiting).Where(x => x.ZZ3_Value != NameOfGoodsElementStrategy.AdditionalElementCode) ?? Array.Empty<TariffAttributeView>();
		}

		public static int MandatoryGoodsSpecModelAttributesCount(this TariffView tariffView, EnteringOrExiting isEnteringOrExiting)
		{
			return tariffView?.Factory.GetCachedValue($"CN|TariffViewExtension|MandatoryGoodsSpecModelCount|{isEnteringOrExiting}|{tariffView.PK}",
				() => tariffView.GetSortedGoodsSpecModelAttribues(isEnteringOrExiting).Select(x => x.ZZ3_Value).Reverse().SkipWhile(x => !AdditionalElementStrategyProvider.GetAdditionalElementStrategy(x).IsMandatory).Count())
				?? 0;
		}

		#endregion
	}
}
