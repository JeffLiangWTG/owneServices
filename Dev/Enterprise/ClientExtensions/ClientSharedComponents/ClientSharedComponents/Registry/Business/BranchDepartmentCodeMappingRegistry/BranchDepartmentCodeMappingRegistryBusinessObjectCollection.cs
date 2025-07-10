using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class BranchDepartmentCodeMappingRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new BranchDepartmentCodeMappingRegistryBusinessObject this[int i]
		{
			get { return (BranchDepartmentCodeMappingRegistryBusinessObject)Elements[i]; }
		}

		public new BranchDepartmentCodeMappingRegistryBusinessObject AddNew()
		{
			return (BranchDepartmentCodeMappingRegistryBusinessObject)base.AddNew();
		}

		public ZString FindProfitCentre(ZString branchCode, ZString departmentCode)
		{
			BranchDepartmentCodeMappingRegistryBusinessObject element = FindElement(branchCode, departmentCode);
			return element != null ? element.ProfitCentre : ZString.Empty;
		}

		public ZString FindNominalDepartmentCentre(ZString branchCode, ZString departmentCode)
		{
			BranchDepartmentCodeMappingRegistryBusinessObject element = FindElement(branchCode, departmentCode);
			return element != null ? element.NominalDepartment : ZString.Empty;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchDepartmentCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BranchDepartmentCodeMappingRegistryBusinessObject();
		}

		BranchDepartmentCodeMappingRegistryBusinessObject FindElement(ZString branchCode, ZString departmentCode)
		{
			BranchDepartmentCodeMappingRegistryBusinessObject result = null;
			if (branchCode.IsEmpty || departmentCode.IsEmpty)
			{
				return result;
			}

			GlbBranch currentBranch;
			GlbDepartment currentDepartment;
			foreach (BranchDepartmentCodeMappingRegistryBusinessObject element in Elements)
			{
				currentBranch = element.CurrentBranch;
				currentDepartment = element.CurrentDepartment;
				if (currentBranch != null && currentDepartment != null && currentBranch.GB_Code == branchCode && currentDepartment.GE_Code == departmentCode)
				{
					result = element;
					break;
				}
			}
			return result;
		}
	}
}
