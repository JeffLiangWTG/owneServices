using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(DefaultBranchPGAContactRegistryDataType))]
	sealed class DefaultBranchPGAContactRegistryDataTypeTest : GuidRegistryDataTypeTest
	{
		public void TestGS_FullNameShouldNotHasInvalidCharacters()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_FullName = "ABC";
			staff.GS_LoginName = "ABC";
			var staff1 = factory.New<GlbStaff>();
			staff1.GS_Code = "BBB";
			staff1.GS_FullName = "赵四!|";
			staff1.GS_LoginName = "DEF";
			factory.Save();
			var defaultBranchPgaContact = CACustomsDataRegistry.Instance.DefaultBranchPgaContact;

			AssertNoExceptionThrown(() =>
			{
				defaultBranchPgaContact.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());
			});
			AssertExceptionThrown(typeof(RegistryValidationException), "The following characters are not allowed in the full name: '赵','四','!','|'", () =>
			{
				defaultBranchPgaContact.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff1.PK.ToGuid());
			});
		}

		protected override GuidRegistryDataType GetNewDataType()
		{
			return new DefaultBranchPGAContactRegistryDataType();
		}
	}
}
