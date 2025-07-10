using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class EntryInstructionAdditionalInformationUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new EntryInstructionAdditionalInformationUserControl())
			{
				AssertEquals("DataSourceType", typeof(CusEntryInstruction), control.DataSourceType);
			}
		}

		public void TestCaption()
		{
			using (var control = new EntryInstructionAdditionalInformationUserControl())
			{
				AssertEquals("AdditionalTariffGroupBox caption should be", "Additional Information", control.AdditionalInformationGroupBox.CaptionResourceString.Caption);
				AssertEquals("FreeTextGroupBox caption should be", "Free Text", control.FreeTextGroupBox.CaptionResourceString.Caption);
				AssertEquals("SystemGeneratedGroupBox caption should be", "System Generated", control.SystemGeneratedGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestComponents()
		{
			using (var control = new EntryInstructionAdditionalInformationUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<ZGroupBox>("AdditionalInformationGroupBox type should be", control.AdditionalInformationGroupBox);
					AssertType<ZGroupBox>("FreeTextGroupBox type should be", control.FreeTextGroupBox);
					AssertType<ZTextBox>("FreeTextTextBox type should be", control.FreeTextTextBox);
					AssertType<ZGroupBox>("SystemGeneratedGroupBox type should be", control.SystemGeneratedGroupBox);
					AssertType<ZTextBox>("SystemGeneratedTextBox type should be", control.SystemGeneratedTextBox);
					AssertType<KSplitter>("Splitter type should be", control.Splitter);
					AssertType<ZDropEdit>("AdditionalInformationDropBox type should be", control.AdditionalInformationDropEdit);
				});
			}
		}
	}
}
