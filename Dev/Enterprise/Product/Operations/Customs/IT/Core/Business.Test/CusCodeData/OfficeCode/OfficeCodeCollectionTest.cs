using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(OfficeCodeCollection))]
sealed class OfficeCodeCollectionTest : EuOfficeCodeCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest() => new OfficeCodeCollection(Factory.New<JobDeclaration>());

	public void TestGetOfficeOfDestination()
	{
		var declaration = Factory.New<JobDeclaration>();
		var officeCodeCollection = new OfficeCodeCollection(declaration);

		AssertNull(officeCodeCollection.GetOfficeOfDestination());

		officeCodeCollection.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit, "IT00000");
		officeCodeCollection.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDestination, "IT00001");
		var officeOfDestination = officeCodeCollection.GetOfficeOfDestination();

		AssertNotNull(officeOfDestination);
		AssertEquals("When OfficeCodeCollection has one Destination office", "IT00001", officeOfDestination.CY_Data);

		officeCodeCollection.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDestination, "IT00002");
		AssertNotNull("When OfficeCodeCollection has more Destination office, return the first one", officeCodeCollection.GetOfficeOfDestination());
	}
}
