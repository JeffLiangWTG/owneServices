using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	[ModuleID(ModuleId.SupplierPart)]
	public class EDIOrgSupplierPartCollection : BusinessObjectCollection<OrgSupplierPart>
	{
		public EDIOrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

