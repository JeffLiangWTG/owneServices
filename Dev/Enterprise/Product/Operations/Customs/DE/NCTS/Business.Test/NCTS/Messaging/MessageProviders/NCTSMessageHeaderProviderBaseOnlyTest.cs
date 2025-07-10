using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSMessageHeaderProviderForTest))]
	sealed class NCTSMessageHeaderProviderBaseOnlyTest : NCTSMessageHeaderProviderAbstractTest<NCTSMessageHeaderProviderForTest, NCTSHeaderProviderForTest>
	{
		public void TestConstructor() => AssertExceptionThrown<ArgumentException>(() => new NCTSMessageHeaderProviderForTest(null));

		public void TestMessageIdentification()
		{
			AssertEquals(MessageHeaderProvider.MessageIdentification, "<<SENDERS REFERENCE PLACE HOLDER>>");
		}

		public void TestInterchangeSender_Arrival()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var provider = new NCTSMessageHeaderProviderForTest(nctsHeader);
			var destinationTrader = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", "DE", new ZString[] { "0000" });
			destinationTrader.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "DE000");
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;
			CombineAssertions(() =>
			{
				var result = provider.InterchangeSender;
				AssertEquals("EORINumber", "DE1234", provider.InterchangeSender.EoriNumber);
				AssertEquals("EORIBranch", "0000", provider.InterchangeSender.EoriBranchSuffix);
				AssertSame("Cached", result, provider.InterchangeSender);
			});
		}

		public void TestInterchangeSender_Departure()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var principal = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", "DE", new ZString[] { "0000" });
			principal.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "DE000");
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("EORINumber", "DE1234", MessageHeaderProvider.InterchangeSender.EoriNumber);
				AssertEquals("EORIBranch", "0000", MessageHeaderProvider.InterchangeSender.EoriBranchSuffix);
			});
		}

		public void TestInterchangeSender_InterChangeSenderIdentificationEmpty_FallBackToOrgProxy()
		{
			var principal = Factory.New<OrgHeader>();
			principal.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "DE000");
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			{
				AssertEquals("EORINumber", "DE0123456REG", MessageHeaderProvider.InterchangeSender.EoriNumber);
				AssertEquals("EORIBranch", "6777", MessageHeaderProvider.InterchangeSender.EoriBranchSuffix);
			}
		}

		public void TestInterchangeSender_ATLASParticipantIdentificationNumberEmpty_FallBackToOrgProxy()
		{
			var principal = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", "DE", new ZString[] { "0000" });
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			{
				CombineAssertions(() =>
				{
					AssertEquals("EORINumber", "DE0123456REG", MessageHeaderProvider.InterchangeSender.EoriNumber);
					AssertEquals("EORIBranch", "6777", MessageHeaderProvider.InterchangeSender.EoriBranchSuffix);
				});
			}
		}

		public void TestAuthentificationNumber_Arrival()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var provider = new NCTSMessageHeaderProviderForTest(nctsHeader);
			var destinationTrader = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", "DE", new ZString[] { "0000" });
			destinationTrader.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "DE000");
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;
			AssertEquals("DE000", provider.AuthenticationNumber);
		}

		public void TestAuthentificationNumber_Departure()
		{
			var principal = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", "DE", new ZString[] { "0000" });
			principal.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "DE000");
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
			AssertEquals("DE000", MessageHeaderProvider.AuthenticationNumber);
		}

		public void TestAuthentificationNumber_InterChangeSenderIdentificationEmpty_FallBackToOrgProxy()
		{
			var principal = Factory.New<OrgHeader>();
			principal.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "DE000");
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6800000310012345123456000"))
			{
				AssertEquals("AuthentificationNumber", "6800000310012345123456000", MessageHeaderProvider.AuthenticationNumber);
			}
		}

		public void TestAuthentificationNumber_ATLASParticipantIdentificationNumberEmpty_FallBackToOrgProxy()
		{
			var principal = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "1234", "DE", new ZString[] { "0000" });
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "DE0123456REG"))
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6777"))
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, "6800000310012345123456000"))
			{
				AssertEquals("AuthentificationNumber", "6800000310012345123456000", MessageHeaderProvider.AuthenticationNumber);
			}
		}

		public void TestInterchangeRecipientID_Arrival()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader.ArrivalMovementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "DE003302", ZDateTime.Empty, true);
			AssertEquals("DE003302", MessageHeaderProvider.InterchangeRecipientID);
		}

		public void TestInterchangeRecipientID_Departure()
		{
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader.MovementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "AA123456", ZDateTime.Empty, true);
			AssertEquals("AA123456", MessageHeaderProvider.InterchangeRecipientID);
		}

		public void TestAuthorisedConsignee_Null()
		{
			AssertNull(MessageHeaderProvider.AuthorisedConsignee);
		}

		public void TestAuthorisedConsignee_Cached()
		{
			nctsHeader.DestinationTrader.E2_OA_Address = Factory.New<OrgHeader>().Addresses.AddNew().PK;
			var authorisedConsignee = MessageHeaderProvider.AuthorisedConsignee;
			AssertSame(authorisedConsignee, MessageHeaderProvider.AuthorisedConsignee);
		}

		public void TestAuthorisedConsignee_Empty()
		{
			nctsHeader.DestinationTrader.E2_OA_Address = Factory.New<OrgHeader>().Addresses.AddNew().PK;
			CombineAssertions(() =>
			{
				var authorisedConsignee = MessageHeaderProvider.AuthorisedConsignee;
				AssertEquals("EoriNumber", null, authorisedConsignee.EoriNumber);
				AssertEquals("EoriBranchSuffix", null, authorisedConsignee.EoriBranchSuffix);
				AssertEquals("Name", null, authorisedConsignee.Name);
				AssertEquals("Position", null, authorisedConsignee.Position);
				AssertEquals("PhoneNumber", null, authorisedConsignee.PhoneNumber);
				AssertEquals("FacsimileNumber", null, authorisedConsignee.FacsimileNumber);
				AssertEquals("MailAddress", null, authorisedConsignee.MailAddress);
			});
		}

		public void TestAuthorisedConsignee_NoAddressSelected()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Greece);
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "VIC";
			orgContact.OC_Title = "DEV";
			orgContact.OC_Phone = "12345";
			orgContact.OC_Fax = "VICFFF";
			orgContact.OC_Email = "VIC@Wisetechglobal.com";
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;
			nctsHeader.DestinationTrader.ContactPK = orgContact.PK;

			var authorisedConsignee = MessageHeaderProvider.AuthorisedConsignee;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GR12345", authorisedConsignee.EoriNumber);
				AssertEquals("EoriBranchSuffix", null, authorisedConsignee.EoriBranchSuffix);
				AssertEquals("Name", "VIC", authorisedConsignee.Name);
				AssertEquals("Position", "DEV", authorisedConsignee.Position);
				AssertEquals("PhoneNumber", "12345", authorisedConsignee.PhoneNumber);
				AssertEquals("FacsimileNumber", "VICFFF", authorisedConsignee.FacsimileNumber);
				AssertEquals("MailAddress", "VIC@Wisetechglobal.com", authorisedConsignee.MailAddress);
			});
		}

		public void TestAuthorisedConsignee_AddressSelected()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Greece);
			var orgAddress = org.Addresses.AddNew();
			orgAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS2", Core.Constants.CountryCodes.Germany);
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "VIC";
			orgContact.OC_Title = "DEV";
			orgContact.OC_Phone = "12345";
			orgContact.OC_Fax = "VICFFF";
			orgContact.OC_Email = "VIC@Wisetechglobal.com";
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;
			nctsHeader.DestinationTrader.E2_OA_Address = orgAddress.PK;
			nctsHeader.DestinationTrader.ContactPK = orgContact.PK;

			var authorisedConsignee = MessageHeaderProvider.AuthorisedConsignee;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GR12345", authorisedConsignee.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS2", authorisedConsignee.EoriBranchSuffix);
				AssertEquals("Name", "VIC", authorisedConsignee.Name);
				AssertEquals("Position", "DEV", authorisedConsignee.Position);
				AssertEquals("PhoneNumber", "12345", authorisedConsignee.PhoneNumber);
				AssertEquals("FacsimileNumber", "VICFFF", authorisedConsignee.FacsimileNumber);
				AssertEquals("MailAddress", "VIC@Wisetechglobal.com", authorisedConsignee.MailAddress);
			});
		}

		[TestDate(2022, 5, 3, 16, 35, 22)]
		public void TestPreparationDateAndTimeUtc()
		{
			AssertEquals(new DateTime(2022, 5, 3, 16, 35, 22), MessageHeaderProvider.PreparationDateAndTimeUtc.DateAndTime);
		}

		protected override INCTSMessageHeader GetMessageHeaderProvider() => new NCTSMessageHeaderProviderForTest(nctsHeader);
	}

	class NCTSMessageHeaderProviderForTest : NCTSMessageHeaderProvider<NCTSHeaderProviderForTest>
	{
		public NCTSMessageHeaderProviderForTest(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}
	}
}
