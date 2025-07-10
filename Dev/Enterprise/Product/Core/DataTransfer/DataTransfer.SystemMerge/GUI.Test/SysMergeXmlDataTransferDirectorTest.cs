using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.GUI.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.SystemMerge.GUI.Testing
{
	sealed class SysMergeXmlDataTransferDirectorTest : XmlDataTransferDirectorTest
	{
		#region Imlementation

		protected override XmlDataTransferDirector createInstanceForLicenceCheck()
		{
			return new SysMergeXmlDataTransferDirectorForTest();
		}

		#endregion

		#region Test Class

		class SysMergeXmlDataTransferDirectorForTest : SysMergeXmlDataTransferDirector
		{
			public SysMergeXmlDataTransferDirectorForTest() : base(StmALogValueObjectDataAdapter.New(null, "Test")) { }

			protected override void ImportCore(string fileName, CargoWise.ComponentModel.INotifications notify, Billing.Integration.ISourceInfo info)
			{
				Globals.Message.Show("LICENCED");
			}

			protected override void PromptUserAndImportCore(Billing.Integration.BillingInterfaceName interfaceName)
			{
				Globals.Message.Show("LICENCED");
			}
		}

		#endregion
	}
}
