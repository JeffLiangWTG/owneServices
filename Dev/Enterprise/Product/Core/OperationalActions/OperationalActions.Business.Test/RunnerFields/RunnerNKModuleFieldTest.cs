using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerNKModuleField))]
	internal sealed class RunnerNKModuleFieldTest : RunnerFieldTest<RunnerNKModuleField, OperationalActionNKModuleFieldSupporter, ZString>
	{
		public void TestValidateProperty()
		{
			Field.Property = "AUSYD";
			AssertNoErrors("Valid Code", Field.PropertyInfo);
			Field.Property = "XXXXX";
			AssertHasErrors("Invalid Code", Field.PropertyInfo);
			Field.Property = "";
			AssertNoErrors("Empty Code", Field.PropertyInfo);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerNKModuleField(Factory, Descriptor, new OperationalActionNKModuleFieldSupporter(ColumnOnDummy.Name, false, 5, CreateCollection));
		}

		protected override OperationalActionNKModuleFieldSupporter NewSupporter()
		{
			return new OperationalActionNKModuleFieldSupporter(ColumnOnDummy.Name, false, 5, CreateCollection);
		}

		IBusinessObjectCollection CreateCollection(BusinessObjectFactory factory)
		{
			return new RefUNLOCOCollection(factory);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_FK_Code;
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
				return "AUBNE";
			}
		}
		#endregion
	}
}
