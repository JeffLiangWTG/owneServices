using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNSWClientSettingRegistryDataType))]
	class CNSWClientSettingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CNSWClientSettingRegistryDataType>
	{
		public void TestValidateDuplicateMachineName()
		{
			var factory = new BusinessObjectFactory();
			var branch1 = CreateBranch(factory, "CN1", "CB1");
			var branch2 = CreateBranch(factory, "CN2", "CB2");
			factory.Save();

			var registryItem = CNCustomsDataRegistry.Instance.CNSWClientSetting;
			var setting1 = CNSWClientSettingCheckerTest.CreateCNSWClientSetting(factory, branch1.Company.PK.ToGuid(), Guid.Empty);
			registryItem.SetValue(branch1.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, setting1);
			AssertExceptionThrown<RegistryValidationException>("Duplicate Machine Name found", "There is already a company or branch which has a same Machine Name.", () =>
			{
				registryItem.SetValue(branch2.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, setting1);
			});
			AssertExceptionThrown<RegistryValidationException>("Duplicate Machine Name found", "There is already a company or branch which has a same Machine Name.", () =>
			{
				registryItem.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, setting1);
			});

			var setting2 = CNSWClientSettingCheckerTest.CreateCNSWClientSetting(factory, branch1.Company.PK.ToGuid(), Guid.Empty);
			((IRegistryItemInternals)registryItem).SetProposedValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, setting2);
			((IRegistryItemInternals)registryItem).SetCurrentValueToUse(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, ValueToUse.ProposedValue);
			AssertExceptionThrown<RegistryValidationException>("Duplicate Machine Name found", "There is already a company or branch which has a same Machine Name.", () =>
			{
				registryItem.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, setting2);
			});

			AssertNoExceptionThrown("Duplicate Machine Name not found", () =>
			{
				registryItem.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, CNSWClientSettingCheckerTest.CreateCNSWClientSetting(factory, Guid.Empty, branch2.PK.ToGuid()));
			});
		}

		protected override CNSWClientSettingRegistryDataType GetNewDataType() => new CNSWClientSettingRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new CNSWClientSetting { MachineName = "Machine Name", SendFolder = @"D:\Folders\SendFolder", ReceiveFolder = @"D:\Folders\ReceiveFolder", ArchiveFolder = @"D:\Folders\ArchiveFolder", ErrorResponseFolder = @"D:\Folders\ErrorResponseFolder", AcdaSendFolder = @"D:\Folders\AcdaSendFolder", AcdaReceiveFolder = @"D:\Folders\AcdaReceiveFolder", AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder", AcdaErrorResponseFolder = @"D:\Folders\AcdaErrorResponseFolder", RunningIntervalInSeconds = 60 };
			var sample2 = new CNSWClientSetting { MachineName = "Machine Name 2", SendFolder = @"D:\Folders\SendFolder", ReceiveFolder = @"D:\Folders\ReceiveFolder", ArchiveFolder = @"D:\Folders\ArchiveFolder", ErrorResponseFolder = @"D:\Folders\ErrorResponseFolder", AcdaSendFolder = @"D:\Folders\AcdaSendFolder", AcdaReceiveFolder = @"D:\Folders\AcdaReceiveFolder", AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder", AcdaErrorResponseFolder = @"D:\Folders\AcdaErrorResponseFolder", RunningIntervalInSeconds = 60 };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, new CNSWClientSettingRegistryDataType().Serialise(sample1)),
				new ValidSampleAndBinaryValueInDB(sample2, new CNSWClientSettingRegistryDataType().Serialise(sample2))
			};
		}

		protected override string ExpectedEditorName => "CNSWClientSettingRegistryItemEditor";

		GlbBranch CreateBranch(BusinessObjectFactory factory, string companyCode, string branchCode)
		{
			var company = factory.New<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_RN_NKCountryCode = "CN";
			var branch = company.Branches.AddNew();
			branch.GB_Code = branchCode;
			return branch;
		}
	}
}
