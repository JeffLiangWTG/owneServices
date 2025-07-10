using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusDV1Detail = Enterprise.Customs.EU.Business.Declaration.CusDV1Detail;
using YesNoList = Enterprise.Customs.Business.YesNoList;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CustomsValueProviderTest : Customs.Business.Testing.DataProviderTestCase<CustomsValueProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CustomsValueProvider(null));
		}

		public void TestDV1DetailForEntryInstruction()
		{
			declaration.DV1Details.AddNew();
			declaration.DV1Details.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction2.PK;

			CombineAssertions(() =>
			{
				AssertNull("No linked dv1Detail", new CustomsValueProvider(entryHeader).DV1DetailForEntryInstruction);

				var provider = new CustomsValueProvider(entryHeader);
				entryInstruction2.DV1DetailsPivots.Last().IsForEntryInstruction = true;
				var linkedDV1Detail = provider.DV1DetailForEntryInstruction;
				AssertEquals("Second dv1Detail is linked", (short)3, linkedDV1Detail.Sequence);

				AssertSame("Cached", linkedDV1Detail, provider.DV1DetailForEntryInstruction);
			});
		}

		public void TestFormerDecisions()
		{
			dv1Detail.DV1_CustomsDecisionNumber = "CDN";
			AssertEquals("CDN", Provider.FormerDecisions);
		}

		public void TestFormerDecisions_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_CustomsDecisionNumber = "CDN";
			AssertProperty_NoLinkedDV1Detail(null, () => Provider.FormerDecisions);
		}

		public void TestAffiliationType_0()
		{
			dv1Detail.DV1_Relationship = YesNoList.Codes.No;
			AssertEquals("0", Provider.AffiliationType);
		}

		public void TestAffiliationType_1()
		{
			dv1Detail.DV1_Relationship = YesNoList.Codes.Yes;
			dv1Detail.DV1_PriceInfluence = YesNoList.Codes.No;
			AssertEquals("1", Provider.AffiliationType);
		}

		public void TestAffiliationType_2()
		{
			dv1Detail.DV1_Relationship = YesNoList.Codes.Yes;
			dv1Detail.DV1_PriceInfluence = YesNoList.Codes.Yes;
			AssertEquals("2", Provider.AffiliationType);
		}

		public void TestAffiliationType_RelationshipEmpty()
		{
			dv1Detail.DV1_Relationship = string.Empty;
			dv1Detail.DV1_PriceInfluence = YesNoList.Codes.Yes;
			AssertNull(Provider.AffiliationType);
		}

		public void TestAffiliationType_PriceInfluenceEmpty()
		{
			dv1Detail.DV1_Relationship = YesNoList.Codes.Yes;
			dv1Detail.DV1_PriceInfluence = string.Empty;
			AssertNull(Provider.AffiliationType);
		}

		public void TestAffiliationType_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_Relationship = YesNoList.Codes.No;
			AssertProperty_NoLinkedDV1Detail(null, () => Provider.AffiliationType);
		}

		public void TestAffiliationDescription()
		{
			dv1Detail.DV1_RelationDetails = "Relation details";
			AssertEquals("Relation details", Provider.AffiliationDescription);
		}

		public void TestAffiliationDescription_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_RelationDetails = "Relation details";
			AssertProperty_NoLinkedDV1Detail(null, () => Provider.AffiliationDescription);
		}

		public void TestRestrictionFlag_True()
		{
			dv1Detail.DV1_Restrictions = YesNoList.Codes.Yes;
			AssertEquals(true, Provider.RestrictionFlag);
		}

		public void TestRestrictionFlag_False()
		{
			dv1Detail.DV1_Restrictions = YesNoList.Codes.No;
			AssertEquals(false, Provider.RestrictionFlag);
		}

		public void TestRestrictionFlag_RestrictionsEmpty()
		{
			dv1Detail.DV1_Restrictions = string.Empty;
			AssertEquals(false, Provider.RestrictionFlag);
		}

		public void TestRestrictionFlag_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_Restrictions = YesNoList.Codes.Yes;
			AssertProperty_NoLinkedDV1Detail(false, () => Provider.RestrictionFlag);
		}

		public void TestConditionFlag_True()
		{
			dv1Detail.DV1_Consideration = YesNoList.Codes.Yes;
			AssertEquals(true, Provider.ConditionFlag);
		}

		public void TestConditionFlag_False()
		{
			dv1Detail.DV1_Consideration = YesNoList.Codes.No;
			AssertEquals(false, Provider.ConditionFlag);
		}

		public void TestConditionFlag_ConsiderationEmpty()
		{
			dv1Detail.DV1_Consideration = string.Empty;
			AssertEquals(false, Provider.ConditionFlag);
		}

		public void TestConditionFlag_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_Consideration = YesNoList.Codes.Yes;
			AssertProperty_NoLinkedDV1Detail(false, () => Provider.ConditionFlag);
		}

		public void TestRestrictionOrConditionDescription()
		{
			dv1Detail.DV1_RestrictionConsiderationDetails = "Restriction consideration details";
			AssertEquals("Restriction consideration details", Provider.RestrictionOrConditionDescription);
		}

		public void TestRestrictionOrConditionDescription_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_RestrictionConsiderationDetails = "Restriction consideration details";
			AssertProperty_NoLinkedDV1Detail(null, () => Provider.RestrictionOrConditionDescription);
		}

		public void TestLicenseFeeFlag_True()
		{
			dv1Detail.DV1_RoyaltiesLicence = YesNoList.Codes.Yes;
			AssertEquals(true, Provider.LicenseFeeFlag);
		}

		public void TestLicenseFeeFlag_False()
		{
			dv1Detail.DV1_RoyaltiesLicence = YesNoList.Codes.No;
			AssertEquals(false, Provider.LicenseFeeFlag);
		}

		public void TestLicenseFeeFlag_RoyaltiesLicenceEmpty()
		{
			dv1Detail.DV1_RoyaltiesLicence = string.Empty;
			AssertEquals(false, Provider.LicenseFeeFlag);
		}

		public void TestLicenseFeeFlag_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_RoyaltiesLicence = YesNoList.Codes.Yes;
			AssertProperty_NoLinkedDV1Detail(false, () => Provider.LicenseFeeFlag);
		}

		public void TestLicenseFeeDescription()
		{
			dv1Detail.DV1_RoyaltiesLicenceDetails = "Royalties licence details";
			AssertEquals("Royalties licence details", Provider.LicenseFeeDescription);
		}

		public void TestLicenseFeeDescription_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_RoyaltiesLicenceDetails = "Royalties licence details";
			AssertProperty_NoLinkedDV1Detail(null, () => Provider.LicenseFeeDescription);
		}

		public void TestResaleFlag_True()
		{
			dv1Detail.DV1_Resale = YesNoList.Codes.Yes;
			AssertEquals(true, Provider.ResaleFlag);
		}

		public void TestResaleFlag_False()
		{
			dv1Detail.DV1_Resale = YesNoList.Codes.No;
			AssertEquals(false, Provider.ResaleFlag);
		}

		public void TestResaleFlag_ResaleEmpty()
		{
			dv1Detail.DV1_Resale = string.Empty;
			AssertEquals(false, Provider.ResaleFlag);
		}

		public void TestResaleFlag_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_Resale = YesNoList.Codes.Yes;
			AssertProperty_NoLinkedDV1Detail(false, () => Provider.ResaleFlag);
		}

		public void TestResaleDescription()
		{
			dv1Detail.DV1_ResaleDetails = "Resale details";
			AssertEquals("Resale details", Provider.ResaleDescription);
		}

		public void TestResaleDescription_NoLinkedDV1Detail()
		{
			dv1Detail.DV1_ResaleDetails = "Resale details";
			AssertProperty_NoLinkedDV1Detail(null, () => Provider.ResaleDescription);
		}

		public void TestVendor_NotPopulated()
		{
			AssertNull(Provider.Vendor);
		}

		public void TestVendor_WhenEmptyInInvoiceHeader_AndFilledInDeclaration_ShouldBeTakenFromDeclarationLevel()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var vendorAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				declaration.JE_OA_SellerAddress = vendorAddress.PK;
				declaration.ZG_IsHighValueOvrd = true;
				AssertNotNull("Populated", Provider.Vendor);
				AssertEquals("Vendor's EORI", "GREOR1", Provider.Vendor.Identification.EoriNumber);
				AssertEquals("Vendor's PK", vendorAddress.PK, Provider.VendorPK);
			});
		}

		public void TestVendor_WhenFilledInInvoiceHeader_AndFilledInDeclaration_ShouldBeTakenFromInvoiceHeader()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var declarationVendorAddress = GetOrgWithEORNumberAndEORIBranch("EOR1_DECL", "EBS1");
			declaration.JE_OA_SellerAddress = declarationVendorAddress.PK;
			declaration.ZG_IsHighValueOvrd = true;

			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var headerVendorAddress = GetOrgWithEORNumberAndEORIBranch("EOR2_HDR", "EBS2");
			header.JZ_OA_SellerAddress = headerVendorAddress.PK;

			AssertNotNull("Populated", Provider.Vendor);
			AssertEquals("Vendor is from InvoiceHeader", "GREOR2_HDR", Provider.Vendor.Identification.EoriNumber);
			AssertEquals("VendorPK is from InvoiceHeader", headerVendorAddress.PK, Provider.VendorPK);
		}

		public void TestVendor_WhenFilledInInvoiceHeader_AndEmptyInDeclaration_ShouldBeTakenFromInvoiceHeader()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var headerVendorAddress = GetOrgWithEORNumberAndEORIBranch("EOR2_HDR", "EBS2");
			header.JZ_OA_SellerAddress = headerVendorAddress.PK;

			AssertNotNull("Populated", Provider.Vendor);
			AssertEquals("Vendor is from InvoiceHeader", "GREOR2_HDR", Provider.Vendor.Identification.EoriNumber);
			AssertEquals("VendorPK is from InvoiceHeader", headerVendorAddress.PK, Provider.VendorPK);
		}

		public void TestVendor_WhenTwoEntries_EachHasFilledVendorInInvoiceHeader_ShouldBeTakenFromCorrespondingInvoiceHeader()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var header1 = declaration.Invoices.AddNew();
			var invoiceLine1_1 = header1.InvoiceLines.AddNew();
			invoiceLine1_1.JI_CEI = entryInstruction.PK;

			var headerVendorAddress1 = GetOrgWithEORNumberAndEORIBranch("EOR_HDR1", "EBS1");
			header1.JZ_OA_SellerAddress = headerVendorAddress1.PK;

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;

			var header2 = declaration.Invoices.AddNew();
			var invoiceLine2_1 = header2.InvoiceLines.AddNew();
			invoiceLine2_1.JI_CEI = entryInstruction2.PK;

			var headerVendorAddress2 = GetOrgWithEORNumberAndEORIBranch("EOR_HDR2", "EBS2");
			header2.JZ_OA_SellerAddress = headerVendorAddress2.PK;

			AssertNotNull("Populated", Provider.Vendor);
			AssertEquals("Vendor is from InvoiceHeader1", "GREOR_HDR1", Provider.Vendor.Identification.EoriNumber);
			AssertEquals("VendorPK is from InvoiceHeader1", headerVendorAddress1.PK, Provider.VendorPK);

			var provider2 = new CustomsValueProvider(entryHeader2);

			AssertNotNull("Populated", provider2.Vendor);
			AssertEquals("Vendor is from InvoiceHeader2", "GREOR_HDR2", provider2.Vendor.Identification.EoriNumber);
			AssertEquals("VendorPK is from InvoiceHeader2", headerVendorAddress2.PK, provider2.VendorPK);
		}

		public void TestVendorPK_NotPopulated()
		{
			AssertEquals(Guid.Empty, Provider.VendorPK);
		}

		public void TestVendorPK()
		{
			var vendorAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_SellerAddress = vendorAddress.PK;
			declaration.ZG_IsHighValueOvrd = true;
			AssertEquals(vendorAddress.PK, Provider.VendorPK);
		}

		public void TestVendee_NotPopulated()
		{
			AssertNull(Provider.Vendee);
		}

		public void TestVendee_WhenEmptyInInvoiceHeader_AndFilledInDeclaration_ShouldBeTakenFromDeclarationLevel()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var vendeeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				declaration.JE_OA_ConsigneeAddress = vendeeAddress.PK;
				declaration.ZG_IsHighValueOvrd = true;
				AssertNotNull("Populated", Provider.Vendee);
				AssertEquals("Vendee's EORI", "GREOR1", Provider.Vendee.Identification.EoriNumber);
				AssertEquals("Vendee's PK", vendeeAddress.PK, Provider.VendeePK);
			});
		}

		public void TestVendee_WhenFilledInInvoiceHeader_AndFilledInDeclaration_ShouldBeTakenFromInvoiceHeader()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var declarationVendeeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1_DECL", "EBS1");
			declaration.JE_OA_ConsigneeAddress = declarationVendeeAddress.PK;
			declaration.ZG_IsHighValueOvrd = true;

			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var headerVendeeAddress = GetOrgWithEORNumberAndEORIBranch("EOR2_HDR", "EBS2");
			header.JZ_OA_BuyerAddress = headerVendeeAddress.PK;

			AssertNotNull("Populated", Provider.Vendee);
			AssertEquals("Vendee is from InvoiceHeader", "GREOR2_HDR", Provider.Vendee.Identification.EoriNumber);
			AssertEquals("VendeePK is from InvoiceHeader", headerVendeeAddress.PK, Provider.VendeePK);
		}

		public void TestVendee_WhenFilledInInvoiceHeader_AndEmptyInDeclaration_ShouldBeTakenFromInvoiceHeader()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var headerVendeeAddress = GetOrgWithEORNumberAndEORIBranch("EOR2_HDR", "EBS2");
			header.JZ_OA_BuyerAddress = headerVendeeAddress.PK;

			AssertNotNull("Populated", Provider.Vendee);
			AssertEquals("Vendee is from InvoiceHeader", "GREOR2_HDR", Provider.Vendee.Identification.EoriNumber);
			AssertEquals("VendeePK is from InvoiceHeader", headerVendeeAddress.PK, Provider.VendeePK);
		}

		public void TestVendee_WhenTwoEntries_EachHasFilledVendeeInInvoiceHeader_ShouldBeTakenFromCorrespondingInvoiceHeader()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var header1 = declaration.Invoices.AddNew();
			var invoiceLine1_1 = header1.InvoiceLines.AddNew();
			invoiceLine1_1.JI_CEI = entryInstruction.PK;

			var vendeeAddress1 = GetOrgWithEORNumberAndEORIBranch("EOR_HDR1", "EBS1");
			header1.JZ_OA_BuyerAddress = vendeeAddress1.PK;

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;

			var header2 = declaration.Invoices.AddNew();
			var invoiceLine2_1 = header2.InvoiceLines.AddNew();
			invoiceLine2_1.JI_CEI = entryInstruction2.PK;

			var vendeeAddress2 = GetOrgWithEORNumberAndEORIBranch("EOR_HDR2", "EBS2");
			header2.JZ_OA_BuyerAddress = vendeeAddress2.PK;

			AssertNotNull("Populated", Provider.Vendee);
			AssertEquals("Vendee is from InvoiceHeader1", "GREOR_HDR1", Provider.Vendee.Identification.EoriNumber);
			AssertEquals("VendeePK is from InvoiceHeader1", vendeeAddress1.PK, Provider.VendeePK);

			var provider2 = new CustomsValueProvider(entryHeader2);

			AssertNotNull("Populated", provider2.Vendee);
			AssertEquals("Vendee is from InvoiceHeader2", "GREOR_HDR2", provider2.Vendee.Identification.EoriNumber);
			AssertEquals("VendeePK is from InvoiceHeader2", vendeeAddress2.PK, provider2.VendeePK);
		}

		public void TestVendeePK_NotPopulated()
		{
			AssertEquals(Guid.Empty, Provider.VendeePK);
		}

		public void TestVendeePK()
		{
			var vendeeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_ConsigneeAddress = vendeeAddress.PK;
			declaration.ZG_IsHighValueOvrd = true;
			AssertEquals(vendeeAddress.PK, Provider.VendeePK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			SetupDV1Details();
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryInstruction entryInstruction;

		protected override CustomsValueProvider GetProvider() => new CustomsValueProvider(entryHeader);

		OrgAddress GetOrgWithEORNumberAndEORIBranch(string eoriNumber, string ebsNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, Core.Constants.CountryCodes.Greece);
			var address = orgHeader.Addresses.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, ebsNumber, Core.Constants.CountryCodes.Germany);
			ebsCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}

		void SetupDV1Details()
		{
			dv1Detail = declaration.DV1Details.AddNew();
			dv1DetailPivot = entryInstruction.DV1DetailsPivots.Single();
			dv1DetailPivot.IsForEntryInstruction = true;
		}
		CusDV1Detail dv1Detail;
		NonPersistentCusDV1DetailPivot dv1DetailPivot;

		void AssertProperty_NoLinkedDV1Detail(bool expected, Func<bool> propertyGetter)
		{
			dv1DetailPivot.IsForEntryInstruction = false;
			AssertEquals(expected, propertyGetter.Invoke());
		}

		void AssertProperty_NoLinkedDV1Detail(string expected, Func<string> propertyGetter)
		{
			dv1DetailPivot.IsForEntryInstruction = false;
			AssertEquals(expected, propertyGetter.Invoke());
		}

		new ICustomsValue Provider => base.Provider;
	}
}
