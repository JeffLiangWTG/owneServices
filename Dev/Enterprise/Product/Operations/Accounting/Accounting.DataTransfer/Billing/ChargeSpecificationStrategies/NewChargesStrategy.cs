using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	internal sealed class NewChargesStrategy : IChargeSpecificationStrategy
	{
		public bool SkipJobHeader(Job header)
		{
			return false;
		}

		public Charge MatchCharge(Job job, AccChargeCode chargeCode, GlbBranch branch, GlbDepartment department, Xsd.ChargeLine line)
		{
			return null;
		}

		public void RemoveUnmatchedCharges()
		{
		}

		public void NotifySkippedHeaders()
		{
		}
	}
}
