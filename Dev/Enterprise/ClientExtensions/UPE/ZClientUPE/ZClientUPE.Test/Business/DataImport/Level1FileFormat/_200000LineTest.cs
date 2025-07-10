using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _200000LineTest : TestCase
	{
		public void TestTrackingNumber()
		{
			AssertEquals("A15V04FXKC7", _200000Line.TrackingNumber);
		}

		public void TestServiceLevel()
		{
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDD5X   NNN NN  NN  N USD           USD    T1                               0000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("5", _200000Line.ServiceLevel);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDDSX   NNN NN  NN  N USD           USD    T1                               0000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("S", _200000Line.ServiceLevel);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDDLX   NNN NN  NN  N USD           USD    T1                               0000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("L", _200000Line.ServiceLevel);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDD6X   NNN NN  NN  N USD           USD    T1                               0000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("6", _200000Line.ServiceLevel);
		}

		public void TestShippedOnBoardDate()
		{
			AssertEquals(new ZDateTime(2000, 6, 23), _200000Line.ShippedOnBoardDate);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDD1X   NNN NN  NN  N USD           USD    T1                               0000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals(ZDateTime.Empty, _200000Line.ShippedOnBoardDate);
		}

		public void TestIncoterm()
		{
			//Always default to this value
			AssertEquals("FOB", _200000Line.IncoTerm);
		}

		public void TestGoodsDescription()
		{
			AssertEquals("", _200000Line.GoodsDescription);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           L 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("Documents Only", _200000Line.GoodsDescription);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           D 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("Documents Only", _200000Line.GoodsDescription);
		}

		public void TestShipmentWeightUnit()
		{
			AssertEquals(Core.Constants.Weight.Pounds, _200000Line.ShipmentWeightUnit);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals(Core.Constants.Weight.Kilograms, _200000Line.ShipmentWeightUnit);
		}

		public void TestDeclaredValue()
		{
			AssertEquals(12.34m, _200000Line.DeclaredValue);
		}

		public void TestCurrencyCodeForDeclaredValue()
		{
			AssertEquals("USD", _200000Line.CurrencyCodeForDeclaredValue);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          GBPNNNN Y987654321GBPAAY71267UPS   09651    123456789GBP223456789GBP4000      GBPN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N GBP12345678901GBP    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("GBP", _200000Line.CurrencyCodeForDeclaredValue);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          UKLNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      UKLN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("GBP", _200000Line.CurrencyCodeForDeclaredValue);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          AUDNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      UKLN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("AUD", _200000Line.CurrencyCodeForDeclaredValue);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          AUSNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      UKLN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("AUD", _200000Line.CurrencyCodeForDeclaredValue);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          CNYNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      UKLN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("CNY", _200000Line.CurrencyCodeForDeclaredValue);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          RMBNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      UKLN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("CNY", _200000Line.CurrencyCodeForDeclaredValue);
		}

		public void TestFreightCollect()
		{
			AssertEquals(false, _200000Line.FreightCollect);
		}

		public void TestShipmentType()
		{
			AssertEquals(ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments, _200000Line.ShipmentType);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           L 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals(ShipmentTypeCodeDescriptionPairList.Codes.Letter, _200000Line.ShipmentType);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           D 1 1    KGS US          USDNNNN Y         USDAAY71267UPS   09651             USD         USD4000      USDN0NN   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD           USD    T1                      23JUN20000000           4000       P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals(ShipmentTypeCodeDescriptionPairList.Codes.Documents, _200000Line.ShipmentType);
		}

		public void TestExpandedInvoiceTotal()
		{
			AssertEquals(998877665.54m, _200000Line.ExpandedInvoiceTotal);
		}

		public void TestInsurance()
		{
			AssertEquals(1234567.89m, _200000Line.Insurance);
		}

		public void TestFreight()
		{
			AssertEquals(2234567.89m, _200000Line.Freight);
		}

		public void TestOtherCharges()
		{
			AssertEquals(123456789.01m, _200000Line.OtherCharges);
		}

		public void TestDiscount()
		{
			AssertEquals(9876543.21m, _200000Line.Discount);
		}

		public void TestGoodsValue()
		{
			AssertEquals(881828283.96m, _200000Line.GoodsValue);
		}

		public void TestCurrencyCodeForInvoiceTotal()
		{
			AssertEquals("USD", _200000Line.CurrencyCodeForInvoiceTotal);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          GBPNNNN Y987654321GBPAAY71267UPS   09651    123456789GBP223456789GBP4000      GBPN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N GBP12345678901GBP    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("GBP", _200000Line.CurrencyCodeForInvoiceTotal);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          GBPNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      UKLN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("GBP", _200000Line.CurrencyCodeForInvoiceTotal);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          GBPNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      AUDN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("AUD", _200000Line.CurrencyCodeForInvoiceTotal);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          GBPNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      AUSN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("AUD", _200000Line.CurrencyCodeForInvoiceTotal);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          GBPNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      CNYN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("CNY", _200000Line.CurrencyCodeForInvoiceTotal);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          GBPNNNN Y987654321UKLAAY71267UPS   09651    123456789UKL223456789UKL4000      RMBN1NL   NEDI  23JUN20001  LBS         GBPD1    NNY NN  NN  N UKL12345678901UKL    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("CNY", _200000Line.CurrencyCodeForInvoiceTotal);
		}

		public void TestThirdPartyIndicator()
		{
			AssertEquals("0", _200000Line.ThirdPartyIndicator);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN1NL   NEDI  23JUN20001  LBS         USDD1    NNY NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("1", _200000Line.ThirdPartyIndicator);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN2NL   NEDI  23JUN20001  LBS         USDD1    NNY NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals("2", _200000Line.ThirdPartyIndicator);
		}

		public void TestConsolidatedClearanceFlag()
		{
			AssertEquals("L", _200000Line.ConsolidatedClearanceFlag);
		}

		public void TestIsGCCLead()
		{
			Assert("PRE: Line is not GCC Lead", !_200000Line.IsGCCLead);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NL   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should not be GCC Lead as DutyType is not GCC", !_200000Line.IsGCCLead);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NV   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should not be GCC Lead as DutyType is not GCC", !_200000Line.IsGCCLead);
			_200000Line = new _200000Line("US4196AU9639000626              CA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NL   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should be GCC Lead as ConsolidatedClearanceFlag is L", _200000Line.IsGCCLead);
			_200000Line = new _200000Line("US4196AU9639000626              CA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NV   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should be GCC Lead as ConsolidatedClearanceFlag is V", _200000Line.IsGCCLead);
			_200000Line = new _200000Line("US4196AU9639000626              CA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NH   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should not be GCC Lead as ConsolidatedClearanceFlag is H", !_200000Line.IsGCCLead);
		}

		public void TestIsGCCChild()
		{
			Assert("PRE: Line is not GCC Child", !_200000Line.IsGCCChild);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NH   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should not be GCC Lead as DutyType is not GCC", !_200000Line.IsGCCChild);
			_200000Line = new _200000Line("US4196AU9639000626              CA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NH   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should be GCC Child as ConsolidatedClearanceFlag is H", _200000Line.IsGCCChild);
			_200000Line = new _200000Line("US4196AU9639000626              CA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NL   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should be GCC Child as ConsolidatedClearanceFlag is L", !_200000Line.IsGCCChild);
			_200000Line = new _200000Line("US4196AU9639000626              CA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NL   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should be GCC Child as ConsolidatedClearanceFlag is V", !_200000Line.IsGCCChild);
		}

		public void TestIsConsolidated()
		{
			Assert("PRE: Line is not consolidated", !_200000Line.IsConsolidated);
			_200000Line = new _200000Line("US4196AU9639000626              CA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NL   NEDI  23JUN20001  LBS         USDD1    NNN NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
			Assert("Line should be consolidated", _200000Line.IsConsolidated);
		}

		public void TestBillingTerms()
		{
			AssertEquals(BillingTermsCodeDescriptionPairList.Codes.Prepaid, _200000Line.BillingTerms);
		}

		public void TestDimensionalWeight()
		{
			AssertEquals(0.453592m, _200000Line.DimensionalWeight);
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US          USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NL   NEDI  23JUN20001  LBS         USDD1    NNY NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         123   LBSNNC0017448372UPS6906         N Y 1   N ");
			AssertEquals(12.3m, _200000Line.DimensionalWeight);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_200000Line = new _200000Line("US4196AU9639000626              DA15V04FXKC7200000A15V04FXKC7           N 1 1    LBS US0000001234USDNNNN Y987654321USDAAY71267UPS   09651    123456789USD223456789USD4000      USDN0NL   NEDI  23JUN20001  LBS         USDD1    NNY NN  NN  N USD12345678901USD    T1                      23JUN20000000           99887766554P/PN         1    LBSNNC0017448372UPS6906         N Y 1   N ");
		}

		_200000Line _200000Line;
	}
}
