using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocumentWrapper))]
	sealed class SupportingDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSupportingDocument()
		{
			var wrapper = (SupportingDocumentWrapper)GetNewBusinessObject();
			AssertType<SupportingDocument>(wrapper.SupportingDocument);
		}

		public void TestType()
		{
			var wrapper = (SupportingDocumentWrapper)GetNewBusinessObject();
			AssertEquals("Type should be code1", "code1", wrapper.Type);
		}

		public void TestDescription()
		{
			var wrapper = (SupportingDocumentWrapper)GetNewBusinessObject();
			AssertEquals("Description should be REF1", "REF1", wrapper.Description);
		}

		public void TestStatus()
		{
			var wrapper = (SupportingDocumentWrapper)GetNewBusinessObject();
			AssertEquals("Status should be Status1", "SNT", wrapper.Status);
		}

		public void TestCustomsNotes()
		{
			var wrapper = (SupportingDocumentWrapper)GetNewBusinessObject();
			AssertEquals("CustomsNotes should be Desc1", "Desc1", wrapper.CustomsNotes);
		}

		public void TestEDoc()
		{
			AssertEntity<SupportingDocumentWrapper>()
				.HasProperty(x => x.EDoc)
				.WithList("Lookups.EDocList")
				.WithCaption("eDoc");
		}

		public void TestValidation()
		{
			var wrapper = (SupportingDocumentWrapper)GetNewBusinessObject();
			AssertType<SupportingDocumentWrapperValidation>("Validation must be of the expected type", wrapper.Validation);
		}

		public void TestLookupsType()
		{
			var wrapper = (SupportingDocumentWrapper)GetNewBusinessObject();
			AssertType<SupportingDocumentLookups>("Lookups must be of the expected type", wrapper.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => new SupportingDocumentWrapper(supportingDocument);

		protected override void SetUp()
		{
			header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "code1";
			supportingDocument.CSI_ReferenceNumber = "REF1";
			supportingDocument.CSI_Status = "SNT";
			supportingDocument.CSI_AdditionalDescription = "Desc1";

			base.SetUp();
		}

		AsycudaManifestHeader header;
		SupportingDocument supportingDocument;
	}
}
