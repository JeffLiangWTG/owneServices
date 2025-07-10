using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineValueSetStrategy : IValueSetStrategy
	{
		public JobComInvoiceLineValueSetStrategy()
		{
		}

		#region IValueSetStrategy Members

		void IValueSetStrategy.ValueSet(ZPropertyInfo valueThatHasChanged, CargoWise.Types.IZType oldValue)
		{
			ValueSetCore(valueThatHasChanged, oldValue);
		}

		#endregion

		protected virtual void ValueSetCore(ZPropertyInfo valueThatHasChanged, CargoWise.Types.IZType oldValue)
		{
		}

		public virtual void HandleSettingOfNewValuationMethod()
		{
			// subclasses do this PER COUNTRY
		}
	}
}
