#if DEBUG

using System;
using System.Data;
using System.Globalization;
using System.Linq;
using static NUnit.Framework.Assertion;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public class RegistryTransformationTestHelper : RegistryTransformationHelper
	{
		public static void AssertAllStmDataResults(DataRow row, string sdName, object sdOwner, object sdDepartmentGuid, string sdType, bool sdIsLogged, string sdBinaryValue, object sdGuidValue, bool sdIsCancelled, bool sdPreserveTestValue)
		{
			AssertEquals(sdName, row["SD_Name"]);
			AssertEquals(sdOwner, row["SD_Owner"]);
			AssertEquals(sdDepartmentGuid, row["SD_DepartmentGuid"]);
			AssertEquals(sdType, row["SD_Type"]);
			AssertEquals(sdIsLogged, row["SD_IsLogged"]);
			var sdBinaryValueString = row["SD_BinaryValue"] is var binaryValue && Convert.IsDBNull(binaryValue) ? DBNull.Value.ToString(CultureInfo.InvariantCulture) : "0x" + string.Join(string.Empty, ((byte[])binaryValue).Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
			AssertEquals(sdBinaryValue, sdBinaryValueString);
			AssertEquals(sdGuidValue, row["SD_GuidValue"]);
			AssertEquals(sdIsCancelled, row["SD_IsCancelled"]);
			AssertEquals(sdPreserveTestValue, row["SD_PreserveTestValue"]);
		}
	}
}

#endif
