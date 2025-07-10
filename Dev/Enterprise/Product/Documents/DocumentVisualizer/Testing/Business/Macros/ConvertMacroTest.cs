using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class ConvertMacroTest : TestCaseWithMacros
	{
		public void TestConvertLength_String()
		{
			const string macro = "Convert(123.456789, \"KM\", \"M\")";
			AssertMacroRun(macro, new MacroRun { Data = new Dummy(), ExpectedResult = 123456.789m });
		}

		public void TestConvertLength_Property()
		{
			const string macro = "Convert(Value, UnitOfMeasure, \"M\")";

			var data = new Dummy
			{
				Value = 123.456789m,
				UnitOfMeasure = "KM"
			};

			AssertMacroRun(macro, new MacroRun { Data = data, ExpectedResult = 123456.789m });
		}

		public void TestConvertWeight_String()
		{
			const string macro = "Convert(123.456789, \"KG\", \"G\")";
			AssertMacroRun(macro, new MacroRun { Data = new Dummy(), ExpectedResult = 123456.789m });
		}

		public void TestConvertWeight_Property()
		{
			const string macro = "Convert(Value, UnitOfMeasure, \"G\")";

			var data = new Dummy
			{
				Value = 123.456789m,
				UnitOfMeasure = "KG"
			};

			AssertMacroRun(macro, new MacroRun { Data = data, ExpectedResult = 123456.789m });
		}

		public void TestConvertVolume_String()
		{
			const string macro = "Convert(123.456789, \"M3\", \"CF\")";
			AssertMacroRun(macro, new MacroRun { Data = new Dummy(), ExpectedResult = 4359.835357m });
		}

		public void TestConvertVolume_Property()
		{
			const string macro = "Convert(Value, UnitOfMeasure, \"CF\")";

			var data = new Dummy
			{
				Value = 123.456789m,
				UnitOfMeasure = "M3"
			};

			AssertMacroRun(macro, new MacroRun { Data = data, ExpectedResult = 4359.835357m });
		}

		public void TestConvertWithInvalidArgument()
		{
			const string macro = "Convert(Value, UnitOfMeasure, \"XXX\")";

			var data = new Dummy
			{
				Value = 123.456789m,
				UnitOfMeasure = "YYY"
			};

			AssertMacroRun(macro, new MacroRun { Data = data, ExpectedResult = 0m });
		}

		public void TestConvertWithNullValue()
		{
			const string macro = "Convert(none, \"KG\", \"G\")";
			AssertMacroRun(macro, new MacroRun { Data = null, ExpectedResult = 0m });
		}

		[UseSnapshotProtection]
		public void TestConvertCurrency_String()
		{
			const string macro = "Convert(123.45, \"AUD\", \"USD\")";

			SetupExchangeRate();
			AssertMacroRun(macro, new MacroRun { Data = new Dummy(), ExpectedResult = 138.26m });
		}

		[UseSnapshotProtection]
		public void TestConvertCurrency_Property()
		{
			const string macro = "Convert(Value, UnitOfMeasure, \"AUD\")";

			var data = new Dummy
			{
				Value = 123.45m,
				UnitOfMeasure = "USD"
			};

			SetupExchangeRate();
			AssertMacroRun(macro, new MacroRun { Data = data, ExpectedResult = 110.22m });
		}

		[UseSnapshotProtection]
		public void TestConvertCurrencyWithInvalidArgument()
		{
			const string macro = "Convert(Value, UnitOfMeasure, \"XXX\")";

			var data = new Dummy
			{
				Value = 123.456789m,
				UnitOfMeasure = "YYY"
			};

			AssertMacroRun(macro, new MacroRun { Data = data, ExpectedResult = 0m });
		}

		[UseSnapshotProtection]
		public void TestConvertCurrencyWithNullValue()
		{
			const string macro = "Convert(none, \"USD\", \"AUD\")";
			AssertMacroRun(macro, new MacroRun { Data = null, ExpectedResult = 0m });
		}

		#region Implementation

		class Dummy
		{
			public ZDecimal Value { get; set; }
			public string UnitOfMeasure { get; set; }
		}

		protected override IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new DataLibrary(); }
		}

		void SetupExchangeRate()
		{
			var factory = new BusinessObjectFactory();
			var usdCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var today = ZDateTime.Now;

			var exRate = usdCurrency.ExchangeRates.AddNew();
			exRate.RE_StartDate = today.AddDays(-1);
			exRate.RE_ExpiryDate = today.AddDays(1);
			exRate.RE_SellRate = 0.98m;
			exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;

			exRate = usdCurrency.ExchangeRates.AddNew();
			exRate.RE_StartDate = today.AddDays(-1);
			exRate.RE_ExpiryDate = today.AddDays(1);
			exRate.RE_SellRate = 1.12m;
			exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;

			factory.Save();
		}

		#endregion
	}
}
