namespace Enterprise.DocumentWrappers.GenericWrappers
{
	partial class ChargeableTableStrategy
	{
		enum ColumnType
		{
			// The order of this enum should reflect the desired column ordering
			// with Unit, Minus & Plus appearing together.

			Min,
			First,
			Additional,

			Flat,
			Base,

			Unit,
			Minus,
			Plus,

			Max,
		}
	}
}
