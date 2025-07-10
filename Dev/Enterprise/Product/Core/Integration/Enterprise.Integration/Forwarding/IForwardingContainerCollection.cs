using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Forwarding
	{
		public interface IForwardingContainerCollection : IBusinessObjectCollection
		{
			new IForwardingContainer this[int i] { get; }
		}
	}
}
