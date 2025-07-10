using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	public class HelpersTest : TestCaseWithFactory
	{
		public void TestGetValueReturnCorrectResult()
		{
			var actual = Helpers.GetValue(GetColumn(), "FRT", GlbCompany.CurrentCompany.PK, Factory);
			var expect = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccChargeCode WHERE AC_CODE = 'FRT' AND AC_GC = '" + GlbCompany.CurrentCompany.PK + "'");
			AssertEquals(actual, expect.Rows[0]["AC_PK"]);
		}

		public void TestGetValueThrowCorrectMessageWhenHaveZeroRecordInTheDB()
		{
			var expectedMessage = "Charge Code 'XXX' cannot be found in receiving system. (Unable to determine primary key from code: E6_AC_ChargeCode, XXX)";
			AssertExceptionThrown(
				typeof(MatchingCriteriaException),
				expectedMessage,
				() => Helpers.GetValue(GetColumn(), "XXX", GlbCompany.CurrentCompany.PK, Factory));
		}

		public void TestGetValueThrowCorrectMessageWhenHaveMoreThanOneRecordsInTheDB()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "FRT";
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			var expectedMessage = "More than one records in the database for Charge Code. (Unable to determine primary key from code: E6_AC_ChargeCode, FRT)";
			AssertExceptionThrown(
				typeof(MatchingCriteriaException),
				expectedMessage,
				() => Helpers.GetValue(GetColumn(), "FRT", GlbCompany.CurrentCompany.PK, Factory));
		}

		SchemaGuidColumn GetColumn()
		{
			var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema("JobConsolCost");
			return new SchemaGuidColumn(schema, "E6_AC_ChargeCode", 0, Guid.Empty, false);
		}
	}
}
