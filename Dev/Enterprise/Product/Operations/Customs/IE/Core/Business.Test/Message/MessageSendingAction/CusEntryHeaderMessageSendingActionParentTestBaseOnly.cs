using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Environment;

namespace Enterprise.Customs.IE.Business.Testing
{
	public class CusEntryHeaderMessageSendingActionParentTestBaseOnly : TestCaseWithFactory
	{
		public void TestTopLevelBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testItem = new CusEntryHeaderMessageSendingActionParentForTesting(declaration);
			AssertSame("TopLevelBusinessObject should return JobDeclaration.", declaration, testItem.TopLevelBusinessObject);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			var testItem = new CusEntryHeaderMessageSendingActionParentForTesting(Factory.New<JobDeclaration>());
			AssertSame("SecurityCheckpointToSendWithMessageError", Env.Security.CustomsDeclarationSendWithMessageErrors, testItem.SecurityCheckpointToSendWithMessageError);
		}

		public void TestGetSendingObjectsCollectionCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			var testItem = new CusEntryHeaderMessageSendingActionParentForTesting(declaration);

			CombineAssertions("", () =>
			{
				var collection = testItem.SendingObjectsCollection;
				AssertType("Should have created a correct SendingObjectsCollection.", typeof(CusEntryHeaderMessageSendingActionCollectionForTesting), collection);
				var items = collection.Cast<CusEntryHeaderMessageSendingActionForTesting>();
				AssertEquals("Should have created SendingAction for each entry header.", 2, items.Count());
				AssertEquals("There should be a SendingAction of Entry Header 1.", true, items.Any(action => action.EntryHeader == entryHeader1));
				AssertEquals("There should be a SendingAction of Entry Header 2.", true, items.Any(action => action.EntryHeader == entryHeader2));
			});
		}

		public void TestColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testItem = new CusEntryHeaderMessageSendingActionParentForTesting(declaration);
			var columns = testItem.MessageSendingObjectProperties.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Expecting 9 columns for CusEntryHeaderMessageSendingAction", 9, columns.Length);
				AssertMessageSendingObjectProperty(columns[0], expectedColumnName: BaseMessageSendingObject.SchemaShouldSend, expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(columns[1], expectedColumnName: nameof(CusEntryHeaderMessageSendingAction.SubStyle), expectedIsMandatory: true, expectedWidth: 100);
				AssertMessageSendingObjectProperty(columns[2], expectedColumnName: nameof(CusEntryHeaderMessageSendingAction.Description), expectedIsMandatory: true, expectedWidth: 150);
				AssertMessageSendingObjectProperty(columns[3], expectedColumnName: nameof(CusEntryHeaderMessageSendingAction.LocalReferenceNumber), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(columns[4], expectedColumnName: nameof(CusEntryHeaderMessageSendingAction.MessageStatus), expectedIsMandatory: true, expectedWidth: 150);
				AssertMessageSendingObjectProperty(columns[5], expectedColumnName: nameof(CusEntryHeaderMessageSendingAction.EntryStatus), expectedIsMandatory: true, expectedWidth: 100);
				AssertMessageSendingObjectProperty(columns[6], expectedColumnName: nameof(CusEntryHeaderMessageSendingAction.DeclarationType), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(columns[7], expectedColumnName: nameof(CusEntryHeaderMessageSendingAction.MessageTypeForDisplay), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(columns[8], expectedColumnName: nameof(CusEntryHeaderMessageSendingAction.MessageTypeDescription), expectedIsMandatory: true, expectedWidth: 150);
			});
		}

		void AssertMessageSendingObjectProperty(MessageSendingObjectProperty property, string expectedColumnName, bool expectedIsMandatory, int expectedWidth)
		{
			AssertEquals("PropertyName", expectedColumnName, property.PropertyName);
			AssertEquals("IsMandatory", expectedIsMandatory, property.IsMandatory);
			AssertEquals("ColumnWidth", expectedWidth, property.ColumnWidth);
		}

		public void TestDuplicationPossibleImportForDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryHeaderMessageSendingActionParent = new CusEntryHeaderMessageSendingActionParentForTesting(declaration);
			var sendingAction = (CusEntryHeaderMessageSendingActionForTesting)cusEntryHeaderMessageSendingActionParent.SendingObjectsCollection?.First();
			sendingAction.ShouldSend = true;
			sendingAction.MessageType = "415";

			entryHeader.CH_EntryStatus = "";
			AssertEquals("Warning should not be displayed when entry status is blank", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REL";
			AssertEquals("Warning should be displayed when entry status is REL", true, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REJ";
			AssertEquals("Warning should not be displayed when entry status is REJ", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "ACC";
			AssertEquals("Warning should be displayed when entry status is ACC", true, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "CAN";
			AssertEquals("Warning should not be displayed when entry status is CAN", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "NOT";
			AssertEquals("Warning should not be displayed when entry status is NOT", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);
		}

		public void TestDuplicationPossibleImportForAmendment()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryHeaderMessageSendingActionParent = new CusEntryHeaderMessageSendingActionParentForTesting(declaration);
			var sendingAction = (CusEntryHeaderMessageSendingActionForTesting)cusEntryHeaderMessageSendingActionParent.SendingObjectsCollection?.First();
			sendingAction.ShouldSend = true;
			sendingAction.MessageType = "413";

			entryHeader.CH_EntryStatus = "";
			AssertEquals("Warning should not be displayed when entry status is blank", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REL";
			AssertEquals("Warning should not be displayed when entry status is REL", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);
		}

		public void TestDuplicationPossibleImportForCancelation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryHeaderMessageSendingActionParent = new CusEntryHeaderMessageSendingActionParentForTesting(declaration);
			var sendingAction = (CusEntryHeaderMessageSendingActionForTesting)cusEntryHeaderMessageSendingActionParent.SendingObjectsCollection?.First();
			sendingAction.ShouldSend = true;
			sendingAction.MessageType = "414";

			entryHeader.CH_EntryStatus = "";
			AssertEquals("Warning should not be displayed when entry status is blank", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REL";
			AssertEquals("Warning should not be displayed when entry status is REL", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);
		}

		public void TestDuplicationPossibleExportForDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryHeaderMessageSendingActionParent = new CusEntryHeaderMessageSendingActionParentForTesting(declaration);
			var sendingAction = (CusEntryHeaderMessageSendingActionForTesting)cusEntryHeaderMessageSendingActionParent.SendingObjectsCollection?.First();
			sendingAction.ShouldSend = true;
			sendingAction.MessageType = "515";

			entryHeader.CH_EntryStatus = "";
			AssertEquals("Warning should not be displayed when entry status is blank", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REL";
			AssertEquals("Warning should be displayed when entry status is REL", true, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REJ";
			AssertEquals("Warning should not be displayed when entry status is REJ", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "ACC";
			AssertEquals("Warning should be displayed when entry status is ACC", true, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REF";
			AssertEquals("Warning should not be displayed when entry status is REF", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "DRJ";
			AssertEquals("Warning should not be displayed when entry status is DRJ", false, entryHeader.DuplicationPossible);

			entryHeader.CH_EntryStatus = "CAN";
			AssertEquals("Warning should not be displayed when entry status is CAN", false, entryHeader.DuplicationPossible);
		}

		public void TestDuplicationPossibleExportForAmendment()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryHeaderMessageSendingActionParent = new CusEntryHeaderMessageSendingActionParentForTesting(declaration);
			var sendingAction = (CusEntryHeaderMessageSendingActionForTesting)cusEntryHeaderMessageSendingActionParent.SendingObjectsCollection?.First();
			sendingAction.ShouldSend = true;
			sendingAction.MessageType = "513";

			entryHeader.CH_EntryStatus = "";
			AssertEquals("Warning should not be displayed when entry status is blank", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REL";
			AssertEquals("Warning should not be displayed when entry status is REL", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);
		}

		public void TestDuplicationPossibleExportForCancelation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryHeaderMessageSendingActionParent = new CusEntryHeaderMessageSendingActionParentForTesting(declaration);
			var sendingAction = (CusEntryHeaderMessageSendingActionForTesting)cusEntryHeaderMessageSendingActionParent.SendingObjectsCollection?.First();
			sendingAction.ShouldSend = true;
			sendingAction.MessageType = "514";

			entryHeader.CH_EntryStatus = "";
			AssertEquals("Warning should not be displayed when entry status is blank", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);

			entryHeader.CH_EntryStatus = "REL";
			AssertEquals("Warning should not be displayed when entry status is REL", false, cusEntryHeaderMessageSendingActionParent.DuplicationPossible);
		}
	}
}
