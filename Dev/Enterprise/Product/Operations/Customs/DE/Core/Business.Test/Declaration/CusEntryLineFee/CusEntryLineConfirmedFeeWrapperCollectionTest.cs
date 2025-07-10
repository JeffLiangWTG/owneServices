using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineConfirmedFeeWrapperCollection))]
	sealed class CusEntryLineConfirmedFeeWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusEntryLineConfirmedFeeWrapperCollection>
	{
		public void TestCollectionCreatedFromConfirmedFeesProperty()
		{
			var cusEntryLineFeeWrapperCollection = new CusEntryLineConfirmedFeeWrapperCollection(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("Count", 6, cusEntryLineFeeWrapperCollection.Count);

				AssertLineData(
					"multiple rates", cusEntryLineFeeWrapperCollection[0], "MUL",
					$"18.70 Percent - Customs value{System.Environment.NewLine}10.06 DM/100 kg or Euro/100 kg - Net weight{System.Environment.NewLine}",
					$"18,70 Prozent - Zollwert{System.Environment.NewLine}10,06 DM/100 kg oder Euro/100kg - Eigenmasse{System.Environment.NewLine}");
				AssertLineData(
					"VAT",
					cusEntryLineFeeWrapperCollection[1],
					"19.00 Percent - VAT value",
					string.Empty,
					"19,00 Prozent - EUSt-Wert");
				AssertLineData(
					"Random fee, single",
					cusEntryLineFeeWrapperCollection[2],
					"12.34 Random",
					string.Empty,
					"12,34 Random");

				AssertLineData(
					"No criteria",
					cusEntryLineFeeWrapperCollection[3],
					"Percent",
					string.Empty,
					"Prozent");
				AssertLineData(
					"No known assessmentScale",
					cusEntryLineFeeWrapperCollection[4],
					"VAT value",
					string.Empty,
					"EUSt-Wert");
				AssertLineData(
					"No known assessmentScale nor criteria",
					cusEntryLineFeeWrapperCollection[5],
					"P1 P2",
					string.Empty,
					"P1 P2");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryLine = Factory.New<CusEntryLine>();
			CreateFees();
		}
		CusEntryLine entryLine;

		void AssertLineData(string assertHint, EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper feeLine, string expectedMethodOfCalculation, string expectedQuickViewCard, string expectedDEText)
		{
			var feeLineDE = (CusEntryLineConfirmedFeeWrapper)feeLine;
			AssertEquals($"{assertHint} - Method of calculation", expectedMethodOfCalculation, feeLineDE.CF_MethodOfCalculation);
			AssertEquals($"{assertHint} - QuickView Text", expectedQuickViewCard, feeLineDE.QuickViewCard);
			AssertEquals($"{assertHint} - DE text", expectedDEText, feeLineDE.MethodOfCalculationDE);
		}

		protected override CusEntryLineConfirmedFeeWrapperCollection GetCollectionToTest() => new CusEntryLineConfirmedFeeWrapperCollection(entryLine);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<EU.Business.Declaration.CusEntryLineFee>();

		void CreateFees()
		{
			CreateEntryLineFee("A00", false, "1 00", 18.7m);
			CreateEntryLineFee("A00", true, "3 21", 10.06m);
			CreateEntryLineFee("B00", false, "7 00", 19m);
			CreateEntryLineFee("A20", false, "Random", 12.34m);
			CreateEntryLineFee("102", false, " 00", 19m);
			CreateEntryLineFee("103", false, "7 P2", 19m);
			CreateEntryLineFee("104", false, "P1 P2", 19m);

			void CreateEntryLineFee(ZString chargeType, ZBool isLandedCostOnly, ZString methodOfCalculation, ZDecimal rate)
			{
				var fee = entryLine.ConfirmedFees.AddNew();
				fee.CF_ChargeType = chargeType;
				fee.CF_IsLandedCostOnly = isLandedCostOnly;
				fee.CF_Rate = rate;
				fee.CF_MethodOfCalculation = methodOfCalculation;
			}
		}
	}
}
