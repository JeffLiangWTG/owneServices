using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(SendAcknowledgementsRegistry))]
	sealed class SendAcknowledgementsRegistryTest : RegistryBusinessObjectTemplateTestCase<SendAcknowledgementsRegistry>
	{
		public void TestEBSCode_Caption()
		{
			AssertEquals("EORI Branch Suffix", DataBoundResourceStrings.GetDataForProperty(sendAcknowledgementsRegistry.EBSCodeInfo).Caption);
		}

		public void TestEBSCodeList()
		{
			CombineAssertions(() =>
			{
				AssertEquals("sendAcknowledgementsRegistry", string.Empty, sendAcknowledgementsRegistry.EBSCodeList.CodesAsString);

				var newSendAcknowledgementsRegistry = new SendAcknowledgementsRegistry(CurrentFallbackLevel, Factory);
				AssertEquals("newSendAcknowledgementsRegistry", "0001, 0002, C001", newSendAcknowledgementsRegistry.EBSCodeList.CodesAsString);
			});
		}

		public void TestValidateEBSCode_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(sendAcknowledgementsRegistry.EBSCodeInfo);
		}

		public void TestValidateEBSCode_ListValidation()
		{
			sendAcknowledgementsRegistry.EBSCode = "0003";
			sendAcknowledgementsRegistry.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertNoErrors("CurrentFallbackLevel is null when reopen the registry", sendAcknowledgementsRegistry.EBSCodeInfo);

				var newSendAcknowledgementsRegistry = new SendAcknowledgementsRegistry(CurrentFallbackLevel, Factory);
				ValidationTestHelper.AssertErrorIfInvalidCode(newSendAcknowledgementsRegistry.EBSCodeInfo, "XXXX", "0001");
			});
		}

		public void TestValidateEBSCode_DuplicateCodes()
		{
			var message = "A row with this Code already exists.";
			var collection = new SendAcknowledgementsRegistryCollection(CurrentFallbackLevel, Factory);
			var newSendAcknowledgementsRegistry = collection.AddNew();
			newSendAcknowledgementsRegistry.EBSCode = "0001";

			CombineAssertions(() =>
			{
				var newSendAcknowledgementsRegistry2 = collection.AddNew();
				newSendAcknowledgementsRegistry2.EBSCode = "0001";
				AssertHasError("Has duplicate code", newSendAcknowledgementsRegistry2.EBSCodeInfo, message);

				newSendAcknowledgementsRegistry2.EBSCode = "0002";
				AssertNoError("No duplicate code", newSendAcknowledgementsRegistry2.EBSCodeInfo, message);

				newSendAcknowledgementsRegistry.EBSCode = "0003";
				newSendAcknowledgementsRegistry2.EBSCode = "0003";
				AssertNoError("Has duplicate and invalid code", newSendAcknowledgementsRegistry2.EBSCodeInfo, message);
			});
		}

		public void TestSendGroupPK_Caption()
		{
			AssertEquals("E-Mail Recipient Group", DataBoundResourceStrings.GetDataForProperty(sendAcknowledgementsRegistry.SendGroupPKInfo).Caption);
		}

		public void TestSendGroupList()
		{
			AssertType<GlbGroupCollection>(sendAcknowledgementsRegistry.SendGroupList);
		}

		public void TestValidateSendGroupPK_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(sendAcknowledgementsRegistry.SendGroupPKInfo);
		}

		public void TestValidateSendGroupPK_ListValidation()
		{
			ValidationTestHelper.AssertErrorIfInvalidPK(sendAcknowledgementsRegistry.SendGroupPKInfo, ZGuid.Invalid, Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestRunPreSaveValidation()
		{
			sendAcknowledgementsRegistry.RunPreSaveValidation();
			CombineAssertions(() =>
			{
				AssertHasErrorContaining("EBS Validation", sendAcknowledgementsRegistry.EBSCodeInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining("SendGroup Validation", sendAcknowledgementsRegistry.SendGroupPKInfo, MandatoryValidation.MustBeEntered);
			});
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override SendAcknowledgementsRegistry GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override SendAcknowledgementsRegistry GetBusinessObjectToSerialise()
		{
			BizObj.EBSCode = "0001";
			BizObj.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			return BizObj;
		}

		protected override void SetUp()
		{
			base.SetUp();
			sendAcknowledgementsRegistry = new SendAcknowledgementsRegistry();
		}
		SendAcknowledgementsRegistry sendAcknowledgementsRegistry;

		FallbackLevel CurrentFallbackLevel
		{
			get
			{
				if (currentFallbackLevel == null)
				{
					var company = GetCompanyWithCustomsCodes();
					currentFallbackLevel = new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				}
				return currentFallbackLevel;
			}
		}
		FallbackLevel currentFallbackLevel;

		GlbCompany GetCompanyWithCustomsCodes()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			company.GC_Code = "DE1";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "DE1OH";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "DE1OH2";
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "DE1OH3";

			var br1 = company.Branches.AddNew();
			br1.GB_Code = "BR1";
			br1.GB_OH_OrgProxy = orgHeader.PK;

			var br2 = company.Branches.AddNew();
			br2.GB_Code = "BR2";
			br2.GB_OH_OrgProxy = orgHeader2.PK;

			var br3 = company.Branches.AddNew();
			br3.GB_Code = "BR3";
			br3.GB_IsActive = false;
			br3.GB_OH_OrgProxy = orgHeader3.PK;

			var noProxyBranch = company.Branches.AddNew();
			noProxyBranch.GB_Code = "NON";

			var customsCode1 = br1.OrgProxy.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "0001", Core.Constants.CountryCodes.Germany);
			customsCode1.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			var customsCode2 = br2.OrgProxy.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "0001", Core.Constants.CountryCodes.Germany);
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.FillWithValidTestData();
			customsCode2.OK_OA_PremisesAddress = orgAddress.PK;
			br1.OrgProxy.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "0002", Core.Constants.CountryCodes.Germany);
			br3.OrgProxy.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "0002", Core.Constants.CountryCodes.Germany);

			var companyOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			companyOrgHeader.OH_Code = "DE1";

			company.GC_OH_OrgProxy = companyOrgHeader.PK;
			companyOrgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "C001", Core.Constants.CountryCodes.Germany);
			Factory.Save();

			return company;
		}
	}
}
