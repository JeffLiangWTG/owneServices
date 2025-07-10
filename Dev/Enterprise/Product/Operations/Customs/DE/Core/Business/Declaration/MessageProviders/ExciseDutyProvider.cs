using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ExciseDutyProvider : IExciseDuty
	{
		public ExciseDutyProvider(CusLineTariffDetail tariff)
		{
			this.tariff = Argument.NotNull(tariff, nameof(tariff));
		}
		readonly CusLineTariffDetail tariff;

		public string Code => tariff.BZ_Tariff;

		public decimal DegreePercentage => tariff.BZ_PercentAlcohol.FormatDecimal(2);

		public decimal Value => tariff.ExciseValue.Normalize();

		public IAmount Amount => CachedValueHelper.GetValue(ref amount, () => tariff.BZ_Type == "EXC" ? new AmountProvider(tariff.BZ_Qty1, tariff.BZ_UQ1) : null);
		CachedValue<IAmount> amount;
	}
}
