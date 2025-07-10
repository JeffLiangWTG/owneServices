using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class GoodsItemValidationHelper
	{
		public static void CheckConditionC075(ZPropertyInfo info, NctsAdditionalInfo additionalInformation)
		{
			var countryCode = additionalInformation.CSI_RN_NKCountryCode;
			var csiCode = additionalInformation.CSI_Code;
			var exportFromEc = additionalInformation.CSI_NctsExportFromEC;
			if (csiCode == "DG0" || csiCode == "DG1")
			{
				if ((!exportFromEc && countryCode.IsEmpty)
					|| (exportFromEc && !countryCode.IsEmpty))
				{
					info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C075, Res.GetString("94E32D31-C3A4-4D28-8251-2A08A255F4F1", "'Export from EC' or 'Export from Country' (Box 44) is required")));
				}
			}
			else
			{
				if (exportFromEc || !countryCode.IsEmpty)
				{
					info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C075, Res.GetString("D5344852-C7B5-46CD-8AD5-A2C949B55C72", "'Export from EC' or 'Export from Country' (Box 44) cannot be used")));
				}
			}
		}

		public static ZBool CheckConditionR0507(this NctsDepartureCargoDesc goodsItem, ZPropertyInfo info, Func<NctsCommonCargoDesc, ZString> valueProvider, ZString? humanReadableName = null)
		{
			var nameOfCheckingDifference = humanReadableName.GetValueOrDefault().IsEmpty ? info.HumanReadableName : humanReadableName.Value;
			var value = valueProvider.Invoke(goodsItem);
			if (goodsItem.ValidationDecider is INctsDepartureCargoDescPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleR0507Active
				&& goodsItem.MoveHeader.IsMixedConsignment
				&& !value.IsEmpty)
			{
				var goodsItemCount = goodsItem.Bill?.GoodsItems.Count ?? 0;
				if (goodsItemCount <= 1)
				{
					info.AddMessageError(Res.GetString("6DDFFA6D-3D11-4F5C-9FF7-B7F40055BD51", "[R0507] For Declaration Type = ‘T’ (Mixed transit) there must be at least two Consignment Items with different {0}.", nameOfCheckingDifference));
					return false;
				}

				if (IsSameForAllGoodsItems())
				{
					info.AddMessageError(Res.GetString("A7593BE0-7ECB-4915-B6BC-E79308645358", "[R0507] {0} must be different for at least one of the consignment items.", nameOfCheckingDifference));
					return false;
				}
			}
			return true;

			bool IsSameForAllGoodsItems() => goodsItem.Bill?.IsSameForAllGoodsItems(valueProvider) ?? false;
		}

		static bool IsSameForAllGoodsItems(this NctsBill bill, Func<NctsCommonCargoDesc, ZString> valueProvider) => bill.GoodsItems.AllSame(valueProvider);

		public static void CheckRuleNR0004(this NctsCommonCargoDesc goodsItem, ZPropertyInfo info)
		{
			var commodityCode = goodsItem.BY_HarmonisedTariff;

			if (!commodityCode.IsEmpty && commodityCode.Length != 6 && commodityCode.Length != 8)
			{
				info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.A060, Res.GetString("C90C37F7-D37C-4D61-9B65-0D755EB0419A", "[NR0004] Commodity Code must be 6 or 8 digits of the full commodity code.")));
			}
		}

		public static void CheckRuleNR0055(this NctsCommonCargoDesc goodsItem, ZPropertyInfo info)
		{
			var commodityCode = goodsItem.BY_HarmonisedTariff;
			if (!commodityCode.IsEmpty && commodityCode.Length != 8 && commodityCode.Length != 10)
			{
				var msgError = goodsItem.Header.Configuration.ValidationRuleConfiguration.Messages.NR0055Message;
				info.AddMessageError(msgError);
			}
		}
	}
}
