using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.TemporaryStorage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;
[TestedType(typeof(UpdateCSVClearanceForm))]
class UpdateCSVClearanceFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var codeInfo = CsvCodeInfo;
		Factory.Save();
		return new UpdateCSVClearanceForm(codeInfo);
	}

	public void TestInitializeUpdateCSVClearanceLayout()
	{
		using (var updateCSVForm = new UpdateCSVClearanceForm(CsvCodeInfo))
		{
			var messageLabel = (ZLabel)(updateCSVForm.Controls.Find("MessageLabel", true).Single());
			var csvClearanceTextBox = (ZTextBox)(updateCSVForm.Controls.Find("CSVClearance", true).Single());
			var clearanceDateDateEdit = (ZDateEdit)(updateCSVForm.Controls.Find("ClearanceDate", true).Single());
			var expectedMessageLabelText = @"You are about to change the clearance number.
Please make sure that the number that you are entering is the right clearance number.";

			updateCSVForm.Show();

			CombineAssertions(() =>
			{
				AssertEquals("Message Label has the correct Text", expectedMessageLabelText, messageLabel.Text);
				AssertEquals("CSV Clearance is visible cause is a EXS Declaration", true, csvClearanceTextBox.Visible);
				AssertEquals("Clearance Date is visible cause is a EXS Declaration", true, clearanceDateDateEdit.Visible);
			});
		}
	}

	CsvCodeInfo CsvCodeInfo
	{
		get
		{
			if (csvCodeInfo == null)
			{
				csvCodeInfo = CsvCodeInfo.LoadNew(Header);
			}
			return csvCodeInfo;
		}
	}
	CsvCodeInfo csvCodeInfo;

	TemporaryStorageHeader Header
	{
		get
		{
			if (header == null)
			{
				var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
				temporaryStorageHeader.ClearanceNumber = "12345678901234567890";
				temporaryStorageHeader.ClearanceDate = ZDate.BrettsBirthday;
				header = temporaryStorageHeader;
			}
			return header;
		}
	}
	TemporaryStorageHeader header;
}
