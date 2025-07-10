using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZClientWebCargoWiseEDI.Base;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class MyAccountWebUser : OrgContactWebUser
	{
		protected override ZGuid GetBranchPKForLogin() => Env.CurrentBranchPK;

		protected override IContactable LoginCore(BusinessObjectFactory factory, string companyCode, string username, string password, byte[] loginHash, bool shouldRecordLoginFailAttempt = true)
		{
			var org = factory.LoadFromNaturalKey<EDIOrgHeader>(OrgHeaderSchema.OH_Code, companyCode);

			if (org != null && EDIOrgHeader.HasCurrentSupport(org.PK.ToGuid()) || string.IsNullOrEmpty(companyCode))
			{
				var user = LoginOrg(factory, org, companyCode, username, password, loginHash, shouldRecordLoginFailAttempt);
				if (user != null && ((WebEnv.AppInstance as Global)?.EnableSingleActiveUserSession ?? false))
				{
					var heartBeatManager = new MyAccountHeartBeatManager(HttpContext.Current.Session);
					heartBeatManager.RemoteLogOff(user.PK.ToGuid());
					heartBeatManager.RegisterOrUpdateUserContext(user.PK.ToGuid());
				}

				return user;
			}
			else
			{
				return null;
			}
		}

		public override ZString LoggedInUserName
		{
			get
			{
				var contact = LoggedInUser as EDIOrgContact;
				return contact?.ContactNameWithoutNumberSuffix ?? base.LoggedInUserName;
			}
		}

		public override string AffiliationName
		{
			get
			{
				var contact = LoggedInUser as EDIOrgContact;
				return contact?.CompanyName ?? base.AffiliationName;
			}
		}

		public ZGuid LoggedInUserAccountPK { get; set; }

		protected override void ApplyAdditionalContactFilter(List<OrgContact> contacts)
		{
			var orgsWithCurrentSupport = EDIOrgHeader.GetOrgsWithCurrentSupport(contacts.Select(x => x.OC_OH.ToGuid()).ToArray());
			contacts.RemoveAll(x => !orgsWithCurrentSupport.Contains(x.OC_OH.ToGuid()));
		}
	}
}
