using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerDateField))]
	internal sealed class RunnerDateFieldTest : RunnerFieldTest<RunnerDateField, OperationalActionDateFieldSupporter, ZDate>
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestPropertyDefault()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.DefaultingStrategy = "FXD";
			Descriptor.DefaultValue = "05-JUN-08";
			AssertEquals(new ZDate(2008, 06, 05), Field.Property);
		}

		public void TestValidateProperty()
		{
			Field.Property = ZDate.Today;
			AssertNoErrors(Field.PropertyInfo);
			Field.Property = ZDate.Invalid;
			AssertHasErrors(Field.PropertyInfo);
			Field.Property = ZDate.Empty;
			AssertNoErrors(Field.PropertyInfo);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerDateField(Factory, Descriptor, new OperationalActionDateFieldSupporter(ColumnOnDummy.Name, false));
		}

		protected override OperationalActionDateFieldSupporter NewSupporter()
		{
			return new OperationalActionDateFieldSupporter(ColumnOnDummy.Name, false);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_DateOnly;
			}
		}

		protected override ZDate EmptyValue
		{
			get
			{
				return ZDate.Empty;
			}
		}

		protected override ZDate Value1
		{
			get
			{
				return ZDate.Today;
			}
		}
		#endregion
	}
}
