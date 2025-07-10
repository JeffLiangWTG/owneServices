using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectParent))]
	sealed class JobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new JobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._929);
		}

		public void TestSendingObjectsCollection()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var objectParent1 = new JobDeclarationMessageSendingObjectParent(declaration1, ElectronicDocumentTypeList.Codes._929);
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.CustomsEntryHeaders.AddNew();
			var objectParent2 = new JobDeclarationMessageSendingObjectParent(declaration2, ElectronicDocumentTypeList.Codes._929);
			AssertEquals(2, objectParent2.SendingObjectsCollection.Count);
		}

		public void TestObjectsToSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._929);
			objectParent.SendingObjectsCollection[1].ShouldSend = true;
			AssertEquals(1, objectParent.ObjectsToSend.Count());
			AssertEquals(entry, objectParent.ObjectsToSend.Single().Header);
		}

		public void TestIncludeNotificationsFromObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportGoodsType = ExportDealingTypeCodeList.Codes._79;

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_IncoTerm = "XXX";
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_IncoTerm = "";

			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CountryOfOrigin = "KK";
			invoiceLine1.JI_PreviousEntryLineNumber = 123;
			invoiceLine1.JI_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.B;

			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_COOLabelLocation = "X";

			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();

			invoiceLine1.JI_CL = entryHeader1.MergedLines.AddNew().PK;
			invoiceLine2.JI_CL = entryHeader2.MergedLines.AddNew().PK;

			declaration.LoadChildEditableObjects();
			declaration.RunPreSaveValidation();
			var parent = new JobDeclarationMessageSendingObjectParentForTest(declaration, ElectronicDocumentTypeList.Codes._929);
			parent.SendingObjectsCollection[0].ShouldSend = true;
			parent.SendingObjectsCollection[1].ShouldSend = true;

			var messageErrors = parent.GetNewMessageErrorCollectorExposed();
			Assert("IncoTerm error for invoice 1", messageErrors.Contains("Incoterm: The code you have selected is not in the list."));
			Assert("IncoTerm error for invoice 2", messageErrors.Contains("Incoterm: Please enter an Incoterm."));
			Assert("Country Of Origin error for invoice line 1", messageErrors.Contains("Goods Origin: The code you have selected is not in the list."));
			Assert("C/O Label Location error for invoice line 2", messageErrors.Contains("C/O Label Location: The code you have selected is not in the list."));
			Assert("Import Entry Line No. error for declaration", messageErrors.Contains("Import Entry Line No.: For the selected transaction type, an import declaration number/entry line number is not relevant."));

			parent.SendingObjectsCollection[0].ShouldSend = false;
			parent.SendingObjectsCollection[1].ShouldSend = true;
			messageErrors = parent.GetNewMessageErrorCollectorExposed();
			Assert("IncoTerm error for invoice 1", !messageErrors.Contains("Incoterm: The code you have selected is not in the list."));
			Assert("IncoTerm error for invoice 2", messageErrors.Contains("Incoterm: Please enter an Incoterm."));
			Assert("Country Of Origin error for invoice line 1", !messageErrors.Contains("Goods Origin: The code you have selected is not in the list."));
			Assert("C/O Label Location error for invoice line 2", messageErrors.Contains("C/O Label Location: The code you have selected is not in the list."));
			Assert("Import Entry Line No. error for declaration", !messageErrors.Contains("Import Entry Line No.: For the selected transaction type, an import declaration number/entry line number is not relevant."));

			parent.SendingObjectsCollection[0].ShouldSend = true;
			parent.SendingObjectsCollection[1].ShouldSend = false;
			messageErrors = parent.GetNewMessageErrorCollectorExposed();
			Assert("IncoTerm error for invoice 1", messageErrors.Contains("Incoterm: The code you have selected is not in the list."));
			Assert("IncoTerm error for invoice 2", !messageErrors.Contains("Incoterm: Please enter an Incoterm."));
			Assert("Country Of Origin error for invoice line 1", messageErrors.Contains("Goods Origin: The code you have selected is not in the list."));
			Assert("C/O Label Location error for invoice line 2", !messageErrors.Contains("C/O Label Location: The code you have selected is not in the list."));
			Assert("Import Entry Line No. error for declaration", messageErrors.Contains("Import Entry Line No.: For the selected transaction type, an import declaration number/entry line number is not relevant."));
		}

		public void TestGetJobDeclarationMessageSendingObjectParent()
		{
			var declaration = Factory.New<JobDeclaration>();

			var result = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._DKJ, MessageFunctions.MessageFunctionCode.Cancellation);
			AssertEquals(typeof(JobDeclarationMiscMessageSendingObjectParent), result.GetType());

			result = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend);
			AssertEquals(typeof(JobDeclarationMiscMessageSendingObjectParent), result.GetType());

			result = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5BD, MessageFunctions.MessageFunctionCode.Original);
			AssertEquals(typeof(EarlyReleaseMiscMessageSendingObjectParent), result.GetType());
		}

		class JobDeclarationMessageSendingObjectParentForTest : JobDeclarationMessageSendingObjectParent
		{
			public JobDeclarationMessageSendingObjectParentForTest(JobDeclaration declaration, string messageType) : base(declaration, messageType)
			{
			}

			public IEnumerable<INotification> GetNewMessageErrorCollectorExposed() => GetNewMessageErrorCollector();
		}
	}
}
