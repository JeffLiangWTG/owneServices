using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class SQLInjectionDetectorTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestEscapeClauseAccepted()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("select * from bb where x like @Param1 escape '~' or true");
		}

		[ExpectNoExceptions]
		public void TestCheckForSQLInjectionValid1()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("select * from TABLENAME");
		}

		[ExpectNoExceptions]
		public void TestCheckForSQLInjectionValid2()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("select * from TABLENAME where Data = '6E404EA9-1EE1-41e5-A310-5E3AE7E887C3'");
		}

		[ExpectNoExceptions]
		public void TestCheckForSQLInjectionValid3()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("SELECT  * FROM dbo.DUMMYBIZO WHERE Z0_PK = '97d275c9-733c-4832-a866-9dac6077b815' ");
		}

		[ExpectNoExceptions]
		public void TestCheckForSQLInjectionValid4()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("SELECT  * FROM dbo.DUMMYDEPENDENTBIZO WHERE  (Z0_PK = 'a8cc1462-8a64-4db5-993c-3b2f78a28e3f')");
		}

		[ExpectNoExceptions]
		public void TestCheckForSQLInjectionValid5()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("SELECT * FROM dbo.DUMMYBIZO WHERE Z0_PK in (CONVERT('27a55065-ac88-4ec3-8bed-e575e79172cb', 'System.Guid'))");
		}

		[ExpectNoExceptions]
		public void TestCheckForSQLInjectionValidEmptyString()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("SELECT  * FROM TABLENAME WHERE Data <> ''");
		}

		[ExpectException(typeof(SQLInjectionException))]
		public void TestCheckForSQLInjectionInvalid()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("select * from TABLENAME where Data = 'value'");
		}

		[ExpectException(typeof(SQLInjectionException))]
		public void TestCheckForSQLInjectionInvalidMultipleLiterals()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("select * from TABLENAME where Data = '' or Data = 'value'");
		}

		[ExpectNoExceptions]
		public void TestCheckForSQLInjectionStringLiteral()
		{
			SQLInjectionDetector detector = new SQLInjectionDetector();
			detector.CheckForSQLInjection("select * from TABLENAME where Data = /* StringLiteral */ 'value' or Data = /*StringLiteral*/'foo' or Data = /* StringLiteral */ 'd''oh'");
		}
	}
}
