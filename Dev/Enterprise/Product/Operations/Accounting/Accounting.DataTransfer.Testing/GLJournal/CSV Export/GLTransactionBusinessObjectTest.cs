using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[TestedType(typeof(GLTransactionBusinessObject))]
	public class GLTransactionBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			AssertEquals("Default value of DescriptionDisplay", "HDR", BizObj.DescriptionDisplay);
			AssertEquals("Default value of SortByPeriod", true, BizObj.SortByPeriod);
		}

		public void TestValidatePeriodAndDateRanges()
		{
			AssertEquals("FromPeriod should have error", true, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should have error", true, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should have error", true, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should have error", true, BizObj.ToDateInfo.HasErrors());

			BizObj.CreateAndExportBatch = true;
			AssertEquals("FromPeriod should not have error", false, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should not have error", false, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should not have error", false, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should not have error", false, BizObj.ToDateInfo.HasErrors());

			BizObj.CreateAndExportBatch = false;
			AssertEquals("FromPeriod should have error", true, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should have error", true, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should have error", true, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should have error", true, BizObj.ToDateInfo.HasErrors());

			BizObj.ExportExistingBatch = true;
			AssertEquals("FromPeriod should not have error", false, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should not have error", false, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should not have error", false, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should not have error", false, BizObj.ToDateInfo.HasErrors());

			BizObj.ExportExistingBatch = false;
			BizObj.FromPeriod = 200603;
			BizObj.RunPreSaveValidation();
			AssertEquals("FromPeriod should have error", true, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should have error", true, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should have error", true, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should have error", true, BizObj.ToDateInfo.HasErrors());

			BizObj.FromPeriod = 0;
			BizObj.ToPeriod = 200609;
			BizObj.RunPreSaveValidation();
			AssertEquals("FromPeriod should have error", true, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should have error", true, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should have error", true, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should have error", true, BizObj.ToDateInfo.HasErrors());

			BizObj.FromPeriod = 200603;
			BizObj.RunPreSaveValidation();
			AssertEquals("FromPeriod should not have error", false, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should not have error", false, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should not have error", false, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should not have error", false, BizObj.ToDateInfo.HasErrors());

			BizObj.FromPeriod = 0;
			BizObj.ToPeriod = 0;
			BizObj.FromDate = new ZDateTime(2006, 03, 27);
			BizObj.RunPreSaveValidation();
			AssertEquals("FromPeriod should not have error", false, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should not have error", false, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should not have error", false, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should not have error", false, BizObj.ToDateInfo.HasErrors());

			BizObj.FromDate = ZDateTime.Empty;
			BizObj.ToDate = new ZDateTime(2006, 03, 27);
			BizObj.RunPreSaveValidation();
			AssertEquals("FromPeriod should not have error", false, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should not have error", false, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should not have error", false, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should not have error", false, BizObj.ToDateInfo.HasErrors());

			BizObj.FromDate = new ZDateTime(2006, 03, 28);
			BizObj.ToDate = new ZDateTime(2006, 03, 27);
			BizObj.RunPreSaveValidation();
			AssertEquals("FromPeriod should not have error", false, BizObj.FromPeriodInfo.HasErrors());
			AssertEquals("ToPeriod should not have error", false, BizObj.ToPeriodInfo.HasErrors());
			AssertEquals("FromDate should not have error", true, BizObj.FromDateInfo.HasErrors());
			AssertEquals("ToDate should not have error", true, BizObj.ToDateInfo.HasErrors());

			BizObj.FromDate = ZDateTime.Invalid;
			BizObj.ToDate = ZDateTime.Empty;
			BizObj.RunPreSaveValidation();
			AssertHasError(BizObj.FromDateInfo, "Enter a valid From Date");

			BizObj.FromDate = ZDateTime.Empty;
			BizObj.ToDate = ZDateTime.Invalid;
			BizObj.RunPreSaveValidation();
			AssertHasError(BizObj.ToDateInfo, "Enter a valid To Date");
		}

		public void TestValidateStartGLAccountPK()
		{
			AssertEquals("StartGLAccountPK should not have error", true, BizObj.StartGLAccountPKInfo.HasErrors());

			BizObj.StartGLAccountPK = ZGuid.Invalid;
			BizObj.RunPreSaveValidation();
			AssertEquals("StartGLAccountPK should have error", true, BizObj.StartGLAccountPKInfo.HasErrors());

			BizObj.StartGLAccountPK = ZGuid.NewZGuid();
			BizObj.RunPreSaveValidation();
			AssertEquals("StartGLAccountPK should have error", false, BizObj.StartGLAccountPKInfo.HasErrors());
		}

		public void TestValidateEndGLAccountPK()
		{
			AssertEquals("EndGLAccountPK should not have error", true, BizObj.EndGLAccountPKInfo.HasErrors());

			BizObj.EndGLAccountPK = ZGuid.Invalid;
			BizObj.RunPreSaveValidation();
			AssertEquals("EndGLAccountPK should have error", true, BizObj.EndGLAccountPKInfo.HasErrors());

			BizObj.EndGLAccountPK = ZGuid.NewZGuid();
			BizObj.RunPreSaveValidation();
			AssertEquals("EndGLAccountPK should have error", false, BizObj.EndGLAccountPKInfo.HasErrors());
		}

		public void TestValidateBranchPK()
		{
			AssertEquals("BranchPK should not have error", false, BizObj.BranchPKInfo.HasErrors());

			BizObj.BranchPK = ZGuid.Invalid;
			AssertEquals("BranchPK should have error", true, BizObj.BranchPKInfo.HasErrors());

			BizObj.BranchPK = ZGuid.NewZGuid();
			AssertEquals("BranchPK should not have error", false, BizObj.BranchPKInfo.HasErrors());
		}

		public void TestValidateDepartmentPK()
		{
			AssertEquals("DepartmentPK should not have error", false, BizObj.DepartmentPKInfo.HasErrors());

			BizObj.DepartmentPK = ZGuid.Invalid;
			AssertEquals("DepartmentPK should have error", true, BizObj.DepartmentPKInfo.HasErrors());

			BizObj.DepartmentPK = ZGuid.NewZGuid();
			AssertEquals("DepartmentPK should not have error", false, BizObj.DepartmentPKInfo.HasErrors());
		}

		public void TestValidateDescriptionDisplay()
		{
			AssertEquals("DescriptionDisplay should not have error", false, BizObj.DescriptionDisplayInfo.HasErrors());

			BizObj.DescriptionDisplay = "";
			BizObj.RunPreSaveValidation();
			AssertEquals("DescriptionDisplay should have error", true, BizObj.DescriptionDisplayInfo.HasErrors());

			BizObj.DescriptionDisplay = "LDR";
			BizObj.RunPreSaveValidation();
			AssertEquals("DescriptionDisplay should not have error", false, BizObj.DescriptionDisplayInfo.HasErrors());
		}

		public void TestValidateSortByPeriod()
		{
			AssertEquals("SortByPeriod should not have error", false, BizObj.SortByPeriodInfo.HasErrors());
		}

		public void TestValidateSortBySource()
		{
			AssertEquals("SortBySource should not have error", false, BizObj.SortBySourceInfo.HasErrors());
		}

		public void TestValidateExportDirectory()
		{
			AssertEquals("ExportDirectory should not have error", true, BizObj.ExportDirectoryInfo.HasErrors());

			BizObj.ExportDirectory = "blah";
			BizObj.RunPreSaveValidation();
			AssertEquals("ExportDirectory should have error", true, BizObj.ExportDirectoryInfo.HasErrors());

			BizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			BizObj.RunPreSaveValidation();
			AssertEquals("ExportDirectory should not have error", false, BizObj.ExportDirectoryInfo.HasErrors());
		}

		public void TestTruncateLog()
		{
			BizObj.AddToLog(BizObj.Log.PadRight(BizObj.LogInfo.MaxLength - 1, 'A'));
			AssertEquals("Log length", BizObj.LogInfo.MaxLength - 1, BizObj.Log.Length);

			BizObj.AddToLog("B");
			AssertEquals("Log length", BizObj.LogInfo.MaxLength - 1000, BizObj.Log.Length);
			AssertEquals("Should contain the last part after truncating", 'B', BizObj.Log[BizObj.Log.Length - 1]);
		}

		public void TestGLAccountRangeReadOnly()
		{
			BizObj.ExportExistingBatch = true;
			AssertEquals(true, BizObj.StartGLAccountPKInfo.ReadOnly);
			AssertEquals(true, BizObj.EndGLAccountPKInfo.ReadOnly);

			BizObj.ExportExistingBatch = false;
			AssertEquals(false, BizObj.StartGLAccountPKInfo.ReadOnly);
			AssertEquals(false, BizObj.EndGLAccountPKInfo.ReadOnly);
			BizObj.StartGLAccountPK = ZGuid.NewZGuid();
			BizObj.EndGLAccountPK = ZGuid.NewZGuid();

			BizObj.CreateAndExportBatch = true;
			AssertEquals(true, BizObj.StartGLAccountPKInfo.ReadOnly);
			AssertEquals(true, BizObj.EndGLAccountPKInfo.ReadOnly);
			Assert(BizObj.StartGLAccountPK.IsEmpty);
			Assert(BizObj.EndGLAccountPK.IsEmpty);
		}

		public void TestBatchNumberReadOnly()
		{
			BizObj.ExportExistingBatch = true;
			AssertEquals(false, BizObj.BatchNumberInfo.ReadOnly);

			BizObj.ExportExistingBatch = false;
			AssertEquals(true, BizObj.BatchNumberInfo.ReadOnly);

			BizObj.CreateAndExportBatch = true;
			AssertEquals(true, BizObj.BatchNumberInfo.ReadOnly);
		}

		public void TestValidateBatchNumber()
		{
			BizObj.BatchNumber = 0;
			AssertNoErrors(BizObj.BatchNumberInfo);

			BizObj.BatchNumber = 100;
			AssertNoErrors(BizObj.BatchNumberInfo);

			BizObj.BatchNumber = -1;
			AssertHasErrors("Value can't be negative.", BizObj.BatchNumberInfo);

			BizObj.BatchNumber = 100;
			BizObj.ExportExistingBatch = true;
			AssertHasErrors("Not existed batch number.", BizObj.BatchNumberInfo);

			GenExportBatchSequence exportBatchSequence = Factory.NewWithValidTestData<GenExportBatchSequence>();
			exportBatchSequence.XB_Type = Core.Constants.DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredPosting;
			exportBatchSequence.XB_BatchNumber = 100;
			Factory.Save();
			BizObj.RunPreSaveValidation();
			AssertHasErrors("Not existed batch number.", BizObj.BatchNumberInfo);
			AssertEquals(100, BizObj.BatchNumber);

			exportBatchSequence.XB_Type = Core.Constants.DataExportBatchSubTypes.Codes.GeneralLedgerPost;
			Factory.Save();
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.BatchNumberInfo);
			AssertEquals(100, BizObj.BatchNumber);
		}

		public void TestCreateAndExportBatchAndExportExistingBatchChecking()
		{
			BizObj.CreateAndExportBatch = false;
			BizObj.ExportExistingBatch = false;

			BizObj.ExportExistingBatch = true;
			AssertEquals(false, BizObj.CreateAndExportBatch);
			AssertEquals(true, BizObj.ExportExistingBatch);

			BizObj.CreateAndExportBatch = true;
			AssertEquals("Only one check box should checked.", true, BizObj.CreateAndExportBatch);
			AssertEquals("Only one check box should checked.", false, BizObj.ExportExistingBatch);

			BizObj.CreateAndExportBatch = false;
			AssertEquals(false, BizObj.CreateAndExportBatch);
			AssertEquals(false, BizObj.ExportExistingBatch);

			BizObj.CreateAndExportBatch = true;
			BizObj.ExportExistingBatch = true;
			AssertEquals("Only one check box should checked.", false, BizObj.CreateAndExportBatch);
			AssertEquals("Only one check box should checked.", true, BizObj.ExportExistingBatch);

			BizObj.ExportExistingBatch = false;
			AssertEquals(false, BizObj.CreateAndExportBatch);
			AssertEquals(false, BizObj.ExportExistingBatch);
		}

		public void TestGenerateBatchNumberOnSave()
		{
			BizObj.ExportExistingBatch = true;
			AssertEquals("Precondition: ", 0, BizObj.BatchNumber);
			AssertEquals("Precondition: ", false, BizObj.CreateAndExportBatch);

			Factory.Save();
			AssertEquals("BatchNumber should not be generated.", 0, BizObj.BatchNumber);

			BizObj.CreateAndExportBatch = true;
			try
			{
				Factory.Save();
			}
			catch (ZCannotSaveException)
			{ }
			AssertEquals("Batch Number should be empty when there are no transactions to export", 0, BizObj.BatchNumber);
		}

		public void TestHasRestrictiveFilters()
		{
			Assert("No restrictive filters set", !BizObj.HasRestrictiveFilters);

			SetPropertyAndAssertValue("FromPeriod", new ZInt(201201));
			SetPropertyAndAssertValue("ToPeriod", new ZInt(201202));
			SetPropertyAndAssertValue("FromDate", ZDateTime.Now);
			SetPropertyAndAssertValue("ToDate", ZDateTime.Now);
			SetPropertyAndAssertValue("StartGLAccountPK", Factory.NewWithValidTestData<AccGLHeader>().PK);
			SetPropertyAndAssertValue("EndGLAccountPK", Factory.NewWithValidTestData<AccGLHeader>().PK);
			SetPropertyAndAssertValue("BranchPK", GlbBranch.CurrentBranch.PK);
			SetPropertyAndAssertValue("DepartmentPK", GlbDepartment.CurrentDepartment.PK);

			Assert("No restrictive filters set", !BizObj.HasRestrictiveFilters);
		}

		void SetPropertyAndAssertValue(string propertyName, object value)
		{
			var property = (BizObj.GetType().GetProperty(propertyName));
			var originalValue = property.GetValue(BizObj, null);
			property.SetValue(BizObj, value, null);
			Assert(string.Format("HasRestrictiveFilters should be true because the following restrictive filter was set: {0}", property.Name), BizObj.HasRestrictiveFilters);
			property.SetValue(BizObj, originalValue, null);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GLTransactionBusinessObject(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BizObj = new GLTransactionBusinessObject(Factory);
			BizObj.RunPreSaveValidation();
		}

		GLTransactionBusinessObject BizObj;

		#endregion
	}

	public class GLTransactionBusinessObjectTestWithoutTransaction : TestCase
	{
		#region Implementation

		BusinessObjectFactory Factory
		{
			get { return Factory_innterValue ?? (Factory_innterValue = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory Factory_innterValue;

		GLTransactionBusinessObject BizObj
		{
			get { return BizObj_innerValue ?? (BizObj_innerValue = new GLTransactionBusinessObject(Factory)); }
		}

		GLTransactionBusinessObject BizObj_innerValue;

		#endregion

		public void TestGenerateBatchNumberOnSave()
		{
			BizObj.CreateAndExportBatch = true;
			AssertEquals("Precondition: ", 0, BizObj.BatchNumber);
			AssertNull("Precondition: ", BizObj.Exporter);
			bool isSavingFailed = false;
			try
			{
				Factory.Save();
			}
			catch (ZCannotSaveException)
			{
				isSavingFailed = true;
			}
			Assert(isSavingFailed);
			AssertEquals("Batch Number should be empty when save fails.", 0, BizObj.BatchNumber);

			isSavingFailed = false;
			try
			{
				Factory.Save();
			}
			catch (ZCannotSaveException)
			{
				isSavingFailed = true;
			}
			Assert(isSavingFailed);
			AssertEquals("Batch Number should be empty when save fails.", 0, BizObj.BatchNumber);
		}
	}
}
