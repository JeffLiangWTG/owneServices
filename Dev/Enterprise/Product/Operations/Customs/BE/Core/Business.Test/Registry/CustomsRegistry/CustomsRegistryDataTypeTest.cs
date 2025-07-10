using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CustomsRegistryDataType))]
class CustomsDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CustomsRegistryDataType>
{
	protected override CustomsRegistryDataType GetNewDataType() => new CustomsRegistryDataType();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var factory = new BusinessObjectFactory();
		OrgHeader org1 = factory.New<OrgHeader>();
		org1.OH_FullName = "Test Company 1";
		org1.OH_Code = "TestComp1";

		var reg1 = new CustomsRegistry { Organization = org1.PK, StartingDate = new ZDateTime(ZDateTime.Today.Year, 1, 1), StartingNo = 1, CurrentNo = 0 };
		var reg2 = new CustomsRegistry { Organization = org1.PK, StartingDate = new ZDateTime(ZDateTime.Today.Year, 1, 1), StartingNo = 1, CurrentNo = 200 };
		var collection = new CustomsRegistryCollection { reg1, reg2 };
		var emptyCollection = new CustomsRegistryCollection();

		return new[]
		{
			new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
			new ValidSampleAndBinaryValueInDB(emptyCollection, DataType.Serialise(emptyCollection))
		};
	}

	protected override string ExpectedEditorName => "CustomsRegistryItemEditor";
}
