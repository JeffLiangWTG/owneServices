using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business
{
	[ModuleID(ModuleId.APComplianceDocument)]
	public class APComplianceDocumentHeaderCollection : AccComplianceDocumentHeaderCollection
	{
		public APComplianceDocumentHeaderCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}
		public APComplianceDocumentHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new APComplianceDocumentHeader this[int index]
		{
			get
			{
				return (APComplianceDocumentHeader)(Elements[index]);
			}
		}

		public virtual new APComplianceDocumentHeader AddNew()
		{
			return (APComplianceDocumentHeader)base.AddNew();
		}
	}
}
