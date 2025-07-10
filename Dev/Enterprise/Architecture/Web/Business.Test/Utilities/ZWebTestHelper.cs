#if DEBUG
using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	/// <summary>
	/// Class to keep all the basic functionality for creation of test objects
	/// </summary>
	public class ZWebTestHelper
	{
		public ZWebTestHelper(BusinessObjectFactory factory)
		{
			OriginalUserContext = Env.CurrentUserContext;

			this.Factory = factory;
			PrepareForSaving();
		}

		protected BusinessObjectFactory Factory;

		/// <summary>
		/// Touches required objects before saving so they will be created 
		/// and then persisted to the DB
		/// </summary>
		protected void PrepareForSaving()
		{
			TestContact.ToString(); //touches TestOrg and TestContact
		}

		#region Random string

		static readonly Random Generator = new Random();
		public static string GetRandomString(int stringLength)
		{
			string result = "";
			for (int i = 0; i < stringLength; i++)
			{
				int index = Generator.Next(65, 90);
				result += (char)index;
			}
			return result;
		}

		#endregion

		#region TestOrg

		public OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
					fTestOrg.OH_Code = "_" + GetRandomString(4);
					fTestOrg.OH_RL_NKClosestPort = "AUSYD";
					if (fTestOrg.CompanyDataCollection.Count == 0)
					{
						fTestOrg.CompanyDataCollection.AddNew();
					}
					fTestOrg.CompanyDataCollection[0].OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
				}
				return fTestOrg;
			}
		}
		OrgHeader fTestOrg;

		#endregion

		#region TestContact

		public OrgContact TestContact
		{
			get
			{
				if (fTestContact == null)
				{
					fTestContact = TestOrg.Contacts.AddNew();
					fTestContact.OC_ContactName = "ContactName";
					fTestContact.OC_Email = "test@cargowise.com";
					fTestContact.SetHashedPassword("abc123");
					fTestContact.OC_WebAccessEnabled = true;
				}
				return fTestContact;
			}
		}
		OrgContact fTestContact;

		#endregion

		#region TestSiteUser

		public WebUser TestSiteUser
		{
			get { return testSiteUser ?? (testSiteUser = GetNewSiteUser()); }
		}

		protected virtual WebUser GetNewSiteUser()
		{
			return new OrgContactWebUser();
		}

		WebUser testSiteUser;

		#endregion

		#region RestoreEnvironmentSettings

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void RestoreEnvironmentSettings()
		{
			if (Env.CurrentUserContext.Branch.PK != OriginalUserContext.Branch.PK)
			{
				Enterprise.Environment.Env.SetUserContext(OriginalUserContext);
			}
		}

		public readonly IUserContext OriginalUserContext;

		#endregion

	}
}

#endif
