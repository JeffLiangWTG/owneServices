using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocDocumentDataWrapper))]
sealed class DocDocumentDataWrapperTest : DocumentWrapperTestCase
{
	public void TestDocument()
	{
		var document = Factory.New<CusSupportingInfo>();
		document.CSI_ItemNumber = 1;
		document.CSI_Code = "TST";
		document.CSI_ReferenceNumber = "123";
		document.CSI_ReferenceNumber2 = "456";

		var documentWrapper = DocDocumentDataWrapper.New(document, Factory);

		CombineAssertions(() =>
		{
			AssertEquals("LineItemNumber should match", 1, documentWrapper.LineItemNumber);
			AssertEquals("Type should match", "TST", documentWrapper.Type);
			AssertEquals("ReferenceNumber should match", "123", documentWrapper.ReferenceNumber);
			AssertEquals("GoodsItemNumber should match", 1, documentWrapper.GoodsItemNumber);
			AssertEquals("ComplementOfInformation should match", "456", documentWrapper.ComplementOfInformation);
		});
	}

	public override DocumentWrapper[] GetDocumentWrappers()
	{
		var document = Factory.New<CusSupportingInfo>();
		return new DocumentWrapper[] { DocDocumentDataWrapper.New(document, Factory) };
	}
}
