using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryNumReferenceCollection : IBusinessObjectCollection
		{
			new ICusEntryNumber this[int i] { get; }
		}
	}
}
