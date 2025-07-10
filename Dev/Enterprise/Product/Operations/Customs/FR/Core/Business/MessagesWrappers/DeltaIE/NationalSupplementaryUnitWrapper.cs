using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class NationalSupplementaryUnitWrapper : INationalSupplementaryUnit
	{
		NationalSupplementaryUnitWrapper(double quantity, string unit)
		{
			this.quantity = Argument.NotNull(quantity, nameof(quantity));
			this.unit = Argument.NotNull(unit, nameof(unit));
		}

		public static NationalSupplementaryUnitWrapper New(double quantity, string unit) => quantity == 0 ? null : new NationalSupplementaryUnitWrapper(quantity, unit);

		readonly double quantity;
		readonly string unit;

		public string NationalMeasurementUnitAndQualifier => nationalMeasurementUnitAndQualifier ?? (nationalMeasurementUnitAndQualifier = unit);
		string nationalMeasurementUnitAndQualifier;

		public double NationalSupplementaryUnits => nationalSupplementaryUnits.Equals(0d) ? GetNationalSupplementaryUnits() : nationalSupplementaryUnits;
		double nationalSupplementaryUnits;

		double GetNationalSupplementaryUnits()
		{
			return nationalSupplementaryUnits = quantity;
		}
	}
}
