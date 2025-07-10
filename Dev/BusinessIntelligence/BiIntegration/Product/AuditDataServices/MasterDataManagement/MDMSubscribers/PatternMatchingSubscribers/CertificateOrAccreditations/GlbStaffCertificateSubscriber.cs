using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class GlbStaffCertificateSubscriber : CertificateSubscriber
	{
		protected override string ParentTableCode => GlbStaffSchema.Constants.Prefix;

		public override string Code => "SRG";

		protected override GlbPerson GetPerson(BusinessObjectFactory factory, GenRegCertAccredMaintList certificate) => factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, certificate.XZ_ParentID))?.Person;
	}
}
