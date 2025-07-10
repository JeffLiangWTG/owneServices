using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionFieldDescriptorCollection))]
	internal sealed class OperationalActionFieldDescriptorCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OperationalActionFieldDescriptorCollection>
	{
		public void TestAllowNewAndAllow()
		{
			Action.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			Action.SU_IsSystemDefined = false;
			AssertEquals("Should allow new as not system defined", true, Action.FieldDescriptors.AllowNew);
			AssertEquals("Should allow remove as not system defined", true, Action.FieldDescriptors.AllowRemove);
			Action.SU_IsSystemDefined = true;
			AssertEquals("Should not allow new as now system defined", false, Action.FieldDescriptors.AllowNew);
			AssertEquals("Should not allow remove as now system defined", false, Action.FieldDescriptors.AllowRemove);
			Action.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("Should allow new as editing of system defined actions is now allowed", true, Action.FieldDescriptors.AllowNew);
			AssertEquals("Should allow remove as editing of system defined actions is now allowed", true, Action.FieldDescriptors.AllowRemove);
		}

		public void TestActionSupportableForChildren()
		{
			AssertEquals("AddNew().Context", Context, Collection.AddNew().Context);
		}

		public void TestOrderForChildren()
		{
			for (var i = 0; i < 256; i++)
			{
				unchecked
				{
					AssertEquals("AddNew().Order", (ZByte)(i + 1), Collection.AddNew().Order);
				}
			}

			AssertEquals("AddNew().Order", ZByte.Zero, Collection.AddNew().Order);
			Collection.RemoveAndDeleteAll();
			for (var i = 0; i < 3; i++)
			{
				AssertEquals("AddNew().Order", (ZByte)(i + 1), Collection.AddNew().Order);
			}

			Collection[2].Order = 4;
			AssertEquals("AddNew().Order", (ZByte)5, Collection.AddNew().Order);
			Collection.Remove(Collection[2]);
			AssertEquals("AddNew().Order (logic changed, it's now the highest entry + 1, not the first gap)", (ZByte)6, Collection.AddNew().Order);
			Collection.RemoveAndDeleteAll();
		}

		public void TestOrderForChildren_NoValidationErrorAfterDuplicateOrderRemoved()
		{
			for (var i = 0; i < 3; i++)
			{
				AssertEquals("AddNew().Order", (ZByte)(i + 1), Collection.AddNew().Order);
			}

			Collection[2].Order = 2;
			Assert("Error of duplicate order", Collection[2].HasErrors);
			Collection.Remove(Collection[1]);
			Assert("No error anymore", !Collection[1].HasErrors);
		}

		public void TestOrderForChildren_NoErrorAfterFirstOrderRemoved()
		{
			for (var i = 0; i < 2; i++)
			{
				AssertEquals("AddNew().Order", (ZByte)(i + 1), Collection.AddNew().Order);
			}

			//dubiously increment OrderUsedCount
			Collection.LoadFromBlob(Collection.SaveToBlob());
			Collection.Remove(Collection[0]);
			Assert("No error reported", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestSerialization()
		{
			const string expectedXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + "<OperationalActionFieldDescriptorCollection>\n" + "<OperationalActionFieldDescriptor>\n" + "<FieldName>Collection.Z0_VarCharMax</FieldName>\n" + "<FieldCaption>Text</FieldCaption>\n" + "<Filter>Collection.Z0_Code == \"TST\"</Filter>\n" + "<Order>1</Order>\n" + "<EmptyBehaviour>SKP</EmptyBehaviour>\n" + "<Default strategy=\"FXD\">Blat</Default>\n" + "</OperationalActionFieldDescriptor>\n" + "<OperationalActionFieldDescriptor>\n" + "<FieldName>Collection.Z0_Date</FieldName>\n" + "<FieldCaption>Date</FieldCaption>\n" + "<Filter />\n" + "<Order>2</Order>\n" + "<EmptyBehaviour>SKP</EmptyBehaviour>\n" + "<Default strategy=\"\" />\n" + "</OperationalActionFieldDescriptor>\n" + "</OperationalActionFieldDescriptorCollection>" + "";
			OperationalActionFieldDescriptor fieldDescriptor1 = Collection.AddNew();
			OperationalActionFieldDescriptor fieldDescriptor2 = Collection.AddNew();
			fieldDescriptor1.Order = 1;
			fieldDescriptor1.FieldCaption = "Text";
			fieldDescriptor1.FieldName = "Collection.Z0_VarCharMax";
			fieldDescriptor1.Filter = "Collection.Z0_Code == \"TST\"";
			fieldDescriptor1.DefaultingStrategy = "FXD";
			fieldDescriptor1.DefaultValue = "Blat";
			fieldDescriptor2.Order = 2;
			fieldDescriptor2.FieldCaption = "Date";
			fieldDescriptor2.FieldName = "Collection.Z0_Date";
			byte[] serializedValue = Collection.SaveToBlob();
			AssertMultilineASCIIEquals("Serialised Value", ((char)65279) + expectedXml, System.Text.Encoding.UTF8.GetString(serializedValue).Replace("><", ">\n<"));
			OperationalAction newAction = Factory.New<OperationalAction>();
			newAction.Context = Context;
			OperationalActionFieldDescriptorCollection deserializedValue = new OperationalActionFieldDescriptorCollection(Action);
			deserializedValue.LoadFromBlob(serializedValue);
			AssertEquals("Count", 2, deserializedValue.Count);
			AssertEquals("HasChanges", false, deserializedValue.HasChanges);
			AssertEquals("[0].Context", Context, deserializedValue[0].Context);
			AssertEquals("[0].FieldCaption", "Text", deserializedValue[0].FieldCaption);
			AssertEquals("[0].FieldName", "Collection.Z0_VarCharMax", deserializedValue[0].FieldName);
			AssertEquals("[0].Filter", "Collection.Z0_Code == \"TST\"", deserializedValue[0].Filter);
			AssertEquals("[0].Order", (ZByte)1, deserializedValue[0].Order);
			AssertEquals("[1].Context", Context, deserializedValue[1].Context);
			AssertEquals("[1].FieldCaption", "Date", deserializedValue[1].FieldCaption);
			AssertEquals("[1].FieldName", "Collection.Z0_Date", deserializedValue[1].FieldName);
			AssertEquals("[1].Filter", "", deserializedValue[1].Filter);
			AssertEquals("[1].Order", (ZByte)2, deserializedValue[1].Order);
		}

		public void TestSortByOrder()
		{
			OperationalActionFieldDescriptor fieldDescriptor1 = Collection.AddNew();
			OperationalActionFieldDescriptor fieldDescriptor2 = Collection.AddNew();
			OperationalActionFieldDescriptor fieldDescriptor3 = Collection.AddNew();
			fieldDescriptor1.Order = 3;
			fieldDescriptor2.Order = 1;
			fieldDescriptor3.Order = 2;
			Collection.SortByOrder();
			AssertEquals("[0]", fieldDescriptor2, Collection[0]);
			AssertEquals("[1]", fieldDescriptor3, Collection[1]);
			AssertEquals("[2]", fieldDescriptor1, Collection[2]);
		}

		#region Implementation

		protected override OperationalActionFieldDescriptorCollection GetCollectionToTest()
		{
			return new OperationalActionFieldDescriptorCollection(Action);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OperationalActionFieldDescriptor(Action);
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
