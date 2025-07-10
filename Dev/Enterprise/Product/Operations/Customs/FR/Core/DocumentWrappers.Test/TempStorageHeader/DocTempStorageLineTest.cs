using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.TempStorageHeader.Testing;

sealed class DocTempStorageLineTest : DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return DocTempStorageLine.New(line, Factory);
	}

	new DocTempStorageLine Wrapper => (DocTempStorageLine)base.Wrapper;

	#region Properties

	public void TestLineNumber()
	{
		AssertEquals(1, Wrapper.LineNumber);
	}

	public void TestPackageType()
	{
		line.TSL_PackageType = "1A";
		AssertEquals("1A", Wrapper.PackageType);
	}

	public void TestPackageNumber()
	{
		line.TSL_PackageQty = 24;
		AssertEquals(24, Wrapper.PackageNumber);
	}

	public void TestGoodsDescription()
	{
		line.TSL_GoodsDescription = "Ceylon Black Tea";
		AssertEquals("Ceylon Black Tea", Wrapper.GoodsDescription);
	}

	public void TestGrossWeight()
	{
		line.TSL_GrossWeight = 16m;
		AssertEquals(16m, Wrapper.GrossWeight);
	}

	[TestDate(2020, 06, 30)]
	public void TestDeadline()
	{
		header.SJH_TempStorageEndDateUtc = ZDateTime.Now;
		AssertEquals("30/06/2020", Wrapper.Deadline);

		header.SJH_TempStorageEndDateUtc = ZDateTime.Now.AddDays(1);
		AssertEquals("01/07/2020", Wrapper.Deadline);
	}

	[TestDate(2020, 06, 30)]
	public void TestSupportingDocument()
	{
		var supportingDocument1 = line.Dec.SupportingDocuments.AddNew();
		supportingDocument1.CSI_Code = "N380";
		supportingDocument1.CSI_DateOfIssue = ZDateTime.Now;
		supportingDocument1.CSI_ReferenceNumber = "FA3445545";

		AssertEquals("N380 20200630 FA3445545", Wrapper.SupportingDocument);

		var supportingDocument2 = line.Dec.SupportingDocuments.AddNew();
		supportingDocument2.CSI_Code = "N355";
		supportingDocument2.CSI_DateOfIssue = ZDateTime.Now.AddDays(1);
		supportingDocument2.CSI_ReferenceNumber = "FA1234567";

		AssertEquals(@"N380 20200630 FA3445545
N355 20200701 FA1234567", Wrapper.SupportingDocument);
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageJobHeader>();
		storageDec = ISTCusTempStorageDec.New(header);
		line = (CusTempStorageLine)storageDec.CusTempStorageLines.AddNew();
	}

	CusTempStorageJobHeader header;
	CusTempStorageLine line;
	CusTempStorageDec storageDec;
}
