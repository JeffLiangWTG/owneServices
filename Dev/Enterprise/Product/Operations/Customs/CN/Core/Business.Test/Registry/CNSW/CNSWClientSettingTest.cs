using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNSWClientSetting))]
	class CNSWClientSettingTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestReadElements()
		{
			var branch = CreateBranch("CNC", "CNB");
			Factory.Save();

			var setting = new CNSWClientSetting(new FallbackLevel(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory)
			{
				MachineName = "MACHINE1123",
				SendFolder = @"D:\DEC OUT",
				ReceiveFolder = @"D:\DEC IN",
				ErrorResponseFolder = @"D:\DEC FAIL",
				ArchiveFolder = @"D:\DEC ARCHIVE",
				AcdaSendFolder = @"D:\ACD OUT",
				AcdaReceiveFolder = @"D:\ACD IN",
				AcdaErrorResponseFolder = @"D:\ACD FAIL",
				AcdaArchiveFolder = @"D:\ACD ARCHIVE",
			};

			var dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(CNSWClientSetting));
			var serialisedValue = dummyDataType.Serialise(setting);
			var deserialisedBusinessObject = (CNSWClientSetting)dummyDataType.Deserialise(serialisedValue);

			CombineAssertions(() =>
			{
				AssertEquals("MachineName", "MACHINE1123", deserialisedBusinessObject.MachineName);
				AssertEquals("EHubClientID", "ENTCNCSVR_CSW", deserialisedBusinessObject.EHubClientID);
				AssertEquals("EHubClientStatus", "OK", deserialisedBusinessObject.EHubClientStatus);
				AssertEquals("SendFolder", @"D:\DEC OUT", deserialisedBusinessObject.SendFolder);
				AssertEquals("ReceiveFolder", @"D:\DEC IN", deserialisedBusinessObject.ReceiveFolder);
				AssertEquals("ErrorResponseFolder", @"D:\DEC FAIL", deserialisedBusinessObject.ErrorResponseFolder);
				AssertEquals("ArchiveFolder", @"D:\DEC ARCHIVE", deserialisedBusinessObject.ArchiveFolder);
				AssertEquals("AcdaSendFolder", @"D:\ACD OUT", deserialisedBusinessObject.AcdaSendFolder);
				AssertEquals("AcdaReceiveFolder", @"D:\ACD IN", deserialisedBusinessObject.AcdaReceiveFolder);
				AssertEquals("AcdaErrorResponseFolder", @"D:\ACD FAIL", deserialisedBusinessObject.AcdaErrorResponseFolder);
				AssertEquals("AcdaArchiveFolder", @"D:\ACD ARCHIVE", deserialisedBusinessObject.AcdaArchiveFolder);
			});
		}

		public void TestEHubClientProperties()
		{
			var setting = new CNSWClientSetting(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			CombineAssertions(() =>
			{
				setting.MachineName = "Machine Name";
				AssertEquals("EHubClientStatusOK is set.", Constants.CNSWClient.EHubClientStatusOK, setting.EHubClientStatus);
				AssertEquals("EHubClientRegistered is true.", true, setting.EHubClientRegistered);
				AssertEquals("ShouldRegisterEHubClient is true", true, setting.ShouldRegisterEHubClient);
				AssertEquals("ShouldUnregisterEHubClient is true", true, Deserialize<CNSWClientSetting>(Serialize(setting)).ShouldUnregisterEHubClient);

				setting.MachineName = ZString.Empty;
				AssertEquals("EHubClientStatusOK is cleared.", ZString.Empty, BizObj.EHubClientStatus);
				AssertEquals("EHubClientRegistered is false", false, setting.EHubClientRegistered);
				AssertEquals("ShouldRegisterEHubClient is false", false, setting.ShouldRegisterEHubClient);
				AssertEquals("ShouldUnregisterEHubClient is false", false, Deserialize<CNSWClientSetting>(Serialize(setting)).ShouldUnregisterEHubClient);
			});
		}

		public void TestEHubClientId()
		{
			var branch = CreateBranch("CNC", "CNB");
			Factory.Save();

			var companyPK = branch.Company.PK.ToGuid();
			var branchPK = branch.PK.ToGuid();

			var registryItem = CNCustomsDataRegistry.Instance.CNSWClientSetting;

			CombineAssertions("Company Level", () =>
			{
				AssertEHubClientID(companyPK, Guid.Empty, ValueToUse.DefaultValue, "ENTCNCSVR_CSW");
				AssertEHubClientID(companyPK, Guid.Empty, ValueToUse.SavedValue, "ENTCNCSVR_CSW");
				AssertEHubClientID(companyPK, Guid.Empty, ValueToUse.ProposedValue, "ENTCNCSVR_CSW");
			});

			CombineAssertions("Branch Level", () =>
			{
				AssertEHubClientID(Guid.Empty, branchPK, ValueToUse.DefaultValue, "ENTCNCSVR_CSW");
				AssertEHubClientID(Guid.Empty, branchPK, ValueToUse.SavedValue, "ENTCNCSVRCNB_CSW");
				AssertEHubClientID(Guid.Empty, branchPK, ValueToUse.ProposedValue, "ENTCNCSVRCNB_CSW");
			});

			void AssertEHubClientID(Guid companyPK, Guid branchPK, ValueToUse valueToUse, string expectedEHubClientID)
			{
				var setting = new CNSWClientSetting(new FallbackLevel(companyPK, branchPK, Guid.Empty), Factory);
				var registryItem = CNCustomsDataRegistry.Instance.CNSWClientSetting;
				setting.RegistryItemInternals = registryItem;
				using (registryItem.SetTemporaryValue(companyPK, branchPK, Guid.Empty, setting))
				{
					registryItem.Inner.SetCurrentValueToUse(companyPK, branchPK, Guid.Empty, valueToUse);
					setting.MachineName = "Machine Name";
					AssertEquals(valueToUse.ToString(), expectedEHubClientID, setting.EHubClientID);

					setting.MachineName = ZString.Empty;
					AssertEquals("EHubClientId is cleared", ZString.Empty, setting.EHubClientID);
				}
			}
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => BizObj;

		protected new CNSWClientSetting BizObj => (CNSWClientSetting)base.BizObj;

		GlbBranch CreateBranch(string companyCode, string branchCode)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_RN_NKCountryCode = "CN";
			var branch = company.Branches.AddNew();
			branch.GB_Code = branchCode;
			return branch;
		}
	}
}
