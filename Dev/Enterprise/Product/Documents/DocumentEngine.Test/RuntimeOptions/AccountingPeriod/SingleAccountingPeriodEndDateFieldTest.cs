using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SingleAccountingPeriodEndDateField))]
	sealed class SingleAccountingPeriodEndDateFieldTest : SingleAccountingPeriodFieldTest
	{
		SingleAccountingPeriodField apf;

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			new AccountingPeriodTestHelper().PostPeriodsForEntireYear(2003);
			apf = (SingleAccountingPeriodEndDateField)GetNewBusinessObject();
			apf.FieldName = "f";
			apf.SinglePeriod = 200301;
		}

		public override void TestSinglePeriod()
		{
			AssertEquals("End date", new DateTime(2002, 7, 31, 23, 59, 0), apf.ValueAsObject);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SingleAccountingPeriodEndDateField(Factory);
		}
	}
}
