using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNJobDeclarationMessageSendingObjectParent))]
	sealed class CNJobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2020, 3, 20)]
		public void AdditionalWarnings_MoreThan3MonthsAgo()
		{
			const string warning = "The Date of Arrival is more than three months ago.";
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.ActiveEntryHeaders.AddNew();
				declaration.JE_DateOfArrival = new ZDateTime(2020, 3, 1);
				AssertNotContains("Not Contains", warning, GetAdditionalWarnings(declaration));
				declaration.JE_DateOfArrival = new ZDateTime(2019, 12, 1);
				AssertContains("Contains", warning, GetAdditionalWarnings(declaration));
			});
		}

		[TestDate(2020, 3, 20)]
		public void AdditionalWarnings_HasBeenDelayed()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("DDF", 0.0005m, "CN", 50m, 0m, "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Constants.UniversalReferenceConstants.RefCusTaxOrFee.DelayedFeeRate);
			Factory.Save();

			const string warning = "The declaration of CUS001 has been delayed for 5 day(s). The estimated fee for that is 500 CNY.\r\n";
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_JE = declaration.PK;
				instruction.CEI_Style = "11";
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_JE = declaration.PK;
				var line = invoice.InvoiceLines.AddNew();
				line.JI_CEI = instruction.PK;
				line.JI_JZ = invoice.PK;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.CH_BGMReference = "CUS001";
				var entryLine1 = entry.AllEntryLines.AddNew();
				var entryLine2 = entry.AllEntryLines.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = instruction.PK;
				invoiceLine1.JI_CL = entryLine1.PK;
				var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction.PK;
				invoiceLine2.JI_CL = entryLine2.PK;
				entryLine1.CL_CustomsValue = 65000m;
				entryLine2.CL_CustomsValue = 135000m;

				declaration.JE_DateOfArrival = new ZDateTime(2020, 3, 1);
				AssertContains("Contains", warning, GetAdditionalWarnings(declaration));
				declaration.JE_DateOfArrival = new ZDateTime(2019, 12, 1);
				AssertNotContains("Not contains", warning, GetAdditionalWarnings(declaration));
			});
		}

		public void TestAdditionalWarnings_NotCurrentBranch()
		{
			const string warning = "The branch of this job is not the current branch. The entry will be declared on behalf of branch";
			CombineAssertions(() =>
			{
				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				company.GC_Code = "TC1";
				var branch1 = Factory.New<GlbBranch>();
				branch1.GB_Code = "GB1";
				branch1.GB_GC = company.PK;
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_GB = branch1.PK;
				declaration.ActiveEntryHeaders.AddNew();

				AssertContains("Contains", warning, GetAdditionalWarnings(declaration));
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				AssertNotContains("Not contains", warning, GetAdditionalWarnings(declaration));
			});
		}

		public void TestAdditionalWarnings_AlreadyAcknowledgedByChina()
		{
			const string warning = "CUS001 has already be acknowledged by China Customs. Are you sure that you want to resend it?\r\n";
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.CH_BGMReference = "CUS001";
				entry.CH_Status = JobMessageStatusList.Codes.AcknowledgedCompletedDeclaration;
				AssertContains("Contains", warning, GetAdditionalWarnings(declaration));
				entry.CH_Status = JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration;
				AssertNotContains("Not contains", warning, GetAdditionalWarnings(declaration));
			});
		}

		public void TestAdditionalWarnings_WaitingForResponse()
		{
			const string warning = "CUS001 is waiting for response. Are you sure that you want to resend it?\r\n";
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.CH_BGMReference = "CUS001";
				entry.CH_Status = JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration;
				AssertContains("Contains", warning, GetAdditionalWarnings(declaration));
				entry.CH_Status = JobMessageStatusList.Codes.AcknowledgedCompletedDeclaration;
				AssertNotContains("Not contains", warning, GetAdditionalWarnings(declaration));
			});
		}

		public void TestMessageSendingObjectProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var properties = new CNJobDeclarationMessageSendingObjectParent(declaration).MessageSendingObjectProperties.ToList();

			AssertContainsExactElementsInExactOrder("MessageSendingObjectProperties", new[]
			{
				(CNJobDeclarationMessageSendingObject.Schema.IntelligentDeclarationType, 150),
				(CNJobDeclarationMessageSendingObject.Schema.LocalReferenceNumber, 150),
				(CNJobDeclarationMessageSendingObject.Schema.DeclarationUnifiedNumber, 150),
				(CNJobDeclarationMessageSendingObject.Schema.EntryNumber, 150),
				(CNJobDeclarationMessageSendingObject.Schema.MessageTypeDescription, 150),
				(CNJobDeclarationMessageSendingObject.Schema.DeclarationTypeDescription, 150),
				(CNJobDeclarationMessageSendingObject.Schema.MessageStatusDescription, 150),
				(CNJobDeclarationMessageSendingObject.Schema.EntryStatus, 100),
			}, properties.Select(x => ((string)x.PropertyName, (int)x.ColumnWidth)));
		}

		public void TestValidateCanSubmit()
		{
			var registryItem = CNCustomsDataRegistry.Instance.CNSWClientSetting;
			var factory = registryItem.Factory;
			var company = factory.New<GlbCompany>();
			company.GC_Code = "CMP";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BR1";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";
			factory.Save();
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			var wrapper = new CNJobDeclarationMessageSendingObjectParent(declaration);
			var result = wrapper.ValidateCanSubmit();
			AssertEquals("Error when no setting", CNSWClientSettingChecker.EHubClientNotRegisteredMessage, result);

			using (CNSWClientSettingCheckerTest.TemporarilySetCNSWClientSetting(Factory, declaration.CompanyPK.ToGuid(), Guid.Empty))
			{
				result = wrapper.ValidateCanSubmit();
				AssertEquals("No error when setting is normal", ZString.Empty, result);
			}
		}

		public void TestSendMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var parent = new CNJobDeclarationMessageSendingObjectParent(declaration);
			declaration.JE_MessageType = "IMP";
			declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			var entryHeader1 = CusEntryHeader(declaration, "REF-001");
			var entryHeader2 = CusEntryHeader(declaration, "REF-002");

			AssertEquals("No messages when shouldSend is false", 0, parent.SendMessages());

			var messageSendingObject = parent.SendingObjectsCollection[0];
			messageSendingObject.ShouldSend = true;
			AssertEquals("Has messages when shouldSend is true", 1, parent.SendMessages());
			AssertEquals("Only sendMessage when shouldSend is true", 1, entryHeader1.Messages.Count);
			AssertEquals("Only sendMessage when shouldSend is true", 0, entryHeader2.Messages.Count);
			AssertEquals("CH_Status updated", JobMessageStatusList.Codes.AwaitingResponseIntegratedDeclaration, entryHeader1.CH_Status);
			AssertEquals("Should add an MSN event", "Send to Customs;REF-001", entryHeader1.Logs.MostRecentLogByEventTime(Events.MessageSent).SL_Reference);

			messageSendingObject = parent.SendingObjectsCollection[1];
			messageSendingObject.ShouldSend = true;
			declaration.JE_PaidBy = "XXX";
			AssertEquals("Zero message is saved when exception occur", 0, parent.SendMessages());
			AssertEquals("Zero message is saved when exception occur", 0, entryHeader2.Messages.Count);
			AssertContains("Constraint_PaidBy", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("CH_Status should not changed", ZString.Empty, entryHeader2.CH_Status);
			AssertNull("Should not add an MSN event on sending failed", entryHeader2.Logs.MostRecentLogByEventTime(Events.MessageSent));
		}

		protected override BusinessObject GetNewBusinessObject() => new CNJobDeclarationMessageSendingObjectParent(Factory.New<JobDeclaration>());

		static string GetAdditionalWarnings(JobDeclaration declaration)
		{
			var messageSendingObjectParent = new CNJobDeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
			messageSendingObject.ShouldSend = true;
			return messageSendingObjectParent.AdditionalWarnings;
		}

		CusEntryHeader CusEntryHeader(JobDeclaration declaration, string reference = null)
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.CH_MessageType = "CUS";
			entryHeader.CH_BGMReference = reference ?? "REF";
			return entryHeader;
		}
	}
}
