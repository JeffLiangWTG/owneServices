using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class ServiceLevelRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ServiceLevelRegistryBusinessObjectCollection() { }

		public ServiceLevelRegistryBusinessObjectCollection(BusinessObjectFactory factory) : base(factory) { }

		public override void Add(BusinessObject businessObject)
		{
			ServiceLevelRegistryBusinessObject serviceLevel = businessObject as ServiceLevelRegistryBusinessObject;
			if (serviceLevel != null && IsDuplicateItem(serviceLevel))
			{
				throw new RegistryValidationException(serviceLevel.ServiceLevel);
			}
			base.Add(businessObject);
		}

		public new ServiceLevelRegistryBusinessObject this[int index]
		{
			get { return (ServiceLevelRegistryBusinessObject)Elements[index]; }
		}

		public new ServiceLevelRegistryBusinessObject AddNew()
		{
			return (ServiceLevelRegistryBusinessObject)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ServiceLevelRegistryBusinessObjectCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ServiceLevelRegistryBusinessObject(CurrentFactory);
		}

		public bool IsDuplicateItem(ServiceLevelRegistryBusinessObject itemToCheck)
		{
			bool result = false;
			foreach (ServiceLevelRegistryBusinessObject item in this)
			{
				if (item != itemToCheck && item.ServiceLevel == itemToCheck.ServiceLevel)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public IList<ZString> ToIListZString
		{
			get
			{
				UniqueList<ZString> result = new UniqueList<ZString>(10);
				foreach (ServiceLevelRegistryBusinessObject item in this)
				{
					result.Add(item.ServiceLevel);
				}
				return result;
			}
		}
	}
}
