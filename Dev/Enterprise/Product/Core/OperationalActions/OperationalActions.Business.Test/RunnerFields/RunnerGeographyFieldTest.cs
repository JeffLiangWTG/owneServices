using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerGeographyField))]
	internal sealed class RunnerGeographyFieldTest : RunnerFieldTest<RunnerGeographyField, OperationalActionGeographyFieldSupporter, ZGeography>
	{
		public void TestPropertyDefault()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.DefaultingStrategy = "FXD";
			Descriptor.DefaultValue = "POINT (-121 49)";
			AssertEquals(new ZGeography("POINT (-121 49)"), Field.Property);
		}

		public void TestValidateProperty()
		{
			Field.Property = new ZGeography("POINT (-121 49)");
			AssertNoErrors(Field.PropertyInfo);
			Field.Property = ZGeography.Invalid;
			AssertHasErrors(Field.PropertyInfo);
			Field.Property = ZGeography.Empty;
			AssertNoErrors(Field.PropertyInfo);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerGeographyField(Factory, Descriptor, new OperationalActionGeographyFieldSupporter(ColumnOnDummy.Name, false));
		}

		protected override OperationalActionGeographyFieldSupporter NewSupporter()
		{
			return new OperationalActionGeographyFieldSupporter(ColumnOnDummy.Name, false);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_Geography;
			}
		}

		protected override ZGeography EmptyValue
		{
			get
			{
				return ZGeography.Empty;
			}
		}

		protected override ZGeography Value1
		{
			get
			{
				return new ZGeography("POINT (-121 49)");
			}
		}
		#endregion
	}
}
