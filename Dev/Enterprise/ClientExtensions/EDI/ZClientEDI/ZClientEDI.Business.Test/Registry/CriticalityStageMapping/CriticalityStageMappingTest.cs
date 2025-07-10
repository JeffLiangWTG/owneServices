using CargoWise.Types;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(CriticalityStageMapping))]
	public class CriticalityStageMappingTest : CodeDescriptionBoolTreeNodeTest
	{
		public void TestSetSystemDefinedForRootNodes()
		{
			var rootNode = (CriticalityStageMapping)GetNewBusinessObject();
			rootNode.SetIdForTesting(ZGuid.NewZGuid());
			rootNode.CodeMaxLength = 3;
			rootNode.Code = "AAA";
			rootNode.Description = (NoResString)"DescriptionA";
			rootNode.IsDefault = true;

			var leafNode = (CriticalityStageMapping)GetNewBusinessObject();
			leafNode.SetIdForTesting(ZGuid.NewZGuid());
			leafNode.CodeMaxLength = 3;
			leafNode.Code = "BBB";
			leafNode.Description = (NoResString)"DescriptionA";
			leafNode.IsDefault = true;
			leafNode.ParentID = rootNode.ID;

			var collection = new CriticalityStageMappingCollection();
			collection.Add(rootNode);
			collection.Add(leafNode);

			var dataType = new CriticalityStageMappingRegistryDataType(new CriticalityStageMappingCollection());
			var serialisedValue = dataType.Serialise(collection);
			var deserialisedBusinessObject = dataType.Deserialise(serialisedValue);

			AssertEquals(2, deserialisedBusinessObject.Count);
			AssertEquals("AAA", deserialisedBusinessObject[0].Code);
			AssertEquals(true, deserialisedBusinessObject[0].SystemDefined);
			AssertEquals("BBB", deserialisedBusinessObject[1].Code);
			AssertEquals(false, deserialisedBusinessObject[1].SystemDefined);
		}

		public void TestValidateIsDefault()
		{
			var collection = new CriticalityStageMappingCollection();

			var node1 = collection.Add("AAA");
			var node1a = collection.Add("AA1", (NoResString)"AA1", true, true, false, node1);
			var node1b = collection.Add("AA2", (NoResString)"AA2", true, true, false, node1);

			var node2 = collection.Add("BBB");
			var node2a = collection.Add("BB1", (NoResString)"BB1", false, true, false, node2);
			var node2b = collection.Add("BB2", (NoResString)"BB2", true, false, false, node2);

			var node3 = collection.Add("CCC");
			var node3a = collection.Add("CC1", (NoResString)"CC1", true, false, false, node3);
			var node3b = collection.Add("CC2", (NoResString)"CC2", true, false, false, node3);

			var node4 = collection.Add("DDD");
			var node4a = collection.Add("DD1", (NoResString)"DD1", true, true, false, node4);
			var node4b = collection.Add("DD2", (NoResString)"DD2", true, false, false, node4);

			var collectionView1 = new CriticalityStageMappingCollectionView(collection, node1.PK);
			node1a.ValidateIsDefault();
			node1b.ValidateIsDefault();
			AssertHasError(node1a.IsDefaultInfo, "There should be exactly one default value.");
			AssertHasError(node1b.IsDefaultInfo, "There should be exactly one default value.");

			var collectionView2 = new CriticalityStageMappingCollectionView(collection, node2.PK);
			node2a.ValidateIsDefault();
			node2b.ValidateIsDefault();
			AssertHasError(node2a.IsDefaultInfo, "Default value must be enabled.");
			AssertNoErrors(node2b.IsDefaultInfo);

			var collectionView3 = new CriticalityStageMappingCollectionView(collection, node3.PK);
			node3a.ValidateIsDefault();
			node3b.ValidateIsDefault();
			AssertHasError(node3a.IsDefaultInfo, "There should be exactly one default value.");
			AssertHasError(node3b.IsDefaultInfo, "There should be exactly one default value.");

			var collectionView4 = new CriticalityStageMappingCollectionView(collection, node4.PK);
			node4a.ValidateIsDefault();
			node4b.ValidateIsDefault();
			AssertNoErrors(node4a.IsDefaultInfo);
			AssertNoErrors(node4b.IsDefaultInfo);
		}

		#region Clone

		protected override void AssertCloneValues(RegistryBusinessObject cloneBizo)
		{
			var clone = cloneBizo as CriticalityStageMapping;
			AssertEquals("Code", "TSCOD", clone.Code);
			AssertEquals("Description", "DescriptionA", clone.Description);
			AssertEquals("CodeMaxLength", 5, clone.CodeMaxLength);
			Assert("SystemDefined", clone.SystemDefined);
			AssertEquals("ParentID", TestParentID, clone.ParentID);
			AssertEquals("ID is cloned", TestID, clone.ID);
			var expectedCodeList = new CodeDescriptionPairList();
			expectedCodeList.AddPair("AAA", "AAA Desc");
			AssertContainsExactElementsInAnyOrder("CodeList is cloned", expectedCodeList, clone.CodeList);
			AssertEquals("IsDefault is cloned", true, clone.IsDefault);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CriticalityStageMapping)GetNewBusinessObject();
			result.SetIdForTesting(TestID);
			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;
			result.ParentID = TestParentID;
			result.CodeList = new CodeDescriptionPairList();
			result.CodeList.AddPair("AAA", "AAA Desc");
			result.IsDefault = true;

			return result;
		}

		readonly ZGuid TestParentID = ZGuid.NewZGuid();
		readonly ZGuid TestID = ZGuid.NewZGuid();

		#endregion

		protected new CriticalityStageMapping BizObj
		{
			get { return (CriticalityStageMapping)base.BizObj; }
		}
	}
}
