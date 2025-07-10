using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(RunnerFieldCollection))]
	internal sealed class RunnerFieldCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RunnerFieldCollection>
	{
		#region Implementation
		protected override RunnerFieldCollection GetCollectionToTest()
		{
			return new RunnerFieldCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RunnerTextField(Factory, Descriptor, new OperationalActionTextFieldSupporter("Field", false, 25));
		}

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
