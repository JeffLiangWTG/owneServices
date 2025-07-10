using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceHeaderLinkCollection : ActiveBusinessObjectCollection<EdiPriceHeaderLink>
	{
		public EdiPriceHeaderLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiPriceHeaderLinkCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public EdiPriceHeaderLinkCollection(LicenceDatabase master)
			: base(master.Factory, master, null, EdiPriceHeaderLinkSchema.PHL_LD)
		{
		}

		public EdiPriceHeaderLinkCollection(ClientLicencePriceHeader master)
			: base(master.Factory, master, null, EdiPriceHeaderLinkSchema.PHL_L6)
		{
		}

		public EdiPriceHeaderLinkCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public static EdiPriceHeaderLinkCollection CreateAdhocCollection(BusinessObjectFactory factory)
		{
			return new EdiPriceHeaderLinkCollection(factory, new AdhocCollectionRelationship(typeof(EdiPriceHeaderLink)));
		}

		#region Implementation

		protected override bool AllowNew
		{
			get { return EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed; }
		}

		#endregion
	}
}

