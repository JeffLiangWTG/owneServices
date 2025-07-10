using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IPortReferenceCollection : IActiveBusinessObjectCollection
		{
			new ICusEntryNumber this[int i] { get; }
		}
	}
}
