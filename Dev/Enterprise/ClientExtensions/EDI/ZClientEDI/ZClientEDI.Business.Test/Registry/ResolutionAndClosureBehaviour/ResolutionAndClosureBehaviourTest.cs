using CargoWise.Types;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(ResolutionAndClosureBehaviour))]
	public class ResolutionAndClosureBehaviourTest : CodeDescriptionBoolTreeNodeTest
	{
		public void TestSetSystemDefinedForRootNodes()
		{
			var rootNode = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			rootNode.SetIdForTesting(ZGuid.NewZGuid());
			rootNode.CodeMaxLength = 3;
			rootNode.Code = "AAA";
			rootNode.Description = (NoResString)"DescriptionA";

			var leafNode = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			leafNode.SetIdForTesting(ZGuid.NewZGuid());
			leafNode.CodeMaxLength = 3;
			leafNode.Code = "BBB";
			leafNode.Description = (NoResString)"DescriptionA";
			leafNode.ParentID = rootNode.ID;

			var collection = new ResolutionAndClosureBehaviourCollection();
			collection.Add(rootNode);
			collection.Add(leafNode);

			var dataType = new ResolutionAndClosureBehaviourRegistryDataType(new ResolutionAndClosureBehaviourCollection());
			var serialisedValue = dataType.Serialise(collection);
			var deserialisedBusinessObject = dataType.Deserialise(serialisedValue);

			AssertEquals(2, deserialisedBusinessObject.Count);
			AssertEquals("AAA", deserialisedBusinessObject[0].Code);
			AssertEquals(true, deserialisedBusinessObject[0].SystemDefined);
			AssertEquals("BBB", deserialisedBusinessObject[1].Code);
			AssertEquals(false, deserialisedBusinessObject[1].SystemDefined);
		}

		public void TestAllowSelfResolve()
		{
			var rootNode = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			rootNode.SetIdForTesting(ZGuid.NewZGuid());

			var leafNode1 = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			leafNode1.SetIdForTesting(ZGuid.NewZGuid());
			leafNode1.ParentID = rootNode.ID;
			leafNode1.AllowSelfResolve = true;

			var leafNode2 = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			leafNode2.SetIdForTesting(ZGuid.NewZGuid());
			leafNode2.ParentID = rootNode.ID;
			leafNode2.AllowSelfResolve = false;

			var collection = new ResolutionAndClosureBehaviourCollection();
			collection.Add(rootNode);
			collection.Add(leafNode1);
			collection.Add(leafNode2);

			var dataType = new ResolutionAndClosureBehaviourRegistryDataType(new ResolutionAndClosureBehaviourCollection());
			var serialisedValue = dataType.Serialise(collection);
			var deserialisedBusinessObject = dataType.Deserialise(serialisedValue);

			AssertEquals(3, deserialisedBusinessObject.Count);
			AssertEquals(true, deserialisedBusinessObject[1].AllowSelfResolve);
			AssertEquals(false, deserialisedBusinessObject[2].AllowSelfResolve);
		}

		public void TestDaysREStoCLS()
		{
			var rootNode = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			rootNode.SetIdForTesting(ZGuid.NewZGuid());

			var leafNode1 = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			leafNode1.SetIdForTesting(ZGuid.NewZGuid());
			leafNode1.ParentID = rootNode.ID;
			leafNode1.DaysResolvedToClosed = 9;

			var leafNode2 = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			leafNode2.SetIdForTesting(ZGuid.NewZGuid());
			leafNode2.ParentID = rootNode.ID;
			leafNode2.DaysResolvedToClosed = 11;

			var collection = new ResolutionAndClosureBehaviourCollection();
			collection.Add(rootNode);
			collection.Add(leafNode1);
			collection.Add(leafNode2);

			var dataType = new ResolutionAndClosureBehaviourRegistryDataType(new ResolutionAndClosureBehaviourCollection());
			var serialisedValue = dataType.Serialise(collection);
			var deserialisedBusinessObject = dataType.Deserialise(serialisedValue);

			AssertEquals(3, deserialisedBusinessObject.Count);
			AssertEquals(9, deserialisedBusinessObject[1].DaysResolvedToClosed);
			AssertEquals(11, deserialisedBusinessObject[2].DaysResolvedToClosed);
		}

		public void TestDaysREStoCLS_Validation()
		{
			var node = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			node.DaysResolvedToClosed = 9;
			AssertNoErrors(node.DaysResolvedToClosedInfo);

			node.DaysResolvedToClosed = 0;
			AssertHasError(node.DaysResolvedToClosedInfo, "Resolved to Closed days value must be between 1 and 98.");

			node.DaysResolvedToClosed = 99;
			AssertHasError(node.DaysResolvedToClosedInfo, "Resolved to Closed days value must be between 1 and 98.");

			node.DaysResolvedToClosed = 1;
			AssertNoErrors(node.DaysResolvedToClosedInfo);
		}

		public void TestDaysPNDtoCLS()
		{
			var rootNode = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			rootNode.SetIdForTesting(ZGuid.NewZGuid());

			var leafNode1 = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			leafNode1.SetIdForTesting(ZGuid.NewZGuid());
			leafNode1.ParentID = rootNode.ID;
			leafNode1.DaysPendingCustomerToClosed = 9;

			var leafNode2 = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			leafNode2.SetIdForTesting(ZGuid.NewZGuid());
			leafNode2.ParentID = rootNode.ID;
			leafNode2.DaysPendingCustomerToClosed = 11;

			var collection = new ResolutionAndClosureBehaviourCollection();
			collection.Add(rootNode);
			collection.Add(leafNode1);
			collection.Add(leafNode2);

			var dataType = new ResolutionAndClosureBehaviourRegistryDataType(new ResolutionAndClosureBehaviourCollection());
			var serialisedValue = dataType.Serialise(collection);
			var deserialisedBusinessObject = dataType.Deserialise(serialisedValue);

			AssertEquals(3, deserialisedBusinessObject.Count);
			AssertEquals(9, deserialisedBusinessObject[1].DaysPendingCustomerToClosed);
			AssertEquals(11, deserialisedBusinessObject[2].DaysPendingCustomerToClosed);
		}

		public void TestDaysPNDtoCLS_Validation()
		{
			var node = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			node.DaysPendingCustomerToClosed = 9;
			AssertNoErrors(node.DaysPendingCustomerToClosedInfo);

			node.DaysPendingCustomerToClosed = 0;
			AssertHasError(node.DaysPendingCustomerToClosedInfo, "Pending to Closed days value must be between 1 and 98.");

			node.DaysPendingCustomerToClosed = 99;
			AssertHasError(node.DaysPendingCustomerToClosedInfo, "Pending to Closed days value must be between 1 and 98.");

			node.DaysPendingCustomerToClosed = 1;
			AssertNoErrors(node.DaysPendingCustomerToClosedInfo);
		}

		public void TestCLSReopenRule()
		{
			var rootNode = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			rootNode.SetIdForTesting(ZGuid.NewZGuid());

			var leafNode1 = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			leafNode1.SetIdForTesting(ZGuid.NewZGuid());
			leafNode1.ParentID = rootNode.ID;
			leafNode1.ClosedReopenRule = "ALW";

			var leafNode2 = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			leafNode2.SetIdForTesting(ZGuid.NewZGuid());
			leafNode2.ParentID = rootNode.ID;
			leafNode2.ClosedReopenRule = "NEV";

			var collection = new ResolutionAndClosureBehaviourCollection();
			collection.Add(rootNode);
			collection.Add(leafNode1);
			collection.Add(leafNode2);

			var dataType = new ResolutionAndClosureBehaviourRegistryDataType(new ResolutionAndClosureBehaviourCollection());
			var serialisedValue = dataType.Serialise(collection);
			var deserialisedBusinessObject = dataType.Deserialise(serialisedValue);

			AssertEquals(3, deserialisedBusinessObject.Count);
			AssertEquals("ALW", deserialisedBusinessObject[1].ClosedReopenRule);
			AssertEquals("NEV", deserialisedBusinessObject[2].ClosedReopenRule);
		}

		public void TestCLSReopenRule_Validation()
		{
			var node = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			node.ClosedReopenRule = "ALW";
			AssertNoErrors(node.ClosedReopenRuleInfo);

			node.ClosedReopenRule = "random";
			AssertHasError(node.ClosedReopenRuleInfo, "Enter a valid selection.");

			node.ClosedReopenRule = "NEV";
			AssertNoErrors(node.ClosedReopenRuleInfo);
		}

		#region Clone

		protected override void AssertCloneValues(RegistryBusinessObject cloneBizo)
		{
			var clone = cloneBizo as ResolutionAndClosureBehaviour;
			AssertEquals("Code", "TSCOD", clone.Code);
			AssertEquals("Description", "DescriptionA", clone.Description);
			AssertEquals("CodeMaxLength", 5, clone.CodeMaxLength);
			Assert("SystemDefined", clone.SystemDefined);
			AssertEquals("ParentID", TestParentID, clone.ParentID);
			AssertEquals("ID is cloned", TestID, clone.ID);
			AssertEquals("AllowSelfResolve", true, clone.AllowSelfResolve);
			AssertEquals("DaysResolvedToClosed", 7, clone.DaysResolvedToClosed);
			AssertEquals("DaysPendingCustomerToClosed", 7, clone.DaysPendingCustomerToClosed);
			AssertEquals("ClosedReopenRule", "ALW", clone.ClosedReopenRule);
			var expectedCodeList = new CodeDescriptionPairList();
			expectedCodeList.AddPair("AAA", "AAA Desc");
			AssertContainsExactElementsInAnyOrder("CodeList is cloned", expectedCodeList, clone.CodeList);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (ResolutionAndClosureBehaviour)GetNewBusinessObject();
			result.SetIdForTesting(TestID);
			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;
			result.ParentID = TestParentID;
			result.CodeList = new CodeDescriptionPairList();
			result.CodeList.AddPair("AAA", "AAA Desc");
			result.AllowSelfResolve = true;
			return result;
		}

		readonly ZGuid TestParentID = ZGuid.NewZGuid();
		readonly ZGuid TestID = ZGuid.NewZGuid();

		#endregion

		protected new ResolutionAndClosureBehaviour BizObj
		{
			get { return (ResolutionAndClosureBehaviour)base.BizObj; }
		}
	}
}
