using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class BranchCodeMappingRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new BranchCodeMappingRegistryBusinessObject this[int i]
		{
			get { return (BranchCodeMappingRegistryBusinessObject)Elements[i]; }
		}

		public new BranchCodeMappingRegistryBusinessObject AddNew()
		{
			return (BranchCodeMappingRegistryBusinessObject)base.AddNew();
		}

		public ZString FindCode(ZString code)
		{
			ZString result = ZString.Empty;
			GlbBranch currentBranchCode;
			foreach (BranchCodeMappingRegistryBusinessObject element in Elements)
			{
				currentBranchCode = element.CurrentCode;
				if (currentBranchCode != null && currentBranchCode.GB_Code == code)
				{
					result = element.ExternalCode;
					break;
				}
			}
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BranchCodeMappingRegistryBusinessObject();
		}
	}
}
