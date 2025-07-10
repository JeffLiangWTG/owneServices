using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class GlbPersonCertificateSubscriber : CertificateSubscriber
	{
		protected override string ParentTableCode => GlbPersonSchema.Constants.Prefix;

		public override string Code => "PRG";

		protected override GlbPerson GetPerson(BusinessObjectFactory factory, GenRegCertAccredMaintList certificate) => factory.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, certificate.XZ_ParentID));
	}
}
