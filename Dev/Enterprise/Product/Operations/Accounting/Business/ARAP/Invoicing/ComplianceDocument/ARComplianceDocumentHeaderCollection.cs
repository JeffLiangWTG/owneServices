using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business
{
	[ModuleID(ModuleId.ARComplianceDocument)]
	public class ARComplianceDocumentHeaderCollection : AccComplianceDocumentHeaderCollection
	{
		public ARComplianceDocumentHeaderCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}

		public ARComplianceDocumentHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new ARComplianceDocumentHeader this[int index]
		{
			get
			{
				return (ARComplianceDocumentHeader)(Elements[index]);
			}
		}

		public virtual new ARComplianceDocumentHeader AddNew()
		{
			return (ARComplianceDocumentHeader)base.AddNew();
		}
	}
}
