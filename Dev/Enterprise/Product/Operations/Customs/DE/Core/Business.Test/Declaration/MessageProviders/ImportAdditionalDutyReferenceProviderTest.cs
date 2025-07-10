using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class ImportAdditionalDutyReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportAdditionalDutyReferenceProvider>
	{
		public void TestReferenceNumber()
		{
			fiscalReference.CFR_Code = "FR1";
			fiscalReference.CFR_Reference = "RN001";
			AssertEquals("FR1RN001", Provider.ReferenceNumber);
		}

		public void TestDutyInterestedParty()
		{
			fiscalReference.CFR_Reference = "RN001";
			TestHelper.CreateCL010CoutryList(Factory);

			CombineAssertions(() =>
			{
				var dutyInterestedPartyAddress = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", Core.Constants.CountryCodes.Greece, "EBS1").MainAddress;
				fiscalReference.CFR_OA_Owner = dutyInterestedPartyAddress.PK;
				AssertNotNull("Populated", Provider.DutyInterestedParty);
				AssertEquals("DutyInterestedParty's EORI", "GR1234", Provider.DutyInterestedParty.Identification.EoriNumber);
			});
		}

		public void TestDutyInterestedParty_Null()
		{
			AssertNull(Provider.DutyInterestedParty);
		}

		public void TestDutyInterestedParty_CEI_ProcedureNotIn42Or63_Atlas10_1()
		{
			CombineAssertions(() =>
			{
				fiscalReference.CFR_Reference = "RN001";
				TestHelper.CreateCL010CoutryList(Factory);
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					entryInstruction.CEI_Procedure = ZString.Empty;
					var dutyInterestedPartyAddress = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", Core.Constants.CountryCodes.Greece, "EBS1").MainAddress;
					fiscalReference.CFR_OA_Owner = dutyInterestedPartyAddress.PK;
					AssertNotNull("Populated for version 10.1", Provider.DutyInterestedParty);
					AssertEquals("DutyInterestedParty's EORI", "GR1234", Provider.DutyInterestedParty.Identification.EoriNumber);
				}
			});
		}

		public void TestDutyInterestedParty_ReferenceNumber()
		{
			fiscalReference.CFR_Reference = "RN001";
			TestHelper.CreateCL010CoutryList(Factory);
			var dutyInterestedPartyAddress = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", Core.Constants.CountryCodes.Greece, "EBS1").MainAddress;
			fiscalReference.CFR_OA_Owner = dutyInterestedPartyAddress.PK;
			AssertEquals("ReferenceNumber EORI", "GR1234", Provider.DutyInterestedParty.Identification.EoriNumber);
		}

		public void TestDutyInterestedParty_Address()
		{
			var orgHeader = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", Core.Constants.CountryCodes.Greece, "EBS1");
			orgHeader.DeleteSingleOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			orgHeader.OH_FullName = "Müller GmbH";
			var dutyInterestedPartyAddress = orgHeader.MainAddress;
			dutyInterestedPartyAddress.PrimaryOrgAddressAdditionalInfoDetail = "Hans Müller";
			dutyInterestedPartyAddress.OA_Address1 = "Mainzer Str 93";
			dutyInterestedPartyAddress.OA_RN_NKCountryCode = "DE";
			dutyInterestedPartyAddress.Postcode = "55262";
			dutyInterestedPartyAddress.City = "Ingelheim";
			dutyInterestedPartyAddress.Address2 = "Heidesheim";
			fiscalReference.CFR_OA_Owner = dutyInterestedPartyAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("DutyInterestedParty Name", "Müller GmbH Hans Müller", Provider.DutyInterestedParty.Address.Name);
				AssertEquals("DutyInterestedParty Line", "Mainzer Str 93", Provider.DutyInterestedParty.Address.Address);
				AssertEquals("DutyInterestedParty Country", "DE", Provider.DutyInterestedParty.Address.Country);
				AssertEquals("DutyInterestedParty Postcode", "55262", Provider.DutyInterestedParty.Address.Postcode);
				AssertEquals("DutyInterestedParty City", "Ingelheim", Provider.DutyInterestedParty.Address.City);
				AssertEquals("DutyInterestedParty District", "Heidesheim", Provider.DutyInterestedParty.Address.District);
			});
		}

		public void TestDutyInterestedParty_Address_NotMapped_EORIFound()
		{
			fiscalReference.CFR_Reference = "RN001";
			TestHelper.CreateCL010CoutryList(Factory);
			var dutyInterestedPartyAddress = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", Core.Constants.CountryCodes.Greece, "EBS1").MainAddress;
			fiscalReference.CFR_OA_Owner = dutyInterestedPartyAddress.PK;
			AssertNull("DutyInterestedParty  Address", Provider.DutyInterestedParty.Address);
		}

		public void TestDutyInterestedPartyPK_Null()
		{
			AssertEquals(Guid.Empty, Provider.DutyInterestedPartyPK);
		}

		public void TestDutyInterestedPartyPK()
		{
			var dutyInterestedPartyAddress = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", Core.Constants.CountryCodes.Greece, "EBS1").MainAddress;
			fiscalReference.CFR_OA_Owner = dutyInterestedPartyAddress.PK;
			AssertEquals(dutyInterestedPartyAddress.PK, Provider.DutyInterestedPartyPK);
		}

		public void TestDutyInterestedPartyPK_CEI_ProcedureNotIn42Or63_Atlas10_1()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				entryInstruction.CEI_Procedure = ZString.Empty;
				var dutyInterestedPartyAddress = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", Core.Constants.CountryCodes.Greece, "EBS1").MainAddress;
				fiscalReference.CFR_OA_Owner = dutyInterestedPartyAddress.PK;
				AssertEquals(dutyInterestedPartyAddress.PK, Provider.DutyInterestedPartyPK);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Procedure = "42";
			fiscalReference = entryInstruction.FiscalReferences.AddNew();
			dataProvider = new ImportAdditionalDutyReferenceProvider(fiscalReference);
		}
		IImportAdditionalDutyReference dataProvider;
		CusFiscalReference fiscalReference;
		CusEntryInstruction entryInstruction;

		protected override ImportAdditionalDutyReferenceProvider GetProvider() => (ImportAdditionalDutyReferenceProvider)dataProvider;
	}
}
