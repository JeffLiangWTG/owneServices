using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicencePriceItemCollection : ActiveBusinessObjectCollection<ClientLicencePriceItem>
	{
		public ClientLicencePriceItemCollection(ClientLicencePriceHeader master)
			: base(master.Factory, master, new ZQuery(), ClientLicencePriceItemSchema.L7_L6)
		{
			Master = master;
			ApplySort(ClientLicencePriceItemSchema.L7_Order.Name, System.ComponentModel.ListSortDirection.Ascending);
		}

		public readonly ClientLicencePriceHeader Master;

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(ClientLicencePriceItem newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.L7_L6 = Master.PK;
		}

		#endregion

		public ClientLicencePriceItem FindByCode(string code)
		{
			return this.FirstOrDefault(x => x.L7_Code == code);
		}

		public IEnumerable<ClientLicencePriceItem> FindAllByCode(string code)
		{
			return this.Where(x => x.L7_Code == code);
		}

		public FilterableClientLicencePriceItemCollection AsFilterable
		{
			get
			{
				Filterable ??= new (this, new ClientLicencePriceItemFilterBusinessObject());
				return Filterable;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		FilterableClientLicencePriceItemCollection Filterable;
	}
}

