using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionMethodDescriptorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateMethodGroup()
		{
			Descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithoutMethods.Guid;
			AssertHasError(Descriptor.MethodGroupInfo, "Enter a valid Defined Process Group.");

			Descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			AssertNoErrors(Descriptor.MethodGroupInfo);

			Descriptor.MethodGroup = ZGuid.Empty;
			AssertHasError(Descriptor.MethodGroupInfo, "Please enter a Defined Process Group.");
		}

		public void TestValidateMethodID()
		{
			Descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;

			Descriptor.MethodID = new ZGuid("{3ED72BF4-4A58-487c-BAB4-34A5F79F4048}");
			AssertHasError(Descriptor.MethodIDInfo, "Enter a valid selection.");

			Descriptor.MethodID = TestingConstants.DummyActionMethodWithGUI;
			AssertNoErrors(Descriptor.MethodIDInfo);

			Descriptor.MethodID = ZGuid.Empty;
			AssertHasError(Descriptor.MethodIDInfo, "Please enter a Defined Process Name.");

			Descriptor.MethodID = TestingConstants.DummyActionMethodWithOversizedGUI;
			AssertNoNotifications(Descriptor.MethodIDInfo);

			Action.SU_FilterList = "";
			Descriptor.Validation.ValidateMethodID();
			AssertHasError(Descriptor.MethodIDInfo, "This method is only valid for the following country/region but this is not reflected in the filter: AU");

			Descriptor.MethodID = TestingConstants.DummyActionMethodWithLargeGUI;
			Action.SU_FilterList = "";
			AssertNoNotifications(Descriptor.MethodIDInfo);

			Descriptor.Validation.ValidateMethodID();
			AssertHasError(Descriptor.MethodIDInfo, "This method is only valid for the following countries/regions but this is not reflected in the filter: AU NZ SG");
		}

		public void TestValidateUniqueMethodIDAndGroup()
		{
			const string error = "This process already exists on this action.";

			Sibling.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			Sibling.MethodID = TestingConstants.DummyActionMethodWithGUI;

			Descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			Descriptor.MethodID = TestingConstants.DummyActionMethodWithGUI;

			AssertHasError(Descriptor.MethodIDInfo, error);

			Sibling.MethodID = new ZGuid("{3ED72BF4-4A58-487c-BAB4-34A5F79F4048}");
			Descriptor.Validation.ValidateMethodID();
			AssertNoError(Descriptor.MethodIDInfo, error);
		}

		public void TestSystemDefinedActionNoGroupError()
		{
			Action.ReadOnly = true;
			Descriptor.MethodGroup = ActionMethodProviderIDs.DummyWithoutMethods.Guid;
			Descriptor.Validation.ValidateMethodGroup();
			AssertNoNotifications("Enter a valid Defined Process Group.", Descriptor.MethodGroupInfo);
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

		OperationalActionMethodDescriptor Sibling
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(sibling))
				{
					sibling = Action.MethodDescriptors.AddNew();
				}
				return sibling;
			}
		}
		OperationalActionMethodDescriptor sibling;

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
			get { return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name")); }
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
