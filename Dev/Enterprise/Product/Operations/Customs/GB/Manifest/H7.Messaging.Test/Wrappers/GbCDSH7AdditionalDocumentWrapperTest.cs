using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.Messaging.Testing
{
	public class GbCDSH7AdditionalDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestWrapper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var supportingDocument = packedItem.SupportingDocuments.AddNew();
			
			supportingDocument.CSI_Code = "108C";
			supportingDocument.CSI_ReferenceNumber = "An intense dissatisfaction with the world";
			supportingDocument.CSI_ReferenceNumber2 = "about it";
			supportingDocument.CSI_Availability = "A";
			supportingDocument.CSI_Actions = "C";
			supportingDocument.CSI_Description = "And a compulsion to do something";
			supportingDocument.CSI_DateOfIssue = new ZDateTime(2013, 01, 01);

			var wrapper = new GbCDSH7AdditionalDocumentWrapper(supportingDocument);

			CombineAssertions("AdditionalInfo", () =>
			{
				AssertEquals("CategoryCode", "1", wrapper.CategoryCode);
				AssertEquals("TypeCode", "08C", wrapper.TypeCode);
				AssertEquals("ID", "An intense dissatisfaction with the world", wrapper.ID);
				AssertEquals("LPCOExemptionCode", "AC", wrapper.LPCOExemptionCode);
				AssertEquals("Name", "And a compulsion to do something", wrapper.Name);
				AssertEquals("Submitter", "about it", wrapper.Submitter);
				AssertEquals("EffectiveDateTime", new ZDateTime(2013, 01, 01), wrapper.EffectiveDateTime);
			});
		}
	}
}
