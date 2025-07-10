using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class TariffWrapper : ITariff
	{
		public TariffWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
			tariff = invoiceLine.UniversalTariff;
		}

		readonly JobComInvoiceLine invoiceLine;
		readonly TariffView tariff;

		ZString ITariff.Code => tariff?.ZZ1_TariffCode ?? ZString.Empty;

		ZString ITariff.Description => tariff?.ZZ1_Description ?? ZString.Empty;

		ZString ITariff.UQ1 => tariff?.ZZ1_ZZ8_UQ1 ?? ZString.Empty;

		ZString ITariff.UQ2 => tariff?.ZZ1_ZZ8_UQ2 ?? ZString.Empty;

		ZString ITariff.UQ3 => GetFormulaUOM(0);

		ZString ITariff.UQ4 => invoiceLine.IsImport ? GetFormulaUOM(1) : ZString.Empty;

		ZString ITariff.UQ5 => ZString.Empty;

		ZString GetFormulaUOM(int index) => tariff?.UnitsOfMeasure
			.Where(x => x.ZZ8_Type == Universal.Constants.UnitOfMeasureTypes.CustomsUOM3Type)
			.OrderBy(x => x.ZZ8_UOM)
			.ElementAtOrDefault(index)?.ZZ8_UOM ?? ZString.Empty;
	}
}
