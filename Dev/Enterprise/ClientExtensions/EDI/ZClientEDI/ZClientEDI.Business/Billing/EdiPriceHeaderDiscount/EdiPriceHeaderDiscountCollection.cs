using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceHeaderDiscountCollection : ActiveBusinessObjectCollection<EdiPriceHeaderDiscount>
	{
		public EdiPriceHeaderDiscountCollection(BusinessObjectFactory factory, string discountListVersion)
			: base(factory, new ZQuery(EdiPriceHeaderDiscountSchema.PHD_Version, discountListVersion))
		{
			this.discountListVersion = discountListVersion;
		}

		public EdiPriceHeaderDiscountCollection(ClientLicencePriceHeader master)
			: this(master.Factory, master.L6_DiscountCode)
		{
			Master = master;
		}

		public EdiPriceHeaderDiscountCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static EdiPriceHeaderDiscountCollection GetCachedValue(BusinessObjectFactory factory, string version)
		{
			return factory.GetCachedValue("EdiPriceHeaderDiscountCollection_" + version,
					() =>
					{
						return new EdiPriceHeaderDiscountCollection(factory, version);
					});
		}

		public static EdiPriceHeaderDiscountCollection GetCachedValue(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EdiPriceHeaderDiscountCollectionAll",
					() =>
					{
						return new EdiPriceHeaderDiscountCollection(factory);
					});
		}

		public string DiscountListVersion { get { return discountListVersion; } }
		readonly string discountListVersion;

		public readonly ClientLicencePriceHeader Master;

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(EdiPriceHeaderDiscount newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.PHD_Version = DiscountListVersion;
		}

		protected override bool AllowNew
		{
			get { return EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed; }
		}

		#endregion
	}
}

