using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDPreviousDocumentWrapperTest : WrapperHelperTest<DeclarationDVDPreviousDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<NullReferenceException>("Null Document", () => new DeclarationDVDPreviousDocumentWrapper(null));
		}

		public void TestLineNumber()
		{
			document.CSI_LineNo = 3;
			AssertEquals("Expected filled LineNumber", "3", wrapper.LineNumber);
		}

		public void TestUnitOfMeasure()
		{
			document.CSI_UnitOfQuantity = "KGM";
			AssertEquals("Expected filled UnitOfMeasure", "KGM", wrapper.UnitOfMeasure);
		}

		public void TestQuantity()
		{
			document.CSI_Quantity = 2.3m;
			AssertEquals("Expected filled Quantity", 2.3m, wrapper.Quantity);
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<PreviousDocument>();
			wrapper = new DeclarationDVDPreviousDocumentWrapper(document);
		}

		PreviousDocument document;
		DeclarationDVDPreviousDocumentWrapper wrapper;

		protected override DeclarationDVDPreviousDocumentWrapper GetProvider() => wrapper;
	}
}
