using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IReleaseStatusWrapper
			{
				ZString ServiceOptionDescription { get; }
				ZString TransactionNumber { get; }
				ZString ProcessingIndicatorDescription { get; }
				ZDateTime ProcessingDate { get; }
				ZDateTime ReleaseDate { get; }
				ZString CCN { get; }
				ZString DeliveryInstructions1 { get; }
				ZString DeliveryInstructions2 { get; }
				ZString ReleaseOffice { get; }
				ZString Warehouse { get; }
				ZString Containers { get; }
			}
		}
	}
}
