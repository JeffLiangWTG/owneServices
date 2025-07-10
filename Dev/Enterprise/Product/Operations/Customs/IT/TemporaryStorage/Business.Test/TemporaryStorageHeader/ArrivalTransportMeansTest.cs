using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(ArrivalTransportMeans))]
sealed class ArrivalTransportMeansTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidationType()
	{
		var transportMeans = (ArrivalTransportMeans)GetNewBusinessObject();
		AssertType<ArrivalTransportMeansValidation>(transportMeans.Validation);
	}

	public void TestIdentificationNumberCaption()
	{
		AssertEntity<ArrivalTransportMeans>()
			.HasProperty(x => x.TPM_IdentificationNumber)
			.WithCaption("Transport ID");
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Factory.New<ArrivalTransportMeans>();
}
