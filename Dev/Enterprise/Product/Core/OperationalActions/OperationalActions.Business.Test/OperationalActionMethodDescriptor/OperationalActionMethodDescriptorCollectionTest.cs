using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionMethodDescriptorCollection))]
	internal sealed class OperationalActionMethodDescriptorCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OperationalActionMethodDescriptorCollection>
	{
		public void TestAllowNewAndAllow()
		{
			Action.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			Action.SU_IsSystemDefined = false;
			AssertEquals("Should allow new as not system defined", true, Action.MethodDescriptors.AllowNew);
			AssertEquals("Should allow remove as not system defined", true, Action.MethodDescriptors.AllowRemove);
			Action.SU_IsSystemDefined = true;
			AssertEquals("Should not allow new as now system defined", false, Action.MethodDescriptors.AllowNew);
			AssertEquals("Should not allow remove as now system defined", false, Action.MethodDescriptors.AllowRemove);
			Action.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("Should allow new as editing of system defined actions is now allowed", true, Action.MethodDescriptors.AllowNew);
			AssertEquals("Should allow remove as editing of system defined actions is now allowed", true, Action.MethodDescriptors.AllowRemove);
		}

		public void TestActionSupportableForChildren()
		{
			AssertEquals("AddNew().ActionSupportable", Context, Collection.AddNew().Context);
		}

		public void TestSerialization()
		{
			ZGuid group1Id = ZGuid.NewZGuid();
			ZGuid group2Id = ZGuid.NewZGuid();
			OperationalActionMethodDescriptor methodDescriptor1 = Collection.AddNew();
			OperationalActionMethodDescriptor methodDescriptor2 = Collection.AddNew();
			methodDescriptor1.MethodGroup = group1Id;
			methodDescriptor1.MethodID = TestingConstants.DummyActionMethodWithGUI;
			methodDescriptor2.MethodGroup = group2Id;
			methodDescriptor2.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			byte[] serializedValue = Collection.SaveToBlob();
			OperationalAction action = Factory.New<OperationalAction>();
			action.Context = Context;
			OperationalActionMethodDescriptorCollection deserializedValue = new OperationalActionMethodDescriptorCollection(Action);
			deserializedValue.LoadFromBlob(serializedValue);
			AssertEquals("Count", 2, deserializedValue.Count);
			AssertEquals("HasChanges", false, deserializedValue.HasChanges);
			AssertEquals("[0].Context", Context, deserializedValue[0].Context);
			AssertEquals("[0].MethodGroup", group1Id, deserializedValue[0].MethodGroup);
			AssertEquals("[0].MethodName", TestingConstants.DummyActionMethodWithGUI, deserializedValue[0].MethodID);
			AssertEquals("[1].Context", Context, deserializedValue[1].Context);
			AssertEquals("[1].MethodGroup", group2Id, deserializedValue[1].MethodGroup);
			AssertEquals("[1].MethodName", TestingConstants.DummyActionMethodWithoutGUI, deserializedValue[1].MethodID);
		}

		public void TestOrderForChildren()
		{
			for (int i = 0; i < 256; i++)
			{
				unchecked
				{
					AssertEquals("AddNew().Order", (ZByte)(i + 1), Collection.AddNew().Order);
				}
			}

			AssertEquals("AddNew().Order", ZByte.Zero, Collection.AddNew().Order);
		}

		#region Implementation

		protected override OperationalActionMethodDescriptorCollection GetCollectionToTest()
		{
			return new OperationalActionMethodDescriptorCollection(Action);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OperationalActionMethodDescriptor(Action);
		}

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
				return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;
		#endregion
	}
}
