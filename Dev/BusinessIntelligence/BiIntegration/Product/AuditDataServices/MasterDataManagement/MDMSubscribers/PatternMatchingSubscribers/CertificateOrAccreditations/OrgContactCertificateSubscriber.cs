using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class OrgContactCertificateSubscriber : CertificateSubscriber
	{
		protected override string ParentTableCode => OrgContactSchema.Constants.Prefix;

		public override string Code => "CRG";

		protected override GlbPerson GetPerson(BusinessObjectFactory factory, GenRegCertAccredMaintList certificate) => factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, certificate.XZ_ParentID))?.Person;
	}
}
