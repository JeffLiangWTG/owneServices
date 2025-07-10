using CargoWise.Types;
using Enterprise.ClientSharedComponents.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.ELG.Testing
{
	public abstract class SagAccountsExportDirectorTest : AccountsExportDirectorARAPTest
	{
		protected override void SetupEnvironment()
		{
			TestHelper.SetValidRegistryAll();
			TestHelper.NewPMG();
			OrgHeader org = TestHelper.AddOrg();
			org.OH_FullName = "CargoWise";
			Factory.Save();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(org.OH_Code, "AUD", "AR", "FRED");
			org = TestHelper.AddOrg();
			org.OH_FullName = "ABC International";
			Factory.Save();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(org.OH_Code, "AUD", "AR", "FRED");
		}

		protected override void SetupNewInvoices()
		{
			TestHelper.NewInvoices("AR");
			//TestHelper.NewInvoices("AP");
		}

		protected override void SetInvalidDirectory()
		{
			TestHelper.SetRegistry("INVALID:\\");
		}

		protected override ZString ExportDirectory
		{
			get
			{
				return ELGDataRegistry.Instance.SagExportDirectory;
			}
		}

		protected ELGTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new ELGTestHelper(Factory));
			}
		}

		ELGTestHelper testHelper;
	}
}
