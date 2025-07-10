using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	sealed class ResponseNotificationHelperTest : TestCaseWithFactory
	{
		#region Warehouse RNS Messages

		public void TestGetWarehouseRNSNotificationEmailGroups()
		{
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemGroup.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1Group.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, company2Group.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, branch1Group.PK.ToGuid());

			var helper = new DeclarationTestHelper(Factory, true);
			const string messageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::257+10207400004068+11'LOC+22+0497:129::3072'DTM+58:201011250820:203'GIS+14'RFF+XC:37132536987'UNT+7+1";
			var ediMessage = (EDIReleaseMessage)helper.GetEDIReleaseResponseMessage(messageText, ZDateTime.Now, "1");
			AssertEquals("Precondition:SubLocation", "3072", ediMessage.SubLocation);

			var emailGroups = new ResponseNotificationHelper(ediMessage).NotificationEmailGroups.ToArray();
			AssertEquals("Do not fall back to system level", 0, emailGroups.Length);

			company1.OrgProxy.CustomsCodes.AddNew("CCP", "3072", "CA");
			emailGroups = new ResponseNotificationHelper(ediMessage).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1", 1, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);

			company2.OrgProxy.CustomsCodes.AddNew("CCP", "3072", "CA");
			company2.OrgProxy.CustomsCodes.AddNew("CCP", "3074", "CA");
			emailGroups = new ResponseNotificationHelper(ediMessage).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1 & company2", 2, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);
			AssertCollectionContains("Get the group for company2", company2Group.PK.ToGuid(), emailGroups);

			branch1.OrgProxy.CustomsCodes.AddNew("CCP", "3072", "CA");
			emailGroups = new ResponseNotificationHelper(ediMessage).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for branch1", 1, emailGroups.Length);
			AssertCollectionContains("Get the group for branch1", branch1Group.PK.ToGuid(), emailGroups);

			branch2.OrgProxy.CustomsCodes.AddNew("CCP", "3072", "CA");
			branch2.OrgProxy.CustomsCodes.AddNew("CCP", "3074", "CA");
			emailGroups = new ResponseNotificationHelper(ediMessage).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for branch1 & branch2", 2, emailGroups.Length);
			AssertCollectionContains("Get the group for branch1", branch1Group.PK.ToGuid(), emailGroups);
			AssertCollectionContains("Get the group for branch2", company2Group.PK.ToGuid(), emailGroups);
		}

		#endregion

		#region Warehouse eManifest Forwarded Manifests

		public void TestGetForwardedManifestsNotificationEmailGroups_WH()
		{
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemGroup.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1Group.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, company2Group.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, branch1Group.PK.ToGuid());

			var wrapper = GetManifestForwardHouseBillWrapper("WH", "3072");
			var emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group from the registry at system level", 1, emailGroups.Length);
			AssertCollectionContains("Get the group from the registry at system level", systemGroup.PK.ToGuid(), emailGroups);

			company1.OrgProxy.CustomsCodes.AddNew("CCP", "3072", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1", 1, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);

			company2.OrgProxy.CustomsCodes.AddNew("CCP", "3072", "CA");
			company2.OrgProxy.CustomsCodes.AddNew("CCP", "3074", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1 & company2", 2, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);
			AssertCollectionContains("Get the group for company2", company2Group.PK.ToGuid(), emailGroups);

			branch1.OrgProxy.CustomsCodes.AddNew("CCP", "3072", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for branch1", 1, emailGroups.Length);
			AssertCollectionContains("Get the group for branch1", branch1Group.PK.ToGuid(), emailGroups);

			branch2.OrgProxy.CustomsCodes.AddNew("CCP", "3072", "CA");
			branch2.OrgProxy.CustomsCodes.AddNew("CCP", "3074", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for branch1 & branch2", 2, emailGroups.Length);
			AssertCollectionContains("Get the group for branch1", branch1Group.PK.ToGuid(), emailGroups);
			AssertCollectionContains("Get the group for branch2", company2Group.PK.ToGuid(), emailGroups);
		}

		#endregion

		#region Carrier eManifest Forwarded Manifests

		public void TestGetForwardedManifestsNotificationEmailGroups_CA()
		{
			CACustomsDataRegistry.Instance.SendCarrierSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemGroup.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendCarrierSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1Group.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendCarrierSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, company2Group.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendCarrierSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, branch1Group.PK.ToGuid());

			var wrapper = GetManifestForwardHouseBillWrapper("CA", "0011");
			var emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group from the registry at system level", 1, emailGroups.Length);
			AssertCollectionContains("Get the group from the registry at system level", systemGroup.PK.ToGuid(), emailGroups);

			company1.OrgProxy.CustomsCodes.AddNew("CCC", "0011", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1", 1, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);

			company2.OrgProxy.CustomsCodes.AddNew("CCC", "0011", "CA");
			company2.OrgProxy.CustomsCodes.AddNew("CCC", "0012", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1 & company2", 2, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);
			AssertCollectionContains("Get the group for company2", company2Group.PK.ToGuid(), emailGroups);

			branch1.OrgProxy.CustomsCodes.AddNew("CCC", "0011", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for branch1", 1, emailGroups.Length);
			AssertCollectionContains("Get the group for branch1", branch1Group.PK.ToGuid(), emailGroups);

			branch2.OrgProxy.CustomsCodes.AddNew("CCC", "0011", "CA");
			branch2.OrgProxy.CustomsCodes.AddNew("CCC", "0012", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for branch1 & branch2", 2, emailGroups.Length);
			AssertCollectionContains("Get the group for branch1", branch1Group.PK.ToGuid(), emailGroups);
			AssertCollectionContains("Get the group for branch2", company2Group.PK.ToGuid(), emailGroups);
		}

		#endregion

		#region FRorwader eManifest Forwarded Manifests

		public void TestGetForwardedManifestsNotificationEmailGroups_FW()
		{
			CACustomsDataRegistry.Instance.SendForwarderSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemGroup.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendForwarderSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1Group.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendForwarderSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, company2Group.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendForwarderSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, branch1Group.PK.ToGuid());

			var wrapper = GetManifestForwardHouseBillWrapper("FW", "0011");
			var emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group from the registry at system level", 1, emailGroups.Length);
			AssertCollectionContains("Get the group from the registry at system level", systemGroup.PK.ToGuid(), emailGroups);

			company1.OrgProxy.CustomsCodes.AddNew("CCC", "0011", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1", 1, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);

			company2.OrgProxy.CustomsCodes.AddNew("CCC", "0011", "CA");
			company2.OrgProxy.CustomsCodes.AddNew("CCC", "0012", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1 & company2", 2, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);
			AssertCollectionContains("Get the group for company2", company2Group.PK.ToGuid(), emailGroups);

			branch1.OrgProxy.CustomsCodes.AddNew("CCC", "0011", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for branch1", 1, emailGroups.Length);
			AssertCollectionContains("Get the group for branch1", branch1Group.PK.ToGuid(), emailGroups);

			branch2.OrgProxy.CustomsCodes.AddNew("CCC", "0011", "CA");
			branch2.OrgProxy.CustomsCodes.AddNew("CCC", "0012", "CA");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for branch1 & branch2", 2, emailGroups.Length);
			AssertCollectionContains("Get the group for branch1", branch1Group.PK.ToGuid(), emailGroups);
			AssertCollectionContains("Get the group for branch2", company2Group.PK.ToGuid(), emailGroups);
		}

		#endregion

		#region Broker eManifest Forwarded Manifests

		public void TestGetForwardedManifestsNotificationEmailGroups_CB()
		{
			CACustomsDataRegistry.Instance.SendBrokerSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemGroup.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendBrokerSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1Group.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendBrokerSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, company2Group.PK.ToGuid());

			var wrapper = GetManifestForwardHouseBillWrapper("CB", "10207");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "11111");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "11111");
			var emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group from the registry at system level", 1, emailGroups.Length);
			AssertCollectionContains("Get the group from the registry at system level", systemGroup.PK.ToGuid(), emailGroups);

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "10207");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1", 1, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "10207");
			emailGroups = new ResponseNotificationHelper(Factory, wrapper).NotificationEmailGroups.ToArray();
			AssertEquals("Get the group for company1 & company2", 2, emailGroups.Length);
			AssertCollectionContains("Get the group for company1", company1Group.PK.ToGuid(), emailGroups);
			AssertCollectionContains("Get the group for company2", company2Group.PK.ToGuid(), emailGroups);
		}

		#endregion

		#region Implement

		ManifestForwardHouseBillWrapper GetManifestForwardHouseBillWrapper(string snpType, string snpIdentifier)
		{
			var message = Factory.New<ACIHouseBillMessage>();
			message.EM_MessageSubType = ACIForwarderReceivedMessageTypes.Codes.ManifestForward;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = string.Format("UNH+1+GOVCBR:D:11B:UN'BGM+714+8036X555+4'RFF+AFM:{0}:{1}'RFF+UCN:UCR555'UNT+4+1'UNE+1+13'", snpIdentifier, snpType);

			return new ManifestForwardHouseBillWrapper(message);
		}

		GlbCompany company1;
		GlbCompany company2;
		GlbBranch branch1;
		GlbBranch branch2;
		GlbGroup company1Group;
		GlbGroup company2Group;
		GlbGroup branch1Group;
		GlbGroup systemGroup;

		protected override void SetUp()
		{
			base.SetUp();
			company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = "CA";
			company1.GC_Code = "CA1";
			company1.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company1.OrgProxy.OH_Code = "CA1OH";
			branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "CB1";
			branch1.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			branch1.OrgProxy.OH_Code = "CB1OH";
			company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = "CA";
			company2.GC_Code = "CA2";
			company2.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company2.OrgProxy.OH_Code = "CA2OH";
			branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "CB2";
			branch2.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			branch2.OrgProxy.OH_Code = "CB2OH";
			systemGroup = Factory.New<GlbGroup>();
			systemGroup.GG_Code = "GG1";
			company1Group = Factory.New<GlbGroup>();
			company1Group.GG_Code = "GG2";
			company2Group = Factory.New<GlbGroup>();
			company2Group.GG_Code = "GG3";
			branch1Group = Factory.New<GlbGroup>();
			branch1Group.GG_Code = "GG4";
			Factory.Save();
		}

		#endregion
	}
}
