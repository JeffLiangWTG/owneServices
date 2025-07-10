using System.Threading;
using System.Windows.Forms;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	[TestedType(typeof(MultistepDataImportWizardForm))]
	sealed class MultistepDataImportWizardFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var processor = new DummyDataTransferProcessor();
			return new MultistepDataImportWizardForm(Helper.CollectionInfo, "", new DataTransferProcessor[] { processor });
		}

		ImportWizardTestHelper Helper
		{
			get { return helper ?? (helper = new ImportWizardTestHelper(Factory)); }
		}

		ImportWizardTestHelper helper;

		public class DummyDataTransferProcessor : DataTransferProcessor
		{
			public bool ImportCalled { get; set; }
			public bool RollbackCalled { get; set; }

			public override void Import()
			{
				Thread.Sleep(10);
				ImportCalled = true;
			}

			public override void Rollback()
			{
				Thread.Sleep(10);
				RollbackCalled = true;
			}
		}
	}
}
