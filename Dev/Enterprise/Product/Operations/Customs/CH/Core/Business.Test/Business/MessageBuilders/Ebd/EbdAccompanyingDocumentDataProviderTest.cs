using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EbdAccompanyingDocumentDataProviderTest : TestCaseWithFactory
{
	public void TestConstructorNullArgument()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new EbdAccompanyingDocumentDataProvider(null));
		sendingObject.EDoc = ZGuid.Empty;
		AssertExceptionThrown<ArgumentNullException>(() => new EbdAccompanyingDocumentDataProvider(sendingObject));
	}

	public void TestFilename()
	{
		EbdAccompanyingDocumentDataProvider dataProvider = new EbdAccompanyingDocumentDataProvider(sendingObject);
		AssertEquals("Invoice.pdf", dataProvider.Filename);
	}

	public void TestType()
	{
		sendingObject.DocumentType = "doc-type";
		EbdAccompanyingDocumentDataProvider dataProvider = new EbdAccompanyingDocumentDataProvider(sendingObject);
		AssertEquals("doc-type", dataProvider.Type);
	}

	public void TestContent()
	{
		var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Invoice.pdf", "CIV");
		sendingObject.EDoc = eDoc.UniqueKey;
		EbdAccompanyingDocumentDataProvider dataProvider = new EbdAccompanyingDocumentDataProvider(sendingObject);
		var expectedContent = eDoc.UniqueKey.ToGuid().ToByteArray();
		AssertArrayEqualsByElements(expectedContent, dataProvider.Content.ToArray());
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
		sendingObject = new SupportingDocSendingObject(declaration);
		sendingObject.EDoc = eDoc.UniqueKey;
	}
	JobDeclaration declaration;
	SupportingDocSendingObject sendingObject;
}
