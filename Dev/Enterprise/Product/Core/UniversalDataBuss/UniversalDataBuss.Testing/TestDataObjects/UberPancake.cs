using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Inner), FlattenedIntoAttributes("Type")]
	public class UberPancake : IDataObject
	{
		[MaxLength(35), Mandatory]
		public ZString? Type { get; set; }
		[MaxLength(80)]
		public ZString? Description { get; set; }

		public CookerType? CookedIn { get; set; }

		public ZDecimal? Diameter { get; set; }
		public ZInt? Calories { get; set; }
		public ZDateTime? RecipeWritten { get; set; }
	}

	public enum CookerType
	{
		FryingPan,
		Oven,
		Microwave,
		Griller,
		Toaster,
	}
}

