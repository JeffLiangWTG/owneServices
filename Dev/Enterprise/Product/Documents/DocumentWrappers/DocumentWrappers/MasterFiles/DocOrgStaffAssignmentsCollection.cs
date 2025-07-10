using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgStaffAssignmentsCollection : DocumentWrapperCollection
	{
			public DocOrgStaffAssignmentsCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public DocOrgStaffAssignmentsCollection(OrgStaffAssignmentsCollection collectionSource, BusinessObjectFactory factory)
				: base(collectionSource, factory)
			{
			}

			public new DocOrgStaffAssignments this[int index]
			{
				get
				{
					return (DocOrgStaffAssignments)base[index];
				}
			}
	}
}
