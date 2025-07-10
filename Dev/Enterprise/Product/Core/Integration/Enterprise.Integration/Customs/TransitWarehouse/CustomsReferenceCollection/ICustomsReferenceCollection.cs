using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICustomsReferenceCollection : IActiveBusinessObjectCollection
		{
			new IBusiness this[int i] { get; }
		}
	}
}
