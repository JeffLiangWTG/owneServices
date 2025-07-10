using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoJobComInvoiceLineValueSetStrategy : IValueSetStrategy
	{
		public AddInfoJobComInvoiceLineValueSetStrategy(AddInfoJobComInvoiceLine addInfoJobComInvoiceLine)
		{
			this.addInfoJobComInvoiceLine = addInfoJobComInvoiceLine;
		}
		readonly AddInfoJobComInvoiceLine addInfoJobComInvoiceLine;

		void IValueSetStrategy.ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			ValueSetCore(valueThatHasChanged, oldValue);
		}

		protected virtual void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case AddInfoJobComInvoiceLine.Schema.ZG_FecDST:
				case AddInfoJobComInvoiceLine.Schema.ZG_CountryOfDestination:
					this.addInfoJobComInvoiceLine.ZG_FecChallengeDST = false;
					break;
			}
		}
	}
}

