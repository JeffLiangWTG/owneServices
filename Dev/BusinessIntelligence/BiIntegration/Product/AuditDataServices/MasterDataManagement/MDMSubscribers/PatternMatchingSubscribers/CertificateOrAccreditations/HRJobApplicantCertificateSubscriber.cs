using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class HRJobApplicantCertificateSubscriber : CertificateSubscriber
	{
		protected override string ParentTableCode => HRJobApplicantSchema.Constants.Prefix;

		public override string Code => "ARG";

		protected override GlbPerson GetPerson(BusinessObjectFactory factory, GenRegCertAccredMaintList certificate) => factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, certificate.XZ_ParentID))?.Person;
	}
}
