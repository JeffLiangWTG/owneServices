using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportDeclarationMessageSendingActionParent))]
	class ExportDeclarationMessageSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingOjectCollection()
		{
			Declaration.CustomsEntryHeaders.AddNew();
			Declaration.CustomsEntryHeaders.AddNew();

			var parent = (ExportDeclarationMessageSendingActionParent)GetNewBusinessObject();
			AssertEquals(typeof(ExportEntryMessageSendingActionCollection), parent.SendingObjectsCollection.GetType());
			AssertEquals("Populated", 2, parent.SendingObjectsCollection.Count);

			parent.SendingObjectsCollection[0].ShouldSend = true;
			parent.SendingObjectsCollection[1].ShouldSend = true;
			Assert(!parent.HasRowErrors);
		}

		public void TestMessageErrorsAreCollectedCorrectly()
		{
			var entryInstruction1 = Declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = Declaration.CustomsEntryInstructions.AddNew();

			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			invoiceLine2.JI_CustomsUnitQty = "";
			invoiceLine2.JI_CustomsQuantity = 10m;
			Assert("PreCondition", invoiceLine2.HasMessageErrors);
			var messageErrors = invoiceLine2.JI_CustomsQuantityInfo.GetMessageErrors().ToUniqueMessageListString();

			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			entry1.CH_CEI_Instruction = entryInstruction1.PK;
			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_CEI_Instruction = entryInstruction2.PK;
			var entryLine2 = entry2.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var parent = (ExportDeclarationMessageSendingActionParent)GetNewBusinessObject();
			parent.SendingObjectsCollection.Cast<MessageSendingAction>().First(x => x.MessagingObject == entry1).ShouldSend = true;
			AssertNotContains(messageErrors, parent.BizObjValidationMessageErrors);

			parent.SendingObjectsCollection.Cast<MessageSendingAction>().First(x => x.MessagingObject == entry1).ShouldSend = false;
			parent.SendingObjectsCollection.Cast<MessageSendingAction>().First(x => x.MessagingObject == entry2).ShouldSend = true;
			AssertContains(messageErrors, parent.BizObjValidationMessageErrors);
		}

		public void TestMessageSendingObjectProperties()
		{
			var parent = (ExportDeclarationMessageSendingActionParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;
			CombineAssertions(() =>
			{
				AssertEquals("MRN caption", "MRN", properties.ElementAt(0).ResourceString.Caption);
				AssertEquals("LRN caption", "LRN", properties.ElementAt(1).ResourceString.Caption);
				AssertEquals("Type (Time) caption", "Type (Time)", properties.ElementAt(2).ResourceString.Caption);
				AssertEquals("Type (Procedure) caption", "Type (Procedure)", properties.ElementAt(3).ResourceString.Caption);
				AssertEquals("Description caption", "Description", properties.ElementAt(4).ResourceString.Caption);
				AssertEquals("Entry Status caption", "Entry Status", properties.ElementAt(5).ResourceString.Caption);
				AssertEquals("Entry Type caption", "Entry Type", properties.ElementAt(6).ResourceString.Caption);

				AssertEquals("MRN width", 80, properties.ElementAt(0).ColumnWidth);
				AssertEquals("LRN width", 80, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Type (Time) width", 80, properties.ElementAt(2).ColumnWidth);
				AssertEquals("Type (Procedure) width", 110, properties.ElementAt(3).ColumnWidth);
				AssertEquals("Description width", 190, properties.ElementAt(4).ColumnWidth);
				AssertEquals("Entry Status width", 80, properties.ElementAt(5).ColumnWidth);
				AssertEquals("Entry Type width", 80, properties.ElementAt(6).ColumnWidth);

				Assert("All properties mandatory", properties.All(x => x.IsMandatory));
			});
		}

		public void TestCopyValuesBackToDeclaration()
		{
			Declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit);
			Declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice);
			Declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.ActualExitOffice);

			var entry = Declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction2 = Declaration.CustomsEntryInstructions.AddNew();
			entry2.CH_CEI_Instruction = entryInstruction2.PK;

			var parent = (ExportDeclarationMessageSendingActionParent)GetNewBusinessObject();
			var action = parent.SendingObjectsCollection[0];
			action.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			action.ExitDate = ZDateTime.BrettsBirthday;
			action.ExitCustomsOffice = "1111";

			var action2 = parent.SendingObjectsCollection[1];
			action2.EntryType = ExportEntryTypeList.Codes.ExitToExport;
			action2.ExitDate = ZDateTime.BrettsBirthday.AddDays(1);
			action2.ExitCustomsOffice = "1111";

			parent.CopyValuesBackToDeclaration();
			AssertEquals("", Declaration.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit).CY_Data);
			AssertEquals(ZDateTime.Empty, entryInstruction.ZG_ExitDate);
			AssertEquals(ZDateTime.Empty, entryInstruction2.ZG_ExitDate);

			action.ShouldSend = true;
			parent.CopyValuesBackToDeclaration();
			AssertEquals("1111", Declaration.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit).CY_Data);
			AssertEquals(ZDateTime.BrettsBirthday, entryInstruction.ZG_ExitDate);
			AssertEquals(ZDateTime.Empty, entryInstruction2.ZG_ExitDate);

			action.ExitCustomsOffice = "1111";
			action2.ExitCustomsOffice = "2222";
			Declaration.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit).CY_Data = "3333";
			parent.CopyValuesBackToDeclaration();
			AssertEquals("Should copy from an object for which users selected Send", "1111", Declaration.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit).CY_Data);

			action.ExitCustomsOffice = "1111";
			action2.ShouldSend = true;
			action2.ExitCustomsOffice = "2222";
			Declaration.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit).CY_Data = "3333";
			parent.CopyValuesBackToDeclaration();
			AssertEquals("Not a unique value", "3333", Declaration.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit).CY_Data);
		}

		public void TestCopyValuesBackToDeclaration_LocalReferenceNumber()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			var entry2 = Declaration.CustomsEntryHeaders.AddNew();

			var parent = (ExportDeclarationMessageSendingActionParent)GetNewBusinessObject();
			var action = parent.SendingObjectsCollection[0];
			action.LocalReferenceNumber = "ABCDE";
			action.ShouldSend = true;

			var action2 = parent.SendingObjectsCollection[1];
			action2.LocalReferenceNumber = "12345";

			parent.CopyValuesBackToDeclaration();
			CombineAssertions(() =>
			{
				AssertEquals("ShouldSend is true", "ABCDE", entry.LocalReferenceNumber);
				AssertEquals("ShouldSend is false", ZString.Empty, entry2.LocalReferenceNumber);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExportDeclarationMessageSendingActionParent(Declaration);
		}

		JobDeclaration Declaration => fJobDeclaration ?? (fJobDeclaration = Factory.New<JobDeclaration>());
		JobDeclaration fJobDeclaration;
	}
}
