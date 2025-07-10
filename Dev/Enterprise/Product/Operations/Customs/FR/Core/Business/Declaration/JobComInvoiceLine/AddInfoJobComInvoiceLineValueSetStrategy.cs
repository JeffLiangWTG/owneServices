using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AddInfoJobComInvoiceLineValueSetStrategy : EU.Business.Declaration.AddInfoJobComInvoiceLineValueSetStrategy
	{
		public AddInfoJobComInvoiceLineValueSetStrategy(AddInfoJobComInvoiceLine addInfoJobComInvoiceLine)
			: base(addInfoJobComInvoiceLine)
		{
			this.addInfoJobComInvoiceLine = addInfoJobComInvoiceLine;
		}
		readonly AddInfoJobComInvoiceLine addInfoJobComInvoiceLine;

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);

			switch (valueThatHasChanged.Name)
			{
				case AddInfoJobComInvoiceLine.Schema.ZG_BypassCode:
					TariffBypassCodeChanged();
					break;
			}
		}

		void TariffBypassCodeChanged()
		{
			addInfoJobComInvoiceLine.ZG_BypassReason = ZString.Empty;
		}
	}
}
