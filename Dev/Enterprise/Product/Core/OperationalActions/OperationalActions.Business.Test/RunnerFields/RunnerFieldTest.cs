using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal abstract class RunnerFieldTest<FieldT, TSupporter, TProperty> : NonPersistentBusinessObjectTestCase where FieldT : RunnerField<TSupporter, TProperty> where TSupporter : OperationalActionFieldSupporter where TProperty : struct, IZType
	{
		public void TestFilter()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.Filter = "BOB";
			AssertType(typeof(AST.FilterEmpty), ((IOperationalActionFieldValuePair)Field).Filter);
			Descriptor.Filter = "Z0_Number == \"0\"";
			AssertType(typeof(AST.FilterCheckEquality), ((IOperationalActionFieldValuePair)Field).Filter);
		}

		public void TestShouldApply()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Apply;
			Field.Property = EmptyValue;
			AssertEquals("Apply + Empty", true, Field.ShouldApply);
			Descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
			AssertEquals("Skip + Empty", false, Field.ShouldApply);
			Field.Property = Value1;
			AssertEquals("Skip + Non-Empty", true, Field.ShouldApply);
		}

		public void TestMandatory()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Mandatory;
			Field.Property = EmptyValue;
			Field.ValidateProperty();
			AssertHasErrorContaining(Field.PropertyInfo, "Please enter a");
			Field.Property = Value1;
			Field.ValidateProperty();
			AssertNoErrors("Should not have errors.", Field.PropertyInfo);
		}

		public void TestRegisteredWithNew()
		{
			Descriptor.FieldName = ColumnOnDummy.Name;
			Descriptor.FieldCaption = "Caption";
			RunnerField field = NewSupporter().NewRunnerField(Factory, Descriptor);
			AssertNotNull(field);
			AssertEquals("should return the correct type", typeof(FieldT), field.GetType());
			AssertSame("should have the correct field descriptor", Descriptor, field.Descriptor);
		}

		#region Implementation
		public FieldT Field
		{
			get
			{
				return field ?? (field = (FieldT)GetNewBusinessObject());
			}
		}

		FieldT field;
		public DummyBusinessObject Dummy
		{
			get
			{
				return dummy ?? (dummy = Factory.New<DummyBusinessObject>());
			}
		}

		DummyBusinessObject dummy;
		public OperationalActionFieldDescriptor Descriptor
		{
			get
			{
				return descriptor ?? (descriptor = new OperationalActionFieldDescriptor(Action));
			}
		}

		OperationalActionFieldDescriptor descriptor;
		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}

				return action;
			}
		}

		OperationalAction action;
		OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "ModuleName"));
			}
		}

		OperationalActionContext context;
		OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = ActionSupportable.OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;
		MockOperationalActionSupportable ActionSupportable
		{
			get
			{
				return actionSupportable ?? (actionSupportable = new MockOperationalActionSupportable());
			}
		}

		MockOperationalActionSupportable actionSupportable;
		protected abstract TSupporter NewSupporter();
		protected abstract SchemaColumn ColumnOnDummy { get; }

		protected abstract TProperty EmptyValue { get; }

		protected abstract TProperty Value1 { get; }
		#endregion
	}
}
