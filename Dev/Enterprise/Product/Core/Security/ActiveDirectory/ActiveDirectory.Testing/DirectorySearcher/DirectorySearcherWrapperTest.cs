using System.DirectoryServices;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class DirectorySearcherWrapperTest : TestCase
	{
		public void TestUserProperties()
		{
			var expectedProperties = AttributeMap.Current.MapItems
				.Cast<AttributeMapItem>()
				.Where(item => item.EnterpriseTableName == GlbStaffSchema.Constants.TableName)
				.Select(item => (string)item.ActiveDirectoryAttributeName)
				.Concat(new[] { ADAttributes.SAMAccountName, ADAttributes.WhenCreated, ADAttributes.WhenChanged, ADAttributes.ObjectGuid })
				.OrderBy(s => s)
				.ToArray();
			var actualProperties = Wrapper.UserProperties_Exposed.OrderBy(s => s).ToArray();

			AssertEquals(expectedProperties.Length, actualProperties.Length);
			CombineAssertions(() =>
			{
				for (int i = 0; i < expectedProperties.Length; i++)
				{
					AssertEquals(expectedProperties[i], actualProperties[i]);
				}
			});
		}

		public void TestGroupProperties()
		{
			var expectedProperties = AttributeMap.Current.MapItems
				.Cast<AttributeMapItem>()
				.Where(item => item.EnterpriseTableName == GlbGroupSchema.Constants.TableName)
				.Select(item => (string)item.ActiveDirectoryAttributeName)
				.Concat(new[] { ADAttributes.WhenChanged, ADAttributes.ObjectGuid })
				.OrderBy(s => s)
				.ToArray();
			var actualProperties = Wrapper.GroupProperties_Exposed.OrderBy(s => s).ToArray();

			AssertEquals(expectedProperties.Length, actualProperties.Length);
			CombineAssertions(() =>
			{
				for (int i = 0; i < expectedProperties.Length; i++)
				{
					AssertEquals(expectedProperties[i], actualProperties[i]);
				}
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			Wrapper = new DirectorySearcherWrapperForTesting();
			base.SetUp();
		}

		DirectorySearcherWrapperForTesting Wrapper { get; set; }

		class DirectorySearcherWrapperForTesting : DirectorySearcherWrapper
		{
			public DirectorySearcherWrapperForTesting()
				: base(TestConstants.ADTestUserAccount.NameWithDomain, TestConstants.ADTestUserAccount.Password, TestConstants.Domain)
			{ }

			internal DirectoryEntry GetDomainRootDirectoryNode_Exposed(string domainName)
			{
				return GetDomainRootDirectoryNode(null, domainName);
			}

			internal string[] UserProperties_Exposed
			{
				get { return UserAttributes; }
			}

			internal string[] GroupProperties_Exposed
			{
				get { return GroupAttributes; }
			}
		}

		#endregion
	}
}
