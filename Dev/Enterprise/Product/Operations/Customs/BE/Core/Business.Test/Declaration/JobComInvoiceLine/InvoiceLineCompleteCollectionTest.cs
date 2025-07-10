using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLineCompleteCollection))]
class InvoiceLineCompleteCollectionTest : EU.Business.Declaration.Testing.InvoiceLineCompleteCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override BaseJobDeclaration GetMeANewJobDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		return declaration;
	}

	protected override Customs.Business.CusEntryInstruction GetCusEntryInstruction(BaseJobDeclaration declaration)
	{
		return ((JobDeclaration)declaration).CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault() ?? ((JobDeclaration)declaration).CustomsEntryInstructions.AddNew();
	}
}
