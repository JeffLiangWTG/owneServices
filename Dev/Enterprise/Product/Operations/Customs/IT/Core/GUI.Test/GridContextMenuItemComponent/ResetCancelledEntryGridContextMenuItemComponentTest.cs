using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ResetCancelledEntryGridContextMenuItemComponent))]
sealed class ResetCancelledEntryGridContextMenuItemComponentTest : GridContextMenuItemComponentAbstractTest<CusEntryHeader>
{
	public void TestResetCancelledEntryMenuItemEnabled_UCC6()
	{
		entryHeader.CH_Status = "ACS";
		entryHeader.CH_EntryStatus = "CNC";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			entriesBoundGrid.Select(0);
			var menu = entriesBoundGrid.ContextMenu;
			menu.DoPopup();
			var menuItem = menu.MenuItems.FindByText("Reset Canceled Entry", true);
			AssertEquals("ResetCancelledEntry Menu item must be visible", true, menuItem.Enabled);
		}
	}

	public void TestResetCancelledEntryMenuItemEnabled_NonUCC6()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			entriesBoundGrid.Select(0);
			var menu = entriesBoundGrid.ContextMenu;
			menu.DoPopup();
			var menuItem = menu.MenuItems.FindByText("Reset Canceled Entry", true);
			AssertEquals("ResetCancelledEntry Menu item must not be visible", false, menuItem.Enabled);
		}
	}

	public void TestResetCancelledEntryGridContextMenuClick()
	{
		entryHeader.MovementReferenceNumberSetter("123");
		entryHeader.CH_Status = "ACS";
		entryHeader.CH_EntryStatus = "CNC";
		entryHeader.CusEntryNumber.CE_EntryLineReference = "789";
		entryHeader.CH_EntrySubmittedDate = ZDateTime.UtcToday;
		entryHeader.CH_EntryReleaseDate = ZDateTime.BrettsBirthday;

		var payInfo1 = entryHeader.EntryPayInfos.AddNew();
		payInfo1.C9_PaymentAmount = 20.0m;
		payInfo1.C9_PaymentDate = new ZDateTime(2016, 03, 17);
		payInfo1.C9_TransactionType = "XYZ";
		payInfo1.C9_PaymentParty = "D";
		var payInfo2 = entryHeader.EntryPayInfos.AddNew();
		payInfo2.C9_PaymentAmount = 1000m;
		payInfo2.C9_PaymentDate = new ZDateTime(2016, 03, 17);
		payInfo2.C9_TransactionType = "ABC";
		payInfo2.C9_PaymentParty = "F";

		var entryNumber1 = NewCusEntryHeader(CusEntryNumberConstants.EntryTypes.RegistrationNumber, "456", ZDate.Today, ZString.Empty);
		var entryNumber2 = NewCusEntryHeader(CusEntryNumberConstants.EntryTypes.ClereanceCode, "135", ZDate.Today, ZString.Empty);
		var entryNumber3 = NewCusEntryHeader(CusEntryNumberConstants.EntryTypes.Ivisto, "456", ZDate.Today, ZString.Empty);
		entryNumber3.CE_EntryLineReference = "IT275100";
		entryNumber3.CE_EntryStatus = "EXC";

		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			entriesBoundGrid.Select(0);

			var menu = entriesBoundGrid.ContextMenu;
			var menuItem = menu.MenuItems.FindByText("Reset Canceled Entry", true);

			CombineAssertions("When No is Clicked", () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();

				AssertEquals("Are you sure you want to Reset this Canceled Entry?\r\nAll registration data will also be deleted", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Message Status should not be empty", "ACS", entryHeader.CH_Status);
				AssertEquals("Entry Status should not be empty", "CNC", entryHeader.CH_EntryStatus);
				AssertEquals("CE_EntryLineReference should not be empty", "789", entryHeader.CusEntryNumber.CE_EntryLineReference);
				AssertEquals("CH_EntrySubmittedDate should not be empty", ZDateTime.UtcToday, entryHeader.CH_EntrySubmittedDate);
				AssertEquals("MRN should not be empty", "123", entryHeader.MovementReferenceNumber);
				AssertEquals("ReleaseDate should not be empty", ZDateTime.BrettsBirthday, entryHeader.CH_EntryReleaseDate);
				AssertEquals("EntryPayInfos collection should not be empty", 2, entryHeader.EntryPayInfos.Count);
				AssertEquals("RegistrationNo. should not be empty", "456", entryHeader.EntryNumbersProvider.RegistrationInfo.CE_EntryNum);
				AssertEquals("Release code should not be empty", "135", entryHeader.EntryNumbersProvider.ReleaseInfo.CE_EntryNum);
				AssertEquals("Exit date should not be empty", ZDate.Today, entryHeader.EntryNumbersProvider.Ivisto.CE_IssueDate);
				AssertEquals("Exit office should not be empty", "IT275100", entryHeader.EntryNumbersProvider.Ivisto.CE_EntryLineReference);
				AssertEquals("Exit status should not be empty", "EXC", entryHeader.EntryNumbersProvider.Ivisto.CE_EntryStatus);
			});

			CombineAssertions("When Yes is Clicked", () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();

				AssertEquals("After clicking Yes, feedback message is shown to user", "Entry has been reset", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty("Message Status should be empty", entryHeader.CH_Status);
				AssertNullOrEmpty("Entry Status should be empty", entryHeader.CH_EntryStatus);
				AssertNull("CE_EntryLineReference should be empty", entryHeader.CusEntryNumber);
				AssertEquals("CH_EntrySubmittedDate should be empty", ZDateTime.Empty, entryHeader.CH_EntrySubmittedDate);
				AssertNullOrEmpty("MRN should be empty", entryHeader.MovementReferenceNumber);
				AssertEquals("ReleaseDate should be empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
				AssertEquals("EntryPayInfos collection should be empty", 0, entryHeader.EntryPayInfos.Count);
				AssertNull("RegistrationNo. should be empty", entryHeader.EntryNumbersProvider.RegistrationInfo);
				AssertNull("Release code should be empty", entryHeader.EntryNumbersProvider.ReleaseInfo);
				AssertNull("Exit date should be empty", entryHeader.EntryNumbersProvider.Ivisto);
			});
		}
	}

	CusEntryNumber NewCusEntryHeader(ZString entryType, ZString entryNum, ZDateTime? issueDate, string entryLineReference = null)
	{
		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = entryType;
		cusEntryNumber.CE_ParentID = entryHeader.PK;
		cusEntryNumber.CE_ParentTable = entryHeader.TableName;
		cusEntryNumber.CE_Category = "CUS";
		cusEntryNumber.CE_EntryNum = entryNum;
		cusEntryNumber.CE_EntryLineReference = entryLineReference;
		if (issueDate.HasValue)
		{
			cusEntryNumber.CE_IssueDate = issueDate.Value;
		}
		return cusEntryNumber;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	protected override GridContextMenuItemComponent<CusEntryHeader> GetNewContextMenuItemComponent(ZGrid grid)
	{
		return new ResetCancelledEntryGridContextMenuItemComponent(grid);
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	protected override string MenuItemText => "Reset Canceled Entry";
}
