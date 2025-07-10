using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class Lookups
	{
		public Lookups(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;

		#endregion

		#region Lists

		public RefUNLOCOCollection Unlocos
		{
			get { return unlocoCollection ?? (unlocoCollection = new RefUNLOCOCollection(Factory)); }
		}

		RefUNLOCOCollection unlocoCollection;

		public CodeDescriptionPairList WeightUnits
		{
			get { return weightUnits ?? (weightUnits = new CodeDescriptionPairList(OLookUpEditType.Weight)); }
		}

		CodeDescriptionPairList weightUnits;

		public CodeDescriptionPairList VolumeUnits
		{
			get { return volumeUnits ?? (volumeUnits = new CodeDescriptionPairList(OLookUpEditType.Volume)); }
		}

		CodeDescriptionPairList volumeUnits;

		public RefPackTypeCollection PackTypes
		{
			get { return packTypes ?? (packTypes = new RefPackTypeCollection(Factory)); }
		}

		RefPackTypeCollection packTypes;

		public RefCurrencyCollection Currency
		{
			get { return currency ?? (currency = new RefCurrencyCollection(Factory)); }
		}

		RefCurrencyCollection currency;

		#endregion
	}
}