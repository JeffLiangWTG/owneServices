using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocSalesCallsCollection : DocumentWrapperCollection
	{
		public DocSalesCallsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocSalesCallsCollection(BusinessObjectFactory factory, OrgSalesCallCollection orgSalesCalls)
			: base(factory)
		{
			if (!orgSalesCalls.ShouldCheckIncludeOnSalesCallDocument)
			{
				foreach (OrgSalesCall salesCall in orgSalesCalls)
				{
					DocSalesCall docCall = DocSalesCall.New(salesCall, factory);
					Add(docCall);
				}
			}
			else
			{
				foreach (OrgSalesCall salesCall in orgSalesCalls)
				{
					if (salesCall.ShouldIncludeOnSalesCallDocument)
					{
						DocSalesCall docCall = DocSalesCall.New(salesCall, factory);
						Add(docCall);
					}
				}
			}
		}

		public new DocSalesCall this[int index]
		{
			get { return (DocSalesCall)base[index]; }
		}
	}
}

