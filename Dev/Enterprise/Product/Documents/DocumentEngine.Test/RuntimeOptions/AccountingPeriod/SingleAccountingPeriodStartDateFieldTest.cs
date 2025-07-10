using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SingleAccountingPeriodStartDateField))]
	sealed class SingleAccountingPeriodStartDateFieldTest : SingleAccountingPeriodFieldTest
	{
		SingleAccountingPeriodField apf;

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			new AccountingPeriodTestHelper().PostPeriodsForEntireYear(2003);
			apf = (SingleAccountingPeriodStartDateField)GetNewBusinessObject();
			apf.FieldName = "f";
			apf.SinglePeriod = 200301;
		}

		public override void TestSinglePeriod()
		{
			AssertEquals("Start date", new DateTime(2002, 7, 1), apf.ValueAsObject);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SingleAccountingPeriodStartDateField(Factory);
		}
	}
}
