using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(IGMAsycudaManifestHeader))]
sealed class IGMAsycudaManifestHeaderTest : AsycudaManifestHeaderAbstractTest
{
	public void TestSetDefaultValues()
	{
		AssertEquals(INManifestTypes.Codes.IGM, Header.AMA_ManifestType);
		AssertEquals(ApplicationCodeTypeList.Codes.ShippingLine, Header.AMA_ApplicationCode);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.NewWithValidTestData<IGMAsycudaManifestHeader>();
		header.SuspendCheckBusinessObjectType();
		return header;
	}

	IGMAsycudaManifestHeader Header => header ??= Factory.New<IGMAsycudaManifestHeader>();
	IGMAsycudaManifestHeader header;
}
