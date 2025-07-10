using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CountryDefaultLanguageBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CountryDefaultLanguageBusinessObjectCollection()
			: base()
		{
		}

		public CountryDefaultLanguageBusinessObjectCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public new CountryDefaultLanguageBusinessObject this[int i]
		{
			get { return (CountryDefaultLanguageBusinessObject)Elements[i]; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CountryDefaultLanguageBusinessObjectCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CountryDefaultLanguageBusinessObject(CurrentFactory);
		}

		public new CountryDefaultLanguageBusinessObject AddNew()
		{
			return (CountryDefaultLanguageBusinessObject)base.AddNew();
		}

		protected override bool AllowNewCore => true;

		protected override bool AllowRemoveCore => true;
	}
}
