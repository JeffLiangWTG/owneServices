using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(AESMessageHeaderProvider))]
	sealed class AESMessageHeaderProviderBaseOnlyTest : AESMessageHeaderProviderAbstractTest<AESMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new AESMessageHeaderProviderBase(null));
		}

		public void TestMessageIdentification()
		{
			AssertEquals("MessageIdentification", "<<SENDERS REFERENCE PLACE HOLDER>>", Provider.MessageIdentification);
		}

		public void TestInterchangeRecipientID()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.InterchangeRecipientID);
				declaration.JE_CustomsOffice = "CO001";
				AssertEquals("ExportCustomsOffice", "CO001", Provider.InterchangeRecipientID);
			});
		}

		public void TestInterchangeSender_Representative()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var declarantAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranchAndAPINumber(Factory, "EOR1", "EBS1", "1111111111111111111111111");
			var representativeAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranchAndAPINumber(Factory, "EOR2", "EBS2", "2222222222222222222222222");
			AssertInterchangeSender("0010", declarantAddress.PK, representativeAddress.PK, "DEEOR2", "EBS2");
		}

		public void TestInterchangeSender_Declarant()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var declarantAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranchAndAPINumber(Factory, "EOR1", "EBS1", "1111111111111111111111111");
			var representativeAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranchAndAPINumber(Factory, "EOR2", "EBS2", "2222222222222222222222222");
			AssertInterchangeSender("0100", declarantAddress.PK, representativeAddress.PK, "DEEOR1", "EBS1");
		}

		public void TestInterchangeSender_FallBalckToRegistry()
		{
			var declarantAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranch(Factory, "EOR1", "EBS1");
			var representativeAddress = EORIHelperTest.GetOrgWithEORNumberAndEORIBranchAndAPINumber(Factory, "EOR2", "EBS2", "2222222222222222222222222");
			AssertInterchangeSender("0100", declarantAddress.PK, representativeAddress.PK, "DEEOR0", "0000");
		}

		public void TestAuthorizationNumber_Representative()
		{
			var declarantAddress = GetOrgWithBinNumber("1111111111111111111111111");
			var representativeAddress = GetOrgWithBinNumber("2222222222222222222222222");
			AssertAuthorizationNumber("0010", declarantAddress.PK, representativeAddress.PK, "2222222222222222222222222");
		}

		public void TestAuthorizationNumber_Declarant()
		{
			var declarantAddress = GetOrgWithBinNumber("1111111111111111111111111");
			var representativeAddress = GetOrgWithBinNumber("2222222222222222222222222");
			AssertAuthorizationNumber("0100", declarantAddress.PK, representativeAddress.PK, "1111111111111111111111111");
		}

		public void TestAuthorizationNumber_Registry()
		{
			var declarantAddress = GetOrgWithBinNumber("1111111111111111111111111");
			var representativeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			AssertAuthorizationNumber("0010", declarantAddress.PK, representativeAddress.PK, "0000000000000000000000000");
		}

		[TestDate(2019, 11, 08, 12, 11, 59)]
		public void TestPreparationDateAndTimeUtc()
		{
			AssertEquals(new DateTime(2019, 11, 08, 12, 11, 0), Provider.PreparationDateAndTimeUtc.DateAndTime);
		}

		public void TestAESHeader()
		{
			AssertType<AESHeaderProviderBase>(Provider.AESHeader);
		}

		protected override AESMessageHeaderProvider GetProvider() => new AESMessageHeaderProviderBase(entryHeader);

		new IAESMessageHeader Provider => base.Provider;

		void AssertInterchangeSender(string partyConstellation, ZGuid declarantAddressPK, ZGuid representativeAddressPK, string expectedEOR, string expectedEBS, bool hasLinkedEntryInstruction = true)
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DEEOR0"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "0000"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "0000000000000000000000000"))
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_OA_DeclarantAddress = declarantAddressPK;
				declaration.JE_OA_Representative = representativeAddressPK;
				var entryInstruction = Factory.New<CusEntryInstruction>();
				entryInstruction.ZG_PartyConstellation = partyConstellation;
				if (hasLinkedEntryInstruction)
				{
					entryInstruction.CEI_JE = declaration.PK;
					entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				}

				var result = Provider.InterchangeSender;
				CombineAssertions(() =>
				{
					AssertEquals("EOR", expectedEOR, result.EoriNumber);
					AssertEquals("EBS", expectedEBS, result.EoriBranchSuffix);
				});
			}
		}

		void AssertAuthorizationNumber(string partyConstellation, ZGuid declarantAddressPK, ZGuid representativeAddressPK, string expectedBIN)
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(Guid.Empty, currentBranchPK, Guid.Empty, "0000000000000000000000000"))
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_OA_DeclarantAddress = declarantAddressPK;
				declaration.JE_OA_Representative = representativeAddressPK;
				var entryInstruction = Factory.New<CusEntryInstruction>();
				entryInstruction.ZG_PartyConstellation = partyConstellation;
				entryInstruction.CEI_JE = declaration.PK;
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;

				var result = Provider.AuthorizationNumber;
				AssertEquals(expectedBIN, result);
			}
		}

		OrgAddress GetOrgWithBinNumber(string apiNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			var apiCode = address.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, apiNumber, Core.Constants.CountryCodes.Germany);
			apiCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}
	}

	class AESMessageHeaderProviderBase : AESMessageHeaderProvider
	{
		public AESMessageHeaderProviderBase(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public override IAESHeader AESHeader => aesHeader ?? (aesHeader = new AESHeaderProviderBase(EntryHeader));
		IAESHeader aesHeader;
	}
}
