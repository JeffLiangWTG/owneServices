using System.Linq;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	sealed class CC414BCciOperationWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC414BCciOperationWrapper>
	{
		#region CustomsRegistrationNumber

		public void TestCustomsRegistrationNumber()
		{
			AssertEquals("Customs Registration Number should be captured from entry header's DeclarationReference", "CRN #", Provider.CustomsRegistrationNumber);
		}

		#endregion

		#region InvalidationMotivation

		public void TestInvalidationMotivation()
		{
			AssertEquals("InvalidationMotivation should be captured from ChangeAcknowledgementIndicator of message object", "XXX", Provider.InvalidationMotivation);
		}

		#endregion

		#region InvalidationReason

		public void TestInvalidationReason()
		{
			AssertEquals("InvalidationReason should be captured from VOCReason of message object", "VOR Reason", Provider.InvalidationReason);
		}

		#endregion

		#region InvalidationRequestDateAndTime

		[TestDate(2023, 5, 13, 16, 13, 2)]
		public void TestInvalidationRequestDateAndTime()
		{
			AssertEquals("InvalidationReason should be captured from InstantiationTime of message object", "2023-05-13T16:13:02", Provider.InvalidationRequestDateAndTime);
		}

		#endregion

		#region LRN

		public void TestLRN()
		{
			AssertEquals("LRN should be captured from entry header's CorrelationID", "LRN #", Provider.LRN);
		}

		#endregion

		#region MRN

		public void TestMRN()
		{
			AssertEquals("MRN should be captured from entry header's MovementReferenceNumber", "MRN #", Provider.MRN);
		}

		#endregion

		#region SupportingDocument

		public void TestSupportingDocument()
		{
			var documents = Provider.SupportingDocument.ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Supporting Document Count", 3, documents.Count);
				SupportingDocumentWrapperTest.AssertSupportingDocumentWrapper(documents[0], type: "AAA", referenceNumber: "1111");
				SupportingDocumentWrapperTest.AssertSupportingDocumentWrapper(documents[1], type: "BBB", referenceNumber: "2222");
				SupportingDocumentWrapperTest.AssertSupportingDocumentWrapper(documents[2], type: "CCC", referenceNumber: "3333");
			});
		}

		#endregion

		protected override CC414BCciOperationWrapper GetProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var decSupportingDoc1 = declaration.SupportingDocuments.AddNew();
			decSupportingDoc1.CSI_Code = "AAA";
			decSupportingDoc1.CSI_ReferenceNumber = "1111";
			var invoice = declaration.Invoices.AddNew();
			var invoiceSupportingDoc = invoice.SupportingDocuments.AddNew();
			invoiceSupportingDoc.CSI_Code = "BBB";
			invoiceSupportingDoc.CSI_ReferenceNumber = "2222";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lineSupportingDoc = invoiceLine.SupportingDocuments.AddNew();
			lineSupportingDoc.CSI_Code = "CCC";
			lineSupportingDoc.CSI_ReferenceNumber = "3333";

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CorrelationID = "LRN #";
			entryHeader.MRN = "MRN #";
			entryHeader.CRN = "CRN #";
			Factory.Save();

			var messageObject = new DeltaIEJobDeclarationMessageSendingObject(entryHeader);
			messageObject.ChangeAcknowledgementIndicator = "XXX";
			messageObject.VOCReason = "VOR Reason";
			return CC414BCciOperationWrapper.New(messageObject);
		}
	}
}
