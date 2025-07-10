using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocEdecCusEntryLineCollection))]
class DocEdecCusEntryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocEdecCusEntryLineCollection>
{
	protected override DocEdecCusEntryLineCollection GetCollectionToTest()
	{
		return new DocEdecCusEntryLineCollection(Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
		declaration.JE_ClusterKey = 1;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;
		return DocEdecCusEntryLine.New(entryLine, Factory);
	}
}
