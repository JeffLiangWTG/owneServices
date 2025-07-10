using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyCMRDutyRate : ICMRDutyRate
	{
		#region ICMRDutyRate Members

		public ZString CalculationTypeExposed;
		public ZString CalculationType
		{
			get
			{
				return CalculationTypeExposed;
			}
		}

		public RateInfo[] RatesApplicableExposed = System.Array.Empty<RateInfo>();
		public RateInfo[] RatesApplicable
		{
			get { return RatesApplicableExposed; }
		}

		#endregion
	}
}
