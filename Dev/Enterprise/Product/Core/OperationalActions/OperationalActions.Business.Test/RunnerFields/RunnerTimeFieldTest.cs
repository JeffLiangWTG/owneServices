using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerTimeField))]
	internal sealed class RunnerTimeFieldTest : RunnerFieldTest<RunnerTimeField, OperationalActionTimeFieldSupporter, ZTime>
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestPropertyDefault()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.DefaultingStrategy = "FXD";
			Descriptor.DefaultValue = "12:34";
			AssertEquals(new ZTime(12, 34), Field.Property);
		}

		public void TestValidateProperty()
		{
			Field.Property = new ZTime(12, 00);
			AssertNoErrors(Field.PropertyInfo);
			Field.Property = ZTime.Invalid;
			AssertHasErrors(Field.PropertyInfo);
			Field.Property = ZTime.Empty;
			AssertNoErrors(Field.PropertyInfo);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerTimeField(Factory, Descriptor, new OperationalActionTimeFieldSupporter(ColumnOnDummy.Name, false));
		}

		protected override OperationalActionTimeFieldSupporter NewSupporter()
		{
			return new OperationalActionTimeFieldSupporter(ColumnOnDummy.Name, false);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_Time;
			}
		}

		protected override ZTime EmptyValue
		{
			get
			{
				return ZTime.Empty;
			}
		}

		protected override ZTime Value1
		{
			get
			{
				return new ZTime(12, 34);
			}
		}
		#endregion
	}
}
