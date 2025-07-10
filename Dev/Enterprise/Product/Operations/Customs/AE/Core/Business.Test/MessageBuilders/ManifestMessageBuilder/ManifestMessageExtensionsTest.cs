using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(ManifestMessageExtensions))]
sealed class ManifestMessageExtensionsTest : TestCaseWithFactory
{
	public void TestAeOrgProxyForManifestMessage() => CombineAssertions(() =>
	{
		AssertEquals("When CurrentCompany = AE, AeOrgProxyForManifestMessage should be AE's OrgProxy", GlbCompany.CurrentCompany.OrgProxy, ManifestMessageExtensions.AeOrgProxyForManifestMessage(Factory));

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
			company1.GC_Code = "DAE";
			var proxy1 = Factory.New<OrgHeader>();
			proxy1.OH_Code = "Proxy1";
			proxy1.CustomsCodes.AddNew(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "AABPVQA", Core.Constants.CountryCodes.UnitedArabEmirates);
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "AUH";
			branch1.GB_BranchName = "AE - Branch 1";
			branch1.GB_OH_OrgProxy = proxy1.PK;
			Factory.Save();

			using (AECustomsRegistry.Instance.DefaultBranchForManifestSubmission.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "AUH"))
			{
				AssertEquals("When CurrentCompany != AE, AeOrgProxyForManifestMessage should be registry branch's OrgProxy", proxy1.PK, ManifestMessageExtensions.AeOrgProxyForManifestMessage(Factory).PK);
			}
		}
	});

	public void TestAeBranchForManifestMessageFromRegistry() => CombineAssertions(() =>
	{
		AssertEquals("When CurrentCompany = AE, AeBranchForManifestMessageFromRegistry should be null", null, ManifestMessageExtensions.AeBranchForManifestMessageFromRegistry(Factory));

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
			company1.GC_Code = "DAE";
			var proxy1 = Factory.New<OrgHeader>();
			proxy1.OH_Code = "Proxy1";
			proxy1.CustomsCodes.AddNew(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "AABPVQA", Core.Constants.CountryCodes.UnitedArabEmirates);
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "AUH";
			branch1.GB_BranchName = "AE - Branch 1";
			branch1.GB_OH_OrgProxy = proxy1.PK;
			Factory.Save();

			using (AECustomsRegistry.Instance.DefaultBranchForManifestSubmission.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "AUH"))
			{
				AssertEquals("When CurrentCompany != AE, AeBranchForManifestMessageFromRegistry's GB_Code should be AUH", "AUH", ManifestMessageExtensions.AeBranchForManifestMessageFromRegistry(Factory).GB_Code);
			}
		}
	});
}
