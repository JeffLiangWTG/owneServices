using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusSeal))]
sealed class CusSealTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidationType()
	{
		var seal = (CusSeal)GetNewBusinessObject();
		AssertType<CusSealValidation>(seal.Validation);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);
	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	CusSeal GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var cusSeal = factory.New<CusSeal>();
		cusSeal.BK_SequenceNumber = 1;
		cusSeal.BK_SealNumber = "ABC";
		cusSeal.BK_ParentTableCode = CusInBondContainerSchema.Constants.Prefix;
		return cusSeal;
	}
}
