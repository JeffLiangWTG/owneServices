using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NotificationUnloadingNCTS5CommodityWrapper : NCTS5CommonCommodityWithCusCodeWrapper, INotifUnloadingCommodity
	{
		public NotificationUnloadingNCTS5CommodityWrapper(NctsArrivalCargoDesc item) : base(item)
		{
			arrivalItem = item;
			isUnloadedStateNEW = arrivalItem.BY_UnloadedState.IsUnloadingStateNEW();
		}

		readonly NctsArrivalCargoDesc arrivalItem;
		readonly ZBool isUnloadedStateNEW;

		public INCTSCommonGoodsMeasure GoodsMeasure => goodsMeasure ?? (goodsMeasure = IsUnloadingStateNEWorGoodsMeasureHasDifferences ? new NCTS5CommonGoodsMeasureWrapper(arrivalItem) : null);
		NCTS5CommonGoodsMeasureWrapper goodsMeasure;

		bool IsUnloadingStateNEWorGoodsMeasureHasDifferences => isUnloadedStateNEW
			|| arrivalItem.HasNctsArrivalGoodsMeasureDifferences();

		protected override ZString CusCodeCore
		{
			get
			{
				var codeDeclared = arrivalItem.BY_CusC4Number;
				var codeUnloaded = arrivalItem.UnloadedGoodsItem?.BY_CusC4Number ?? ZString.Empty;
				return isUnloadedStateNEW
											? codeDeclared
											: codeDeclared != codeUnloaded
												? codeUnloaded
												: ZString.Empty;
			}
		}

		protected override ZString DescriptionOfGoodsCore
		{
			get
			{
				var codeDeclared = arrivalItem.BY_Description.SubstringSafe(0, DescriptionMaxLength);
				var codeUnloaded = arrivalItem.UnloadedGoodsItem?.BY_Description.SubstringSafe(0, DescriptionMaxLength) ?? ZString.Empty;
				return isUnloadedStateNEW
											? codeDeclared
											: codeDeclared != codeUnloaded
												? codeUnloaded
												: ZString.Empty;
			}
		}

		protected override NCTS5CommonCommodityCodeWrapper GetCommodityCode()
		{
			var harmonizedLength = 6;
			var combinedLength = 2;

			var tariffCodeDeclared = arrivalItem.BY_HarmonisedTariff;
			var harmonizedCodeDeclared = tariffCodeDeclared.SubstringSafe(0, tariffCodeDeclared.Length < harmonizedLength ? tariffCodeDeclared.Length : harmonizedLength);
			var combinedCodeDeclared = tariffCodeDeclared.SubstringSafe(harmonizedLength, combinedLength);

			var tariffCodeUnloaded = arrivalItem.UnloadedGoodsItem?.BY_HarmonisedTariff ?? ZString.Empty;
			var harmonizedCodeUnloaded = tariffCodeUnloaded.SubstringSafe(0, tariffCodeUnloaded.Length < harmonizedLength ? tariffCodeUnloaded.Length : harmonizedLength);
			var combinedCodeUnloaded = tariffCodeUnloaded.SubstringSafe(harmonizedLength, combinedLength);

			var harmonizedCodeToDeclare = isUnloadedStateNEW
														? harmonizedCodeDeclared
														: IsTariffUnloadedDifferent()
															? harmonizedCodeUnloaded
															: ZString.Empty;
			var combinedCodeToDeclare = isUnloadedStateNEW
														? combinedCodeDeclared
														: IsTariffUnloadedDifferent()
															? combinedCodeUnloaded
															: ZString.Empty;
			return harmonizedCodeToDeclare.IsEmpty && combinedCodeToDeclare.IsEmpty ? null : new NCTS5CommonCommodityCodeWrapper(harmonizedCodeToDeclare, combinedCodeToDeclare);

			bool IsTariffUnloadedDifferent() => tariffCodeDeclared != tariffCodeUnloaded;
		}
	}
}
