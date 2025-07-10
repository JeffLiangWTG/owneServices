using Enterprise.Billing.Integration;
using Enterprise.Client.WCB.DaimlerChrysler;
using Enterprise.Client.WCB.DaimlerChrysler.GUI;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.DataTransfer.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.GUI.Testing
{
	[TestedType(typeof(DCDataImporterForm))]
	public class DCDataImporterFormTest : DataImporterFormTest
	{
		public void TestImportFileFilter()
		{
			string daimlerPrefix = WCBDataRegistry.Instance.MercedesImportFileNamePrefix;
			string chryslerPrefix = WCBDataRegistry.Instance.ChryslerImportFileNamePrefix;
			using (MockDCDataImporterForm form = new MockDCDataImporterForm())
			{
				form.Importer = new DCFlatFileDataImporter(true, null);
				string expectedFilter = "(" + daimlerPrefix + "*.txt)|" + daimlerPrefix + "*.txt";
				AssertEquals("ImportFileFilter", expectedFilter, form.ImportFileFilter);
				form.Importer = new DCFlatFileDataImporter(false, null);
				expectedFilter = "(" + chryslerPrefix + "*.txt)|" + chryslerPrefix + "*.txt";
				AssertEquals("ImportFileFilter", expectedFilter, form.ImportFileFilter);
			}
		}

		#region Implementation
		protected override DataImporterForm NewDataImporterForm()
		{
			return new DCDataImporterForm(new DataImporterBusinessObject(Factory), null);
		}

		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return DCDataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport);
		}

		class MockDCDataImporterForm : DCDataImporterForm
		{
			public new string ImportFileFilter
			{
				get
				{
					return base.ImportFileFilter;
				}
			}
		}
		#endregion
	}
}
