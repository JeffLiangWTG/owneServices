using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyFourRates : IFourRates
	{
		#region IFourRates Members

		public ZDecimal CustomsRate
		{
			get
			{
				return CustomsRateExposed;
			}
		}
		public ZDecimal CustomsRateExposed;

		public ZDecimal FirstQtyRate
		{
			get
			{
				return FirstQtyRateExposed;
			}
		}
		public ZDecimal FirstQtyRateExposed;

		public ZString FirstUQ
		{
			get
			{
				return FirstUQExpsoed;
			}
		}
		public ZString FirstUQExpsoed;

		public ZDecimal SecondQtyRate
		{
			get
			{
				return SecondQtyRateExposed;
			}
		}
		public ZDecimal SecondQtyRateExposed;

		public ZString SecondUQ
		{
			get
			{
				return SecondUQExposed;
			}
		}
		public ZString SecondUQExposed;

		public ZDecimal OtherDutyFactorRate
		{
			get
			{
				return OtherDutyFactorRateExposed;
			}
		}
		public ZDecimal OtherDutyFactorRateExposed;

		#endregion

	}
}
