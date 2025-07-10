using System;
using System.Reflection;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class GuidConstantsTest : TestCase
	{
		public void TestAllGuidsActuallyExist()
		{
			foreach (FieldInfo field in typeof(CountryGuids).GetFields())
			{
				Guid guid = (Guid)field.GetValue(CountryGuids.Instance);
				int result = (int)Db.Connection.ExecuteScalar(
					"select count(*) from dbo.REFCountry where RN_PK = @pk",
					cmd => cmd.AddParameterBasedOnDbColumn("@pk", guid, RefCountrySchema.PK));
				AssertEquals("Guid constant for '" + field.Name + "' is incorrect in the resources solution.", 1, result);
			}
		}
	}
}
