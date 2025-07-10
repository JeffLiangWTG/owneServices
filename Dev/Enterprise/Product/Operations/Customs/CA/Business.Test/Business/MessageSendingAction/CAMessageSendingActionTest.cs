using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CAMessageSendingAction))]
	sealed class CAMessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCA_MessageDescription()
		{
			CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			entry.CH_BGMReference = "B01234223";

			CAMessageSendingAction action = new CAMessageSendingAction(entry, MessageType.DataLoadingModule, Actions);
			AssertEquals("CA_MessageDescription", "B01234223", action.CA_MessageDescription);
		}

		public void TestCA_MessageContents()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			DeclarationTestHelper helper = new DeclarationTestHelper(Factory, false);
			JobDeclaration declaration = Declaration;
			declaration.JE_OH_Supplier = helper.Consignor.PK;
			declaration.JE_OH_Importer = helper.Consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "B00123457";
			declaration.JE_VesselName = helper.VesselWithCountry.RV_Code;
			declaration.JE_RL_NKPortOfLoading = helper.CAVAR.Code;
			declaration.JE_RL_NKPortOfArrival = helper.AUSYD.Code;
			declaration.JE_ExportDate = new ZDateTime(2007, 10, 25);
			declaration.JE_RL_NKOrigin = helper.CATOR.Code;
			declaration.JE_RL_NKFinalDestination = helper.NZAKL.Code;
			declaration.JE_TotalWeight = 142.30m;
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalNoOfPacksPackType = "PAL";
			CusContainer container1 = helper.CreateCusContainer(declaration, "TURE1111111", "SEAL1", helper.Container40US, "FCL");
			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_Weight = 62.30m;
			invoice1.JZ_InvoiceNumber = "INV123456";
			invoice1.JZ_InvoiceDate = new ZDateTime(2007, 10, 23);
			invoice1.JZ_InvoiceAmount = 7000.00m;
			InvoiceCharge invoice1Charge = invoice1.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 500m);
			JobComInvoiceLine invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_Tariff = "08010101";
			invoice1Line1.JI_LinePrice = 4000m;
			invoice1Line1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoice1Line1.JI_InvoiceQuantity = 10;
			invoice1Line1.JI_InvoiceUQ = UnitOfMeasureListForDLM.Codes.Number;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			CAMessageSendingAction action = new CAMessageSendingAction(declaration.CustomsEntryHeaders[0], MessageType.DataLoadingModule, Actions);
			DataLoadingModuleMessageBuilder builder = new DataLoadingModuleMessageBuilder(declaration.CustomsEntryHeaders[0]);
			EDIMessage message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("CA_MessageContents", message.EM_MessageInterpretation, action.CA_MessageContents);
		}

		public void TestCA_SaveWithoutSendingAndCA_SendMessage()
		{
			CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			var mock = new Mock<CAMessageSendingAction>(entry, MessageType.DataLoadingModule, Actions);
			CAMessageSendingAction action = mock.Object;
			action.CA_SendMessage = false;
			action.CA_SaveWithoutSending = true;

			action.CA_SendMessage = true;
			AssertEquals("CA_SaveWithoutSending is set back to false again: CA_SaveWithoutSending is readonly", false, action.CA_SaveWithoutSending);

			action.CA_SaveWithoutSending = true;
			AssertEquals("CA_SendMessage is set back to false again: CA_SaveWithoutSending is readonly", false, action.CA_SendMessage);
		}

		public void TestProcessWhenSavedWithoutSending()
		{
			CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			var mock = new Mock<CAMessageSendingAction>(entry, MessageType.DataLoadingModule, Actions);
			CAMessageSendingAction action = mock.Object;

			action.CA_SendMessage = true;
			action.ProcessWhenSavedWithoutSending();
			AssertEquals("no log should have been added against action.entry", false, new Customs.Business.OutstandingAmendmentLogManager(action.entry).HasOutstandingAmendments);

			action.CA_SaveWithoutSending = true;
			action.CA_SaveWithoutSendingReasonText = "Don't like amendment";
			action.ProcessWhenSavedWithoutSending();
			ZString reference = new Customs.Business.OutstandingAmendmentLogManager(action.entry).AllOustandingAmendmentReferences;
			AssertEquals("A log should have been added against action.entry", true, reference.Contains(action.CA_SaveWithoutSendingReasonText));
		}

		public void TestCA_SaveWithoutSendingReasonTextInfoReadOnly()
		{
			CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			var mock = new Mock<CAMessageSendingAction>(entry, MessageType.DataLoadingModule, Actions) { CallBase = true };
			CAMessageSendingAction action = mock.Object;
			action.CA_SaveWithoutSending = true;
			AssertEquals("CA_SaveWithoutSendingReasonTextInfo.ReadOnly", false, action.CA_SaveWithoutSendingReasonTextInfo.ReadOnly);
			action.CA_SaveWithoutSending = false;
			AssertEquals("CA_SaveWithoutSendingReasonTextInfo.ReadOnly", true, action.CA_SaveWithoutSendingReasonTextInfo.ReadOnly);
		}

		#region Implementation
		CAMessageSendingActionCollection Actions
		{
			get { return actions ?? (actions = new CAMessageSendingActionCollection(Declaration, MessageSendingMessageType.Original)); }
		}
		CAMessageSendingActionCollection actions;

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		protected override BusinessObject GetNewBusinessObject()
		{
			CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			return new CAMessageSendingAction(entry, MessageType.DataLoadingModule, Actions);
		}
		#endregion Implementation
	}
}
