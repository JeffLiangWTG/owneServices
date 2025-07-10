using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class FRAutoD2MMessageSenderTest : TestCaseWithFactory
	{
		public void TestAutomationDeltaProcessorWorkingCorrectly()
		{
			using (FRCustomsDataRegistry.Instance.NbDaysWaitBeforeSendingDeltaDStep2.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2))
			{
				var branchPK = GlbBranch.CurrentBranch.PK;
				var declarantAddressPK = Factory.NewWithValidTestData<OrgAddress>().PK;
				var entryHeader = CreateEntryHeaderForTesting(branchPK, declarantAddressPK, "19212081311", ZDateTime.Now.AddDays(-2));
				CreateEntryHeaderForTesting(branchPK, declarantAddressPK, "19212081312", ZDateTime.Now.AddDays(-10));
				CreateEntryHeaderForTesting(branchPK, declarantAddressPK, "19212081313", ZDateTime.Now.AddDays(-3));
				CreateEntryHeaderForTesting(branchPK, declarantAddressPK, "19212081314", ZDateTime.Now.AddDays(-1));
				CreateEntryHeaderForTesting(branchPK, declarantAddressPK, "19212081315", ZDateTime.Now.AddDays(-10)).CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;

				Factory.Save();

				var logger1 = new TestServiceLogger();

				var autoDeltaProcessor1 = new FRAutoD2MMessageSender(new LoggerWrapper(logger1));
				AssertEquals("Processor DaysWaitBeforeSendingDeltaDStep2 should match registry NbDaysWaitBeforeSendingDeltaDStep2 value", 2, autoDeltaProcessor1.DaysWaitBeforeSendingDeltaDStep2);

				autoDeltaProcessor1.Process(GlbCompany.CurrentCompany);
				AssertEquals("A D2M message shoudl have been aitomatically sent.", 1, entryHeader.Messages.Count);

				var message = entryHeader.Messages[0];

				CombineAssertions("Entry status, sequence number and auto sent D2M message content", () =>
				{
					AssertEquals("CH_Status", MessageStatusCodeList.Codes.AWR, entryHeader.CH_Status);
					AssertEquals("CH_SequenceNumber", 1, entryHeader.CH_SequenceNumber);
					AssertContains("Action code in EM_MessageText", "<codact>2</codact>", message.EM_MessageText);
					AssertContains("Entry number in EM_MessageText", "<refdos>19212081311</refdos>", message.EM_MessageText);
					AssertEquals("EM_ApplicationCode", FREDIMessage.ApplicationCodes.FRCustomsMessage, message.EM_ApplicationCode);
					AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("EM_MessageSubType", EntryActionCodeList.Codes.D2M, message.EM_MessageSubType);
				});

				AssertEquals(@"Information|Start to run Automated Delta D2M messages in Company EDI.
Information|Automated Delta D2M messages have been sent for Customs Entries 19212081311,19212081312,19212081313
Information|Finished running Automated Delta D2M messages in Company EDI.
", logger1.ToString());

				var logger2 = new TestServiceLogger();
				var autoDeltaProcessor2 = new FRAutoD2MMessageSender(new LoggerWrapper(logger2));
				autoDeltaProcessor2.Process(GlbCompany.CurrentCompany);
				AssertEquals(@"Information|Start to run Automated Delta D2M messages in Company EDI.
Information|No candidate entries in Company EDI.
Information|Finished running Automated Delta D2M messages in Company EDI.
", logger2.ToString());
			}
		}

		Declaration.CusEntryHeader CreateEntryHeaderForTesting(ZGuid branchPK, ZGuid declarantAddressPK, ZString entryReferenceNumber, ZDateTime entryNumberDate)
		{
			var declaration = Factory.New<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MainAddress.OA_PostCode = "123456";
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI001", ZString.Empty, ZString.Empty, "C0E1C9EB");
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "12345678", Core.Constants.CountryCodes.France);
			declaration.JE_OH_Importer = importer.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "12345678", Core.Constants.CountryCodes.France);
			declarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "12345678", Core.Constants.CountryCodes.France);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MasterBill = "UnitTest";
			declaration.JE_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			declaration.JE_GB = branchPK;
			declaration.JE_OA_DeclarantAddress = declarantAddressPK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var cei = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_CustomsQuantity = 1m;

			Assert("Merge failed", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));

			foreach (SupportingDocument item in declaration.SupportingDocuments)
			{
				if (item.CSI_DateOfIssue == ZDateTime.Empty)
				{
					item.CSI_DateOfIssue = ZDateTime.Today;
				}
			}

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader.CH_BGMReference = entryReferenceNumber;

			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = entryHeader.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "IMP";
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusNum.CE_EntryNum = "1111";
			cusNum.CE_IssueDate = entryNumberDate;

			return entryHeader;
		}
	}
}
