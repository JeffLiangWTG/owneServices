using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(GreenhouseGasEmission))]
	class GreenhouseGasEmissionTest : DataObjectTestCase<GreenhouseGasEmission>
	{
		public void TestTryGetCO2eValueInKG_Success()
		{
			// Arrange
			var data = new GreenhouseGasEmission
			{
				CO2ePerTonne = 111m,
				CO2ePerTonneUnit = new UnitOfWeight { Code = "KG" }
			};

			// Act
			var result = data.TryGetCO2eValueInKG(out var value);

			// Assert
			Assert(result);
			AssertEquals(value, 111m);
		}
		public void TestTryGetCO2eValueInKG_Fail()
		{
			// Arrange
			var data = new GreenhouseGasEmission
			{
				CO2ePerTonneUnit = new UnitOfWeight { Code = "KG" }
			};

			// Act
			var result = data.TryGetCO2eValueInKG(out var value);

			// Assert
			Assert(!result);
			AssertEquals(value, 0m);
		}

		public void TestTryGetCO2eTEUValueInKG_Success()
		{
			// Arrange
			var data = new GreenhouseGasEmission
			{
				CO2ePerTEU = 111m,
				CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" }
			};

			// Act
			var result = data.TryGetCO2eTEUValueInKG(out var value);

			// Assert
			Assert(result);
			AssertEquals(value, 111m);
		}

		public void TestTryGetCO2eTEUValueInKG_Fail()
		{
			// Arrange
			var data = new GreenhouseGasEmission
			{
				CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" }
			};

			// Act
			var result = data.TryGetCO2eTEUValueInKG(out var value);

			// Assert
			Assert(!result);
			AssertEquals(value, 0m);
		}

		public void TestIsCO2eValueValid_PerTonne()
		{
			// Arrange
			var data = new GreenhouseGasEmission
			{
				CO2ePerTonne = 100000m,
				CO2ePerTonneUnit = new UnitOfWeight { Code = "KG" }
			};

			// Act
			var result = data.IsCO2eValueValid(out var value, out var isTEU);

			// Assert
			Assert(!result);
			AssertEquals(value, 100000m);
			Assert(!isTEU);
		}

		public void TestIsCO2eValueValid_PerTEU()
		{
			// Arrange
			var data = new GreenhouseGasEmission
			{
				CO2ePerTEU = 100000m,
				CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" }
			};

			// Act
			var result = data.IsCO2eValueValid(out var value, out var isTEU);

			// Assert
			Assert(!result);
			AssertEquals(value, 100000m);
			Assert(isTEU);
		}

		public void TestTryGetTotalCO2eValueInKG_Success()
		{
			// Arrange
			var data = new GreenhouseGasEmission
			{
				CO2e = 111m,
				CO2eUnit = new UnitOfWeight { Code = "KG" }
			};

			// Act
			var result = data.TryGetTotalCO2eValueInKG(out var value);

			// Assert
			Assert(result);
			AssertEquals(value, 111m);
		}
		public void TestTryGetTotalCO2eValueInKG_Fail()
		{
			// Arrange
			var data = new GreenhouseGasEmission
			{
				CO2eUnit = new UnitOfWeight { Code = "KG" }
			};

			// Act
			var result = data.TryGetTotalCO2eValueInKG(out var value);

			// Assert
			Assert(!result);
			AssertEquals(value, 0m);
		}

		public void TestIsTotalCO2eValueValid()
		{
			// Arrange
			var data = new GreenhouseGasEmission
			{
				CO2e = 1e14m,
				CO2eUnit = new UnitOfWeight { Code = "KG" }
			};

			// Act
			var result = data.IsTotalCO2eValueValid(out var value);

			// Assert
			Assert(!result);
			AssertEquals(value, 1e14m);
		}
	}
}
