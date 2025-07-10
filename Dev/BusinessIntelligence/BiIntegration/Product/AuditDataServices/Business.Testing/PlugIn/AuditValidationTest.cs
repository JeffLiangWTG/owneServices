namespace Enterprise.AuditDataServices.Business.Testing
{
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.Data;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Business.Testing;

	class AuditValidationTest : TestCaseWithFactory
	{
		public void TestValidateAll()
		{
			var testBizObj = Factory.New<DummyLogged>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);
			var auditFilterValidation = new AuditValidation(auditWrapper);

			auditWrapper.FilterTimeLocalFrom = ZDateTime.Now;
			auditWrapper.FilterTimeLocalTo = auditWrapper.FilterTimeLocalFrom.AddMonths(1);
			auditWrapper.FilterSourceEntity = "~!invalid!@";
			auditFilterValidation.ValidateAll();
			AssertEquals("Has errors?", true, auditWrapper.HasErrors());
			AssertEquals("Has errors?", false, auditWrapper.FilterTimeLocalFromInfo.HasErrors());
			AssertEquals("Has errors?", false, auditWrapper.FilterTimeLocalToInfo.HasErrors());
			AssertEquals("FilterSourceEntity Error", "Please select a valid source.", auditWrapper.FilterSourceEntityInfo.GetErrors().First().Message);

			auditWrapper.FilterSourceEntity = auditWrapper.SourceEntities.First().Code;
			auditFilterValidation.ValidateAll();
			AssertEquals("Has errors?", false, auditWrapper.HasErrors());
		}

		public void TestValidateFilterTimeLocalFrom()
		{
			var testBizObj = Factory.New<DummyLogged>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);
			var auditFilterValidation = new AuditValidation(auditWrapper);

			auditWrapper.FilterTimeLocalFrom = ZDateTime.Invalid;
			auditFilterValidation.ValidateFilterTimeLocalFrom();
			AssertEquals("Has errors?", true, auditWrapper.FilterTimeLocalFromInfo.HasErrors());
			AssertEquals("Error", "Please enter a valid date.", auditWrapper.FilterTimeLocalFromInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalFrom = new ZDateTime(1999, 12, 31, 23, 59, 59);
			auditFilterValidation.ValidateFilterTimeLocalFrom();
			AssertEquals("Has errors?", true, auditWrapper.FilterTimeLocalFromInfo.HasErrors());
			AssertEquals("Error", "Year must be equal or greater than 2000.", auditWrapper.FilterTimeLocalFromInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalFrom = ZDateTime.Now.AddMonths(-1).AddDays(1);
			auditFilterValidation.ValidateFilterTimeLocalFrom();
			AssertEquals("Has errors?", false, auditWrapper.FilterTimeLocalFromInfo.HasErrors());

			auditWrapper.FilterTimeLocalTo = auditWrapper.FilterTimeLocalFrom.AddSeconds(-1);
			auditFilterValidation.ValidateFilterTimeLocalFrom();
			AssertEquals("Has errors?", true, auditWrapper.FilterTimeLocalFromInfo.HasErrors());
			AssertEquals("Error", "Date From must be older than Date To.", auditWrapper.FilterTimeLocalFromInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalTo = ZDateTime.Now;
			auditWrapper.FilterTimeLocalFrom = auditWrapper.FilterTimeLocalTo.AddMonths(-25);
			auditFilterValidation.ValidateFilterTimeLocalFrom();
			AssertEquals("Has errors?", true, auditWrapper.FilterTimeLocalFromInfo.HasErrors());
			AssertEquals("Error", $"Date range must not exceed audit data retention period of {SystemDataRegistry.Instance.AuditRetentionPeriod.Value} months.", auditWrapper.FilterTimeLocalFromInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalFrom = auditWrapper.FilterTimeLocalTo.AddDays(-10);
			auditFilterValidation.ValidateFilterTimeLocalFrom();
			AssertEquals("Has errors?", false, auditWrapper.FilterTimeLocalFromInfo.HasErrors());
		}

		public void TestValidateFilterTimeLocalTo()
		{
			var testBizObj = Factory.New<DummyLogged>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);
			var auditFilterValidation = new AuditValidation(auditWrapper);

			auditWrapper.FilterTimeLocalTo = ZDateTime.Invalid;
			auditFilterValidation.ValidateFilterTimeLocalTo();
			AssertEquals("Has errors?", true, auditWrapper.FilterTimeLocalToInfo.HasErrors());
			AssertEquals("Error", "Please enter a valid date.", auditWrapper.FilterTimeLocalToInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalTo = new ZDateTime(1990, 1, 1);
			auditFilterValidation.ValidateFilterTimeLocalTo();
			AssertEquals("Has errors?", true, auditWrapper.FilterTimeLocalToInfo.HasErrors());
			AssertEquals("Error", "Year must be equal or greater than 2000.", auditWrapper.FilterTimeLocalToInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalTo = ZDateTime.Now.AddDays(-1);
			auditFilterValidation.ValidateFilterTimeLocalTo();
			AssertEquals("Has errors?", false, auditWrapper.FilterTimeLocalToInfo.HasErrors());

			auditWrapper.FilterTimeLocalFrom = auditWrapper.FilterTimeLocalTo.AddSeconds(1);
			auditFilterValidation.ValidateFilterTimeLocalTo();
			AssertEquals("Has errors?", true, auditWrapper.FilterTimeLocalToInfo.HasErrors());
			AssertEquals("Error", "Date From must be older than Date To.", auditWrapper.FilterTimeLocalToInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalFrom = auditWrapper.FilterTimeLocalTo.AddMonths(-25);
			auditFilterValidation.ValidateFilterTimeLocalTo();
			AssertEquals("Has errors?", true, auditWrapper.FilterTimeLocalToInfo.HasErrors());
			AssertEquals("Error", $"Date range must not exceed audit data retention period of {SystemDataRegistry.Instance.AuditRetentionPeriod.Value} months.", auditWrapper.FilterTimeLocalToInfo.GetErrors().First().Message);

			auditWrapper.FilterTimeLocalFrom = auditWrapper.FilterTimeLocalTo.AddDays(-15);
			auditFilterValidation.ValidateFilterTimeLocalTo();
			AssertEquals("Has errors?", false, auditWrapper.FilterTimeLocalToInfo.HasErrors());
		}

		public void TestValidateFilterSourceEntity()
		{
			var testBizObj = Factory.New<DummyLogged>();
			var auditWrapper = new Audit(testBizObj, Db.ServerName);
			var auditFilterValidation = new AuditValidation(auditWrapper);

			auditWrapper.FilterSourceEntity = "";
			auditFilterValidation.ValidateFilterSourceEntity();
			AssertEquals("Has errors?", true, auditWrapper.FilterSourceEntityInfo.HasErrors());
			AssertEquals("FilterSourceEntity Error", "Please enter a value.", auditWrapper.FilterSourceEntityInfo.GetErrors().First().Message);

			auditWrapper.FilterSourceEntity = "~!invalid!@";
			auditFilterValidation.ValidateFilterSourceEntity();
			AssertEquals("Has errors?", true, auditWrapper.FilterSourceEntityInfo.HasErrors());
			AssertEquals("FilterSourceEntity Error", "Please select a valid source.", auditWrapper.FilterSourceEntityInfo.GetErrors().First().Message);

			auditWrapper.FilterSourceEntity = auditWrapper.SourceEntities.First().Code;
			auditFilterValidation.ValidateFilterSourceEntity();
			AssertEquals("Has errors?", false, auditWrapper.FilterSourceEntityInfo.HasErrors());
		}
	}
}
