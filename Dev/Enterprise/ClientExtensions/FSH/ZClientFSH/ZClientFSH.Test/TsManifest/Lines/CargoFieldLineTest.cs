namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	sealed class CargoFieldLineTest : BaseLineTest
	{
		public void TestProperties()
		{
			AssertEquals("CargoSequenceNumber", 1, CargoField.CargoSequenceNumber);
			AssertEquals("NumberOfPackages", 64m, CargoField.NumberOfPackages);
			AssertEquals("CargoType", "PK", CargoField.CargoType);
			AssertEquals("GrossWeight", 22832.77m, CargoField.GrossWeight);
			AssertEquals("NetWeight", 0m, CargoField.NetWeight);
			AssertEquals("GrossCube", 52.2m, CargoField.GrossCube);
		}

		public void TestGoodsDescription()
		{
			AssertEquals("Precondition: no goods descriptions", 0, CargoField.GoodsDescriptions.Count);
			var newGoodsDescription = CargoField.AddNewGoodsDescription(GoodsDescriptionRow);
			AssertNotNull(newGoodsDescription);
			AssertEquals(1, CargoField.GoodsDescriptions.Count);
			AssertEquals(newGoodsDescription, CargoField.GoodsDescriptions[0]);
			AssertEquals(newGoodsDescription.Description, CargoField.CompleteGoodsDescription);
			CargoField.AddNewGoodsDescription(GoodsDescriptionRow);
			AssertEquals(newGoodsDescription.Description + "\n" + newGoodsDescription.Description, CargoField.CompleteGoodsDescription);
		}

		public void TestMarksAndNumbers()
		{
			AssertEquals("Precondition: no marks", 0, CargoField.MarksAndNumbers.Count);
			var newMarksAndNumbers = CargoField.AddNewMarksAndNumbers(MarksAndNumbersRow);
			AssertNotNull(newMarksAndNumbers);
			AssertEquals(1, CargoField.MarksAndNumbers.Count);
			AssertEquals(newMarksAndNumbers, CargoField.MarksAndNumbers[0]);
			AssertEquals(newMarksAndNumbers.Description, CargoField.CompleteMarksAndNumbers);
			CargoField.AddNewMarksAndNumbers(MarksAndNumbersRow);
			AssertEquals(newMarksAndNumbers.Description + "\n" + newMarksAndNumbers.Description, CargoField.CompleteMarksAndNumbers);
		}

		public void TestHazardous()
		{
			AssertNull("Precodition: Hazardous doesn't exist", CargoField.Hazardous);
			var newHazardous = CargoField.AddNewHazardous(HazardousRow);
			AssertNotNull(newHazardous);
			AssertEquals(newHazardous, CargoField.Hazardous);
			AssertEquals(Hazardous.FlashPoint, newHazardous.FlashPoint);
			Assert("Lazy-loading hazardous replaces our one", CargoField.Hazardous != newHazardous);
		}

		public void TestContainerFields()
		{
			AssertEquals("Precondition: ContainerFields is empty", 0, CargoField.ContainerFields.Count);
			CargoField.AttachContainerField(ContainerField);
			AssertEquals(1, CargoField.ContainerFields.Count);
			AssertEquals(ContainerField, CargoField.ContainerFields[0]);
		}
	}
}
