namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	public class CargoDescriptionLineTest : BaseLineTest
	{
		public void TestDescription()
		{
			AssertEquals("1 X 40'GP CONTAINER STC", GoodsDescription.Description);
			AssertEquals("N/M", MarksAndNumbers.Description);
		}

		public void TestCargoField()
		{
			AssertEquals(CargoField, GoodsDescription.CargoField);
		}
	}
}
