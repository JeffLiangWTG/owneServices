using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(MonthlyClosingDecBodyProvider))]
	public abstract class MonthlyClosingDecBodyProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : MonthlyClosingDecBodyProvider
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

		protected JobComInvoiceHeader AddInvoiceWithInvoiceLine()
		{
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 42;
			invoice = jobDeclaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;

			reconEntryLine = entry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_OriginalEntryLineNumber = entryLine.CL_LineNumber;

			return invoice;
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<CusReconDeclaration>();
			entry = declaration.CusReconEntries.AddNew();

			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			entry.CRE_CH_OriginalEntry = entryHeader.PK;
		}
		protected CusReconDeclaration declaration;
		protected CusReconEntry entry;
		protected CusReconEntryLine reconEntryLine;
		protected JobDeclaration jobDeclaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryHeader entryHeader;
		protected CusEntryLine entryLine;
		protected CusEntryInstruction entryInstruction;
	}
}
