using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerPKModuleField))]
	internal sealed class RunnerPKModuleFieldTest : RunnerFieldTest<RunnerPKModuleField, OperationalActionPKModuleFieldSupporter, ZGuid>
	{
		public void TestValidateProperty()
		{
			RefUNLOCO loco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			Field.Property = loco.PK;
			AssertNoErrors("Valid Code", Field.PropertyInfo);
			Field.Property = ZGuid.NewZGuid();
			AssertHasErrors("Invalid Code", Field.PropertyInfo);
			Field.Property = ZGuid.Empty;
			AssertNoErrors("Empty Code", Field.PropertyInfo);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerPKModuleField(Factory, Descriptor, new OperationalActionPKModuleFieldSupporter(ColumnOnDummy.Name, false, CreateCollection));
		}

		protected override OperationalActionPKModuleFieldSupporter NewSupporter()
		{
			return new OperationalActionPKModuleFieldSupporter(ColumnOnDummy.Name, false, CreateCollection);
		}

		IBusinessObjectCollection CreateCollection(BusinessObjectFactory factory)
		{
			return new RefUNLOCOCollection(factory);
		}

		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_Guid;
			}
		}

		protected override ZGuid EmptyValue
		{
			get
			{
				return ZGuid.Empty;
			}
		}

		protected override ZGuid Value1
		{
			get
			{
				return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE").PK;
			}
		}
		#endregion
	}
}
