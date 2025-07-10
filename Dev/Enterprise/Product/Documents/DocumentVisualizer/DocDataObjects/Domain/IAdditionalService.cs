using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IAdditionalService
	{
		IAddress Contractor { get; }
		IAddress Location { get; }
		ICodeDescription ServiceCode { get; }
		ZDateTime Booked { get; }
		ZDateTime Completed { get; }
		ZDateTime Duration { get; }
		ZDecimal ServiceCount { get; }
		ZString References { get; }
		ZString ServiceNote { get; }
	}
}
