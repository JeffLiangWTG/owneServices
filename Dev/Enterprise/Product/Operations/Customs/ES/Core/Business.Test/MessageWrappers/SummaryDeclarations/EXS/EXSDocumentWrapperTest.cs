using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	class EXSDocumentWrapperTest : WrapperHelperTest<EXSDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertNotNull("supportingDocument not null", new EXSDocumentWrapper(Factory.New<SupportingDocument>()));
			AssertNotNull("previousDocument not null", new EXSDocumentWrapper(Factory.New<PreviousDocument>()));
		}

		public void TestLineNumber()
		{
			doc.CSI_LineNo = 1;
			wrapper = new EXSDocumentWrapper(doc);
			AssertEquals(ZString.Empty, wrapper.LineNumber);
		}

		public void TestName()
		{
			doc.CSI_Code = "Code";
			doc.CSI_SubType = "Y";
			wrapper = new EXSDocumentWrapper(doc);
			AssertEquals("YCode", wrapper.Name);
		}

		public void TestLineNumberForPreviousDocument()
		{
			var previousDoc = Factory.New<PreviousDocument>();
			CombineAssertions(() =>
			{
				previousDoc.CSI_ReferenceNumber = "Reference";
				previousDoc.CSI_Code = "SUM";
				previousDoc.CSI_LineNo = 0;
				wrapper = new EXSDocumentWrapper(previousDoc);
				AssertEquals("For non N337 previous documents reference number must have line number attached when 0", ZString.Empty, wrapper.LineNumber);

				previousDoc.CSI_LineNo = 5;
				wrapper = new EXSDocumentWrapper(previousDoc);
				AssertEquals("For non N337 previous documents reference number must not have line number attached when not 0", ZString.Empty, wrapper.LineNumber);

				previousDoc.CSI_Code = "N337";
				wrapper = new EXSDocumentWrapper(previousDoc);
				AssertEquals("For N337 previous documents reference number must have line number attached when not 0", "5", wrapper.LineNumber);

				previousDoc.CSI_LineNo = 0;
				wrapper = new EXSDocumentWrapper(previousDoc);
				AssertEquals("For N337 previous documents reference number must not have line number attached when 0", ZString.Empty, wrapper.LineNumber);
			});
		}

		public void TestNumberForPreviousDocument()
		{
			var previousDoc = Factory.New<PreviousDocument>();
			CombineAssertions(() =>
			{
				previousDoc.CSI_ReferenceNumber = "Reference";
				previousDoc.CSI_Code = "N337";
				previousDoc.CSI_LineNo = 1;
				wrapper = new EXSDocumentWrapper(previousDoc);
				AssertEquals("For N337 previous documents reference number must have line number attached when not 0", "Reference", wrapper.Number);

				previousDoc.CSI_ReferenceNumber = "Reference2";
				previousDoc.CSI_LineNo = 0;
				wrapper = new EXSDocumentWrapper(previousDoc);
				AssertEquals("For N337 previous documents reference number must not have line number attached when 0", "Reference2", wrapper.Number);

				previousDoc.CSI_ReferenceNumber = "Reference3";
				previousDoc.CSI_Code = "SUM";
				previousDoc.CSI_LineNo = 5;
				wrapper = new EXSDocumentWrapper(previousDoc);
				AssertEquals("For non N337 previous documents reference number must have line number attached", "Reference300005", wrapper.Number);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			doc = Factory.New<SupportingDocument>();
			wrapper = new EXSDocumentWrapper(doc);
		}

		SupportingDocument doc;
		EXSDocumentWrapper wrapper;

		protected override EXSDocumentWrapper GetProvider() => wrapper;
	}
}
