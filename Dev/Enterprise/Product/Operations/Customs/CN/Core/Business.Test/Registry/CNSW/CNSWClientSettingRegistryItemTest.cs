using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNSWClientSettingRegistryItem))]
	class CNSWClientSettingRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CNSWClientSetting>
	{
		public void TestOnAllValuesSavedAction_CompanyLevel()
		{
			const string interchangeXml = @"
<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""CNCustomsSW"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""ENTSVR"">
		<Group Type=""Company"" Reference=""CMP"">
			<Credential Name=""Current"">
				<UserName />
				<Password />
			</Credential>
		</Group>
	</Group>
</Configuration>
";

			var branch = CreateBranch();
			AssertOnAllValuesSavedAction(branch.Company.PK.ToGuid(), Guid.Empty, XmlHelper.IgnoreXmlnsAttrOrder(interchangeXml), "ENTCMPSVR_CSW");
		}

		public void TestOnAllValuesSavedAction_BranchLevel()
		{
			const string interchangeXml = @"
<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""CNCustomsSW"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""ENTSVR"">
		<Group Type=""Company"" Reference=""CMP"">
			<Group Type=""Branch"" Reference=""CNB"">
				<Credential Name=""Current"">
					<UserName />
					<Password />
				</Credential>
			</Group>
		</Group>
	</Group>
</Configuration>
";

			var branch = CreateBranch();
			AssertOnAllValuesSavedAction(Guid.Empty, branch.PK.ToGuid(), XmlHelper.IgnoreXmlnsAttrOrder(interchangeXml), "ENTCMPSVRCNB_CSW");
		}

		GlbBranch CreateBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CMP";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "CNB";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";
			Factory.Save();

			return branch;
		}

		void AssertOnAllValuesSavedAction(Guid companyPK, Guid branchPK, string expectedInterchangeXml, string expectedUserName)
		{
			var registryItem = CNCustomsDataRegistry.Instance.CNSWClientSetting;
			registryItem.Options &= RegistryOptions.NotCached;
			var factory = registryItem.Factory;

			using (RawDataRegistry.Instance.EncryptedRegistrationKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "password"))
			{
				var setting = CNSWClientSettingCheckerTest.CreateCNSWClientSetting(Factory, companyPK, branchPK);
				using (CNSWClientSettingCheckerTest.TemporarilySetCNSWClientSetting(companyPK, branchPK, setting))
				{
					registryItem.OnUpdateAction(companyPK, branchPK, Guid.Empty, setting);
					registryItem.OnAllValuesSaved();
					AssertEquals("Registry status should have been set to OK", "OK", setting.EHubClientStatus);
					var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub") { OrderBy = EDIInterchangeSchema.EI_InterchangeNum.Name + " DESC" };
					var interchange = factory.LoadTop1<EDIInterchange>(interchangeQuery);
					CNCusEntryHeaderHelper.AssertEqualsIgnoreLineBreaksAndIndent("Contains <UserName>", expectedInterchangeXml.Replace("<UserName />", $"<UserName>{expectedUserName}</UserName>"), interchange.EI_BodyText);
					interchange.Delete();

					setting = registryItem.GetValueWithoutFallback(companyPK, branchPK, Guid.Empty);
					setting.MachineName = setting.ArchiveFolder = setting.SendFolder = setting.ErrorResponseFolder = "";
					registryItem.OnUpdateAction(companyPK, branchPK, Guid.Empty, setting);
					registryItem.OnAllValuesSaved();
					AssertEquals("Registry status should have been cleared", "", setting.EHubClientStatus);
					interchange = factory.LoadTop1<EDIInterchange>(interchangeQuery);
					CNCusEntryHeaderHelper.AssertEqualsIgnoreLineBreaksAndIndent("Not Contains <UserName>", expectedInterchangeXml.Replace("<UserName />", ""), interchange.EI_BodyText);
					interchange.Delete();

					((IRegistryItemInternals)registryItem).DeleteValue(companyPK, branchPK, Guid.Empty);
					registryItem.OnAllValuesSaved();
					var previousInterchangePk = interchange.PK;
					interchange = factory.LoadTop1<EDIInterchange>(interchangeQuery);
					AssertNotEquals("Should have created another interchange when deleting", previousInterchangePk, interchange.PK);
					CNCusEntryHeaderHelper.AssertEqualsIgnoreLineBreaksAndIndent("Not Contains <UserName> too", expectedInterchangeXml.Replace("<UserName />", ""), interchange.EI_BodyText);
				}
			}
		}

		protected override StronglyTypedRegistryItem<CNSWClientSetting, CNSWClientSetting> GetNewRegistryItem() => new CNSWClientSettingRegistryItem("", null, null, null, RegistryStorageFlags.Company, new CNSWClientSetting
		{
			MachineName = Guid.NewGuid().ToString(),
			SendFolder = @"D:\Folders\SendFolder",
			ReceiveFolder = @"D:\Folders\ReceiveFolder",
			ArchiveFolder = @"D:\Folders\ArchiveFolder",
			ErrorResponseFolder = @"D:\Folders\ErrorResponseFolder",
			AcdaSendFolder = @"D:\Folders\AcdaSendFolder",
			AcdaReceiveFolder = @"D:\Folders\AcdaReceiveFolder",
			AcdaErrorResponseFolder = @"D:\Folders\AcdaErrorResponseFolder",
			AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder",
			RunningIntervalInSeconds = 60
		});
	}
}
