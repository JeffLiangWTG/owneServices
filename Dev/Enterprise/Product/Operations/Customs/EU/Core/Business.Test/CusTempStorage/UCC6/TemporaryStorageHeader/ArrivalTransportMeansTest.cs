using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(ArrivalTransportMeans))]
	sealed class ArrivalTransportMeansTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationType()
		{
			var transportMeans = (ArrivalTransportMeans)GetNewBusinessObject();
			AssertType<ArrivalTransportMeansValidation>(transportMeans.Validation);
		}

		public void TestTPM_IdentificationNumberCaption()
		{
			AssertEquals("TPM_IdentificationNumber caption should be equal to Arrival Transport Means", "Arrival Transport Means", DataBoundResourceStrings.GetDataForProperty(typeof(ArrivalTransportMeans), nameof(ArrivalTransportMeans.TPM_IdentificationNumber)).Caption);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Factory.New<ArrivalTransportMeans>();
	}
}
