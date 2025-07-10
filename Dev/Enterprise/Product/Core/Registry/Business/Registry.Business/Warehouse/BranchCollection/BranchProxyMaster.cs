using System;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BranchProxyMaster : RegistryProxyBusinessObjectMaster<BranchProxy>
	{
		#region FindBoxCollection

		protected override IBusinessObjectCollection GetNewFindBoxCollection()
		{
			return (IGlbBranchCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbBranchCollection>(), CurrentFactory);
		}

		#endregion

		#region Elements

		protected override RegistryProxyBusinessObjectCollection<BranchProxy> GetNewCollection()
		{
			return new BranchProxyCollection();
		}

		#endregion

		#region Clone

		protected override RegistryProxyBusinessObjectMaster<BranchProxy> GetNewMaster()
		{
			return new BranchProxyMaster();
		}

		#endregion
	}
}
