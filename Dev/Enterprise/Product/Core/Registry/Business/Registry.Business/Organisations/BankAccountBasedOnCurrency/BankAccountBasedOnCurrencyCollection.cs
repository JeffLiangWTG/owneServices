using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BankAccountBasedOnCurrencyCollection : RegistryBusinessObjectCollectionTemplate
	{
		public BankAccountBasedOnCurrencyCollection()
		{
		}

		public BankAccountBasedOnCurrencyCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new BankAccountBasedOnCurrency this[int x]
		{
			get { return (BankAccountBasedOnCurrency)Elements[x]; }
		}

		public new BankAccountBasedOnCurrency AddNew()
		{
			return (BankAccountBasedOnCurrency)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BankAccountBasedOnCurrencyCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BankAccountBasedOnCurrency(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
