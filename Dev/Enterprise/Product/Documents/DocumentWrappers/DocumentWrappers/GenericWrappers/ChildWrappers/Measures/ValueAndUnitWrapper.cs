using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("ValueAndUnitCodeBlankIfZero")]
	public class ValueAndUnitWrapper : GenericWrapper
	{
		public ValueAndUnitWrapper(ZInt value, ZString unitCode, IBusinessObjectCollection unitList, BusinessObjectFactory factory) : this(new ZDecimal(value), new UnitWrapper(unitCode, unitList, factory), 0, factory) { }
		public ValueAndUnitWrapper(ZInt value, ZString unitCode, CodeDescriptionPairList unitList, BusinessObjectFactory factory) : this(new ZDecimal(value), new UnitWrapper(unitCode, unitList, factory), 0, factory) { }
		public ValueAndUnitWrapper(ZDecimal value, ZString unitCode, IBusinessObjectCollection unitList, BusinessObjectFactory factory) : this(new ZDecimal(value), new UnitWrapper(unitCode, unitList, factory), GetDefaultDecimalsFromUnit(unitCode), factory) { }
		public ValueAndUnitWrapper(ZDecimal value, ZString unitCode, CodeDescriptionPairList unitList, BusinessObjectFactory factory) : this(new ZDecimal(value), new UnitWrapper(unitCode, unitList, factory), GetDefaultDecimalsFromUnit(unitCode), factory) { }
		public ValueAndUnitWrapper(ZDecimal value, ZString unitCode, ZInt decimalPlaces, IBusinessObjectCollection unitList, BusinessObjectFactory factory) : this(value, new UnitWrapper(unitCode, unitList, factory), decimalPlaces, factory) { }
		public ValueAndUnitWrapper(ZDecimal value, ZString unitCode, ZInt decimalPlaces, CodeDescriptionPairList unitList, BusinessObjectFactory factory) : this(value, new UnitWrapper(unitCode, unitList, factory), decimalPlaces, factory) { }

		internal static ValueAndUnitWrapper Empty
		{
			get { return new ValueAndUnitWrapper(ZInt.Zero, ZString.Empty, new CodeDescriptionPairList(), null); }
		}

		static int GetDefaultDecimalsFromUnit(string unitCode)
		{
			if (Core.Constants.Weight.ContainsCode(unitCode))
			{
				return WeightWrapper.StandardDecimalPlaces;
			}
			else if (Core.Constants.Volume.ContainsCode(unitCode))
			{
				return VolumeWrapper.StandardDecimalPlaces;
			}
			return 0;
		}

		public ZDecimal Value
		{
			get { return fValue.Round(fDecimalPlaces); }
		}

		public UnitWrapper Unit
		{
			get { return fUnit; }
		}

		public ZString ValueAndUnitCode
		{
			get { return new ZString(Res.IsRightToLeft(Res.CurrentLanguage) ? Unit.Code + " " + FormatNumeric(Value, fDecimalPlaces) : FormatNumeric(Value, fDecimalPlaces) + " " + Unit.Code).Trim(); }
		}

		public ZString ValueAndUnitCodeBlankIfZero
		{
			get { return Value.IsEmpty ? ZString.Empty : ValueAndUnitCode; }
		}

		public ZString FormatValueAndUnitCode
		{
			get { return new ZString(Res.IsRightToLeft(Res.CurrentLanguage) ? Unit.Code + " " + FormatNumeric(Value, fDecimalPlaces, true) : FormatNumeric(Value, fDecimalPlaces, true) + " " + Unit.Code).Trim(); }
		}

		public ZString FormatValueAndUnitCodeBlankIfZero
		{
			get { return Value.IsEmpty ? ZString.Empty : FormatValueAndUnitCode; }
		}

		protected internal ZInt DecimalPlaces
		{
			get { return fDecimalPlaces; }
		}

		#region Implementation
		protected ValueAndUnitWrapper(ZDecimal value, UnitWrapper unit, ZInt decimalPlaces, BusinessObjectFactory factory)
			: base(null, factory)
		{
			fValue = value;
			fUnit = unit;
			fDecimalPlaces = decimalPlaces;
		}
		readonly ZDecimal fValue;
		readonly UnitWrapper fUnit;
		readonly ZInt fDecimalPlaces;
		#endregion
	}
}
