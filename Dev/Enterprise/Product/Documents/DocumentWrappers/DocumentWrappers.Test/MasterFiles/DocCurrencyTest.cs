using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCurrency))]
	public class DocCurrencyTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			Money newMoney = new Money(12M, BizCurrency);
			return new DocumentWrapper[]
				{
					DocCurrency.New(BizCurrency, Factory),
					DocCurrency.New(Factory, newMoney.Currency)
				};
		}

		public void TestConstructor()
		{
			int hashCode = DocCurrency.GetHashCode();
			DocCurrency = DocCurrency.New(BizCurrency, Factory);
			AssertEquals("Hash code must be the same", hashCode, DocCurrency.GetHashCode());
		}

		public void TestFormatMoney()
		{
			BizCurrency.RX_Symbol = "$";
			BizCurrency.RX_Code = "USD";
			AssertEquals("$199.95 USD", DocCurrency.FormatMoney(199.95m));

			BizCurrency.RX_Code = "BLA";
			BizCurrency.RX_Symbol = "#";
			AssertEquals("#200.00 BLA", DocCurrency.FormatMoney(200m));
		}

		public void TestCode()
		{
			ZString code = new ZString("TST");

			BizCurrency.RX_Code = code;
			AssertEquals("Wrapped Value", code, DocCurrency.Code);
		}

		public void TestDescription()
		{
			ZString description = new ZString("Test Description");

			BizCurrency.RX_Desc = description;
			AssertEquals("Wrapped Value", description, DocCurrency.Desc);
		}

		public void TestIsActive()
		{
			ZBool isActive = ZBool.True;

			BizCurrency.RX_IsActive = isActive;
			AssertEquals("Wrapped Value", isActive, DocCurrency.IsActive);
		}

		public void TestIsSystem()
		{
			ZBool isSystem = ZBool.True;

			BizCurrency.RX_IsSystem = isSystem;
			AssertEquals("Wrapped Value", isSystem, DocCurrency.IsSystem);
		}

		public void TestSubUnitName()
		{
			ZString subUnitName = new ZString("TEST");

			BizCurrency.RX_SubUnitName = subUnitName;
			AssertEquals("Wrapped Value", subUnitName, DocCurrency.SubUnitName);
		}

		public void TestSubUnitRatio()
		{
			ZInt subUnitRatio = new ZInt(5);

			BizCurrency.RX_SubUnitRatio = subUnitRatio;
			AssertEquals("Wrapped Value", subUnitRatio, DocCurrency.SubUnitRatio);
		}

		public void TestDecimals()
		{
			BizCurrency.RX_SubUnitRatio = 100;
			AssertEquals("Wrapped Value", 2, DocCurrency.Decimals);
			BizCurrency.RX_SubUnitRatio = 1000;
			AssertEquals("Wrapped Value", 3, DocCurrency.Decimals);
		}

		public void TestSymbol()
		{
			ZString symbol = new ZString("$");

			BizCurrency.RX_Symbol = symbol;
			AssertEquals("Wrapped Value", symbol, DocCurrency.Symbol);
		}

		public void TestUnitName()
		{
			ZString unitName = new ZString("Dollar");

			BizCurrency.RX_UnitName = unitName;
			AssertEquals("Wrapped Value", unitName, DocCurrency.UnitName);
		}

		public void TestToString()
		{
			ZString code = new ZString("TST");

			BizCurrency.RX_Code = code;
			AssertEquals("Wrapped Value", code, DocCurrency.ToString());
		}

		#region Implementation

		protected override void SetUp()
		{
			BizCurrency = Factory.New<RefCurrency>();
			DocCurrency = DocCurrency.New(BizCurrency, Factory);
			AssertNotNull("PreCondition: Valid DocCurrency", DocCurrency);

			base.SetUp();
		}

		RefCurrency BizCurrency;
		DocCurrency DocCurrency;

		#endregion
	}
}
