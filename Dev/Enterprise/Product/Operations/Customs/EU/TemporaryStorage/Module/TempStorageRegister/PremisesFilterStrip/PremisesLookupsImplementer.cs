using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class PremisesLookupsImplementer : NonPersistentBusinessObject
	{
		public PremisesLookupsImplementer(BusinessObjectFactory factory)
		: base(factory)
		{
		}

		static PremisesLookupsImplementer New(BusinessObjectFactory factory)
		{
			var typeForBinding = TypeDecider.GetTypeForBinding(typeof(PremisesLookupsImplementer));
			var args = new BusinessObjectFactory[1] { factory };
			return (PremisesLookupsImplementer)Activator.CreateInstance(typeForBinding, args);
		}

		public static PremisesLookupsImplementer Get(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PremisesLookupsImplementer", () => New(factory));
		}

		public ICollection CustomsLocationList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(base.Factory,
								GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
								new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType },
								ZDateTime.Today,
								System.Array.Empty<RefCusCodeListAttributeFilter>(),
								includeParentDataGroupings: false);
	}
}
