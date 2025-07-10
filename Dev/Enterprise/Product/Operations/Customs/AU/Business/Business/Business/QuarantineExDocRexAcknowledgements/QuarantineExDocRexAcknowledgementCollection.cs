using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocRexAcknowledgementCollection : CusCodeDataCollection<QuarantineExDocRexAcknowledgement>
	{
		public QuarantineExDocRexAcknowledgementCollection(BusinessObject master)
			: base(master, QuarantineExDocRexAcknowledgement.AcknowledgementCode)
		{
		}
	}
}
