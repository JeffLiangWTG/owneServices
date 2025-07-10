using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(UpdateImportEntryStatusForm))]
	class UpdateImportEntryStatusFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_RiskChannel = "2";

			return new UpdateImportEntryStatusForm(new UpdateImportEntryStatusObject(declaration));
		}

		public void TestFormComponents()
		{
			using (var form = GetFormToBashCore() as UpdateImportEntryStatusForm)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Update Entry Status", form.CaptionResourceString.Caption);
					AssertType<ZLabel>(form.InstructionLabel);
					AssertEquals("Enter the Entry Status, Risk Channel or Release Date and then click Update", form.InstructionLabel.CaptionResourceString.FullDescription);
					AssertType<ZDropEdit>(form.EntryStatusDropEdit);
					AssertType<ZDateEdit>(form.EventDateEdit);
					AssertType<ZDropEdit>(form.RiskChannelDropEdit);
					AssertType<ZButton>(form.UpdateButton);
					AssertType<ZButton>(form.CloseButton);
				});
			}
		}

		public void TestOnOkButton_Click()
		{
			ReferenceTestDataHelper.CreateEntryStatusForISWList(Factory);
			var date = ZDateTime.Now;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "S01";
			entryHeader.CH_EntryReleaseDate = date.AddDays(-1);
			entryHeader.CH_RiskChannel = "2";

			using (var form = new UpdateImportEntryStatusFormForTest(new UpdateImportEntryStatusObject(declaration)))
			{
				var source = form.DataSource as UpdateImportEntryStatusObject;
				var declaratiom = source.Declaration;
				using (declaratiom.SuspendSettingHasChanges())
				{
					source.EntryStatus = "S02";
					source.EventDate = date;
					source.RiskChannel = "3";
				}
				form.Show();

				AssertEquals("CH_EntryStatus should be", "S01", entryHeader.CH_EntryStatus);
				AssertEquals("CH_EntryReleaseDate should be", date.AddDays(-1), entryHeader.CH_EntryReleaseDate);
				AssertEquals("CH_RiskChannel should be", "2", entryHeader.CH_RiskChannel);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.PerformClickOkButton();

				AssertEquals("CH_EntryStatus should be", source.EntryStatus, entryHeader.CH_EntryStatus);
				AssertEquals("CH_EntryReleaseDate should be", source.EventDate, entryHeader.CH_EntryReleaseDate);
				AssertEquals("CH_RiskChannel should be", source.RiskChannel, entryHeader.CH_RiskChannel);
			}
		}

		class UpdateImportEntryStatusFormForTest : UpdateImportEntryStatusForm
		{
			public UpdateImportEntryStatusFormForTest(UpdateImportEntryStatusObject declaration)
				: base(declaration)
			{
			}

			public void PerformClickOkButton() => UpdateButton.PerformClick();
		}
	}
}
