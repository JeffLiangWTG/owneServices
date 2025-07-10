using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class FR
		{
			public interface IHarbourJob
			{
				ZDateTime ValuationDate { get; }

				ZString HarbourType { get; }

				ZString DataGrouping { get;  }

				ZString ContainerMode { get; }

				ZBool IsContainerised { get; }

				ZString BarrierPort { get; }

				ZString CustomsOffice { get; }

				ZBool IsDCN { get; }
			}
		}
	}
}
