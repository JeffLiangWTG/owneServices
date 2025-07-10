using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GoodsMeasureWrapper : IGoodsMeasure
	{
		GoodsMeasureWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		public static GoodsMeasureWrapper New(CusEntryLine entryLine) => entryLine == null ? null : new GoodsMeasureWrapper(entryLine);

		public double GrossMass => grossMass.Equals(0d) ? GetGrossMass() : grossMass;
		double GetGrossMass()
		{
			return grossMass = (double)entryLine.EffectiveGrossWeight.InKilograms;
		}
		double grossMass;

		public ICollection<INationalSupplementaryUnit> NationalSupplementaryUnits => nationalSupplementaryUnits ?? (nationalSupplementaryUnits = GetNationalSupplementaryUnits());
		ICollection<INationalSupplementaryUnit> nationalSupplementaryUnits;

		ICollection<INationalSupplementaryUnit> GetNationalSupplementaryUnits()
		{
			var result = new Collection<INationalSupplementaryUnit>();

			var supplementaryQuantityWrapper = NationalSupplementaryUnitWrapper.New((double)entryLine.SupplementaryQuantity, entryLine.SupplementaryUQ.ToString());
			if (supplementaryQuantityWrapper != null)
			{
				result.Add(supplementaryQuantityWrapper);
			}

			var thirdQuantityWrapper = NationalSupplementaryUnitWrapper.New((double)entryLine.ThirdQuantity, entryLine.ThirdUQ.ToString());
			if (thirdQuantityWrapper != null)
			{
				result.Add(thirdQuantityWrapper);
			}

			return result;
		}

		public double NetMass => netMass.Equals(0d) ? GetNetMass() : netMass;
		double GetNetMass()
		{
			return netMass = (double)entryLine.EffectiveNetWeight.InKilograms;
		}
		double netMass;

		public double SupplementaryUnits => supplementaryUnits.Equals(0d) ? GetSupplementaryUnits() : supplementaryUnits;
		double GetSupplementaryUnits()
		{
			return supplementaryUnits = (double)entryLine.SupplementaryQuantity;
		}
		double supplementaryUnits;

		readonly CusEntryLine entryLine;
	}
}
