using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ActivitySubtypeAssignmentsRegistryItem))]
	class ActivitySubtypeAssignmentsRegistryItemTest : StronglyTypedRegistryItemTestCase<ActivitySubtypeAssignmentCollection, ActivitySubtypeAssignmentCollection>
	{
		public void TestDefaultValue()
		{
			var collection = new ActivitySubtypeAssignmentCollection();
			var assignment1 = collection.AddNew("ARC", "Description for ARC", true);
			var assignment2 = collection.AddNew("CUS", "Description for CUS", true);

			var item = new ActivitySubtypeAssignmentsRegistryItem("", null, null, null, RegistryStorageFlags.System, collection);
			AssertEquals(2, item.DefaultValue.Count);
		}

		protected override StronglyTypedRegistryItem<ActivitySubtypeAssignmentCollection, ActivitySubtypeAssignmentCollection> GetNewRegistryItem()
		{
			return new ActivitySubtypeAssignmentsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(ActivitySubtypeAssignmentsRegistryDataType))]
	class ActivitySubtypeAssignmentsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ActivitySubtypeAssignmentsRegistryDataType>
	{
		#region Implementation

		protected override ActivitySubtypeAssignmentsRegistryDataType GetNewDataType()
		{
			return new ActivitySubtypeAssignmentsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ActivitySubtypeAssignmentsRegistryEditor"; }
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			ActivitySubtypeAssignment lhsParent = (ActivitySubtypeAssignment)lhs;
			ActivitySubtypeAssignment rhsParent = (ActivitySubtypeAssignment)rhs;

			AssertEquals(lhsParent.ActivitySubtype, rhsParent.ActivitySubtype);
			AssertEquals(lhsParent.Description, rhsParent.Description);
			AssertEquals(lhsParent.Capitalization, rhsParent.Capitalization);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			SetupWorkitemTree();

			var collection = new ActivitySubtypeAssignmentCollection();
			collection.AddNew("PRD", "Description for PRD", true);
			collection.AddNew("FIX", "Description for FIX", true);

			string xml1 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfActivitySubtypeAssignment xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">"
+ @"<ActivitySubtypeAssignment><ActivitySubtype>PRD</ActivitySubtype><Description>Description for PRD</Description><Capitalization>1</Capitalization></ActivitySubtypeAssignment>"
+ @"<ActivitySubtypeAssignment><ActivitySubtype>FIX</ActivitySubtype><Description>Description for FIX</Description><Capitalization>1</Capitalization></ActivitySubtypeAssignment>"
+ @"</ArrayOfActivitySubtypeAssignment>";

			var collection2 = new ActivitySubtypeAssignmentCollection();
			collection2.AddNew("TST", "Description for TST", true);
			collection2.AddNew("TS2", "Description for TS2", true);

			string xml2 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfActivitySubtypeAssignment xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">"
+ @"<ActivitySubtypeAssignment><ActivitySubtype>TST</ActivitySubtype><Description>Description for TST</Description><Capitalization>1</Capitalization></ActivitySubtypeAssignment>"
+ @"<ActivitySubtypeAssignment><ActivitySubtype>TS2</ActivitySubtype><Description>Description for TS2</Description><Capitalization>1</Capitalization></ActivitySubtypeAssignment>"
+ @"</ArrayOfActivitySubtypeAssignment>";

			byte[] byteArrayValue1 = Encoding.Unicode.GetBytes(xml1);
			byte[] byteArrayValue2 = Encoding.Unicode.GetBytes(xml2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(collection2, byteArrayValue2),
			};
		}

		void SetupWorkitemTree()
		{
			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4);
			tree.AddSystemChildren();
			const string ALL = CodeDescriptionBoolTreeNode.AllCode;
			var parent = tree.Find(ALL, ALL, ALL);
			tree.Add("PRD", (NoResString)"Production", true, false, parent);
			tree.Add("FIX", (NoResString)"Fix Defect", true, false, parent);
			tree.Add("TST", (NoResString)"Test", true, false, parent);
			tree.Add("TS2", (NoResString)"Test Two", true, false, parent);
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
		}

		#endregion
	}
}
