using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionMethodDescriptorLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMethodGroups()
		{
			AssertEquals("General, Dummy With Methods", Descriptor.Lookups.MethodGroups.CodesAsString);
		}

		public void TestMethodNames()
		{
			AssertEquals("", Descriptor.Lookups.MethodNames.CodesAsString);
			Descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			AssertEquals("Dummy Action Method Without GUI, Dummy Action Method With Large GUI, Dummy Action Method With Over-sized GUI, Dummy Action Method Run Without UI, Dummy Action Method With GUI", Descriptor.Lookups.MethodNames.CodesAsString);
		}

		#region Implementation
		OperationalActionMethodDescriptor Descriptor
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(descriptor))
				{
					descriptor = Action.MethodDescriptors.AddNew();
				}

				return descriptor;
			}
		}

		OperationalActionMethodDescriptor descriptor;
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
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		OperationalActionSupporter ActionSupporter
		{
			get
			{
				if (actionSupporter == null)
				{
					actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter;
					actionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
				}

				return actionSupporter;
			}
		}

		OperationalActionSupporter actionSupporter;
		#endregion
	}
}
