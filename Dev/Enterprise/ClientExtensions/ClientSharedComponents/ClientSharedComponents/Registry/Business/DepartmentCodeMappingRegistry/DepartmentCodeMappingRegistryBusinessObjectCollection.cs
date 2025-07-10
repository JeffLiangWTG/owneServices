using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class DepartmentCodeMappingRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new DepartmentCodeMappingRegistryBusinessObject this[int i]
		{
			get { return (DepartmentCodeMappingRegistryBusinessObject)Elements[i]; }
		}

		public new DepartmentCodeMappingRegistryBusinessObject AddNew()
		{
			return (DepartmentCodeMappingRegistryBusinessObject)base.AddNew();
		}

		public ZString FindCode(ZString code)
		{
			ZString result = ZString.Empty;
			GlbDepartment currentDepartmentCode;
			foreach (DepartmentCodeMappingRegistryBusinessObject element in Elements)
			{
				currentDepartmentCode = element.CurrentCode;
				if (currentDepartmentCode != null && currentDepartmentCode.GE_Code == code)
				{
					result = element.ExternalCode;
					break;
				}
			}
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DepartmentCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DepartmentCodeMappingRegistryBusinessObject();
		}
	}
}
