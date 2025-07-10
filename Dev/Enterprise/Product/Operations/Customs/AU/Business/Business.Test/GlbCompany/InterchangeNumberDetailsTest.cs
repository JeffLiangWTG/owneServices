using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InterchangeNumberDetails))]
	sealed class InterchangeNumberDetailsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_CustomsRegistrationNo = "TestCustomsRegistrationNo";

			var interchangeNumber = new InterchangeNumberDetails(company);
			CombineAssertions(() =>
			{
				AssertEquals("CompanyEDISite", "TestCustomsRegistrationNo", interchangeNumber.CompanyEDISite);
				AssertEquals("CustomsEDISite", "AAA336C", interchangeNumber.CustomsEDISite);
				AssertEquals("CurrentInterchangeNumber", 0L, interchangeNumber.CurrentInterchangeNumber);
			});
		}

		public void TestTypes()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var interchangeNumber = new InterchangeNumberDetails(company);

			AssertType<InterchangeNumberDetailsValidation>(interchangeNumber.Validation);
		}

		public void TestCurrentInterchangeNumber()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_CustomsRegistrationNo = "Test001";
			InsertTestRecord("ITest001AAA336C", 1234);

			var interchangeNumberDetails = new InterchangeNumberDetails(company);
			AssertEquals("CurrentInterchangeNumber", 1234L, interchangeNumberDetails.CurrentInterchangeNumber);
		}

		public void TestSetNewInterchangeNumberAutomatically()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_CustomsRegistrationNo = "Test001";
			InsertTestRecord("ITest001AAA336C", 1234);

			var interchangeNumberDetails = new InterchangeNumberDetails(company);
			interchangeNumberDetails.SetNewInterchangeNumberAutomatically();
			AssertEquals(1234 + InterchangeNumberDetails.NewInterchangeNumberIncrement, interchangeNumberDetails.NewInterchangeNumber);
		}

		public void TestSaveNewInterchangeNumber()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_CustomsRegistrationNo = "Test001";
			InsertTestRecord("ITest001AAA336C", 123456L);

			var interchangeNumberDetails = new InterchangeNumberDetails(company);
			interchangeNumberDetails.NewInterchangeNumber = 123457L;
			Assert(!interchangeNumberDetails.SaveNewInterchangeNumber());
			var snValue = GetSNValueFromDatabase("ITest001AAA336C");
			AssertEquals("123457 Should not saved into db, because of interchangeNumber has errors", 123456L, snValue);

			interchangeNumberDetails.NewInterchangeNumber = 323456L;
			Assert(interchangeNumberDetails.SaveNewInterchangeNumber());
			snValue = GetSNValueFromDatabase("ITest001AAA336C");
			AssertEquals(323456L, snValue);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			return new InterchangeNumberDetails(company);
		}

		public static void InsertTestRecord(string snName, long snValue)
		{
			Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.StmNums (SN_Name, SN_Value, SN_SystemCreateTimeUtc) VALUES ('{snName}', {snValue}, GETUTCDATE());");
		}

		public static long GetSNValueFromDatabase(string snName)
		{
			return Db.Connection.ExecuteScalar<long>($"select top 1 [SN_Value] from dbo.StmNums where [SN_Name] = '{snName}'");
		}
	}
}
