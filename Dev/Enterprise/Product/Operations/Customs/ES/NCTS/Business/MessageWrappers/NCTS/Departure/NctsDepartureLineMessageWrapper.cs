using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NctsDepartureLineMessageWrapper : INctsLineMessageProvider
	{
		public NctsDepartureLineMessageWrapper(NctsDepartureCargoDesc goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		protected readonly NctsDepartureCargoDesc goodsItem;

		public ZInt GoodsItemNumber => goodsItem.BY_LineNo;
		public ZString GoodsCustomsProcedureCategory1 => goodsItem.BY_HarmonisedTariff;

		public ZString GoodsDescription => goodsItem.BY_Description.IsEmpty ? goodsItem.TariffDescription : goodsItem.BY_Description;
		public ZDecimal GrossWeightInKG
		{
			get
			{
				var result = goodsItem.GrossMassInKilograms;
				return result > 0 && result <= 1 ? 1 : Math.Ceiling(result);
			}
		}

		public IExternalPackagesInfoCommon ExternalPackages => externalPackages ?? (externalPackages = new ExternalPackagesInfoCommonWrapper(goodsItem.ContainersSelected));
		ExternalPackagesInfoCommonWrapper externalPackages;
	}
}
