using System.Linq;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.Testing;

[TestedType(typeof(TemporaryStorageBillWrapperCollection))]
[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
sealed class TemporaryStorageBillWrapperCollectionTest : DocumentWrapperCollectionTest<TemporaryStorageBillWrapperCollection>
{
	public void TestConstruct()
	{
		_ = header.Bills.AddNew();
		AssertEquals("Bills Collection Count (one default bill is added on TemporaryStorageHeader creation)", 2, Collection.Count);
	}

	protected override TemporaryStorageBillWrapperCollection GetNewDocumentWrapperCollection()
		=> new TemporaryStorageBillWrapperCollection(header.Bills.Cast<TemporaryStorageBill>(), Factory);

	protected override object GetNewObjectToWrap() => null;

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader header;
}
