using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class BranchGroupSettingsCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new BranchGroupSettings this[int i]
		{
			get { return (BranchGroupSettings)Elements[i]; }
		}

		public new BranchGroupSettings AddNew()
		{
			return (BranchGroupSettings)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BranchGroupSettings();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchGroupSettingsCollection();
		}
	}
}