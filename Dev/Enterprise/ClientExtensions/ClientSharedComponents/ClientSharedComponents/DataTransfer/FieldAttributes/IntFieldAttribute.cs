using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class IntFieldAttribute : FieldAttribute
	{
		public IntFieldAttribute(short position) : this(position, 1, AlignTypes.None) { }
		public IntFieldAttribute(short position, short length) : this(position, length, AlignTypes.None) { }
		public IntFieldAttribute(short position, short length, AlignTypes align) : this(position, length, align, '0') { }
		public IntFieldAttribute(short position, short length, AlignTypes align, char padChar) : base(position, length, align, padChar) { }

		protected override IZType GetValueCore(FlatFileDataRow rawData)
		{
			return rawData.GetFieldAsZInt(Position);
		}

		protected override void SetValueAsStringAlignLeft(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, value.ToString().PadRight(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignRight(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, value.ToString().PadLeft(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignNone(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, value.ToString());
		}
	}
}
