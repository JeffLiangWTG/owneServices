using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.Business.Billing.ClientLicencePriceHeader;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicencePriceHeaderCollection : ActiveBusinessObjectCollection<ClientLicencePriceHeader>
	{
		public ClientLicencePriceHeaderCollection(LicenceCompany master)
			: base(master.Factory, master, new ZQuery(), ClientLicencePriceHeaderSchema.L6_LC)
		{
			Master = master;
			ApplySort(ClientLicencePriceHeaderSchema.Constants.L6_ValidFrom, ListSortDirection.Descending);
		}

		protected ClientLicencePriceHeaderCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		readonly LicenceCompany Master;

		public static ClientLicencePriceHeaderCollection CreateAdhocCollection(BusinessObjectFactory factory)
		{
			return new ClientLicencePriceHeaderCollection(factory, new AdhocCollectionRelationship(typeof(ClientLicencePriceHeader)));
		}

		public ClientLicencePriceHeader FindSettings(ClientLicencePriceHeader other)
		{
			foreach (ClientLicencePriceHeader item in this)
			{
				if (item.HasSettings(other))
				{
					return item;
				}
			}

			return null;
		}

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(ClientLicencePriceHeader newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			if (Master != null)
			{
				newElement.L6_LC = Master.PK;
			}
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region Validation

		internal void CheckNoDuplicateSettings()
		{
			for (var i = 0; i < this.Count; i++)
			{
				for (var j = i + 1; j < this.Count; j++)
				{
					if (this[i].HasSettings(this[j]))
					{
						this[i].AddRowError("Duplicate Price List already exists");
						this[j].AddRowError("Duplicate Price List already exists");
					}
				}
			}
		}

		internal void ValidateStandardPricesExist()
		{
			foreach (var header in this)
			{
				if (header.L6_IsStandard)
				{
					if (LicenceCompany.StandardPricesCompany == null)
					{
						header.AddRowError("Standard prices company not found");
					}
					else if (null == LicenceCompany.StandardPricesCompany.PriceHeaders.FindSettings(header))
					{
						header.AddRowError("Standard prices for these settings not found. Please have a pricelist matching to " + LicenceCompany.StandardPricesCompany.Header.OH_Code);
					}
				}
			}
		}

		#endregion

		public FilterableClientLicencePriceHeaderCollection AsFilterable
		{
			get
			{
				Filterable ??= new (this, new ClientLicencePriceHeaderFilterBusinessObject());
				return Filterable;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		FilterableClientLicencePriceHeaderCollection Filterable;
	}
}

