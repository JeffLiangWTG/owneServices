using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("ValueAndUnitCodeBlankIfZero")]
	public class DimensionsWrapper : GenericWrapper
	{
		public DimensionsWrapper(ZInt length, ZInt width, ZInt height, ZString unitCode, CodeDescriptionPairList unitList, BusinessObjectFactory factory) : this(new ZDecimal(length), new ZDecimal(width), new ZDecimal(height), new UnitWrapper(unitCode, unitList, factory), 0, factory) { }
		public DimensionsWrapper(ZDecimal length, ZDecimal width, ZDecimal height, ZString unitCode, CodeDescriptionPairList unitList, BusinessObjectFactory factory) : this(length, width, height, new UnitWrapper(unitCode, unitList, factory), StandardDecimalPlaces, factory) { }
		public DimensionsWrapper(ZDecimal length, ZDecimal width, ZDecimal height, ZString unitCode, ZInt decimalPlaces, CodeDescriptionPairList unitList, BusinessObjectFactory factory) : this(length, width, height, new UnitWrapper(unitCode, unitList, factory), decimalPlaces, factory) { }

		internal static DimensionsWrapper Empty
		{
			get { return new DimensionsWrapper(ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, new CodeDescriptionPairList(), null); }
		}

		internal const int StandardDecimalPlaces = 2;

		public ZDecimal Height
		{
			get { return height.Round(DecimalPlaces); }
		}

		public ZString HeightAndUnitCode
		{
			get { return new ZString(FormatDimension(height) + " " + Unit.Code).Trim(); }
		}

		public ZString HeightAndUnitCodeBlankIfZero
		{
			get { return height == ZDecimal.Zero ? ZString.Empty : HeightAndUnitCode; }
		}

		public ZDecimal Width
		{
			get { return width.Round(DecimalPlaces); }
		}

		public ZString WidthAndUnitCode
		{
			get { return new ZString(FormatDimension(width) + " " + Unit.Code).Trim(); }
		}

		public ZString WidthAndUnitCodeBlankIfZero
		{
			get { return width == ZDecimal.Zero ? ZString.Empty : WidthAndUnitCode; }
		}

		public ZDecimal Length
		{
			get { return length.Round(DecimalPlaces); }
		}

		public ZString LengthAndUnitCode
		{
			get { return new ZString(FormatDimension(length) + " " + Unit.Code).Trim(); }
		}

		public ZString LengthAndUnitCodeBlankIfZero
		{
			get { return length == ZDecimal.Zero ? ZString.Empty : LengthAndUnitCode; }
		}

		public UnitWrapper Unit
		{
			get { return unit; }
		}

		public ZString Value
		{
			get { return ZString.Format((NoResString)"{0} x {1} x {2}", FormatDimension(length), FormatDimension(width), FormatDimension(height)); }
		}

		public ZString ValueAndUnitCode
		{
			get { return new ZString(Value + " " + Unit.Code).Trim(); }
		}

		public ZString ValueAndUnitCodeBlankIfZero
		{
			get { return (length == ZDecimal.Zero || width == ZDecimal.Zero || height == ZDecimal.Zero) ? ZString.Empty : ValueAndUnitCode; }
		}

		protected internal ZInt DecimalPlaces
		{
			get { return decimalPlaces; }
		}

		#region Implementation
		protected DimensionsWrapper(ZDecimal length, ZDecimal width, ZDecimal height, UnitWrapper unit, ZInt decimalPlaces, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.length = length;
			this.width = width;
			this.height = height;
			this.unit = unit;
			this.decimalPlaces = decimalPlaces;
		}
		readonly ZDecimal length;
		readonly ZDecimal width;
		readonly ZDecimal height;
		readonly UnitWrapper unit;
		readonly ZInt decimalPlaces;
		#endregion

		protected ZString FormatDimension(ZDecimal number)
		{
			ZString result = number.Round(DecimalPlaces).ToString();

			if (result.Contains(DecimalSeparator))
			{
				result = result.TrimEnd('0');
				result = result.TrimEnd(DecimalSeparator.ToCharArray());
			}

			return result;
		}
	}
}
