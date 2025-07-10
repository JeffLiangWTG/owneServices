using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ImportSiscomexOfficesUserControl))]
	class ImportSiscomexOfficesUserControlTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			using (var control = new ImportSiscomexOfficesUserControl())
			{
				AssertType<ZGroupBox>("ClearanceGroupBox type should be", control.ClearanceGroupBox);
				AssertType<ZDropEdit>("ClearanceOfficeDropEdit type should be", control.ClearanceOfficeDropEdit);
				AssertType<ZDropEdit>("ClearanceEnclosureDropEdit type should be", control.ClearanceEnclosureDropEdit);
				AssertType<ZDropEdit>("SectorCustomOfficeDropEdit type should be", control.SectorCustomOfficeDropEdit);
				AssertType<ZGroupBox>("EntranceLocationGroupBox type should be", control.EntranceLocationGroupBox);
				AssertType<ZDropEdit>("EntranceOfficeDropEdit type should be", control.EntranceOfficeDropEdit);
				AssertType<ZButton>("WarehouseAreaIDButton type should be", control.WarehouseAreaIDButton);
				AssertType<ZTextBox>("WarehouseAreaIDConcatenatedTextBox type should be", control.WarehouseAreaIDConcatenatedTextBox);
			}
		}

		public void TestCaptions()
		{
			using (var control = new ImportSiscomexOfficesUserControl())
			{
				AssertEquals("ClearanceGroupBox caption should be", "Clearance Local", control.ClearanceGroupBox.CaptionResourceString.Caption);
				AssertEquals("WarehouseAreaIDButton caption should be", "More..", control.WarehouseAreaIDButton.CaptionResourceString.Caption);
				AssertEquals("EntranceLocationGroupBox caption should be", "Entrance Location", control.EntranceLocationGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestWarehouseAreaIDButton()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.ClearanceOfficeIsCustomsEnclosure = true;

			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = new CustomsDeclarationUserControl())
				{
					control.JobDeclaration = declaration;
					form.Controls.Add(control);
					form.Show();

					var importSiscomexOfficesUserControl = (ImportSiscomexOfficesUserControl)control.Controls.Find("ImportSiscomexOfficesUserControl", true).First();
					importSiscomexOfficesUserControl.WarehouseAreaIDButton.PerformClick();
					AssertType<WarehouseAreaIDsForm>("WarehouseAreaIDsForm popped up", ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}
	}
}
