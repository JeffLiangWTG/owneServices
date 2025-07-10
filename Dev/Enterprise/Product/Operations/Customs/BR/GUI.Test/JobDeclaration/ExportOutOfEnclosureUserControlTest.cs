using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ExportOutOfEnclosureUserControl))]
	class ExportOutOfEnclosureUserControlTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			using (var control = new ExportOutOfEnclosureUserControl())
			{
				AssertType<ZGroupBox>("ClearanceLocalGroupBox type should be", control.ClearanceLocalGroupBox);
				AssertType<ZDocAddressControl>("InvolvedPartyDocAddress type should be", control.InvolvedPartyDocAddress);
				AssertType<ZCheckBox>("IsHomeDispatchCheckBox type should be", control.IsHomeDispatchCheckBox);
				AssertType<ZGroupBox>("BoardingLocalGroupBox type should be", control.BoardingLocalGroupBox);
				AssertType<ZDocAddressControl>("BoardingLocalDocAddress type should be", control.BoardingLocalDocAddress);
			}
		}

		public void TestCaptions()
		{
			using (var control = new ExportOutOfEnclosureUserControl())
			{
				AssertEquals("ClearanceLocalGroupBox caption should be", "Clearance Local", control.ClearanceLocalGroupBox.CaptionResourceString.Caption);
				AssertEquals("BoardingLocalGroupBox caption should be", "Boarding Local", control.BoardingLocalGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestCheckEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.ClearanceOfficeIsCustomsEnclosure = true;
			declaration.BoardingOfficeIsCustomsEnclosure = true;

			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = new CustomsDeclarationUserControl())
				{
					control.JobDeclaration = declaration;
					form.Controls.Add(control);
					form.Show();

					control.CustomsOfficesTabControl.SelectedTab = control.OutOfEnclosureTabPage;

					var exportOutOfEnclosureUserControl = (ExportOutOfEnclosureUserControl)control.Controls.Find("ExportOutOfEnclosureUserControl", true).First();

					Assert("ClearanceLocalGroupBox should NOT be Enable", !exportOutOfEnclosureUserControl.ClearanceLocalGroupBox.Enabled);
					Assert("BoardingLocalGroupBox should NOT be Enable", !exportOutOfEnclosureUserControl.BoardingLocalGroupBox.Enabled);

					declaration.ClearanceOfficeIsCustomsEnclosure = false;
					Assert("ClearanceLocalGroupBox should be Enable", exportOutOfEnclosureUserControl.ClearanceLocalGroupBox.Enabled);

					declaration.BoardingOfficeIsCustomsEnclosure = false;
					Assert("BoardingLocalGroupBox should be Enable", exportOutOfEnclosureUserControl.BoardingLocalGroupBox.Enabled);
				}
			}
		}
	}
}
