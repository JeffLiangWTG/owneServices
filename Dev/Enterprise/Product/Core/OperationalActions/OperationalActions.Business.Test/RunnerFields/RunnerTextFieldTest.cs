using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerTextField))]
	internal sealed class RunnerTextFieldTest : RunnerFieldTest<RunnerTextField, OperationalActionTextFieldSupporter, ZString>
	{
		public void TestPropertyDefault()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.DefaultingStrategy = "FXD";
			Descriptor.DefaultValue = "Blat";
			AssertEquals("Blat", Field.Property);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerTextField(Factory, Descriptor, new OperationalActionTextFieldSupporter(ColumnOnDummy.Name, false, 25));
		}

		protected override OperationalActionTextFieldSupporter NewSupporter()
		{
			return new OperationalActionTextFieldSupporter(ColumnOnDummy.Name, false, 25);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_VarCharMax;
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
				return (ZString)"Random Text";
			}
		}
		#endregion
	}
}
