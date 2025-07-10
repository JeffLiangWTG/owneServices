using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerCodeField))]
	internal sealed class RunnerCodeFieldTest : RunnerFieldTest<RunnerCodeField, OperationalActionCodeFieldSupporter, ZString>
	{
		public void TestPropertyDefault()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.DefaultingStrategy = "FXD";
			Descriptor.DefaultValue = "COD";
			AssertEquals("COD", Field.Property);
		}

		public void TestValidateProperty()
		{
			Field.Property = "a";
			AssertNoErrors("Valid Code", Field.PropertyInfo);
			Field.Property = "X";
			AssertHasErrors("Invalid Code", Field.PropertyInfo);
			Field.Property = "";
			AssertNoErrors("Empty Code", Field.PropertyInfo);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerCodeField(Factory, Descriptor, new OperationalActionCodeFieldSupporter(ColumnOnDummy.Name, false, List));
		}

		protected override OperationalActionCodeFieldSupporter NewSupporter()
		{
			return new OperationalActionCodeFieldSupporter(ColumnOnDummy.Name, false, List);
		}

		CodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CodeDescriptionPairList();
					list.AddPair("a", "alpha");
					list.AddPair("b", "beta");
					list.AddPair("g", "gamma");
				}

				return list;
			}
		}

		CodeDescriptionPairList list;
		protected override SchemaColumn ColumnOnDummy
		{
			get
			{
				return DummyBizoSchema.Z0_Code;
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
				return (ZString)"a";
			}
		}
		#endregion
	}
}
