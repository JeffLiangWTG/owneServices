using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CurrencyWrapper))]
	sealed class CurrencyWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CurrencyWrapper wrapperEmpty = new CurrencyWrapper(null, Factory);
			AssertEquals("wrapperEmpty.Code", ZString.Empty, wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.CodeAndDescription", ZString.Empty, wrapperEmpty.CodeAndDescription);
			AssertEquals("wrapperEmpty.Description", ZString.Empty, wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.RX_Symbol", ZString.Empty, wrapperEmpty.RX_Symbol);
			AssertEquals("wrapperEmpty.DecimalPlaces", 2, wrapperEmpty.DecimalPlaces);
		}

		public void TestWrapperMappingUSD()
		{
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			CurrencyWrapper wrapperUSD = new CurrencyWrapper(uSD, Factory);
			AssertEquals("wrapperUSD.Code", uSD.RX_Code, wrapperUSD.Code);
			AssertEquals("wrapperUSD.CodeAndDescription", uSD.RX_Code + " - " + uSD.RX_Desc, wrapperUSD.CodeAndDescription);
			AssertEquals("wrapperUSD.Description", uSD.RX_Desc, wrapperUSD.Description);
			AssertEquals("wrapperUSD.RX_Symbol", uSD.RX_Symbol, wrapperUSD.RX_Symbol);
			AssertEquals("wrapperUSD.DecimalPlaces", uSD.Decimals, wrapperUSD.DecimalPlaces);
		}

		public void TestWrapperMappingJPY()
		{
			RefCurrency jPY = RefCurrency.LoadFromCurrencyCode(Factory, "JPY");
			CurrencyWrapper wrapperJPY = new CurrencyWrapper(jPY, Factory);
			AssertEquals("wrapperJPY.Code", jPY.RX_Code, wrapperJPY.Code);
			AssertEquals("wrapperJPY.CodeAndDescription", jPY.RX_Code + " - " + jPY.RX_Desc, wrapperJPY.CodeAndDescription);
			AssertEquals("wrapperJPY.Description", jPY.RX_Desc, wrapperJPY.Description);
			AssertEquals("wrapperJPY.Description", jPY.RX_Symbol, wrapperJPY.RX_Symbol);
			AssertEquals("wrapperJPY.DecimalPlaces", jPY.Decimals, wrapperJPY.DecimalPlaces);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Currency                           (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndDescription                      String
DecimalPlaces                           Int
Description                             String
RX_Symbol                               String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			RefCurrency jPY = RefCurrency.LoadFromCurrencyCode(Factory, "JPY");
			return new CurrencyWrapper(jPY, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CurrencyWrapper(null, Factory);
		}
	}
}
