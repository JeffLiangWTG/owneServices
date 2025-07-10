using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerErrorField))]
	internal sealed class RunnerErrorFieldTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunnerErrorField(Factory, Descriptor, "You cant hug your children with nucular arms!");
		}

		public RunnerErrorField Field
		{
			get
			{
				return field ?? (field = (RunnerErrorField)GetNewBusinessObject());
			}
		}

		RunnerErrorField field;
		public OperationalActionFieldDescriptor Descriptor
		{
			get
			{
				return descriptor ?? (descriptor = new OperationalActionFieldDescriptor(Action));
			}
		}

		OperationalActionFieldDescriptor descriptor;
		public OperationalAction Action
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
		public OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		public OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;
		#endregion
	}
}
