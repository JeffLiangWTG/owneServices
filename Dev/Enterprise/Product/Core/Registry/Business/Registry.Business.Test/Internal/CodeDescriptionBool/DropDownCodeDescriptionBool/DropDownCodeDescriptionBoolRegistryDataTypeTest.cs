using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DropDownCodeDescriptionBoolRegistryDataType))]
	sealed class DropDownCodeDescriptionBoolRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DropDownCodeDescriptionBoolRegistryDataType>
	{
		protected override DropDownCodeDescriptionBoolRegistryDataType GetNewDataType()
		{
			return new DropDownCodeDescriptionBoolRegistryDataType(16, null, null);
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		// Using custom EditorInfo.
		protected override bool HasEditor
		{
			get { return false; }
		}

		public void TestManagerSecurityMappingValidate_ManagerSecurityTypeFormat()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "ABC", (NoResString)"ABD", true, false, true },
				{ "DEF", (NoResString)"DEF", true, false, true }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var dataType = new DropDownCodeDescriptionBoolRegistryDataType(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			var list = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			list.Add("MANAGERSECURITY1", (NoResString)"ABC");
			list.Add("MANAGERSECURITY4", (NoResString)"DEF");

			AssertExceptionThrown<RegistryValidationException>("Manager security type wrong format", "Manager Security type 'MANAGERSECURITY4' is wrong.\r\nManager Security type must be in the format 'MANAGERSECURITYX' where X is 1, 2, or 3.", () => dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestManagerMappingValidate_DescriptionNotInStaffRole()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "ABC", (NoResString)"ABD", true, false, true },
				{ "DEF", (NoResString)"DEF", true, false, true }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var dataType = new DropDownCodeDescriptionBoolRegistryDataType(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			var list = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			list.Add("MANAGERSECURITY1", (NoResString)"ABC");
			list.Add("MANAGERSECURITY2", (NoResString)"XXX");

			AssertExceptionThrown<RegistryValidationException>("No Staff Role", "Description 'XXX' is wrong.\r\nDescription must be a 3 letter code that matches a value in Staff Reporting Roles.", () => dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestManagerMappingValidate_DescriptionLength()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "ABC", (NoResString)"ABD", true, false, true },
				{ "DEF", (NoResString)"DEF", true, false, true }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var dataType = new DropDownCodeDescriptionBoolRegistryDataType(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			var list = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			list.Add("MANAGERSECURITY1", (NoResString)"ABC");
			list.Add("MANAGERSECURITY2", (NoResString)"XXXXXX");

			AssertExceptionThrown<RegistryValidationException>("No Staff Role", "Description 'XXXXXX' is wrong.\r\nDescription must be a 3 letter code that matches a value in Staff Reporting Roles.", () => dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestManagerMappingValidate_NoMoreThanThreeValues()
		{
			var dataType = new DropDownCodeDescriptionBoolRegistryDataType(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);

			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "ABC", (NoResString)"ABC", true, false, true },
				{ "DEF", (NoResString)"DEF", true, false, true },
				{ "GHI", (NoResString)"GHI", true, false, true },
				{ "JKL", (NoResString)"JKL", true, false, true }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var list = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			list.Add("MANAGERSECURITY1", (NoResString)"ABC");
			list.Add("MANAGERSECURITY2", (NoResString)"DEF");
			list.Add("MANAGERSECURITY3", (NoResString)"GHI");
			list.Add("MANAGERSECURITY4", (NoResString)"JKL");

			AssertExceptionThrown<RegistryValidationException>("No More Than 3 Values", "There are 4 values. The number of values cannot exceed 3.", () => dataType.Validate(null, list, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "ABC", (NoResString)"ABD", true, false, true },
				{ "DEF", (NoResString)"DEF", true, false, true }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var list1 = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			var list2 = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			list2.Add("MANAGERSECURITY1", (NoResString)"ABC");
			byte[] list1_byteArray = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,
				105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,114,0,111,0,112,0,
				68,0,111,0,119,0,110,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,32,0,120,0,
				109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,
				103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,
				109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,
				0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,47,0,62,0,
			};

			byte[] list2_byteArray = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,
				103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,114,0,111,0,112,0,68,0,111,0,119,0,110,
				0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,
				0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,
				77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,
				61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,
				99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,68,0,114,0,111,0,112,0,68,0,111,0,119,0,110,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,
				105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,49,0,54,0,60,0,47,0,67,0,111,
				0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,77,0,65,0,78,0,65,0,71,0,69,0,82,0,83,0,69,0,67,0,85,0,
				82,0,73,0,84,0,89,0,49,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,65,0,66,0,67,0,60,0,47
				,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,89,0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,0,83,0,
				121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,101,0,100,0,62,0,70,0,97,0,108,0,115,0,101,0,60,0,47,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,
				0,102,0,105,0,110,0,101,0,100,0,62,0,60,0,47,0,68,0,114,0,111,0,112,0,68,0,111,0,119,0,110,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,
				0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,114,0,111,0,112,0,68,0,111,0,119,0,110,0,67,0,111,0,100,0,
				101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(list1, list1_byteArray),
				new ValidSampleAndBinaryValueInDB(list2, list2_byteArray)
			};
		}

		protected override object[] GetInvalidSamples()
		{
			var list = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			list.Add("MANAGERSECURITY1", (NoResString)"ABC");
			return new object[] { list };
		}
	}
}
