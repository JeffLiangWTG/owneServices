using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CodeDescriptionPairListRegistryDataType))]
	public class CodeDescriptionPairListRegistryDataTypeTest : RegistryDataTypeTestCase<CodeDescriptionPairListRegistryDataType>
	{
		[ExpectExceptionMessage(typeof(RegistryValidationException), "Please enter at least one record for this list.")]
		public void TestValidate()
		{
			CodeDescriptionPairListRegistryDataType dataType = new CodeDescriptionPairListRegistryDataType(3, false);
			dataType.Validate(null, new CodeDescriptionPairList(), Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectNoExceptions]
		public void TestValidate_EmptyCodes_Allowed()
		{
			CodeDescriptionPairListRegistryDataType dataType = new CodeDescriptionPairListRegistryDataType(3, false);
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("", "hello");
			dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "You cannot enter an item with no Code.")]
		public void TestValidate_EmptyCodes_NotAllowed()
		{
			CodeDescriptionPairListRegistryDataType dataType = new CodeDescriptionPairListRegistryDataType(3, false);
			dataType.AllowEmptyCodes = false;
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("", "hello");
			dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectNoExceptions]
		public void TestValidate_NullOrEmptyDescriptions_Allowed()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			CodeDescriptionPairListRegistryDataType dataType = new CodeDescriptionPairListRegistryDataType(3, false);

			list.AddPair("ABC", "This is ABC");
			list.AddPair("DEF", string.Empty);
			list.AddPair("GHI", null);

			dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "You cannot enter an item with no Description.")]
		public void TestValidate_NullOrEmptyDescriptions_NotAllowed()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			CodeDescriptionPairListRegistryDataType dataType = new CodeDescriptionPairListRegistryDataType(3, false)
			{
				AllowEmptyDescriptions = false
			};

			list.AddPair("ABC", "This is ABC");
			list.AddPair("DEF", string.Empty);
			list.AddPair("GHI", null);

			dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void TestValiate_DuplicateDescriptions_Allowed()
		{
			var list = new CodeDescriptionPairList();
			var dataType = new CodeDescriptionPairListRegistryDataType(10, false) { AllowDuplicateDescriptions = true };

			list.AddPair("ABC", "This is ABC");
			list.AddPair("DEF", "This is ABC");

			AssertNoExceptionThrown(() => dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestValiate_DuplicateDescriptions_NotAllowed()
		{
			var list = new CodeDescriptionPairList();
			var dataType = new CodeDescriptionPairListRegistryDataType(10, false) { AllowDuplicateDescriptions = false };

			list.AddPair("ABC", "This is ABC");
			list.AddPair("DEF", "This is ABC");

			AssertExceptionThrown<RegistryValidationException>("The descriptions cannot be duplicate", () => dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		[ExpectNoExceptions]
		public void TestEmptyListAllowedByDefault()
		{
			DataType.Validate(null, new CodeDescriptionPairList(), Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "The default code 'CAP' has been removed from this list. Default values on this list cannot be removed.")]
		public void TestValidate_DefaultValuesToBeKept()
		{
			var item = new OrgListRegistryItem("OrgListOfContactAllocations",
						(NoResString)"List of Contact Allocations",
						(NoResString)"This is a list of external organizations that need a Contact point from within an Enterprise organization.",
						RegistryStorageFlags.System,
						new DataRegistry().OrgListOfContactAllocations);
			((CodeDescriptionPairListRegistryDataType)item.DataType).AllowEmptyCodes = false;
			((CodeDescriptionPairListRegistryDataType)item.DataType).AllowDuplicateCodes = false;
			((CodeDescriptionPairListRegistryDataType)item.DataType).IsEmptyListAllowed = false;
			((CodeDescriptionPairListRegistryDataType)item.DataType).KeepDefaultValues = true;

			var dataType = new CodeDescriptionPairListRegistryDataType(3, false);
			dataType.KeepDefaultValues = true;
			var changedList = new CodeDescriptionPairList();
			dataType.Validate(item, changedList, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#region Implementation

		protected override CodeDescriptionPairListRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionPairListRegistryDataType(3);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CodeDescriptionPairList list1 = new CodeDescriptionPairList();
			CodeDescriptionPairList list2 = new CodeDescriptionPairList();
			list2.AddPair("x", "y");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(list1, list1.ToXMLByteArray()),
				new ValidSampleAndBinaryValueInDB(list2, list2.ToXMLByteArray())
			};
		}

		protected override object[] GetInvalidSamples()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("toobig");
			return new object[] { list };
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			ReadOnlyCodeDescriptionPairList lhsList = lhs as ReadOnlyCodeDescriptionPairList;
			ReadOnlyCodeDescriptionPairList rhsList = rhs as ReadOnlyCodeDescriptionPairList;

			if (lhs != null || rhs != null)
			{
				Assert(message, lhsList != null && rhsList != null);
				AssertEquals(message, lhsList.Count, rhsList.Count);
				for (int i = 0; i < lhsList.Count; i++)
				{
					AssertEquals(message, lhsList[i].Code, rhsList[i].Code);
					AssertEquals(message, lhsList[i].Description, rhsList[i].Description);
				}
			}
		}

		#endregion
	}
}
