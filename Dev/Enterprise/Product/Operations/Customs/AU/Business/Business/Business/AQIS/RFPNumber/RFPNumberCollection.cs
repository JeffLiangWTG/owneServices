
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RFPNumberCollection : DependentCusAddInfoCollection<RFPNumber, JobComInvoiceLine>
	{
		public RFPNumberCollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.AURFPNumber)
		{
		}

		public void Clone(JobComInvoiceLine clonedInvoiceLine)
		{
			foreach (RFPNumber number in this)
			{
				number.Clone(clonedInvoiceLine);
			}
		}
	}
}
