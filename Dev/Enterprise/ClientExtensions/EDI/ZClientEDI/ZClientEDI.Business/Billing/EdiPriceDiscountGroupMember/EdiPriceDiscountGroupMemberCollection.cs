using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceDiscountGroupMemberCollection : ActiveBusinessObjectCollection<EdiPriceDiscountGroupMember>
	{
		public EdiPriceDiscountGroupMemberCollection(BusinessObjectFactory factory, string discountListVersion)
			: base(factory, new MyCollectionRelationship(typeof(EdiPriceDiscountGroupMember), discountListVersion))
		{
			this.discountListVersion = discountListVersion;
		}

		public EdiPriceDiscountGroupMemberCollection(ClientLicencePriceHeader master)
			: this(master.Factory, master.L6_DiscountCode)
		{
			Master = master;
		}

		public EdiPriceDiscountGroupMemberCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public string DiscountListVersion { get { return discountListVersion; } }
		readonly string discountListVersion;

		public readonly ClientLicencePriceHeader Master;

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(EdiPriceDiscountGroupMember newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			if (DiscountListVersion != null)
			{
				var firstHeaderDiscount = EdiPriceHeaderDiscountCollection.GetCachedValue(Factory, DiscountListVersion).FirstOrDefault();
				if (firstHeaderDiscount != null)
				{
					newElement.PGM_PHD = firstHeaderDiscount.PK;
				}
			}
		}

		#endregion

		class MyCollectionRelationship : CollectionRelationship
		{
			public MyCollectionRelationship(Type elementType, string discountListVersion)
				: base(elementType, BuildFilter(discountListVersion))
			{
				this.discountListVersion = discountListVersion;
			}

			readonly string discountListVersion;

			static ZQuery BuildFilter(string discountListVersion)
			{
				var subSquery = new ZDBOnlySubQuery(typeof(EdiPriceHeaderDiscount), EdiPriceDiscountGroupMemberSchema.PGM_PHD);
				subSquery.AddToFilter(EdiPriceHeaderDiscountSchema.PHD_Version, discountListVersion);

				var query = new ZDBOnlyQuery(typeof(EdiPriceDiscountGroupMember));
				query.AddSubQuery(subSquery, JoinCondition.And);

				return query;
			}

			protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
			{
				var header = ((EdiPriceDiscountGroupMember)businessObject).HeaderDiscount;
				return header != null && header.PHD_Version == discountListVersion;
			}
		}
	}
}

