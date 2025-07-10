using System.Collections;

namespace CargoWise.Integration
{
	public interface IBusinessObjectCollectionView : IEnumerable
	{
		int Count { get; }
	}
}
