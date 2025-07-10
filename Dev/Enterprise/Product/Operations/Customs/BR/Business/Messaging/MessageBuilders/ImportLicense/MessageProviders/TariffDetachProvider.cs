using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportLicense.Outgoing;

namespace Enterprise.Customs.BR.Business.ImportLicense
{
	public class TariffDetachProvider : ITariffDetach
	{
		public TariffDetachProvider(TariffDetach invoiceLineTariffDetach)
		{
			this.invoiceLineRefs = Argument.NotNull(invoiceLineTariffDetach, nameof(invoiceLineTariffDetach));
		}

		public static TariffDetachProvider New(TariffDetach invoiceLineTariffDetach) => invoiceLineTariffDetach == null ? null : new TariffDetachProvider(invoiceLineTariffDetach);

		readonly TariffDetach invoiceLineRefs;

		public string TariffDetachCode => invoiceLineRefs.CY_Code;
	}
}
