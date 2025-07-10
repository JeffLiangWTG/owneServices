using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business
{
	sealed class TariffWrapper : ITariffData, ITariff
	{
		public TariffWrapper(JobComInvoiceLine invoiceLine)
		{
			Argument.NotNull(invoiceLine, nameof(invoiceLine));
			tariff = invoiceLine.UniversalTariff;
		}

		public TariffWrapper(TariffView tariff)
		{
			this.tariff = tariff;
		}
		readonly TariffView tariff;

		ZString ITariffData.TariffCode => tariff?.ZZ1_TariffCode ?? ZString.Empty;

		ZString ITariffData.TariffDescription => tariff?.ZZ1_Description ?? ZString.Empty;

		ZString ITariffData.TariffUnits => tariff?.ZZ1_ZZ8_UQ1 ?? ZString.Empty;

		ZBool ITariffData.ConveyanceIDRequired => tariff?.HasAttribute(Constants.RefCusTariffAttribute.AttributeName.ConveyanceRequired, Constants.RefCusTariffAttribute.AttributeValue.Y) ?? false;

		ZString ITariff.Code => tariff?.ZZ1_TariffCode ?? ZString.Empty;

		ZString ITariff.Description => tariff?.ZZ1_Description ?? ZString.Empty;

		ZString ITariff.UQ1 => tariff?.ZZ1_ZZ8_UQ1 ?? ZString.Empty;

		ZString ITariff.UQ2 => ZString.Empty;

		ZString ITariff.UQ3 => ZString.Empty;

		ZString ITariff.UQ4 => ZString.Empty;

		ZString ITariff.UQ5 => ZString.Empty;
	}
}
