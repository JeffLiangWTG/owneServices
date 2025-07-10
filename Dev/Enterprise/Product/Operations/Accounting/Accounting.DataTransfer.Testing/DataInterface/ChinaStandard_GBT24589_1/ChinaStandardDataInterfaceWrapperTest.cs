using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.DataInterface.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	[TestedType(typeof(ChinaStandard2010DataInterfaceWrapper))]
	public class ChinaStandardDataInterfaceWrapperTest : ChinaStandardWrapperTest
	{
		public void TestSelectBranchShouldShowWarning()
		{
			var branch = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK)).FirstOrDefault();
			AssertNotNull(branch);
			BizObj.ExportGeneralLedger = true;
			BizObj.Branch = branch.PK;
			AssertHasWarning(BizObj.BranchInfo, "You cannot select Branch Filter if you are printing Job Costing Voucher.");
			BizObj.Branch = ZGuid.Empty;
			AssertNoWarnings(BizObj.BranchInfo);
		}

		public void TestPropertiesZboolAndExportFiles()
		{
			AssertEquals("ExportFiles count should be 0", 0, BizObj.ExportFiles.Count);
			BizObj.ExportFixedAssets = true;
			AssertEquals("ExportFiles count should be 1", 1, BizObj.ExportFiles.Count);
			BizObj.ExportARAP = true;
			BizObj.ExportGeneralLedger = true;
			BizObj.ExportReferenceFiles = true;
			BizObj.ExportPayrolls = true;
			AssertEquals("ExportFiles count should be 5", 5, BizObj.ExportFiles.Count);
			BizObj.ExportFixedAssets = false;
			BizObj.ExportARAP = false;
			AssertEquals("ExportFiles count should be 3", 3, BizObj.ExportFiles.Count);
			BizObj.ExportGeneralLedger = false;
			BizObj.ExportReferenceFiles = false;
			BizObj.ExportPayrolls = false;
			AssertEquals("ExportFiles count should be 0", 0, BizObj.ExportFiles.Count);
		}

		public void TestPeriodDateReadOnly()
		{
			Assert("PeriodDateReadOnly should be false", !BizObj.PeriodReadOnly);
			BizObj.ExportReferenceFiles = true;
			Assert("PeriodDateReadOnly should be true", BizObj.PeriodReadOnly);
			BizObj.ExportFixedAssets = true;
			Assert("PeriodDateReadOnly should be false", !BizObj.PeriodReadOnly);
			BizObj.ExportFixedAssets = false;
			Assert("PeriodDateReadOnly should be true", BizObj.PeriodReadOnly);
			BizObj.ExportReferenceFiles = false;
			Assert("PeriodDateReadOnly should be false", !BizObj.PeriodReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChinaStandard2010DataInterfaceWrapper(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BizObj = new ChinaStandard2010DataInterfaceWrapper(Factory);
			BizObj.RunPreSaveValidation();
		}

		new ChinaStandard2010DataInterfaceWrapper BizObj;

		#endregion
	}
}
