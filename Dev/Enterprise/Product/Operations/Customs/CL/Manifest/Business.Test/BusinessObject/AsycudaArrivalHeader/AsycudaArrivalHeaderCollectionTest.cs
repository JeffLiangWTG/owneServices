using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaArrivalHeaderCollection))]
	class AsycudaArrivalHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<AsycudaArrivalHeaderCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaArrivalHeaderCollection);
		}

		protected override AsycudaArrivalHeaderCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new AsycudaArrivalHeaderCollection(header);
		}

		public void TestDefaultValueFlightNo()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Voyage = "X1";

			var outtturnHeader = header.ArrivalHeaders.AddNew();

			AssertEquals("Flight No should be equal in the Manifest Header and Arrival Header", header.AMA_Voyage, outtturnHeader.ATH_VoyageFlightNo);
		}

		public void TestDefaultValueArrivalDate()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_E_ARV = ZDateTime.Today;

			var outtturnHeader = header.ArrivalHeaders.AddNew();

			AssertEquals("Arrival Date should be equal in the Manifest Header and Arrival Header", header.AMA_E_ARV, outtturnHeader.ATH_ETAAtDischargePort);
		}

		public void TestDefaultValueRegitrationNo()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationNumber = "REG1234";

			var outtturnHeader = header.ArrivalHeaders.AddNew();

			AssertEquals("Regitration No should be equal in the Manifest Header and Arrival Header", header.RegistrationNumber, outtturnHeader.ATH_Reference);
		}

		public void TestDefaultValueRegistrationDate()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationDate = ZDateTime.Today;

			var outtturnHeader = header.ArrivalHeaders.AddNew();

			AssertEquals("Registration Date should be equal in the Manifest Header and Arrival Header", header.RegistrationDate, outtturnHeader.ATH_ReferenceIssueDate);
		}
	}
}
