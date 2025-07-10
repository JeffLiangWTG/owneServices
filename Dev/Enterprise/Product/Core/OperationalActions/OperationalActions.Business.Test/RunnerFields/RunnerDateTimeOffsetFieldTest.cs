using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerDateTimeOffsetField))]
	internal sealed class RunnerDateTimeOffsetFieldTest : RunnerFieldTest<RunnerDateTimeOffsetField, OperationalActionDateTimeOffsetFieldSupporter, ZDateTimeOffset>
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestPropertyDefault()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.DefaultingStrategy = "FXD";
			Descriptor.DefaultValue = "05-JUN-08";
			AssertEquals(new ZDateTimeOffset(2008, 06, 05, 0, 0, 0, TimeSpan.FromHours(10)), Field.Property);
		}

		public void TestValidateProperty()
		{
			Field.Property = ZDateTimeOffset.Today;
			AssertNoErrors(Field.PropertyInfo);
			Field.Property = ZDateTimeOffset.Invalid;
			AssertHasErrors(Field.PropertyInfo);
			Field.Property = ZDateTimeOffset.Empty;
			AssertNoErrors(Field.PropertyInfo);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerDateTimeOffsetField(Factory, Descriptor, new OperationalActionDateTimeOffsetFieldSupporter(ColumnOnDummy.Name, false));
		}

		protected override OperationalActionDateTimeOffsetFieldSupporter NewSupporter()
		{
			return new OperationalActionDateTimeOffsetFieldSupporter(ColumnOnDummy.Name, false);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_DateTimeOffset;
			}
		}

		protected override ZDateTimeOffset EmptyValue
		{
			get
			{
				return ZDateTimeOffset.Empty;
			}
		}

		protected override ZDateTimeOffset Value1
		{
			get
			{
				return ZDateTimeOffset.Today;
			}
		}
		#endregion
	}
}
