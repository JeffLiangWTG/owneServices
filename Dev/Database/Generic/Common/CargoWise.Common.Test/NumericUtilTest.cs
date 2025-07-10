using System;
using System.Collections.Generic;

namespace CargoWise.Common.Testing
{
	class NumericUtilTest : NUnit.Framework.TestCase
	{
		public void TestIsNumeric()
		{
			foreach (Type type in typeof(int).Assembly.GetExportedTypes())
			{
				AssertEquals(ExpectedNumericTypes.Contains(type), NumericUtil.IsNumeric(type));
			}
		}

		public void TestToStringWithDecimalPlaces()
		{
			AssertEquals("-1.00", NumericUtil.ToStringWithDecimalPlaces(-1, 2));
			AssertEquals("-1.00", NumericUtil.ToStringWithDecimalPlaces(-1f, 2));
			AssertEquals("-1.20", NumericUtil.ToStringWithDecimalPlaces(-1.2, 2));
			AssertEquals("-1.23", NumericUtil.ToStringWithDecimalPlaces(-1.23456, 2));
			AssertEquals("-2", NumericUtil.ToStringWithDecimalPlaces(-2.3456, 0));
			AssertEquals(float.MaxValue.ToString("0", null) + ".00", NumericUtil.ToStringWithDecimalPlaces(float.MaxValue, 2));
			AssertEquals(double.MaxValue.ToString("0", null) + ".00", NumericUtil.ToStringWithDecimalPlaces(double.MaxValue, 2));
			AssertEquals(decimal.MaxValue.ToString("0", null) + ".00", NumericUtil.ToStringWithDecimalPlaces(decimal.MaxValue, 2));
		}

		public void TestChangeDecimalPlaces()
		{
			AssertEquals(1f, NumericUtil.ChangeDecimalPlaces(1f, 2));
			AssertEquals(1.23f, NumericUtil.ChangeDecimalPlaces(1.23456f, 2));
			AssertEquals(1d, NumericUtil.ChangeDecimalPlaces(1d, 2));
			AssertEquals(1.23d, NumericUtil.ChangeDecimalPlaces(1.23456d, 2));
			AssertEquals(1m, NumericUtil.ChangeDecimalPlaces(1m, 2));
			AssertEquals(1.23m, NumericUtil.ChangeDecimalPlaces(1.23456m, 2));
		}

		public void TestIncreaseIndexSafe()
		{
			byte index = 100;
			NumericUtil.IncreaseIndexSafe(ref index);
			AssertEquals((byte)101, index);
			for (byte i = index; i < 255; i++)
			{
				AssertEquals(i, index);
				NumericUtil.IncreaseIndexSafe(ref index);
				AssertEquals((byte)(i + 1), index);
			}

			AssertEquals((byte)255, index);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			NumericUtil.IncreaseIndexSafe(ref index);
			AssertEquals((byte)255, index);
			AssertEquals("IncreaseIndex exceeded value range.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDecreaseIndexSafe()
		{
			byte index = 100;
			NumericUtil.DecreaseIndexSafe(ref index);
			AssertEquals((byte)99, index);
			for (byte i = index; i > 0; i--)
			{
				AssertEquals(i, index);
				NumericUtil.DecreaseIndexSafe(ref index);
				AssertEquals((byte)(i - 1), index);
			}

			AssertEquals((byte)0, index);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			NumericUtil.DecreaseIndexSafe(ref index);
			AssertEquals((byte)0, index);
			AssertEquals("DecreaseIndex exceeded value range.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Implementation
		IList<Type> ExpectedNumericTypes
		{
			get
			{
				return new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), };
			}
		}
		#endregion
	}
}
