using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.DE.Messaging.MessageSchema;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CusTempStorageLineBaseOnlyTest : TestCaseWithFactory
	{
		public void TestTSL_GoodsDescription()
		{
			AssertEquals(CusTempStorageLine.Schema.TSL_GoodsDescriptionMaxLength, storageLine.TSL_GoodsDescriptionInfo.MaxLength);
		}

		public void TestTSL_OwnerReferenceNumber()
		{
			AssertEquals(CusTempStorageLine.Schema.TSL_OwnerReferenceNumberMaxLength, storageLine.TSL_OwnerReferenceNumberInfo.MaxLength);
		}

		public void TestCustodianName()
		{
			CombineAssertions(() =>
			{
				var custodianOrg = Factory.NewWithValidTestData<OrgHeader>();
				custodianOrg.OH_Code = "CUSTODIAN";
				custodianOrg.OH_FullName = "TEST CUSTODIAN NAME";
				var custodianAddress = custodianOrg.Addresses.AddNew();
				AssertEquals("ReadOnly", true, storageLine.CustodianNameInfo.ReadOnly);
				storageLine.TSL_OA_Custodian = custodianAddress.PK;
				AssertEquals("Custodian Name", "TEST CUSTODIAN NAME", storageLine.CustodianName);
			});
		}

		public void TestDefaultOfTSL_OA_Custodian_ZAddress()
		{
			var custodian = Factory.New<OrgHeader>();
			var mainAddress = custodian.Addresses.AddNew();
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			mainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			storageLine.TSL_OA_Custodian_ZAddress.OrgPK = custodian.PK;
			AssertEquals(mainAddress.PK, storageLine.TSL_OA_Custodian);
		}

		public void TestDefaultOfTSL_OA_GoodsOwner_ZAddress()
		{
			var disposalEntitledTrader = Factory.New<OrgHeader>();
			var mainAddress = disposalEntitledTrader.Addresses.AddNew();
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			mainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			storageLine.TSL_OA_GoodsOwner_ZAddress.OrgPK = disposalEntitledTrader.PK;
			AssertEquals(mainAddress.PK, storageLine.TSL_OA_GoodsOwner);
		}

		public void TestTSL_LineNo()
		{
			storageLine.TSL_LineNo = 10000;
			CombineAssertions(() =>
			{
				AssertEquals("Max Allowed Value", 9999, storageLine.TSL_LineNo);
				AssertEquals("Max Length", 4, storageLine.TSL_LineNoInfo.MaxLength);
			});
		}

		public void TestGoodsOwnerName()
		{
			CombineAssertions(() =>
			{
				var goodsOwnerOrg = Factory.NewWithValidTestData<OrgHeader>();
				goodsOwnerOrg.OH_Code = "TESTORG";
				goodsOwnerOrg.OH_FullName = "TEST Goods Owner NAME";
				var goodsOwnerAddress = goodsOwnerOrg.Addresses.AddNew();
				AssertEquals("ReadOnly", true, storageLine.GoodsOwnerNameInfo.ReadOnly);
				storageLine.TSL_OA_GoodsOwner = goodsOwnerAddress.PK;
				AssertEquals("Goods Owner Name", "TEST Goods Owner NAME", storageLine.GoodsOwnerName);
			});
		}

		public void TestIsSingleCountPackgeType()
		{
			CombineAssertions(() =>
			{
				foreach (var packageType in new ZString[] { "VG", "VL", "VO", "VQ", "VR", "VS", "VY" })
				{
					storageLine.TSL_PackageType = packageType;
					AssertEquals("Single PackageType" + packageType, true, storageLine.IsSingleCountPackgeType);
				}
				storageLine.TSL_PackageType = "12";
				AssertEquals("Not single", false, storageLine.IsSingleCountPackgeType);
			});
		}

		public void TestIsULD()
		{
			CombineAssertions(() =>
			{
				storageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ULD;
				AssertEquals("Is ULD", true, storageLine.IsULD);
				storageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;
				AssertEquals("Not ULD", false, storageLine.IsULD);
			});
		}

		public void TestPackageQtyShouldBeOne()
		{
			CombineAssertions(() =>
			{
				Assert(!storageLine.PackageQtyShouldBeOne);
				storageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ULD;
				Assert(storageLine.PackageQtyShouldBeOne);
				storageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;
				Assert(!storageLine.PackageQtyShouldBeOne);
				storageLine.TSL_PackageType = "VG";
				Assert(storageLine.PackageQtyShouldBeOne);
				storageLine.TSL_PackageType = "12";
				Assert(!storageLine.PackageQtyShouldBeOne);
			});
		}

		public void TestTSL_CustodianIdentifier()
		{
			AssertEquals(ATLASMessageSchema.EoriCodeMaxLength, storageLine.TSL_CustodianIdentifierInfo.MaxLength);
		}

		public void TestTSL_CustodianIdentifierBranchNo()
		{
			AssertEquals(ATLASMessageSchema.EoriBranchCodeMaxLength, storageLine.TSL_CustodianIdentifierBranchNoInfo.MaxLength);
		}

		public void TestEoriAndBranchFromCustodian()
		{
			CombineAssertions(() =>
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var storageHeader = Factory.New<CusTempStorageJobHeader>();
				storageHeader.SJH_OH_Customer = organisation.PK;
				var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
				storageDec.CusTempStorageLines.Add(storageLine);

				Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
				var custodianOrg = Factory.New<OrgHeader>();
				var custodianAddress1 = custodianOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z12345678901234", custodianOrg, custodianAddress1, null);

				storageLine.TSL_OA_Custodian = custodianAddress1.PK;
				AssertEquals("Custodian loaded", custodianAddress1, storageLine.Custodian);
				AssertEquals("Custodian Eori", "NLZ12345678901234", storageLine.TSL_CustodianIdentifier);
				AssertEquals("Custodian Branch", "0001", storageLine.TSL_CustodianIdentifierBranchNo);

				storageLine.TSL_OA_Custodian = ZGuid.Empty;
				AssertNull("Empty Custodian", storageLine.Custodian);
				AssertEquals("Empty Eori", ZString.Empty, storageLine.TSL_CustodianIdentifier);
				AssertEquals("Empty Branch", ZString.Empty, storageLine.TSL_CustodianIdentifierBranchNo);
			});
		}

		public void TestCustodianFromEoriAndSingleBranch()
		{
			CombineAssertions(() =>
			{
				var custodianOrg = Factory.New<OrgHeader>();
				var custodianAddress1 = custodianOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z98765432122", custodianOrg, custodianAddress1, null);

				storageLine.TSL_CustodianIdentifier = "NLZ98765432122";
				AssertEquals("Branch", "0001", storageLine.TSL_CustodianIdentifierBranchNo);
				AssertEquals("Address", custodianAddress1.PK, storageLine.TSL_OA_Custodian);
			});
		}

		public void TestCustodianFromEoriAndMultipleBranches()
		{
			CombineAssertions(() =>
			{
				var custodianOrg = Factory.New<OrgHeader>();
				var custodianAddress1 = custodianOrg.Addresses.AddNew();
				var custodianAddress2 = custodianOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z98765432123", custodianOrg, custodianAddress1, custodianAddress2);

				storageLine.TSL_CustodianIdentifier = "NLZ98765432123";
				AssertNull("Custodian", storageLine.Custodian);
				AssertEquals("Branch", ZString.Empty, storageLine.TSL_CustodianIdentifierBranchNo);
			});
		}

		public void TestSuspendSettingDefaultCustodianDetails_TSL_OA_Custodian()
		{
			CombineAssertions(() =>
			{
				var custodianOrg = Factory.New<OrgHeader>();
				var custodianAddress1 = custodianOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z98765432124", custodianOrg, custodianAddress1, null);

				using (storageLine.SuspendSettingDefaultCustodianDetails())
				{
					storageLine.TSL_OA_Custodian = custodianAddress1.PK;
					AssertEquals("Eori", ZString.Empty, storageLine.TSL_CustodianIdentifier);
					AssertEquals("Branch", ZString.Empty, storageLine.TSL_CustodianIdentifierBranchNo);
				}
			});
		}

		public void TestSuspendSettingDefaultCustodianDetails_TSL_CustodianIdentifier()
		{
			CombineAssertions(() =>
			{
				var custodianOrg = Factory.New<OrgHeader>();
				var custodianAddress1 = custodianOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z987654321", custodianOrg, custodianAddress1, null);

				using (storageLine.SuspendSettingDefaultCustodianDetails())
				{
					storageLine.TSL_CustodianIdentifier = "NLZ987654321";
					AssertEquals("Branch", ZString.Empty, storageLine.TSL_CustodianIdentifierBranchNo);
					AssertEquals("OrgAddress", ZGuid.Empty, storageLine.TSL_OA_Custodian);
				}
			});
		}

		public void TestSuspendSettingDefaultCustodianDetails_TSL_CustodianIdentifierBranchNo()
		{
			var custodianOrg = Factory.New<OrgHeader>();
			var custodianAddress1 = custodianOrg.Addresses.AddNew();
			SetupEoriAndBranches("Z987654321", custodianOrg, custodianAddress1, null);

			using (storageLine.SuspendSettingDefaultCustodianDetails())
			{
				storageLine.TSL_CustodianIdentifier = "NLZ987654321";
				storageLine.TSL_CustodianIdentifierBranchNo = "0001";
				AssertEquals(ZGuid.Empty, storageLine.TSL_OA_Custodian);
			}
		}

		public void TestTSL_GoodsOwnerIdentifierInfo()
		{
			AssertEquals(ATLASMessageSchema.EoriCodeMaxLength, storageLine.TSL_GoodsOwnerIdentifierInfo.MaxLength);
		}

		public void TestDisposalEntitledTraderBranch()
		{
			AssertEquals(4, storageLine.TSL_GoodsOwnerIdentifierBranchNoInfo.MaxLength);
		}

		public void TestEoriAndBranchFromDisposalEntitledTrader()
		{
			CombineAssertions(() =>
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var storageHeader = Factory.New<CusTempStorageJobHeader>();
				storageHeader.SJH_OH_Customer = organisation.PK;
				var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
				storageDec.CusTempStorageLines.Add(storageLine);

				Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
				var disposalOrg = Factory.New<OrgHeader>();
				var disposalAddress1 = disposalOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z12345678901234", disposalOrg, disposalAddress1, null);

				storageLine.TSL_OA_GoodsOwner = disposalAddress1.PK;
				AssertEquals("Disposal Trader Loaded", disposalAddress1, storageLine.GoodsOwner);
				AssertEquals("Disposal Eori", "NLZ12345678901234", storageLine.TSL_GoodsOwnerIdentifier);
				AssertEquals("Disposal Trader Branch", "0001", storageLine.TSL_GoodsOwnerIdentifierBranchNo);

				storageLine.TSL_OA_GoodsOwner = ZGuid.Empty;
				AssertEquals("Empty Disposal Trader", null, storageLine.Custodian);
				AssertEquals("Empty Disposal Trader Eori", ZString.Empty, storageLine.TSL_GoodsOwnerIdentifier);
				AssertEquals("Empty Disposal Trader Branch", ZString.Empty, storageLine.TSL_GoodsOwnerIdentifierBranchNo);
			});
		}

		public void TestDisposalEntitledTraderFromEoriAndSingleBranch()
		{
			CombineAssertions(() =>
			{
				var disposalOrg = Factory.New<OrgHeader>();
				var disposalAddress = disposalOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z123456789", disposalOrg, disposalAddress, null);

				storageLine.TSL_GoodsOwnerIdentifier = "NLZ123456789";
				AssertEquals("Branch", "0001", storageLine.TSL_GoodsOwnerIdentifierBranchNo);
				AssertEquals("Address", disposalAddress.PK, storageLine.TSL_OA_GoodsOwner);
			});
		}

		public void TestDisposalEntitledTraderFromEoriAndMultipleBranches()
		{
			CombineAssertions(() =>
			{
				var disposalOrg = Factory.New<OrgHeader>();
				var disposalAddress1 = disposalOrg.Addresses.AddNew();
				var disposalAddress2 = disposalOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z123456789256", disposalOrg, disposalAddress1, disposalAddress2);

				storageLine.TSL_GoodsOwnerIdentifier = "NLZ123456789256";
				AssertNull("Address", storageLine.GoodsOwner);
				AssertEquals("Branch", ZString.Empty, storageLine.TSL_GoodsOwnerIdentifierBranchNo);
			});
		}

		public void TestDefaultOfCustodianZAddressFromEoriCodeAndBranch()
		{
			CombineAssertions(() =>
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var storageHeader = Factory.New<CusTempStorageJobHeader>();
				storageHeader.SJH_OH_Customer = organisation.PK;
				var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
				storageDec.CusTempStorageLines.Add(storageLine);

				Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
				var custodianOrg = Factory.New<OrgHeader>();
				var custodianAddress1 = custodianOrg.Addresses.AddNew();
				var custodianAddress2 = custodianOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z123456789", custodianOrg, custodianAddress1, custodianAddress2);

				storageLine.TSL_CustodianIdentifier = "NLZ123456789";
				storageLine.TSL_CustodianIdentifierBranchNo = "0001";
				AssertEquals("Custodian 1", custodianAddress1, storageLine.Custodian);
				storageLine.TSL_CustodianIdentifierBranchNo = "0002";
				AssertEquals("Custodian 2", custodianAddress2, storageLine.Custodian);
				storageLine.TSL_CustodianIdentifierBranchNo = "0003";
				AssertEquals("Custodian 2 as that branch not in config", custodianAddress2, storageLine.Custodian);
			});
		}

		public void TestDefaultOfDisposalEntitledTraderZAddressFromEoriCodeAndBranch()
		{
			CombineAssertions(() =>
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var storageHeader = Factory.New<CusTempStorageJobHeader>();
				storageHeader.SJH_OH_Customer = organisation.PK;
				var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
				storageDec.CusTempStorageLines.Add(storageLine);

				Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
				var disposalOrg = Factory.New<OrgHeader>();
				var disposalAddress1 = disposalOrg.Addresses.AddNew();
				var disposalAddress2 = disposalOrg.Addresses.AddNew();
				SetupEoriAndBranches("Z123456789", disposalOrg, disposalAddress1, disposalAddress2);

				storageLine.TSL_GoodsOwnerIdentifier = "NLZ123456789";
				storageLine.TSL_GoodsOwnerIdentifierBranchNo = "0001";
				AssertEquals("Disposal Entitled Trader 1", disposalAddress1, storageLine.GoodsOwner);
				storageLine.TSL_GoodsOwnerIdentifierBranchNo = "0002";
				AssertEquals("Disposal Entitled Trader 2", disposalAddress2, storageLine.GoodsOwner);
				storageLine.TSL_GoodsOwnerIdentifierBranchNo = "0003";
				AssertEquals("Disposal Entitled Trader 2 as no branch in config", disposalAddress2, storageLine.GoodsOwner);
			});
		}

		public void TestIsAWBDeclaration()
		{
			AssertDeclarationIdentificationIndicator(Messaging.TemporaryStorageIdentificationIndicatorList.Codes.AWB, () => storageLine.IsAWBDeclaration);
		}

		public void TestIsREGDeclaration()
		{
			AssertDeclarationIdentificationIndicator(Messaging.TemporaryStorageIdentificationIndicatorList.Codes.REG, () => storageLine.IsREGDeclaration);
		}

		public void TestIsSINDeclaration()
		{
			AssertDeclarationIdentificationIndicator(Messaging.TemporaryStorageIdentificationIndicatorList.Codes.SIN, () => storageLine.IsSINDeclaration);
		}

		public void TestTSL_RN_NKDepartureCountry_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(storageLine.TSL_RN_NKDepartureCountryInfo, multipleResourceKey: null, "Country/Region Of Departure", "Departure Ctry./Rgn.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageLine = Factory.New<CusTempStorageLineBaseForTest>();
		}
		CusTempStorageLineBaseForTest storageLine;

		void SetupEoriAndBranches(ZString eoriRegoValue, OrgHeader orgHeader, OrgAddress address1, OrgAddress address2)
		{
			var eoriCode = orgHeader.CustomsCodes.AddNew();
			eoriCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
			eoriCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eoriCode.OK_CustomsRegNo = eoriRegoValue;
			var branchCode1 = address1.CustomsCodes.AddNew();
			branchCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			branchCode1.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			branchCode1.OK_CustomsRegNo = "0001";
			if (address2 != null)
			{
				var branchCode2 = address2.CustomsCodes.AddNew();
				branchCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
				branchCode2.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
				branchCode2.OK_CustomsRegNo = "0002";
			}
		}

		void AssertDeclarationIdentificationIndicator(ZString identificationIndicator, Func<bool> propertyToTest)
		{
			var storageDec = Factory.New<CUSPRLCusTempStorageDec>();
			storageDec.CusTempStorageLines.Add(storageLine);

			CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = ZString.Empty;
				AssertEquals("IdentificationIndicator empty", false, propertyToTest.Invoke());

				storageDec.STH_IdentificationIndicator = identificationIndicator;
				AssertEquals($"IdentificationIndicator: {identificationIndicator}", true, propertyToTest.Invoke());
			});
		}
	}

	class CusTempStorageLineBaseForTest : CusTempStorageLine
	{
		public CusTempStorageLineBaseForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
