using Enterprise.DataTransfer.GUI;
using Enterprise.DataTransfer.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.GUI.Import
{
	[TestedType(typeof(USDataImportForm))]
	class USOrganisationDataImportFormTest : DataImporterFormTest
	{
		public void TestImportFileFilter()
		{
			using (var form = new USDataImportFormForTest())
			{
				AssertEquals("CSV Files (*.csv)|*.csv", form.ImportFileFilter);
			}
		}

		class USDataImportFormForTest : USDataImportForm
		{
			public USDataImportFormForTest() : base("BLAH")
			{
			}

			public new string ImportFileFilter
			{
				get
				{
					return base.ImportFileFilter;
				}
			}
		}

		protected override DataImporterForm NewDataImporterForm() => new USDataImportFormForTest();
		protected override string ExpectedFormCaption => "BLAH";
	}
}
