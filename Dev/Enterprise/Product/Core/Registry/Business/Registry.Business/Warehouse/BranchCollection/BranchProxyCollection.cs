using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.Business
{
	[ModuleID(ModuleId.GlbBranch)]
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BranchProxyCollection : RegistryProxyBusinessObjectCollection<BranchProxy>
	{
		public BranchProxyCollection()
			: base(null, null)
		{
		}

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchProxyCollection();
		}

		#endregion
	}
}
