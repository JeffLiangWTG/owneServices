using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeader))]
sealed class CusTempStorageRegHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestSetDefaultValues()
	{
		AssertEquals("SRH_AppCode", ITConstants.TemporaryStorage.AppCodeTSR, header.SRH_AppCode);
	}

	public void TestCusTempStorageRegLines()
	{
		AssertType<CusTempStorageRegLineCollection>(header.CusTempStorageRegLines);
		AssertNotNull("CusTempStorageRegLines", header.CusTempStorageRegLines);
	}

	public void TestSRH_Reference()
	{
		AssertEquals("Caption", "Register Reference", DataBoundResourceStrings.GetDataForProperty(header.SRH_ReferenceInfo).Caption);
	}

	public void TestSRH_PreviousReference()
	{
		AssertEquals("Caption", "Bill MRN", DataBoundResourceStrings.GetDataForProperty(header.SRH_PreviousReferenceInfo).Caption);
	}

	public void TestSRH_TransportID()
	{
		AssertEquals("Caption", "Transport ID", DataBoundResourceStrings.GetDataForProperty(header.SRH_TransportIDInfo).Caption);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
	}
	CusTempStorageRegHeader header;

	BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var cusTempStorageRegHeader = factory.New<CusTempStorageRegHeader>();
		cusTempStorageRegHeader.SRH_Reference = "TEST";
		return cusTempStorageRegHeader;
	}
}
