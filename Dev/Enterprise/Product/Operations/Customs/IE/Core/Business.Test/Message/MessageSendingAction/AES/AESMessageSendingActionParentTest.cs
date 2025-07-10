using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.ExitControl.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AESMessageSendingActionParent))]
	class AESMessageSendingActionParentTest : CusEntryHeaderMessageSendingActionParentTest<AESMessageSendingActionParent, AESMessageSendingAction>
	{
		public void TestGetNewMessageErrorCollector()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.JE_LocationOtherInformation = "XX";

			var parent = new AESMessageSendingActionParent(declaration);
			parent.SendingObjectsCollection.Cast<AESMessageSendingAction>().First(x => x.EntryHeader == entry).ShouldSend = true;

			CombineAssertions(() =>
			{
				var warning = "This Incoterm (FOB) is only valid for sea and inland waterway transport. Please check against the transport mode.";
				var messageError = "The code you have selected is not in the list.";
				AssertHasWarning("Precondition, has warning", declaration.JE_ShipmentIncoTermInfo, warning);
				AssertHasMessageError("Precondition, has message error", declaration.JE_LocationOtherInformationInfo, messageError);
				var errors = parent.BizObjValidationMessageErrors;
				AssertNotContains("Should not contain warning", warning, errors);
				AssertContains("Should contain message error", messageError, errors);
			});
		}

		public void TestGetNewMessageErrorCollectorDosNotContainExitControlErrors()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			declaration.JE_DeclarationReference = "DEC";
			var header = Factory.New<CusExitHeader>();
			header.Parent = declaration;
			// the following RegisterEditableChildObject(header) would be done by the ExitControlPlugIn
			declaration.RegisterEditableChildObject(header);

			var parent = new AESMessageSendingActionParent(declaration);
			parent.SendingObjectsCollection.Cast<AESMessageSendingAction>().First(x => x.EntryHeader == entry).ShouldSend = true;

			CombineAssertions(() =>
			{
				var errors = parent.BizObjValidationMessageErrors;
				var messageError = "Carrier: You have not entered a CusExitHeader.CXH_OA_Carrier";
				AssertNotContains("Should contain message error", messageError, errors);
			});
		}

		public void TestIsNeedConfirm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			var sendingActionParent = new AESMessageSendingActionParent(declaration);

			Assert("Not Need Confirm", !sendingActionParent.IsNeedConfirm);

			var collection = (AESMessageSendingActionCollection)sendingActionParent.SendingObjectsCollection;
			collection[0].ShouldSend = true;
			collection[1].ShouldSend = true;
			Assert("Not Need Confirm", !sendingActionParent.IsNeedConfirm);

			entryHeader1.CH_Status = "SNT";
			sendingActionParent = new AESMessageSendingActionParent(declaration);
			Assert("Not Need Confirm", !sendingActionParent.IsNeedConfirm);

			collection = (AESMessageSendingActionCollection)sendingActionParent.SendingObjectsCollection;
			collection[1].ShouldSend = true;
			Assert("Not Need Confirm", !sendingActionParent.IsNeedConfirm);

			collection[0].ShouldSend = true;
			Assert("Need Confirm", sendingActionParent.IsNeedConfirm);
		}

		protected override Type ExpectedSendingObjectCollectionType => typeof(AESMessageSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AESMessageSendingActionParent(Factory.New<JobDeclaration>());
		}
	}
}
