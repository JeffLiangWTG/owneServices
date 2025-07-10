using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(CommonPreviousDocument))]
sealed class CommonPreviousDocumentTest : CusSupportingInfoTest<CommonPreviousDocument>
{
	public void TestValidation()
	{
		AssertType<CommonPreviousDocumentValidation>(previousDocument.Validation);
	}

	public void TestCSI_ReferenceNumberN785Pos1()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumberN785Pos1 = "1";
			AssertEquals("1      L       *", previousDocument.CSI_ReferenceNumber);
			previousDocument.CSI_ReferenceNumber = "2      L       *    ";
			AssertEquals("2", previousDocument.CSI_ReferenceNumberN785Pos1);
		});
	}

	public void TestCSI_ReferenceNumberN785Pos2To7()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumberN785Pos2To7 = "123456";
			AssertEquals(" 123456L       *", previousDocument.CSI_ReferenceNumber);
			previousDocument.CSI_ReferenceNumber = " 7890ABL       *    ";
			AssertEquals("7890AB", previousDocument.CSI_ReferenceNumberN785Pos2To7);
		});
	}

	public void TestCSI_ReferenceNumberN785Pos8()
	{
		AssertEquals("L", previousDocument.CSI_ReferenceNumberN785Pos8);
	}

	public void TestCSI_ReferenceNumberN785Pos9To15()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumberN785Pos9To15 = "1234567";
			AssertEquals("       L1234567*", previousDocument.CSI_ReferenceNumber);
			previousDocument.CSI_ReferenceNumber = "       LABCDEFG*";
			AssertEquals("ABCDEFG", previousDocument.CSI_ReferenceNumberN785Pos9To15);
		});
	}

	public void TestCSI_ReferenceNumberN785Pos16()
	{
		AssertEquals("*", previousDocument.CSI_ReferenceNumberN785Pos16);
	}

	public void TestCSI_ReferenceNumberN785Pos17To20()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumberN785Pos17To20 = "1234";
			AssertEquals("       L       *1234", previousDocument.CSI_ReferenceNumber);
			previousDocument.CSI_ReferenceNumber = "       L       *5678";
			AssertEquals("5678", previousDocument.CSI_ReferenceNumberN785Pos17To20);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => CreatePreviousDocument(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreatePreviousDocument(Factory);

	protected override IEnumerable<CommonPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return CreatePreviousDocument(factory);
	}

	protected override void SetUp()
	{
		base.SetUp();
		previousDocument = CreatePreviousDocument(Factory);
	}
	CommonPreviousDocument previousDocument;

	static CommonPreviousDocument CreatePreviousDocument(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.PreviousDocuments.AddNew();
	}
}
