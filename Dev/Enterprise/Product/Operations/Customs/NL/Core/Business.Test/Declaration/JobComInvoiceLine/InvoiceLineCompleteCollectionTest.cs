using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLineCompleteCollection))]
class InvoiceLineCompleteCollectionTest : EU.Business.Declaration.Testing.InvoiceLineCompleteCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override BaseJobDeclaration GetMeANewJobDeclaration()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		return dec;
	}

	public void TestSetDefaultsForNewChildForExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = ProcedureCodes._10;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("CEI_Style is the same as that of entry instruction", entryInstruction.PK, invoiceLine1.JI_CEI);
			AssertEquals("FormattedProcedure initialised to 1000", "1000", invoiceLine1.JI_FormattedProcedure);
			AssertEquals("Country of export initialised to NL", "NL", invoiceLine1.JI_RN_NKCountryOfExport);
		});
	}

	protected override Customs.Business.CusEntryInstruction GetCusEntryInstruction(BaseJobDeclaration declaration) => ((JobDeclaration)declaration).CustomsEntryInstructions.FirstOrDefault() ?? ((JobDeclaration)declaration).CustomsEntryInstructions.AddNew();
}
