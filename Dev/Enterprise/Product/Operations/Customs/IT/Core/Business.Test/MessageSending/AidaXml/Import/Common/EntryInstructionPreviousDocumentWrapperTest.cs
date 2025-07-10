using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class EntryInstructionPreviousDocumentWrapperTest : TestCaseWithFactory
{
	public void TestQuantity()
	{
		var previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertNull(nameof(IPreviousDocument.Quantity), previousDocumentWrapper.Quantity);

		previousDocument.CSI_Quantity = 2120m;
		previousDocument.CSI_UnitOfQuantity = "G";
		previousDocumentWrapper = GetNewPreviousDocumentWrapper();
		AssertEquals(nameof(IPreviousDocument.Quantity), 2.12m, previousDocumentWrapper.Quantity);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		previousDocument = declaration.CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();
	}

	PreviousDocument previousDocument;

	IPreviousDocument GetNewPreviousDocumentWrapper() => new EntryInstructionPreviousDocumentWrapper(previousDocument);
}
