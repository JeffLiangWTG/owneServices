using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAgreementManager : NonPersistentBusinessObject
	{
		public EdiUserAgreement Agreement { get; set; }
		public LicenceEnterprise Enterprise { get; set; }
		public LicenceDatabase Database { get; set; }
		public OrgHeader ClientAgreementOrg { get; set; }
		public IEnumerable<EdiUserAgreementAssignment> Assignments { get; set; }
		public EnterpriseUserAgreementInfo AgreementInfo { get; set; }
		public UserAgreementResponseData CurrentUserAgreementResponse { get; set; }
		public EDIOrgContact FromContact { get; set; }

		public bool IsDatabaseLevelAgreement => Assignments?.Any() ?? false;

		public List<IeDoc> EDocs
		{
			get
			{
				if (Agreement ==  null)
				{
					return new List<IeDoc>();
				}

				return Agreement.DocManagerInfo.AllEDocs.OfType<IeDoc>().Where(x => x.IsPublished).ToList();
			}
		}

		public IEnumerable<LicenceDatabase> Databases
		{
			get
			{
				if (Enterprise != null)
				{
					return Enterprise.Databases.Cast<LicenceDatabase>().ToArray();
				}

				if (ClientAgreementOrg != null)
				{
					return Assignments.Select(x => x.Parent as LicenceDatabase).ToArray();
				}

				return Array.Empty<LicenceDatabase>();
			}
		}
	}
}
