using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.ELG.Testing
{
	public class SagBatchExportDirectorTest : SagAccountsExportDirectorTest
	{
		protected override void AssertNotificationsWhenSuccess()
		{
			//base.AssertNotificationsWhenSuccess();
			Assert("Exported batch number", NotifyBuffer.AsString.Contains(" Exported batch number: 1"));
			//Assert("Batch 1 was exported successfully.", NotifyBuffer.AsString.Contains("Batch 1 was exported successfully."));
			Assert("Email sent", Env.OutgoingMailManager.EmailsCreated.Count >= 1);
		}

		protected override void AssertNotificationsWhenFailure()
		{
			base.AssertNotificationsWhenFailure();
			Assert("Batch wasn't created.", NotifyBuffer.AsString.Contains("Batch wasn't created."));
			Assert("Email sent", Env.OutgoingMailManager.EmailsCreated.Count >= 1);
		}

		public void TestInvalidExecuteWithException()
		{
			TestHelper.SetValidRegistryDataTransfer(ZDateTime.Now.AddMinutes(-30));
			TestHelper.SetInvalidRegistryBranchDepartmentCodeCollectionItem();
			TestHelper.SetValidRegistryTransportAndChargeCodeCollectionItem();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem();
			TestHelper.NewPMG();
			OrgHeader debtor = TestHelper.AddOrg();
			debtor.OH_FullName = "CargoWise";
			Factory.Save();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, "AUD", "AR", debtor.OH_Code);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, "AUD", "AP", debtor.OH_Code);
			debtor = TestHelper.AddOrg();
			debtor.OH_FullName = "ABC International";
			Factory.Save();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, "AUD", "AR", debtor.OH_Code);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(debtor.OH_Code, "AUD", "AP", debtor.OH_Code);
			SetupNewInvoices();
			TempDirectory.DeleteDirectory(ExportDirectory);
			Factory.Save();
			Assert("Empty folder", DirInfo.GetFiles().Length == 0);
			ExecuteExportDirector();
			//Assert("Batch wasn't created.", NotifyBuffer.AsString.Contains(string.Format("\r\nPlease fix the above errors and then manually export batch {0} at Accounts -> Receivables -> Receivables Transactions -> Actions -> Data Transfer -> Export Transactions in Sage Format\r\n", Director.Exporter.FilterProvider.CurrentBatchNo)));
			TempDirectory.DeleteDirectory(ExportDirectory);
		}

		protected override void SetupEnvironment()
		{
			base.SetupEnvironment();
			TestHelper.SetRegistry(tempPath);
		}

		protected override void ExecuteExportDirector()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				Director.Execute();
			}
		}

		protected override void AsserFileCreated()
		{
			Assert("file should not exist", TempDirInfo.GetFiles().Length == 0);
		}

		SagBatchExportDirector Director
		{
			get
			{
				return director ?? (director = new SagBatchExportDirector(Factory, NotifyBuffer));
			}
		}

		SagBatchExportDirector director;
	}
}
