using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerNumericField))]
	internal sealed class RunnerNumericFieldTest : RunnerFieldTest<RunnerNumericField, OperationalActionNumericFieldSupporter, ZDecimal>
	{
		public void TestGetValue()
		{
			RunnerNumericField field = NewField(-10, 10, 2, 3);
			field.Property = 2.178;
			IOperationalActionFieldValuePair pair = field;
			IZType value;
			value = pair.GetValue(typeof(ZDecimal));
			AssertType(typeof(ZDecimal), value);
			AssertEquals(2.178m, value);
			value = pair.GetValue(typeof(ZInt));
			AssertType(typeof(ZInt), value);
			AssertEquals(2, value);
			value = pair.GetValue(typeof(ZShort));
			AssertType(typeof(ZShort), value);
			AssertEquals((short)2, value);
			value = pair.GetValue(typeof(ZByte));
			AssertType(typeof(ZByte), value);
			AssertEquals((byte)2, value);
		}

		public void TestValidateBounds()
		{
			AssertBounds(NewField(-100, 100, 3, 0), -100, 100, "Must be between -100 and 100.");
			AssertBounds(NewField(-50, 150, 6, 3), -50, 150, "Must be between -50.000 and 150.000.");
		}

		public void TestValidateBoundsIgnoreDecimalPrecisionCheck()
		{
			AssertBounds(NewFieldIgnoreDecimalPrecisionCheck(-50, 150, 6, 3), -50, 150, "Must be between -50.000 and 150.000.");
			AssertBounds(NewFieldIgnoreDecimalPrecisionCheck(-100, 100, 4, 3), -100, 100, "Must be between -100.000 and 100.000.");
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return NewField(-100, 100, 5, 3);
		}

		protected override OperationalActionNumericFieldSupporter NewSupporter()
		{
			return new OperationalActionNumericFieldSupporter(ColumnOnDummy.Name, false, -10, 10, 5, 3);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_Decimal;
			}
		}

		protected override ZDecimal EmptyValue
		{
			get
			{
				return ZDecimal.Zero;
			}
		}

		protected override ZDecimal Value1
		{
			get
			{
				return 7m;
			}
		}

		void AssertBounds(RunnerNumericField field, int min, int max, string errorText)
		{
			field.Property = min;
			AssertNoNotifications(field.PropertyInfo);
			field.Property = min - 1;
			AssertHasError(field.PropertyInfo, errorText);
			field.Property = max;
			AssertNoNotifications(field.PropertyInfo);
			field.Property = max + 1;
			AssertHasError(field.PropertyInfo, errorText);
		}

		RunnerNumericField NewField(decimal min, decimal max, int precision, int scale)
		{
			return new RunnerNumericField(Factory, Descriptor, new OperationalActionNumericFieldSupporter(ColumnOnDummy.Name, false, min, max, precision, scale));
		}

		RunnerNumericField NewFieldIgnoreDecimalPrecisionCheck(decimal min, decimal max, int precision, int scale)
		{
			return new RunnerNumericField(Factory, Descriptor, new OperationalActionNumericFieldSupporter(ColumnOnDummy.Name, false, min, max, precision, scale)
			{ IgnoreDecimalPrecisionCheck = true });
		}
		#endregion
	}
}
