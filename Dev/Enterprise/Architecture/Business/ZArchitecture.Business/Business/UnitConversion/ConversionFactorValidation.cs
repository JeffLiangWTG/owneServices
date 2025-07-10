using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public interface IConversionFactorValidation
	{
		void ValidateConversionFactorString();

		void ValidateAll();
	}

	public class ConversionFactorValidation : ZValidation, IConversionFactorValidation
	{
		public ConversionFactorValidation(ConversionFactorViewModel parent)
			: base(parent)
		{
			Parent = parent;
		}

		#region Properties

		public ConversionFactorViewModel Parent
		{
			get;
			private set;
		}

		IEnumerable<UnitsList> SupportedUnits
		{
			get
			{
				if (supportedUnits == null)
				{
					supportedUnits = GetSupportedUnits();
				}

				return supportedUnits;
			}
		}
		IEnumerable<UnitsList> supportedUnits;

		protected virtual IEnumerable<UnitsList> GetSupportedUnits()
		{
			return new[]
			{
				new UnitsList(MeasureUnitType.Weight, Constants.Weight.Codes),
				new UnitsList(MeasureUnitType.Volume, Constants.Volume.Codes),
				new UnitsList(MeasureUnitType.LoadingLength, Constants.LoadingLength.Codes.ToArray()),
			};
		}

		#region StringForBinding

		public void ValidateConversionFactorString()
		{
			ValidateCalculatedProperty(Parent.ConversionFactorStringInfo);
		}

		protected virtual void CheckConversionFactorString()
		{
			var knownUnits = SupportedUnits.SelectMany(u => u.Units).ToArray();
			var numeratorIsKnown = knownUnits.Contains(Parent.ConversionFactor.NumeratorUnit);
			var denominatorIsKnown = knownUnits.Contains(Parent.ConversionFactor.DenominatorUnit);

			if (!Parent.ConversionFactorString.IsEmpty)
			{
				if (Parent.ConversionFactor.Factor > 0 && numeratorIsKnown && denominatorIsKnown)
				{
					var areUnitsFromMeasurementType = SupportedUnits.Any(u => u.Units.Contains(Parent.ConversionFactor.NumeratorUnit) && u.Units.Contains(Parent.ConversionFactor.DenominatorUnit));
					if (areUnitsFromMeasurementType)
					{
						var msg = GetMeasurementTypeErrorMessage(Parent.ConversionFactor.NumeratorUnit, Parent.ConversionFactor.DenominatorUnit, SupportedUnits);
						Parent.ConversionFactorStringInfo.AddError(msg);
					}

					var factor = new ZDecimal(Parent.ConversionFactor.Factor).Normalize();
					if (factor.DecimalPlaces > ConversionFactorViewModel.NumberOfDecimals)
					{
						var msg = Res.GetString("17a67d10-e607-11e8-b0e5-1c1b0d09faa1", "Only {0} decimals for conversion factors are allowed", ConversionFactorViewModel.NumberOfDecimals);
						Parent.ConversionFactorStringInfo.AddError(msg);
					}
				}
				else
				{
					Parent.ConversionFactorStringInfo.AddError(GetInvalidFormatErrorMessage(SupportedUnits));
				}
			}
			else
			{
				MandatoryValidation.CheckEntered(Parent.ConversionFactorStringInfo);
			}
		}

		#endregion

		#endregion

		#region ZValidation

		public override void ValidateAll()
		{
			ValidateConversionFactorString();
		}

		public override Type AutoValidationType
		{
			get
			{
				return typeof(ConversionFactorViewModel);
			}
		}

		#endregion

		#region Errors

		static string GetMeasureTypeName(MeasureUnitType type)
		{
			switch (type)
			{
				case MeasureUnitType.Area:
					return Res.GetString("1d1bfa21-9741-11e6-ac19-fcaa14295823", "Area");
				case MeasureUnitType.Length:
					return Res.GetString("77bef30f-9741-11e6-98e8-fcaa14295823", "Length");
				case MeasureUnitType.LoadingLength:
					return Res.GetString("744c3a80-9741-11e6-bb22-fcaa14295823", "Loading Length");
				case MeasureUnitType.Temperature:
					return Res.GetString("70b77b00-9741-11e6-abb1-fcaa14295823", "Temperature");
				case MeasureUnitType.Volume:
					return Res.GetString("6d1a3000-9741-11e6-89f3-fcaa14295823", "Volume");
				case MeasureUnitType.Weight:
					return Res.GetString("68dd97c0-9741-11e6-aabc-fcaa14295823", "Weight");
				default:
					return type.GetCaption();
			}
		}

		public static string GetMeasurementTypeErrorMessage(string numeratorUnit, string denominatorUnit, IEnumerable<UnitsList> allowedUnits)
		{
			var allowedUnitsAsStrings = allowedUnits.Select(u => ZString.Format("\t{0} ({1})", GetMeasureTypeName(u.MeasureType), string.Join(", ", u.Units)));
			var measurementType = allowedUnits.First(a => a.Units.Contains(numeratorUnit)).MeasureType;

			return Res.GetString("5e752b80-68b3-11e5-8cc6-fcaa14295823",
@"{0} and {1} are both {2} units. Please enter units of different measurement type.
				
Allowed units: 
{3}", numeratorUnit, denominatorUnit, GetMeasureTypeName(measurementType), string.Join("\n", allowedUnitsAsStrings));
		}

		public static string GetInvalidFormatErrorMessage(IEnumerable<UnitsList> allowedUnits)
		{
			var allowedUnitsAsStrings = allowedUnits.Select(u => ZString.Format("\t{0} ({1})", GetMeasureTypeName(u.MeasureType), string.Join(", ", u.Units)));

			return Res.GetString("4a59c51e-68b3-11e5-944b-fcaa14295823",
@"The conversion factor format is invalid.
Please ensure that the value includes positive factor, numerator and denominator volumetric units. For example: 6 L/KG.
				
Allowed units: 
{0}", string.Join("\n", allowedUnitsAsStrings));
		}

		#endregion

		#region Type

		public class UnitsList
		{
			public UnitsList(MeasureUnitType measureType, IEnumerable<string> units)
			{
				MeasureType = measureType;
				Units = units;
			}

			public MeasureUnitType MeasureType { get; private set; }
			public IEnumerable<string> Units { get; private set; }
		}

		#endregion
	}
}
