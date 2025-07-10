using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class HeaderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControls_HeaderDetailsGroupBox()
		{
			using (var control = new HeaderDetailsUserControl())
			{
				var headerDetailsGroupBox = control.HeaderDetailsGroupBox;
				var dynamicHeaderDetailsPanel = control.DynamicHeaderDetailsPanel;
				AssertEquals("DynamicHeaderDetailsPanel is within GroupBox", true, headerDetailsGroupBox.Controls.Contains(dynamicHeaderDetailsPanel));

				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel, nameof(CommonHeaderDetailsControlBag.EntryTypeDropEdit),
					nameof(CommonHeaderDetailsControlBag.EntryStatusTextBox),
					nameof(HeaderDetailsControlBag.UnlinkedDeclarationsNumberLabel),
					nameof(CommonHeaderDetailsControlBag.PeriodFromDateEdit),
					nameof(CommonHeaderDetailsControlBag.MessageStatusTextBox),
					nameof(CommonHeaderDetailsControlBag.PeriodToDateEdit),
					nameof(HeaderDetailsControlBag.RegistrationNumberTextBox),
					nameof(CommonHeaderDetailsControlBag.DeclarationTypeDropEdit),
					nameof(HeaderDetailsControlBag.BranchGuidFindBox),
					nameof(CommonHeaderDetailsControlBag.DeclarantTypeDropEdit),
					nameof(HeaderDetailsControlBag.IsFinalizedCheckBox),
					nameof(CommonHeaderDetailsControlBag.DeclarantAddressControl),
					nameof(HeaderDetailsControlBag.IsDeclarantImporterCheckBox),
					nameof(CommonHeaderDetailsControlBag.RepresentativeAddressControl),
					nameof(CommonHeaderDetailsControlBag.AuthorizationNumberGuidDropEdit),
					nameof(CommonHeaderDetailsControlBag.BuyingAgentAddressControl),
					nameof(CommonHeaderDetailsControlBag.CustomsOfficeCodeFindBox));
			}
		}

		public void TestBindingSourceDataSourceType()
		{
			using (var control = new HeaderDetailsUserControl())
			{
				AssertEquals(typeof(CusReconDeclaration), control.BindingSource.DataSourceType);
			}
		}

		public void TestControls()
		{
			using (var control = new HeaderDetailsUserControl())
			{
				var headerDetailsPanel = control.HeaderDetailsPanel;

				CombineAssertions(() =>
				{
						AssertEquals("HeaderDetailsPanel is within HeaderDetailsUserControl", true, control.Controls.Contains(headerDetailsPanel));
						AssertNoExceptionThrown("splitter1", () => control.FindSingle<KSplitter>("splitter1"));
						AssertNoExceptionThrown("bottomPanel", () => control.FindSingle<KPanel>("bottomPanel"));
				});
			}
		}
	}
}
