using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CrossTradeDebtorConfigurationCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new CrossTradeDebtorConfiguration this[int i]
		{
			get { return (CrossTradeDebtorConfiguration)Elements[i]; }
		}

		public new CrossTradeDebtorConfiguration AddNew()
		{
			return (CrossTradeDebtorConfiguration)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CrossTradeDebtorConfiguration();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CrossTradeDebtorConfigurationCollection();
		}
	}
}
