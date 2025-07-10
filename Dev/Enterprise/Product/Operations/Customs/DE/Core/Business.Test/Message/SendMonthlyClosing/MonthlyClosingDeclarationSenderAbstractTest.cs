using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(MonthlyClosingDeclarationSender))]
	public abstract class MonthlyClosingDeclarationSenderAbstractTest<T> : TestCaseWithFactory
		where T : MonthlyClosingDeclarationSender
	{
		public void TestConstructor_NullDeclaration()
		{
			AssertExceptionThrown<ArgumentException>(() => GetMonthlyClosingDeclarationSender(null));
		}

		public void TestSendMessageATLASVersion10_2()
		{
			reconEntry.CRE_EntryType = "TYP";
			reconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			TestHelper.CreateCL010CoutryList(Factory);
			if (!ExpectedMessageTypeATLASVersion10_2.IsEmpty)
			{
				DoUsingATLAS102(() =>
				{
					AssertEDIMessageAndLogbookDetailsAndLinesNote(ExpectedMessageTypeATLASVersion10_2);
				});
			}
			else
			{
				Assert("ATLAS Version 10.2 Sending not implemented yet for Sending Type", true);
			}
		}

		protected void DoUsingATLAS102(Action action)
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._102 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				action();
			}
		}

		protected void AssertHasInformationToSend_NoInformation_NoCusReconEntryLines_102()
		{
			reconEntry.CusReconEntryLines.DeleteAll();
			DoUsingATLAS102(() => AssertEquals(false, MonthlyClosingDeclarationSender.MessageHasInformationToSend));
		}

		protected void AssertHasInformationToSend_HasInformation_HasCusReconEntryLines_102()
		{
			DoUsingATLAS102(() => AssertEquals(true, MonthlyClosingDeclarationSender.MessageHasInformationToSend));
		}

		public void TestSendMessageATLASVersion10_1()
		{
			reconEntry.CRE_EntryType = "TYP";
			reconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			TestHelper.CreateCL010CoutryList(Factory);
			if (!ExpectedMessageTypeATLASVersion10_1.IsEmpty)
			{
				DoUsingATLAS101(() =>
				{
					AssertEDIMessageAndLogbookDetailsAndLinesNote(ExpectedMessageTypeATLASVersion10_1);
				});
			}
			else
			{
				Assert("ATLAS Version 10.1 Sending not implemented yet for Sending Type", true);
			}
		}

		protected void DoUsingATLAS101(Action action)
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				action();
			}
		}

		protected void AssertHasInformationToSend_NoInformation_NoCusReconEntryLines_101()
		{
			reconEntry.CusReconEntryLines.DeleteAll();
			DoUsingATLAS101(() => AssertEquals(false, MonthlyClosingDeclarationSender.MessageHasInformationToSend));
		}

		protected void AssertHasInformationToSend_HasInformation_HasCusReconEntryLines_101()
		{
			DoUsingATLAS101(() => AssertEquals(true, MonthlyClosingDeclarationSender.MessageHasInformationToSend));
		}

		protected MonthlyClosingDeclarationSender MonthlyClosingDeclarationSender => monthlyClosingDeclarationSender ?? (monthlyClosingDeclarationSender = GetMonthlyClosingDeclarationSender(declaration));
		MonthlyClosingDeclarationSender monthlyClosingDeclarationSender;

		protected abstract T GetMonthlyClosingDeclarationSender(CusReconDeclaration declaration);

		protected abstract ZString ExpectedMessageTypeATLASVersion10_1 { get; }
		protected abstract ZString ExpectedMessageTypeATLASVersion10_2 { get; }
		protected abstract ZString ExpectedMessageSubType { get; }

		protected CusReconDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 3;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 6;
			var invoice = jobDeclaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var declarant = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "EOR1", Core.Constants.CountryCodes.Germany, "EBS1");
			declarant.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
			declaration = Factory.New<CusReconDeclaration>();
			declaration.CRD_DeclarantType = RepresentationTypeList.Codes._1Self;
			declaration.CRD_JobReferenceNumber = "WTG1234";
			declaration.CRD_OA_DeclarantAddress = declarant.MainAddress.PK;

			reconEntry = declaration.CusReconEntries.AddNew();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var reconEntryLine1 = reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine1.CRL_LineNumber = 1;
			reconEntryLine1.CRL_OriginalEntryLineNumber = 3;
			var reconEntryLine2 = reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine2.CRL_LineNumber = 2;
			reconEntryLine2.CRL_OriginalEntryLineNumber = 6;
		}
		protected CusReconEntry reconEntry;

		void AssertEDIMessageAndLogbookDetailsAndLinesNote(ZString expectedMessageType)
		{
			MonthlyClosingDeclarationSender.Send();
			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, declaration.Messages.Count);
				var message = declaration.Messages[0];
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", Messaging.EDIMessageTypeList.Codes.Import, message.EM_MessageType);
				AssertEquals("EM_ApplicationReference", expectedMessageType, message.EM_ApplicationReference);
				AssertEquals("EM_MessageSubType", ExpectedMessageSubType, message.EM_MessageSubType);
				AssertEquals("LinkedObject", declaration.PK, message.EM_LinkedObject.PK);
				AssertEquals("Declaration Message-Status", Common.Shared.MessageStatusList.Codes.Sent, declaration.CRD_MessageStatus);
				AssertEquals("LogbookEORIBranchSuffix exists", "EBS1", message.GetLogbookEORIBranchSuffix());
				AssertEquals("LogbookLocalReferenceNumber exists", "WTG1234", message.GetLogbookLocalReferenceNumber());
				AssertEquals("1|2", message.GetMonthlyClosingLinesNote());
			});
		}
	}
}
