using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.DataConverters.Accounting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Accounting
{
	[TestedType(typeof(AccountingImportController))]
	sealed internal class AccountingImportControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ImportAccountingData;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = Factory.New<APOverpayment>();
			result.AH_InvoiceDate = Env.Time.CurrentLocalDate;
			result.AH_DueDate = Env.Time.CurrentLocalDate;
			result.AH_PostDate = Env.Time.CurrentLocalDate;
			result.AH_FullyPaidDate = Env.Time.CurrentLocalDate;
			result.AH_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			result.AH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery()).PK;
			Factory.Save();
			return result;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}
	}
}
