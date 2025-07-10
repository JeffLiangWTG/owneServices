using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageSupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMaxCount_TemporaryStorageBill()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			for (var i = 0; i < 99; i++)
			{
				bill.SupportingDocuments.AddNew();
			}

			var supportingDocument = bill.SupportingDocuments.First();
			supportingDocument.Validation.ValidateAll();
			AssertNoRowError(supportingDocument, "You are only allowed a maximum of 99 Supporting Documents here.");

			supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.Validation.ValidateAll();
			AssertHasRowError(supportingDocument, "You are only allowed a maximum of 99 Supporting Documents here.");
		}

		public void TestMaxCount_TemporaryStoragePackedItem()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			for (var i = 0; i < 99; i++)
			{
				packedItem.SupportingDocuments.AddNew();
			}

			var supportingDocument = packedItem.SupportingDocuments.First();
			supportingDocument.Validation.ValidateAll();
			AssertNoRowError(supportingDocument, "You are only allowed a maximum of 99 Supporting Documents here.");

			supportingDocument = packedItem.SupportingDocuments.AddNew();
			supportingDocument.Validation.ValidateAll();
			AssertHasRowError(supportingDocument, "You are only allowed a maximum of 99 Supporting Documents here.");
		}

		public void TestRuleBR_PN_TS_025_TemporaryStorageBill()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			bill.SupportingDocuments.AddNew();

			var supportingDocument = packedItem.SupportingDocuments.AddNew();
			supportingDocument.Validation.ValidateAll();
			AssertHasRowMessageError("It is prohibited to add supporting documents when bill already has at least one", supportingDocument, "The Supporting Document should be entered at Bill level or Bill Item level, not both.");

			bill.SupportingDocuments.DeleteAll();
			supportingDocument.Validation.ValidateAll();
			AssertNoRowMessageError("It is allowed to add supporting documents when bill has no one", supportingDocument, "The Supporting Document should be entered at Bill level or Bill Item level, not both.");
		}

		public void TestRuleBR_PN_TS_025_TemporaryStoragePackedItem()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.SupportingDocuments.AddNew();

			var supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.Validation.ValidateAll();
			AssertHasRowMessageError("It is prohibited to add supporting documents when packed item already has at least one", supportingDocument, "The Supporting Document should be entered at Bill level or Bill Item level, not both.");

			packedItem.SupportingDocuments.DeleteAll();
			supportingDocument.Validation.ValidateAll();
			AssertNoRowMessageError("It is allowed to add supporting documents when packed item has no one", supportingDocument, "The Supporting Document should be entered at Bill level or Bill Item level, not both.");
		}
	}
}
