using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ExpeditionDocumentSubmittedWrapperTest : WrapperHelperTest<ExpeditionDocumentSubmittedWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ExpeditionDocumentSubmittedWrapper(null));
		}

		public void TestCode()
		{
			doc.CSI_Code = SupportingDocumentData.Code;
			AssertEquals("Expected filled Code", SupportingDocumentData.Code, wrapper.Code);
		}

		public void TestNumber()
		{
			doc.CSI_ReferenceNumber = SupportingDocumentData.Reference;
			AssertEquals("Expected filled Number", SupportingDocumentData.Reference, wrapper.Number);
		}

		public void TestDate()
		{
			var dateOfIssue = new ZDate(2020, 07, 14);
			doc.CSI_DateOfIssue = dateOfIssue;
			AssertEquals("Expected filled Date", dateOfIssue, wrapper.Date);
		}

		protected override void SetUp()
		{
			base.SetUp();
			doc = Factory.New<SupportingDocument>();
			wrapper = new ExpeditionDocumentSubmittedWrapper(doc);
		}

		SupportingDocument doc;
		ExpeditionDocumentSubmittedWrapper wrapper;

		protected override ExpeditionDocumentSubmittedWrapper GetProvider() => wrapper;
	}
}
