using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EbdDocumentImportDataProviderTest : TestCaseWithFactory
{
	public void TestConstructorNullArgument()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new EbdDocumentImportDataProvider(null));
	}

	public void TestUidNumber()
	{
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";
		AssertEquals("123456", dataProvider.UidNumber);
	}

	public void TestCustomsDeclarationNumber()
	{
		sendingObject.LocalReferenceNumber = "MRN100";
		AssertEquals("MRN100", dataProvider.CustomsDeclarationNumber);
	}

	public void TestAccompanyingDocument()
	{
		var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "invoice.pdf", "inv");
		sendingObject.EDoc = eDoc.UniqueKey;
		CombineAssertions(() =>
		{
			AssertType<EbdAccompanyingDocumentDataProvider>(dataProvider.AccompanyingDocument);
			AssertSame("is cached", dataProvider.AccompanyingDocument, dataProvider.AccompanyingDocument);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		sendingObject = new SupportingDocSendingObject(declaration);
		dataProvider = new EbdDocumentImportDataProvider(sendingObject);
	}
	JobDeclaration declaration;
	SupportingDocSendingObject sendingObject;
	EbdDocumentImportDataProvider dataProvider;
}
