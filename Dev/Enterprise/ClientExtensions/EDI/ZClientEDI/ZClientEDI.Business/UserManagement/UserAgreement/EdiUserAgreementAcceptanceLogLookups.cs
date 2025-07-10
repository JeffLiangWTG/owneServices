//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiUserAgreementAcceptanceLogLookups
//
//    This class should be used for overriding collections in AutoEdiUserAgreementAcceptanceLogLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
 using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementAcceptanceLogLookups : AutoEdiUserAgreementAcceptanceLogLookups
	{
		public EdiUserAgreementAcceptanceLogLookups(AutoEdiUserAgreementAcceptanceLog parent) : base(parent)
		{
			this.parent = parent as EdiUserAgreementAcceptanceLog;
		}

		readonly EdiUserAgreementAcceptanceLog parent;

		public LicenceDatabaseCollection LicenceDatabaseList
		{
			get
			{
				if (licenceDatabaseList == null)
				{
					if (!parent.EUL_LE.IsEmpty)
					{
						licenceDatabaseList = new LicenceDatabaseCollection(parent.Enterprise, Factory);
					}
					else
					{
						licenceDatabaseList = new LicenceDatabaseCollection(Factory);
					}
				}

				return licenceDatabaseList;
			}
		}

		LicenceDatabaseCollection licenceDatabaseList;

		public LicenceEnterpriseCollection EnterpriseList
		{
			get
			{
				if (enterpriseList == null)
				{
					enterpriseList = new LicenceEnterpriseCollection(Factory, new CargoWise.EntityFramework.ZQuery(LicenceEnterpriseSchema.PK, parent.EUL_LE));
				}

				return enterpriseList;
			}
		}
		LicenceEnterpriseCollection enterpriseList;
	}
}
