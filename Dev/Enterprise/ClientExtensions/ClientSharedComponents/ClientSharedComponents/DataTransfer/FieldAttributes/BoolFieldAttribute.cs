using System;
using System.Collections;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class BoolFieldAttribute : FieldAttribute
	{
		public BoolFieldAttribute(short position) : this(position, 1, BoolTypes.YesNo, AlignTypes.None, ' ') { }
		public BoolFieldAttribute(short position, BoolTypes boolType) : this(position, 1, boolType, AlignTypes.None, ' ') { }
		public BoolFieldAttribute(short position, short length) : this(position, length, BoolTypes.YesNo, AlignTypes.None, ' ') { }
		public BoolFieldAttribute(short position, short length, BoolTypes boolType) : this(position, length, boolType, AlignTypes.None, ' ') { }
		public BoolFieldAttribute(short position, short length, BoolTypes boolType, AlignTypes align) : this(position, length, boolType, align, ' ') { }
		public BoolFieldAttribute(short position, short length, BoolTypes boolType, AlignTypes align, char padChar)
			: base(position, length, align, padChar)
		{
			BoolType = boolType;
		}

		protected override IZType GetValueCore(FlatFileDataRow rawData)
		{
			string value = rawData.GetField(Position).ToUpper();
			int idx = (((IList)mFalseCollection).IndexOf(value) + 1) * -1;
			idx += ((IList)mTrueCollection).IndexOf(value) + 1;
			return (ZBool)Convert.ToBoolean(idx);
		}

		protected override void SetValueAsStringAlignLeft(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ((ZBool)value ? mTrueCollection[(int)BoolType] : mFalseCollection[(int)BoolType]).PadRight(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignRight(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ((ZBool)value ? mTrueCollection[(int)BoolType] : mFalseCollection[(int)BoolType]).PadLeft(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignNone(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ((ZBool)value ? mTrueCollection[(int)BoolType] : mFalseCollection[(int)BoolType]));
		}

		public readonly BoolTypes BoolType;

		public enum BoolTypes
		{
			TrueFalse,
			TF,
			YesNo,
			YN
		}

		static readonly string[] mTrueCollection = { "TRUE", "T", "YES", "Y" };
		static readonly string[] mFalseCollection = { "FALSE", "F", "NO", "N" };
	}
}
