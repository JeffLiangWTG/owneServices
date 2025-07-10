using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common.Testing
{
	sealed class DefaultBrokerAndCredentialLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDefaultBrokerCodeList()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_IsResource = true;
			var cantLoginStaff = Factory.NewWithValidTestData<GlbStaff>();
			cantLoginStaff.GS_CanLogin = false;
			Factory.Save();

			var list = defaultBrokerAndCredential.Lookups.DefaultBrokerCodeList;
			AssertCollectionContains(cantLoginStaff, list);
			AssertCollectionNotContains(resource, list);
		}

		public void TestCredentialSEAList()
		{
			defaultBrokerAndCredential.DefaultBrokerCode = "AN";
			var list = defaultBrokerAndCredential.Lookups.CredentialSEAList;
			var codes = list.GetAllCodes();

			Assert(codes.Length == 2);
			Assert(codes.Contains("Test1001"));
			Assert(codes.Contains("Test3003"));

			Assert(list.GetDescriptionFromCode("Test1001").Equals(UserCodeSpecificTransportModeList.Descriptions.SEA));
			Assert(list.GetDescriptionFromCode("Test3003").Equals(UserCodeSpecificTransportModeList.Descriptions.BTH));
		}

		public void TestCredentialAIRList()
		{
			defaultBrokerAndCredential.DefaultBrokerCode = "AN";
			var list = defaultBrokerAndCredential.Lookups.CredentialAIRList;
			var codes = list.GetAllCodes();

			Assert(codes.Length == 2);
			Assert(codes.Contains("Test2002"));
			Assert(codes.Contains("Test3003"));

			Assert(list.GetDescriptionFromCode("Test2002").Equals(UserCodeSpecificTransportModeList.Descriptions.AIR));
			Assert(list.GetDescriptionFromCode("Test3003").Equals(UserCodeSpecificTransportModeList.Descriptions.BTH));
		}

		protected override void SetUp()
		{
			base.SetUp();
			new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaff();
			defaultBrokerAndCredential = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		}

		DefaultBrokerAndCredential defaultBrokerAndCredential;
	}
}
