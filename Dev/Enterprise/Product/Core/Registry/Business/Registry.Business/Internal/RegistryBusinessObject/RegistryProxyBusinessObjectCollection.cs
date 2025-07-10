using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public abstract class RegistryProxyBusinessObjectCollection<T> : RegistryBusinessObjectCollectionTemplate<T>
		where T : RegistryProxyBusinessObject
	{
		protected RegistryProxyBusinessObjectCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region AllowNew

		protected sealed override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region CreateNonPersistentBusinessObject

		protected sealed override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
