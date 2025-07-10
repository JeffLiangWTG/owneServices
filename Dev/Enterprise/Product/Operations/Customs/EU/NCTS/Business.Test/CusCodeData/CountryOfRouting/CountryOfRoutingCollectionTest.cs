using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CountryOfRoutingCollection<CountryOfRouting>))]
	sealed class CountryOfRoutingCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCY_Order_SequenceNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var countryOfRouting1 = header.CountriesOfRouting.AddNew();
			var countryOfRouting2 = header.CountriesOfRouting.AddNew();
			var countryOfRouting3 = header.CountriesOfRouting.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("#1", (short)1, countryOfRouting1.CY_Order);
				AssertEquals("#2", (short)2, countryOfRouting2.CY_Order);
				AssertEquals("#3", (short)3, countryOfRouting3.CY_Order);

				header.CountriesOfRouting.RemoveAndDelete(countryOfRouting2);
				AssertEquals("#1 Unchanged", (short)1, countryOfRouting1.CY_Order);
				AssertEquals("#3 Updated #2", (short)2, countryOfRouting3.CY_Order);

				header.CountriesOfRouting.Delete(countryOfRouting1);
				AssertEquals("#3 Updated #1", (short)1, countryOfRouting3.CY_Order);
			});
		}

		public void TestOnCountChanged_MovementMarkAsNeedingValidation_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;
			departureMovement.MarkLightValidationAsValidForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Initially departureMovement.LightValidationIsValid = True", true, departureMovement.LightValidationIsValid);

				var country = nctsHeader.CountriesOfRouting.AddNew();
				AssertEquals("After adding CountriesOfRouting, departureMovement.LightValidationIsValid = True", true, departureMovement.LightValidationIsValid);

				nctsHeader.CountriesOfRouting.Delete(country);
				AssertEquals("After removing CountriesOfRouting, departureMovement.LightValidationIsValid = True", true, departureMovement.LightValidationIsValid);
			});
		}

		public void TestOnCountChanged_MovementMarkAsNeedingValidation_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = nctsHeader.MovementHeader;
			departureMovement.MarkLightValidationAsValidForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Initially departureMovement.LightValidationIsValid = True", true, departureMovement.LightValidationIsValid);

				var country = nctsHeader.CountriesOfRouting.AddNew();
				AssertEquals("After adding CountriesOfRouting, departureMovement.LightValidationIsValid = False", false, departureMovement.LightValidationIsValid);

				nctsHeader.CountriesOfRouting.Delete(country);
				AssertEquals("After removing CountriesOfRouting, departureMovement.LightValidationIsValid = False", false, departureMovement.LightValidationIsValid);
			});
		}

		public void TestMaxCountValidation()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var collection = (ISupportMaxCountValidation)header.CountriesOfRouting;
			CombineAssertions(() =>
			{
				AssertEquals("MaxCount", 99, collection.MaxCountValidator.MaxCount);
				AssertEquals("Notification Type", NotificationType.MessageError, collection.MaxCountValidator.Notification.Type);
				AssertEquals("Notification Message", "You may enter a maximum of 99 Countries.", collection.MaxCountValidator.Notification.Message);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<NctsHeader>();
			return new CountryOfRoutingCollection<CountryOfRouting>(parent);
		}
	}
}
