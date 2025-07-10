using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerBooleanField))]
	internal sealed class RunnerBooleanFieldTest : RunnerFieldTest<RunnerBooleanField, OperationalActionBooleanFieldSupporter, ZString>
	{
		public void TestValidation()
		{
			RunnerBooleanField field = new RunnerBooleanField(Factory, Descriptor, new OperationalActionBooleanFieldSupporter(ColumnOnDummy.Name, false));
			field.Property = "XXX";
			AssertHasError(field.PropertyInfo, "Enter a valid selection.");
			field.Property = BooleanChangeType.Codes.Set;
			AssertNoNotifications(field.PropertyInfo);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerBooleanField(Factory, Descriptor, new OperationalActionBooleanFieldSupporter(ColumnOnDummy.Name, false));
		}

		protected override OperationalActionBooleanFieldSupporter NewSupporter()
		{
			return new OperationalActionBooleanFieldSupporter(ColumnOnDummy.Name, false);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_Bool;
			}
		}

		protected override ZString EmptyValue
		{
			get
			{
				return ZString.Empty;
			}
		}

		protected override ZString Value1
		{
			get
			{
				return BooleanChangeType.Codes.Set;
			}
		}
		#endregion
	}
}
