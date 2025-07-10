using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerDateTimeField))]
	internal sealed class RunnerDateTimeFieldTest : RunnerFieldTest<RunnerDateTimeField, OperationalActionDateTimeFieldSupporter, ZDateTime>
	{
		public void TestPropertyDefault()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.DefaultingStrategy = "FXD";
			Descriptor.DefaultValue = "05-JUN-08";
			AssertEquals(new ZDateTime(2008, 06, 05), Field.Property);
		}

		public void TestValidateProperty()
		{
			Field.Property = ZDateTime.Today;
			AssertNoErrors(Field.PropertyInfo);
			Field.Property = ZDateTime.Invalid;
			AssertHasErrors(Field.PropertyInfo);
			Field.Property = ZDateTime.Empty;
			AssertNoErrors(Field.PropertyInfo);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerDateTimeField(Factory, Descriptor, new OperationalActionDateTimeFieldSupporter(ColumnOnDummy.Name, false, ZDateTimePickerFormat.Long));
		}

		protected override OperationalActionDateTimeFieldSupporter NewSupporter()
		{
			return new OperationalActionDateTimeFieldSupporter(ColumnOnDummy.Name, false, ZDateTimePickerFormat.Long);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_Date;
			}
		}

		protected override ZDateTime EmptyValue
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		protected override ZDateTime Value1
		{
			get
			{
				return ZDateTime.Today;
			}
		}
		#endregion
	}
}
