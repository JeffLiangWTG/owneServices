using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class CNSWClientSettingCheckerTest : TestCaseWithFactory
	{
		public void TestCheckForAcdAgrMessageSending()
		{
			AssertExceptionThrown<ArgumentNullException>(() => CNSWClientSettingChecker.CheckForAcdAgrMessageSending(null));

			var branch1 = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var company = branch1.Company;
			var branch2 = company.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch1.PK;

			using (CNCustomsDataRegistry.Instance.CNSWClientSetting.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new CNSWClientSetting()))
			{
				AssertContains("Single Window Client Application Settings should be entered for submitting the messages to CN Customs.", CNSWClientSettingChecker.CheckForAcdAgrMessageSending(declaration));
			}

			var setting = new CNSWClientSetting(new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory)
			{
				MachineName = "MachineName",
				RunningIntervalInSeconds = 60,
				SendFolder = @"D:\Folders\SendFolder\",
				ReceiveFolder = @"D:\Folders\ReceiveFolder\",
				ErrorResponseFolder = @"D:\Folders\ErrorResponseFolder\",
				ArchiveFolder = @"D:\Folders\ArchiveFolder\",
			};
			using (CNCustomsDataRegistry.Instance.CNSWClientSetting.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, setting))
			{
				AssertContains(CNSWClientSettingChecker.AcdaSendFolderIsEmptyMessage, CNSWClientSettingChecker.CheckForAcdAgrMessageSending(declaration));
			}

			setting.AcdaSendFolder = @"D:\Folders\AcdaSendFolder\";
			setting.AcdaReceiveFolder = @"D:\Folders\AcdaReceiveFolder\";
			setting.AcdaErrorResponseFolder = @"D:\Folders\AcdaErrorResponseFolder\";
			setting.AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder\";
			using (CNCustomsDataRegistry.Instance.CNSWClientSetting.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, setting))
			{
				AssertEquals("The Send Folder of Agreement of Customs Declaration Agent all set", ZString.Empty, CNSWClientSettingChecker.CheckForAcdAgrMessageSending(declaration));
			}

			using (CNCustomsDataRegistry.Instance.CNSWClientSetting.SetTemporaryValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, setting))
			{
				AssertEquals("No error when JE_GB = Branch1", ZString.Empty, CNSWClientSettingChecker.CheckForAcdAgrMessageSending(declaration));

				declaration.JE_GB = branch2.PK;
				AssertContains("Has error when JE_GB = Branch2.", CNSWClientSettingChecker.EHubClientNotRegisteredMessage, CNSWClientSettingChecker.CheckForAcdAgrMessageSending(declaration));
			}
		}

		public void TestCheckForDeclarationMessageSending()
		{
			var branch1 = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var company = branch1.Company;
			var branch2 = company.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch1.PK;

			var result = CNSWClientSettingChecker.CheckForDeclarationMessageSending(declaration);
			AssertEquals("Has error when Setting not set", CNSWClientSettingChecker.EHubClientNotRegisteredMessage, result);

			using (TemporarilySetCNSWClientSetting(Factory, declaration.CompanyPK.ToGuid(), Guid.Empty))
			{
				AssertEquals("No error when Setting on Company", ZString.Empty, CNSWClientSettingChecker.CheckForDeclarationMessageSending(declaration));
			}

			using (TemporarilySetCNSWClientSetting(Factory, Guid.Empty, branch1.PK.ToGuid()))
			{
				AssertEquals("No error when JE_GB = Branch1", ZString.Empty, CNSWClientSettingChecker.CheckForDeclarationMessageSending(declaration));

				declaration.JE_GB = branch2.PK;
				AssertContains("Has error when JE_GB = Branch2.", CNSWClientSettingChecker.EHubClientNotRegisteredMessage, CNSWClientSettingChecker.CheckForDeclarationMessageSending(declaration));
			}
		}

		public static IDisposable TemporarilySetCNSWClientSetting(Guid companyPK, Guid branchPK, CNSWClientSetting setting)
		{
			return CNCustomsDataRegistry.Instance.CNSWClientSetting.SetTemporaryValue(companyPK, branchPK, Guid.Empty, setting);
		}

		public static IDisposable TemporarilySetCNSWClientSetting(BusinessObjectFactory factory, Guid companyPK, Guid branchPK)
		{
			return TemporarilySetCNSWClientSetting(companyPK, branchPK, CreateCNSWClientSetting(factory, companyPK, branchPK));
		}

		public static CNSWClientSetting CreateCNSWClientSetting() => CreateCNSWClientSetting(null, null);

		public static CNSWClientSetting CreateCNSWClientSetting(BusinessObjectFactory factory, Guid companyPK, Guid branchPK)
		{
			return CreateCNSWClientSetting(new FallbackLevel(companyPK, branchPK, Guid.Empty), factory);
		}

		static CNSWClientSetting CreateCNSWClientSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CNSWClientSetting(fallbackLevel, factory)
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
				RunningIntervalInSeconds = 15
			};
		}
	}
}
