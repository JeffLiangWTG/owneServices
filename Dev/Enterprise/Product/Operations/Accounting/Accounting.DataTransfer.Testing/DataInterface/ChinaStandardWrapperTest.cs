using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.Testing
{
	[TestedType(typeof(ChinaStandardWrapper))]
	public class ChinaStandardWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBranches()
		{
			BizObj.Branches.Load();

			var collection = new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK));
			collection.Load();

			Assert("Precondition Current Company has at least one Branch", collection.Count > 0);

			var result = collection.All(branch => BizObj.Branches.Contains(branch));

			Assert("Test Branches is OK", result);
		}

		public void TestChartType()
		{
			AssertEquals("Empty value for invalid Branch", false, BizObj.OLdChartType);
		}

		public void TestValidateBranchCode()
		{
			BizObj.Branches.Load();

			BizObj.Branch = ZGuid.NewZGuid();
			AssertEquals("Empty value for invalid Branch", ZString.Empty, BizObj.BranchCode);

			BizObj.Branch = BizObj.Branches[0].PK;
			AssertEquals("Should have value", BizObj.Branches[0].GB_Code, BizObj.BranchCode);
		}

		public void TestValidateBranch()
		{
			var com = Factory.NewWithValidTestData<GlbCompany>();
			var inValidBranch = Factory.NewWithValidTestData<GlbBranch>();
			inValidBranch.GB_GC = com.PK;

			BizObj.Branches.Load();

			BizObj.Branch = ZGuid.NewZGuid();
			AssertHasErrors(BizObj.BranchInfo);

			BizObj.Branch = inValidBranch.PK;
			AssertHasErrors(BizObj.BranchInfo);

			BizObj.Branch = BizObj.Branches[0].PK;
			AssertNoErrors(BizObj.BranchInfo);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("Default value of ExportDirectory", ZString.Empty, BizObj.ExportDirectory);
			AssertEquals("Default value of Period", 0, BizObj.Period);
			AssertEquals("Default value of BranchCode", ZString.Empty, BizObj.BranchCode);
		}

		public void TestValidateExportDirectory()
		{
			AssertHasErrors("Please enter a valid directory.", BizObj.ExportDirectoryInfo);
			AssertEquals("Default value of ExportDirectory", ZString.Empty, BizObj.ExportDirectory);

			BizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			BizObj.RunPreSaveValidation();
			AssertEquals("ExportDirectory should not have error", false, BizObj.ExportDirectoryInfo.HasErrors());

			BizObj.ExportDirectory = "blah";
			BizObj.RunPreSaveValidation();
			AssertHasErrors("Please enter a valid directory.", BizObj.ExportDirectoryInfo);
		}

		public void TestTruncateLog()
		{
			BizObj.AddToLog(BizObj.Log.PadRight(BizObj.LogInfo.MaxLength - 1, 'A'));
			AssertEquals("Log length", BizObj.LogInfo.MaxLength - 1, BizObj.Log.Length);

			BizObj.AddToLog("B");
			AssertEquals("Log length", BizObj.LogInfo.MaxLength - 1000, BizObj.Log.Length);
			AssertEquals("Should contain the last part after truncating", 'B', BizObj.Log[BizObj.Log.Length - 1]);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChinaStandardWrapper(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BizObj = new ChinaStandardWrapper(Factory);
			BizObj.RunPreSaveValidation();
		}

		protected ChinaStandardWrapper BizObj;

		#endregion
	}
}
