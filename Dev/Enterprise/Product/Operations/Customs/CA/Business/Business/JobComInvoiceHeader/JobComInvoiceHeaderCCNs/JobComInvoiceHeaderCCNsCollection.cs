using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class JobComInvoiceHeaderCCNsCollection : ActiveBusinessObjectCollection<JobComInvoiceHeaderCCNs>
	{
		public JobComInvoiceHeaderCCNsCollection(JobComInvoiceHeader parent)
			: base(parent, GetQuery())
		{
		}

		public JobComInvoiceHeaderCCNs AddNew(ZString ccNumber)
		{
			var newCCN = AddNew();
			newCCN.J2_ReferenceNumber = ccNumber;
			return newCCN;
		}

		static ZQuery GetQuery() => new ZQuery(JobComInvoiceHeaderRefsSchema.J2_ReferenceType, JobComInvoiceHeaderCCNs.Constants.CCN);
	}
}
