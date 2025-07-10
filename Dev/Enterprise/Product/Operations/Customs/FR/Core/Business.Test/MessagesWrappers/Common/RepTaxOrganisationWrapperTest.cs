using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	public class RepTaxOrganisationWrapperTest : TestCaseWithFactory
	{
		public void TestProperties_WhenImport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var errorCollector = new ErrorCollector();
			var wrapper = RepTaxOrganisationWrapper.New(entryHeader, errorCollector);
			AssertNull(wrapper);
			AssertEquals(0, errorCollector.ErrorCount);

			SetupFiscalReference(importer);
			wrapper = RepTaxOrganisationWrapper.New(entryHeader, new ErrorCollector());
			AssertProperties(wrapper, "I-TVA", "I-FullName", "I-Address", "IT", "I-PostCode", "I-City", "I-PartnerConsigneeID", "I-PartnerDestID", "I-EORI", "00");
		}

		public void TestProperties_WhenExport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var errorCollector = new ErrorCollector();
			var wrapper = RepTaxOrganisationWrapper.New(entryHeader, errorCollector);
			AssertNull(wrapper);
			AssertEquals(0, errorCollector.ErrorCount);

			SetupFiscalReference(supplier);
			wrapper = RepTaxOrganisationWrapper.New(entryHeader, new ErrorCollector());
			AssertProperties(wrapper, "S-TVA", "S-FullName", "S-Address", "SG", "S-PostCode", "S-City", "S-PartnerConsigneeID", "S-PartnerDestID", "S-EORI", "11");
		}

		void AssertProperties(RepTaxOrganisationWrapper wrapper, ZString organisationNumber, ZString fullName, ZString address, ZString countryCode, ZString postCode, ZString city, ZString partnerConsigneeId, ZString partnerDestId, ZString eoriCode, ZString eoriSuffix)
		{
			CombineAssertions(() =>
			{
				AssertEquals($"Assert OrganisationNumber message type is '{declaration.JE_MessageType}'.", organisationNumber, wrapper.OrganisationNumber);
				AssertEquals($"Assert OrganisationNumberEoriOnly message type is '{declaration.JE_MessageType}'.", organisationNumber, wrapper.OrganisationNumberEoriOnly);
				AssertEquals($"Assert FullName message type is '{declaration.JE_MessageType}'.", fullName, wrapper.FullName);
				AssertEquals($"Assert Address message type is '{declaration.JE_MessageType}'.", address, wrapper.Address);
				AssertEquals($"Assert CountryCode message type is '{declaration.JE_MessageType}'.", countryCode, wrapper.CountryCode);
				AssertEquals($"Assert PostCode message type is '{declaration.JE_MessageType}'.", postCode, wrapper.PostCode);
				AssertEquals($"Assert City message type is '{declaration.JE_MessageType}'.", city, wrapper.City);
				AssertEquals($"Assert PartnerConsigneeID message type is '{declaration.JE_MessageType}'.", partnerConsigneeId, wrapper.PartnerConsigneeID);
				AssertEquals($"Assert PartnerDestIDInfo message type is '{declaration.JE_MessageType}'.", partnerDestId, wrapper.PartnerDestIDInfo);
				AssertEquals($"Assert EoriCode message type is '{declaration.JE_MessageType}'.", eoriCode + eoriSuffix, wrapper.EoriCode);
			});
		}

		void SetupFiscalReference(OrgHeader orgHeader)
		{
			var euAddInfo = EUOrgImpAddInfo.Get(orgHeader, Core.Constants.CountryCodes.France);
			euAddInfo.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();

			var fiscalReference = entryHeader.EntryInstruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			fiscalReference.CFR_Reference = orgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.FranceCodeTypes.TVA, Core.Constants.CountryCodes.France);
			fiscalReference.CFR_OA_Owner = orgHeader.MainAddress.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			importer = SetupOrgHeader("I-TVA", "I-FullName", "I-Address", "IT", "I-PostCode", "I-City", "I-PartnerConsigneeID", "I-PartnerDestID", "I-EORI");
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00", Core.Constants.CountryCodes.France);

			supplier = SetupOrgHeader("S-TVA", "S-FullName", "S-Address", "SG", "S-PostCode", "S-City", "S-PartnerConsigneeID", "S-PartnerDestID", "S-EORI");
			supplier.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "11", Core.Constants.CountryCodes.France);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		}

		OrgHeader SetupOrgHeader(ZString organisationNumber, ZString fullName, ZString address, ZString countryCode, ZString postCode, ZString city, ZString partnerConsigneeId, ZString partnerDestIdInfo, ZString eoriCode)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.SetupCusCode(OrgCusCode.FranceCodeTypes.TVA, organisationNumber);
			orgHeader.OH_FullName = fullName;
			orgHeader.MainAddress.Address1 = address;
			orgHeader.MainAddress.OA_RN_NKCountryCode = countryCode;
			orgHeader.MainAddress.OA_PostCode = postCode;
			orgHeader.MainAddress.OA_City = city;
			orgHeader.CustomsClientID = partnerConsigneeId;
			orgHeader.MainAddress.UnrestrictedAdditionalAddressInformation = partnerDestIdInfo;
			orgHeader.SetupCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode);
			return orgHeader;
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		OrgHeader importer;
		OrgHeader supplier;
	}
}
