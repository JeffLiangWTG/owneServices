using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestsSubclassesOf(typeof(AESLineProvider))]
	public abstract class AESLineProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
	where T : AESLineProvider
	{
		protected OrgAddress GetOrgWithEORNumberAndEORIBranch(ZString eoriNumber, ZString ebsNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, Core.Constants.CountryCodes.Greece);
			var address = orgHeader.Addresses.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, ebsNumber, Core.Constants.CountryCodes.Germany);
			ebsCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}

		protected static void AssertPartyWithoutContactPerson(IAESParty party, ZString eoriNumber, ZString eoriBranch, ZString name, ZString line, ZString city, ZString postCode, ZString country)
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

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 2;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}
		protected JobDeclaration declaration;
		protected CusEntryLine entryLine;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryInstruction entryInstruction;
	}
}
