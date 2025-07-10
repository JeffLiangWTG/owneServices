using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DutyRateDescriptorTest : TestCaseWithFactory
	{
		public void TestDutyRateDescriptionForFreeInfoInCalc()
		{
			var dummyRate = new DummyCompositeRate();

			dummyRate.CalculationType = Constants.DutyCalcTypes.Free;
			AssertEquals("Duty rate description for free", Constants.DutyCalcTypes.Free, DutyRateDescriptor.GetDutyRateDescription(dummyRate));

			dummyRate.CalculationType = Constants.DutyCalcTypes.InCalc;
			AssertEquals("Duty rate description for InCalc", "Incalculable, DTY in AddInfo Required", DutyRateDescriptor.GetDutyRateDescription(dummyRate));

			dummyRate.CalculationType = Constants.DutyCalcTypes.Info;
			AssertEquals("Duty rate description for Info", "Information Only", DutyRateDescriptor.GetDutyRateDescription(dummyRate));
		}

		public void TestDutyRateDescriptionForCalc()
		{
			var dummyRate = new DummyCompositeRate();
			dummyRate.CalculationType = Constants.DutyCalcTypes.Calc;

			dummyRate.CustomsRate = 5m;
			dummyRate.FirstQtyRate = 10m;
			dummyRate.FirstUQ = "KG";
			dummyRate.SecondQtyRate = 3m;
			dummyRate.SecondUQ = "L";

			AssertEquals("GetSingleDutyRateDescription", "5% + $10.00000/KG + $3.00000/L", DutyRateDescriptor.GetSingleDutyRateDescription(dummyRate));
			AssertEquals("Duty rate description", "5% + $10.00000/KG + $3.00000/L", DutyRateDescriptor.GetDutyRateDescription(dummyRate));
		}

		public void TestDutyRateDescriptionForHigher()
		{
			var dummyRate = new DummyCompositeRate();
			dummyRate.CalculationType = Constants.DutyCalcTypes.Higher;

			dummyRate.CustomsRate = 5m;

			var additionalDutyRate = new DummyCompositeRate();
			additionalDutyRate.FirstQtyRate = 10m;
			additionalDutyRate.FirstUQ = "KG";

			dummyRate.AdditionalDutyRate = additionalDutyRate;

			AssertEquals("Duty rate description", "HIGHER of 5% or $10.00000/KG", DutyRateDescriptor.GetDutyRateDescription(dummyRate));
		}

		public void TestDutyRateDescriptionForLowerWithAdditionalDutyRateBeingNull()
		{
			var dummyRate = new DummyCompositeRate();
			dummyRate.CalculationType = Constants.DutyCalcTypes.Lower;

			dummyRate.CustomsRate = 5m;
			dummyRate.AdditionalDutyRate = null;

			AssertEquals("Duty rate description", "5%", DutyRateDescriptor.GetDutyRateDescription(dummyRate));
		}

		sealed class DummyCompositeRate : ICompositeDutyRate
		{
			public IFourRates AdditionalDutyRate
			{
				get => additionalDutyRate;
				set => additionalDutyRate = value;
			}
			IFourRates additionalDutyRate;

			public ZString CalculationType
			{
				get => calculationType;
				set => calculationType = value;
			}
			ZString calculationType;

			public ZDecimal CustomsRate
			{
				get => customsRate;
				set => customsRate = value;
			}
			ZDecimal customsRate;

			public ZDecimal FirstQtyRate
			{
				get => firstQtyRate;
				set => firstQtyRate = value;
			}
			ZDecimal firstQtyRate;

			public ZString FirstUQ
			{
				get => firstUQ;
				set => firstUQ = value;
			}
			ZString firstUQ;

			public ZDecimal SecondQtyRate
			{
				get => secondQtyRate;
				set => secondQtyRate = value;
			}
			ZDecimal secondQtyRate;

			public ZString SecondUQ
			{
				get => secondUQ;
				set => secondUQ = value;
			}
			ZString secondUQ;

			public ZDecimal OtherDutyFactorRate
			{
				get => otherDutyFactorRate;
				set => otherDutyFactorRate = value;
			}
			ZDecimal otherDutyFactorRate;
		}
	}
}
