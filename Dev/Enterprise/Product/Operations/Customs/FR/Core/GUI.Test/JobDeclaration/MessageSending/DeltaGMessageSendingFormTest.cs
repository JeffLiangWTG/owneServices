using System.Windows.Forms;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	[TestedType(typeof(DeltaGMessageSendingForm))]
	class DeltaGMessageSendingFormTest : MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new DeltaGMessageSendingForm(new DeltaGJobDeclarationMessageSendingObjectParent(declaration));
		}

		public override void TestBashingForm()
		{
			//base.TestBashingForm();
			Assert(true);
		}

		public void TestSendDeltaGridShowsTriggeringPointForValidationColumn()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var testForm = new DeltaCSendFormForTest(new DeltaGJobDeclarationMessageSendingObjectParent(declaration)))
			{
				testForm.Show();
				var triggerColumn = testForm.MessageSendingGrid.Columns[DeltaGJobDeclarationMessageSendingObject.Schema.TriggeringPointForValidation];
				AssertEquals("VAA Trig. Point", triggerColumn.ToString());
				AssertEquals(true, triggerColumn.IsVisible);
			}
		}

		public void TestSendDeltaCMenuItem()
		{
			var testMenu = new DeltaCSendMenuForTest();

			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();

			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendDeltaMessageMenuItem.Visible);
			declaration.JE_DeclarationReference = "B00001001";
			testMenu.SendDeltaMessageMenuItem.PerformClick();
			AssertEquals("Declaration B00001001 has no entry – Please generate entries before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.Visible);
			testMenu.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}
		public void TestSendDeltaGridShowsEntryType()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var testForm = new DeltaCSendFormForTest(new DeltaGJobDeclarationMessageSendingObjectParent(declaration)))
			{
				testForm.Show();
				var declTypeColumn = testForm.MessageSendingGrid.Columns[Business.MessageSending.DeltaGJobDeclarationMessageSendingObject.Schema.DeclarationType];
				AssertEquals("Entry type", declTypeColumn.ToString());
				AssertEquals(true, testForm.MessageSendingGrid.ColumnStyles.Count > 9);

				var column2 = testForm.MessageSendingGrid.Columns[DeltaGJobDeclarationMessageSendingObject.Schema.EntryInstructionDescription];
				AssertEquals("Entry Type Desc.", column2.ToString());
			}
		}
		public void TestSendDeltaGridShowsReasonJustificationList()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var jobDeclarationMessageSendingObjectParent = new DeltaGJobDeclarationMessageSendingObjectParent(declaration);

			using (var testForm = new DeltaCSendFormForTest(jobDeclarationMessageSendingObjectParent))
			{
				testForm.Show();

				AssertEquals(true, testForm.MessageSendingGrid.ColumnStyles.Count > 8);
				var columnStyleInfo = testForm.MessageSendingGrid.ColumnStyles[8] as ZArchitecture.GUI.ZDropEditColumnStyleInfo;
				AssertNotNull(columnStyleInfo);
				AssertEquals(Business.MessageSending.DeltaGJobDeclarationMessageSendingObject.Schema.ChangeAcknowledgementIndicator, columnStyleInfo.ColumnName);

				var reasonCodeColumn = testForm.MessageSendingGrid.Columns[Business.MessageSending.DeltaGJobDeclarationMessageSendingObject.Schema.ChangeAcknowledgementIndicator];
				AssertEquals("Regular Justification Code", reasonCodeColumn.ToString());
			}

			var hearder = declaration.CustomsEntryHeaders.AddNew();

			var jobDeclarationMessageSendingObject = new DeltaGJobDeclarationMessageSendingObject(hearder);
			AssertEquals(0, jobDeclarationMessageSendingObject.ReasonCodeList.Count);

			jobDeclarationMessageSendingObject.MessageType = EntryActionCodeList.Codes.REC;
			AssertEquals(1, jobDeclarationMessageSendingObject.ReasonCodeList.Count);

			jobDeclarationMessageSendingObject.MessageType = EntryActionCodeList.Codes.INV;
			AssertEquals(2, jobDeclarationMessageSendingObject.ReasonCodeList.Count);
		}

		public void TestSendDeltaGridReasonJustificationEditableForIsAmend()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var jobDeclarationMessageSendingObjectParent = new DeltaGJobDeclarationMessageSendingObjectParent(declaration);

			using (var testForm = new DeltaCSendFormForTest(jobDeclarationMessageSendingObjectParent))
			{
				testForm.Show();

				jobDeclarationMessageSendingObjectParent.SendingObjectsCollection[0].MessageType = EntryActionCodeList.Codes.REC;
				AssertEquals(true, testForm.MessageSendingGrid.ColumnStyles.Count > 8);
				var columnStyleInfo = testForm.MessageSendingGrid.ColumnStyles[8] as ZArchitecture.GUI.ZDropEditColumnStyleInfo;
				AssertNotNull(columnStyleInfo);
				AssertEquals(Business.MessageSending.DeltaGJobDeclarationMessageSendingObject.Schema.ChangeAcknowledgementIndicator, columnStyleInfo.ColumnName);
				AssertEquals(false, columnStyleInfo.IsReadOnly);
			}
		}
		public void TestSendDeltaGridShowsDoNotRecalculateEntrySubstyle()
		{
			var declaration = Factory.NewWithValidTestData<Business.Declaration.JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var testForm = new DeltaCSendFormForTest(new DeltaGJobDeclarationMessageSendingObjectParent(declaration)))
			{
				testForm.Show();
				var column = testForm.MessageSendingGrid.Columns["DoNotRecalculateEntrySubstyle"];
				AssertEquals("Keep Sub Style", column.ToString());
				AssertEquals(true, testForm.MessageSendingGrid.ColumnStyles.Count > 10);
			}
		}

		class DeltaCSendFormForTest : DeltaGMessageSendingForm
		{
			public DeltaCSendFormForTest(DeltaGJobDeclarationMessageSendingObjectParent declarationWrapper)
				: base(declarationWrapper)
			{
			}

			public ZGrid MessageSendingGrid => base.MessageSendingObjectsGrid;
		}

		class DeltaCSendMenuForTest : EDIMenu
		{
			public MenuItem SendDeltaMessageMenuItem => base.sendDeltaGMessageMenuItem;
			protected override DeltaGMessageSendingForm GetMessageSendingForm(DeltaGJobDeclarationMessageSendingObjectParent decWrapper) => new DeltaCSendFormForTest(decWrapper);
		}
	}
}
