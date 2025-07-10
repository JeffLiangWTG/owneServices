using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	internal interface IChargeSpecificationStrategy
	{
		bool SkipJobHeader(Job header);
		Charge MatchCharge(Job job, AccChargeCode chargeCode, GlbBranch branch, GlbDepartment department, Xsd.ChargeLine line);

		void RemoveUnmatchedCharges();
		void NotifySkippedHeaders();
	}
}
