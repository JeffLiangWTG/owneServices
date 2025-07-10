
namespace CargoWise.Types
{
	public interface IObjectCache
	{
		ICultureProvider CultureProvider { get; }
		IDateTimeProvider DateTimeProvider { get; }
		IRoundingProvider RoundingProvider { get; }
	}
}
