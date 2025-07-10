using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IGovernmentProcedure
	{
		ZString CurrentCode { get; }
		ZString PreviousCode { get; }
	}

	public class GovernmentProcedureWrapper : IGovernmentProcedure
	{
		GovernmentProcedureWrapper(ZString currentCode, ZString previous)
		{
			CurrentCode = currentCode;
			PreviousCode = previous;
		}

		public static GovernmentProcedureWrapper New(ZString currentCode, ZString previous)
		{
			return new GovernmentProcedureWrapper(currentCode, previous);
		}

		public ZString CurrentCode { get; }

		public ZString PreviousCode { get; }
	}
}
