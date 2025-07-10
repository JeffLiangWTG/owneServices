using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class Currency : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T105";
		public ZString CurrencyCode { get; set; }
		public ZString CurrencyName { get; set; }
	}

	public sealed class CurrencyCollection : NonPersistentBusinessObjectCollection<Currency>	{
		public CurrencyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddDefaultElements();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Currency();
		}

		void AddDefaultElements()
		{
			RefCurrencyCollection collection = new RefCurrencyCollection(Factory, new ZQuery(RefCurrencySchema.RX_IsActive, true));
			collection.ApplySort(AutoRefCurrency.Schema.RX_Code, ListSortDirection.Ascending);
			foreach (var refCurrency in collection)
			{
				Currency currency = AddNew();
				currency.CurrencyCode = refCurrency.RX_Code;
				currency.CurrencyName = refCurrency.RX_DescMultilingual.ToString(Constants.Languages.ChineseSimplified);
			}
		}
	}
}

