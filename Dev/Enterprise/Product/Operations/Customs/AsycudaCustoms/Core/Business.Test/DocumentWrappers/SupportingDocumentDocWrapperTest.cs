using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(SupportingDocumentDocWrapper))]
	class SupportingDocumentDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCode()
		{
			var document = Factory.New<SupportingDocument>();
			document.CSI_Code = "DOC";
			var wrapper = new SupportingDocumentDocWrapper(document);
			AssertEquals("DOC", wrapper.Code);
			wrapper = new SupportingDocumentDocWrapper(null);
			AssertEquals(ZString.Empty, wrapper.Code);
		}

		public void TestReference()
		{
			var document = Factory.New<SupportingDocument>();
			document.CSI_ReferenceNumber = "refer1";
			var wrapper = new SupportingDocumentDocWrapper(document);
			AssertEquals("refer1", wrapper.Reference);
			wrapper = new SupportingDocumentDocWrapper(null);
			AssertEquals(ZString.Empty, wrapper.Reference);
		}

		public void TestComments()
		{
			var document = Factory.New<SupportingDocument>();
			document.CSI_AdditionalDescription = "description";
			var wrapper = new SupportingDocumentDocWrapper(document);
			AssertEquals("description", wrapper.Comments);
			wrapper = new SupportingDocumentDocWrapper(null);
			AssertEquals(ZString.Empty, wrapper.Comments);
		}

		protected override BusinessObject GetNewBusinessObject() => new SupportingDocumentDocWrapper(null);
	}
}
