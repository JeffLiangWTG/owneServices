using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Security.ActiveDirectory.Test
{
	public class ADTestHelper
	{
		public ADTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		BusinessObjectFactory Factory { get; set; }

		public void PurgeAllStaff()
		{
			Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_IsSystemAccount, false)).ForEach(s => s.Delete());
			Factory.Save();
		}

		public void PurgeAllGroups()
		{
			Factory.Load<GlbGroup>(new ZQuery(GlbGroupSchema.GG_IsSystemDefined, false)).ForEach(g => g.Delete());
			Factory.Save();
		}

		public GlbStaff CreateStaff(string loginName)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = loginName;
			return staff;
		}

		public GlbStaff CreateStaffWithSecurityRights(bool allowed, params string[] securityKeys)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			foreach (var securityKey in securityKeys)
			{
				var checkPoint = EnvProxy.Instance.Security.FindCheckPoint(securityKey);
				if (checkPoint != null)
				{
					var security = Factory.New<GlbSecurity>();
					security.GU_GS = staff.PK;
					security.GU_SecurityRight = checkPoint.Code;
					security.GU_SecurityItemIsAllowed = allowed;
				}
			}
			return staff;
		}

		public GlbGroup CreateGroup(string groupName)
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = groupName;
			group.GG_Code = groupName;
			return group;
		}

		public static readonly string PasswordMatchingPolicy = Enterprise.Security.ActiveDirectory.DomainCredentials.DefaultPasswordValue;

		public static DomainCredentials CreateDomainCredentials(string domainName = null, string domainUserName = null, string domainUserPassword = null, string userOrganisationalUnit = null, string groupOrganisationalUnit = null, string defaultPassword = null, bool isDefaultDomain = true)
		{
			var domainCredentials = new DomainCredentials()
			{
				DomainName = domainName ?? TestConstants.Domain,
				DomainUserName = domainUserName ?? TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = domainUserPassword ?? TestConstants.ADTestUserAccount.Password,
				IsDefaultDomain = isDefaultDomain,
				UserOrganisationalUnit = userOrganisationalUnit ?? TestConstants.ValidOU,
				GroupOrganisationalUnit = groupOrganisationalUnit ?? TestConstants.ValidOU,
				DefaultPassword = defaultPassword ?? PasswordMatchingPolicy
			};
			return domainCredentials;
		}

		public static DomainCredentialsCollection CreateDomainCredentialsCollection(string domainName = null, string domainUserName = null, string domainUserPassword = null, string userOrganisationalUnit = null, string groupOrganisationalUnit = null, string defaultPassword = null, bool isDefaultDomain = true)
		{
			return new DomainCredentialsCollection { CreateDomainCredentials(domainName, domainUserName, domainUserPassword, userOrganisationalUnit, groupOrganisationalUnit, defaultPassword, isDefaultDomain) };
		}

		public static DomainCredentialsCollection CreateDomainCredentialsCollection(params DomainCredentials[] domainCredentialsList)
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			foreach (var dc in domainCredentialsList)
			{
				domainCredentialsCollection.Add(dc);
			}
			return domainCredentialsCollection;
		}

		public static void MockSearcherWithDomainAttributes(IEnumerable<string> adAttributesForTest)
		{
			var searcher = new Mock<IDirectorySearcher>().Object;
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = searcher;
			if (adAttributesForTest == null)
			{
				adAttributesForTest = ADAttributeList.Instance.GetPredefinedAttributes().ToList();
			}
			Mock.Get(searcher).Setup(x => x.GetAllAttributes(It.IsAny<string>())).Returns(adAttributesForTest);
		}
	}
}
