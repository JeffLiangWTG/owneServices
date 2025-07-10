using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class FaxPriceLookups : ZLookups
	{
		public FaxPriceLookups(FaxPrice parent)
			: base(parent) { }

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (currencies == null)
				{
					currencies = new RefCurrencyCollection(Factory);
					currencies.ApplySort(RefCurrencySchema.Constants.RX_Code, System.ComponentModel.ListSortDirection.Ascending);
				}
				return currencies;
			}
		}

		RefCurrencyCollection currencies;

		#region Implementation

		protected new FaxPrice Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (FaxPrice)base.Parent; }
		}

		protected override BusinessObjectFactory Factory
		{
			get { return ((ICurrentFactory)Parent).CurrentFactory; }
		}

		#endregion
	}
}

