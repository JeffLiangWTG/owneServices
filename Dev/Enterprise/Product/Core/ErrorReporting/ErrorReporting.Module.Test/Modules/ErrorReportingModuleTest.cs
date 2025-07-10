using System;
using Enterprise.Environment;
using Enterprise.ErrorReporting.Business;
using NUnit.Framework;

namespace Enterprise.ErrorReporting.Module.Test
{
	public class ErrorReportingModuleTest : TestCase
	{
		public void TestOnlyAllowsView()
		{
			AssertEquals(false, module.AllowNew);
			AssertEquals(false, module.AllowEdit);
			AssertEquals(false, module.AllowDelete);
			AssertEquals(true, module.AllowView);
		}

		public void TestSecurityCheckPoint()
		{
			AssertEquals(Env.Security.None, module.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
		}

		public void TestGridCollectionIsStmErrorReportCollection()
		{
			AssertType<StmErrorReportCollection>(module.GridCollection);
		}

		public void TestFilterBizOIsErrorReportFilter()
		{
			AssertType<ErrorReportFilterBusinessObject>(module.FilterBusinessObject);
		}

		public void TestFilterControlIsErrorReportFilter()
		{
			var control = module.GetNewFilterControlForGrid();
			using (control as IDisposable)
			{
				AssertType<ErrorReportFilterControl>(control);
			}
		}

		ErrorReportingModule module;

		protected override void SetUp()
		{
			base.SetUp();

			module = new ErrorReportingModule();
		}

		protected override void TearDown()
		{
			module.Dispose();
			module = null;

			base.TearDown();
		}
	}
}
