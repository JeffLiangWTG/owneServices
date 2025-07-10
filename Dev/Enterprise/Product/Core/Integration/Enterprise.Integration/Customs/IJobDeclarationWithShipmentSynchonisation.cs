using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IJobDeclarationWithShipmentSynchonisation : IBaseJobDeclaration
		{
			void SynchroniseWithShipmentIfNeeded();

			ZBool ShouldSynchroniseWithShipmentForDocument { get; }
		}
	}
}
