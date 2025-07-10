using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class UpgradeRequestCollection : NonPersistentBusinessObjectCollection<UpgradeRequest>
	{
		public UpgradeRequestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public UpgradeRequestCollection(BusinessObjectFactory factory, params EDIOrgHeader[] organisationsToUpgrade)
			: base(factory)
		{
			foreach (var organisation in organisationsToUpgrade)
			{
				var company = organisation.LicCompany;
				if (company != null)
				{
					foreach (var licence in company.LicHeadersForAllDatabases.Where(x => x.LA_IsActive && x.Database.LD_IsActive))
					{
						var request = new UpgradeRequest(factory, organisation, licence.Database);
						Add(request);
					}
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UpgradeRequest(Factory, null, null);
		}
	}
}

