using CargoWise.Types;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(IncidentClosureDisposition))]
	public class IncidentClosureDispositionTest : CodeDescriptionBoolTreeNodeTest
	{
		public void TestWriteAndReadNodes()
		{
			var rootNode = (IncidentClosureDisposition)GetNewBusinessObject();
			rootNode.SetIdForTesting(ZGuid.NewZGuid());
			rootNode.CodeMaxLength = 3;
			rootNode.Code = "AAA";
			rootNode.Description = (NoResString)"DescriptionA";
			rootNode.IsResolution = true;

			var leafNode = (IncidentClosureDisposition)GetNewBusinessObject();
			leafNode.SetIdForTesting(ZGuid.NewZGuid());
			leafNode.CodeMaxLength = 3;
			leafNode.Code = "BBB";
			leafNode.Description = (NoResString)"DescriptionA";
			leafNode.IsResolution = false;
			leafNode.ParentID = rootNode.ID;

			var collection = new IncidentClosureDispositionCollection();
			collection.Add(rootNode);
			collection.Add(leafNode);

			var dataType = new IncidentClosureDispositionRegistryDataType(new IncidentClosureDispositionCollection());
			var serialisedValue = dataType.Serialise(collection);
			var deserialisedBusinessObject = dataType.Deserialise(serialisedValue);

			AssertEquals(2, deserialisedBusinessObject.Count);
			AssertEquals("AAA", deserialisedBusinessObject[0].Code);
			AssertEquals(true, deserialisedBusinessObject[0].IsResolution);
			AssertEquals("BBB", deserialisedBusinessObject[1].Code);
			AssertEquals(false, deserialisedBusinessObject[1].IsResolution);
		}

		public void TestCodeValidation_ResolvedCode()
		{
			var node = (IncidentClosureDisposition)GetNewBusinessObject();

			node.Code = IncidentClosureDisposition.ResolvedCode;
			AssertHasError(node.CodeInfo, "This code cannot be used because it is reserved for the Resolved disposition.");

			node.Code = "ZZZ";
			AssertNoErrors(node.CodeInfo);
		}

		public void TestCodeValidation_ResolvedAndClosedCode()
		{
			var node = (IncidentClosureDisposition)GetNewBusinessObject();

			node.Code = IncidentClosureDisposition.ResolvedAndClosedCode;
			AssertHasError(node.CodeInfo, "This code cannot be used because it is reserved for the Closed disposition.");

			node.Code = "ZZZ";
			AssertNoErrors(node.CodeInfo);
		}

		#region Clone

		protected override void AssertCloneValues(RegistryBusinessObject cloneBizo)
		{
			var clone = cloneBizo as IncidentClosureDisposition;
			AssertEquals("Code", "TST", clone.Code);
			AssertEquals("Description", "DescriptionA", clone.Description);
			AssertEquals("CodeMaxLength", 3, clone.CodeMaxLength);
			Assert("SystemDefined", clone.SystemDefined);
			AssertEquals("ParentID", TestParentID, clone.ParentID);
			AssertEquals("ID is cloned", TestID, clone.ID);
			var expectedCodeList = new CodeDescriptionPairList();
			expectedCodeList.AddPair("AAA", "AAA Desc");
			AssertContainsExactElementsInAnyOrder("CodeList is cloned", expectedCodeList, clone.CodeList);
			AssertEquals("IsResolution is cloned", true, clone.IsResolution);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (IncidentClosureDisposition)GetNewBusinessObject();
			result.SetIdForTesting(TestID);
			result.CodeMaxLength = 3;
			result.Code = "TST";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;
			result.ParentID = TestParentID;
			result.CodeList = new CodeDescriptionPairList();
			result.CodeList.AddPair("AAA", "AAA Desc");
			result.IsResolution = true;

			return result;
		}

		readonly ZGuid TestParentID = ZGuid.NewZGuid();
		readonly ZGuid TestID = ZGuid.NewZGuid();

		#endregion

		protected new IncidentClosureDisposition BizObj
		{
			get { return (IncidentClosureDisposition)base.BizObj; }
		}
	}
}
