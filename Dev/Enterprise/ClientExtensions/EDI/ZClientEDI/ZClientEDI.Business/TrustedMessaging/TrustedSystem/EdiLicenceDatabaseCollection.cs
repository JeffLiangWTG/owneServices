using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	[ModuleID("LicenceDatabase")]
	public class EdiLicenceDatabaseCollection : ActiveBusinessObjectCollection<LicenceDatabase>
	{
		public EdiLicenceDatabaseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiLicenceDatabaseCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override void SetRelationshipDefaultsForElementCore(LicenceDatabase newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, false);
		}
	}
}
