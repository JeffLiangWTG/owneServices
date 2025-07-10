using CargoWise.Types;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public abstract class EUImportInvoiceLineUserControlTest<T> : Customs.GUI.Testing.BaseInvoiceLineUserControlForVirtualPropertiesTest<T>
		where T : EUImportInvoiceLineUserControl, new()
	{
		protected override ZString DefaultUniversalTariffType => Universal.Constants.TariffTypes.Import;
	}
}
