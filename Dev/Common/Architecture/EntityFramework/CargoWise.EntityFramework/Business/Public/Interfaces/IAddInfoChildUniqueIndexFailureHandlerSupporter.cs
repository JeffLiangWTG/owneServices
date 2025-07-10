using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		string UniqueIndexName { get; }
		IAddInfoChildSupporter Parent { get; }
		ZString SystemLastEditUser { get; }
		ZDateTime SystemLastEditTimeUtc { get; }
	}
}
