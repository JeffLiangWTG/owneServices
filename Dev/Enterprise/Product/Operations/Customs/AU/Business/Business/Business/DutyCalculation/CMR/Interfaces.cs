using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IDutyCalculator
	{
		DutyResult Duty { get; }
		ZDecimal GST { get; }
		ZDecimal WET { get; }
		ZDecimal LCT { get; }
		ZDecimal WoodLevy { get; }
		ZDecimal DutyRate { get; }
	}

	public enum DutyRateField { CustomsValue, FirstQty, SecondQty, OtherDutyFactor }

	public struct RateInfo
	{
		public DutyRateField DutyRateField;
		public ZDecimal Rate;
		public ZString Unit;
	}

	public interface ICMRDutyRate
	{
		ZString CalculationType { get; }
		RateInfo[] RatesApplicable { get; }
	}

	public interface IFourRates
	{
		ZDecimal CustomsRate { get; }
		ZDecimal FirstQtyRate { get; }
		ZString FirstUQ { get; }
		ZDecimal SecondQtyRate { get; }
		ZString SecondUQ { get; }
		ZDecimal OtherDutyFactorRate { get; }
	}
}
