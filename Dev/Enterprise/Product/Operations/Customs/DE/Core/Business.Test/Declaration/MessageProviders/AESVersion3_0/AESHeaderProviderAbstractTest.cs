using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestsSubclassesOf(typeof(AESHeaderProvider))]
	public abstract class AESHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : AESHeaderProvider
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
		}
		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;

		protected OrgAddress GetOrgWithEORNumberAndEORIBranch(ZString eoriNumber, ZString ebsNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "MAX MUSTERMANN";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, Core.Constants.CountryCodes.Greece);
			var address = orgHeader.Addresses.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			address.OA_Address1 = "TESTSTRASSE 1";
			address.OA_City = "MAINZ";
			address.OA_PostCode = "55126";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, ebsNumber, Core.Constants.CountryCodes.Germany);
			ebsCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}

		protected OrgAddress GetOrgWithoutEORNumberAndEORIBranch()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			return address;
		}

		protected OrgAddress GetAPINumberOrg(ZString apiNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			var apiCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, apiNumber, Core.Constants.CountryCodes.Germany);
			apiCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}

		protected static void AssertPartyWithoutContactPerson(IAESParty party, string eoriNumber, string eoriBranch, ZString name, string line, string city, string postCode, string country)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Party EoriNumber", eoriNumber, party.EoriNumber);
				AssertEquals("Party EoriBranchSuffix", eoriBranch, party.EoriBranchSuffix);
				AssertEquals("Party Name", name, party.Name);
				AssertEquals("Party Address", line, party.Address);
				AssertEquals("Party City", city, party.City);
				AssertEquals("Party PostCode", postCode, party.Postcode);
				AssertEquals("Party Country", country, party.Country);
			});
		}

		protected static void AssertPartyWithContactPerson(IAESParty party, ZString eoriNumber, ZString eoriBranch, ZString name, ZString line, ZString city, ZString postCode, ZString country, ZString contactTitle, ZString contactName, ZString contactPhone, ZString contactFax, ZString contactEmail)
		{
			AssertPartyWithoutContactPerson(party, eoriNumber, eoriBranch, name, line, city, postCode, country);
			CombineAssertions(() =>
			{
				AssertEquals("Contact Title", contactTitle, party.ContactPerson.Position);
				AssertEquals("Contact Name", contactName, party.ContactPerson.PersonName);
				AssertEquals("Contact Phone", contactPhone, party.ContactPerson.PhoneNumber);
				AssertEquals("Contact Fax", contactFax, party.ContactPerson.FacsimileNumber);
				AssertEquals("Contact EMail", contactEmail, party.ContactPerson.MailAddress);
			});
		}

		protected void PrepareTransportEquipment()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "12345678";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "23456789";

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			var pivot = invoiceLine.ContainersPivot.AddNew();
			pivot.C2_CO = container.PK;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			var pivot2 = invoiceLine2.ContainersPivot.AddNew();
			pivot2.C2_CO = container2.PK;
		}
	}
}
