using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.ExtensionMethods
{
	public class BusinessObjectFactoryExtensionsTest : TestCaseWithFactory
	{
		public void TestLoadScalarValue()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var code = objectCreator.CC1;
			var code1 = objectCreator.CC2;

			var count = Factory.LoadScalarValue<ZInt>("SELECT COUNT(AC_PK) as CNT FROM dbo.AccChargeCode WHERE AC_Code = @chargeCode", ZSqlParameter.New("@chargeCode", code.AC_Code, AccChargeCodeSchema.AC_Code));
			AssertEquals("With Parameter", 1, count);

			count = Factory.LoadScalarValue<ZInt>("SELECT COUNT(*) as CNT FROM dbo.GlbCompany");
			Assert("Without Parameter", count > 0);

			count = Factory.LoadScalarValue<ZInt>("SELECT COUNT(*) as CNT FROM dbo.AccChargeCode WHERE AC_Code = @chargeCode AND AC_GC = @companyPK",
				ZSqlParameter.New("@chargeCode", code.AC_Code, AccChargeCodeSchema.AC_Code),
				ZSqlParameter.New("@companyPK", Guid.NewGuid(), AccChargeCodeSchema.AC_GC));
			AssertEquals("No Result", 0, count);

			AssertExceptionThrown<InvalidOperationException>(
			"Error Should Occur, as number rows is more than 1",
			"Incompatible result table returned by the query. So a scalar value cannot be returned.\r\nTable details:\r\nColumns: PK.\r\nNumber of rows: 2.",
			() =>
			{
				count = Factory.LoadScalarValue<ZInt>("SELECT AC_PK as PK FROM dbo.AccChargeCode WHERE AC_Code IN (@chargeCode, @chargeCode1)",
									ZSqlParameter.New("@chargeCode", code.AC_Code, AccChargeCodeSchema.AC_Code),
									ZSqlParameter.New("@chargeCode1", code1.AC_Code, AccChargeCodeSchema.AC_Code));
			});

			AssertExceptionThrown<InvalidOperationException>(
			"Error Should Occur, as number of columns is more than 1",
			"Incompatible result table returned by the query. So a scalar value cannot be returned.\r\nTable details:\r\nColumns: PK, CodeLength.\r\nNumber of rows: 1.",
			() =>
			{
				count = Factory.LoadScalarValue<ZInt>("SELECT AC_PK as PK, LEN(AC_Code) as CodeLength FROM dbo.AccChargeCode WHERE AC_Code = @chargeCode",
									ZSqlParameter.New("@chargeCode", code.AC_Code, AccChargeCodeSchema.AC_Code));
			});
		}
	}
}
