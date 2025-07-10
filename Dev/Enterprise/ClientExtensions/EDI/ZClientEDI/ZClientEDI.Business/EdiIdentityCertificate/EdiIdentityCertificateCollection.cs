using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityApplication.Business;

namespace Enterprise.Client.EDI.IdentityCertificate.Business
{
	public class EdiIdentityCertificateCollection : ActiveBusinessObjectCollection<EdiIdentityCertificate>
	{
		public EdiIdentityCertificateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiIdentityCertificateCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public EdiIdentityCertificateCollection(BusinessObjectFactory factory, EdiIdentityApplication parent) : base(factory, parent)
		{
		}

		protected override bool AllowNew => false;
	}
}
