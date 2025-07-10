using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ParentAndChildCodeDescriptionBoolRegistryDataType))]
	sealed class ParentAndChildCodeDescriptionBoolRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ParentAndChildCodeDescriptionBoolRegistryDataType>
	{
		#region Implementation

		protected override ParentAndChildCodeDescriptionBoolRegistryDataType GetNewDataType()
		{
			return new ParentAndChildCodeDescriptionBoolRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			ParentCodeDescriptionBool lhsParent = (ParentCodeDescriptionBool)lhs;
			ParentCodeDescriptionBool rhsParent = (ParentCodeDescriptionBool)rhs;

			AssertEquals("ChildCodeDescriptions.Count", lhsParent.ChildList.Count, rhsParent.ChildList.Count);

			for (int i = 0; i < lhsParent.ChildList.Count; i++)
			{
				AssertEquals("ChildCodeDescriptions[" + i + "].Code", lhsParent.ChildList[i].Code, rhsParent.ChildList[i].Code);
				AssertEquals("ChildCodeDescriptions[" + i + "].Description", lhsParent.ChildList[i].Description, rhsParent.ChildList[i].Description);
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ParentCodeDescriptionBoolCollection collection = new ParentCodeDescriptionBoolCollection();

			ParentCodeDescriptionBool parent = collection.AddNew();
			parent.Code = "ABC";
			parent.Description = (NoResString)"ABC Description";

			CodeDescriptionBool child1 = parent.ChildList.AddNew();
			CodeDescriptionBool child2 = parent.ChildList.AddNew();
			child1.Code = "C1";
			child2.Code = "C2";
			child1.Description = (NoResString)"Child 1";
			child2.Description = (NoResString)"Child 2";

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,
				0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,
				0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,80,0,97,0,114,0,101,0,110,0,116,0,
				67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,
				111,0,108,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,
				112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,
				0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,
				120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,
				46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,
				45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,80,0,97,0,114,0,101,0,110,0,116,0,
				67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,
				111,0,108,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,65,0,66,0,67,0,60,0,47,0,67,0,111,0,100,0,101,0,62,
				0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,65,0,66,0,67,0,32,0,68,0,
				101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,
				0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,78,0,60,0,47,0,66,0,111,0,111,
				0,108,0,62,0,60,0,67,0,104,0,105,0,108,0,100,0,76,0,105,0,115,0,116,0,62,0,60,0,67,0,111,0,100,0,101,
				0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,
				67,0,111,0,100,0,101,0,62,0,67,0,49,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,
				0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,67,0,104,0,105,0,108,0,100,0,32,0,49,0,60,0,47,0,68,
				0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,
				78,0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,0,47,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,
				114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,67,0,111,0,100,0,101,0,68,
				0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,67,0,
				111,0,100,0,101,0,62,0,67,0,50,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,
				114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,67,0,104,0,105,0,108,0,100,0,32,0,50,0,60,0,47,0,68,0,
				101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,78,
				0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,0,47,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,
				105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,47,0,67,0,104,0,105,0,108,0,100,
				0,76,0,105,0,115,0,116,0,62,0,60,0,47,0,80,0,97,0,114,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,68,
				0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,47,0,
				65,0,114,0,114,0,97,0,121,0,79,0,102,0,80,0,97,0,114,0,101,0,110,0,116,0,67,0,111,0,100,0,101,0,68,0,
				101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0          };

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
