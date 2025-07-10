using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForHungary))]
	public sealed class ElectronicMessagingProcessingServiceTaskForHungaryTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForHungary>
	{
		protected override ElectronicMessagingProcessingServiceTaskForHungary GetCountrySpecificServiceTask()
			=> new ElectronicMessagingProcessingServiceTaskForHungary();

		[TestDate(2020, 4, 29)]
		public void TestEDIInterchangeIsNotCreatedForCreditNoteWhenOriginalInvoiceIsNotSent()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			Helper.AddCustomsCodeForCountryIfMissing(company1.FirstActiveBranch.OrgProxy, CountryCodes.Hungary, "VAT");
			Helper.AddCustomsCodeForCountryIfMissing(TestObjectCreator.AALSHI, CountryCodes.Hungary, "VAT");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", company1.LocalCurrency, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
				Factory.Save();

				var arInvoicePivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertEquals("Invoice Pivot got created", 1, arInvoicePivots.Length);
				var arInvoicePivot = arInvoicePivots[0];
				AssertEquals("Invoice Pivot Status", EInvoicingPivotState.Queued, arInvoicePivot.AIP_Status);
				AssertEquals("Invoice Pivot Action Type", EInvoicingPivotActionType.Submit, arInvoicePivot.AIP_ActionType);

				var reverser = new ARInvoiceReversing(arInvoice as ARInvoice);
				reverser.Reverse();
				Factory.Save();

				var reversedInvoice = arInvoice.ReverseTransaction;
				var creditNote = Factory.Load<ARCreditNote>(reversedInvoice.PK);
				AssertEquals("Reversed AR Transaction type", TransactionTypes.CreditNote, creditNote.AH_TransactionType);

				var creditNotePivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, creditNote.PK));
				AssertEquals("Pivot got created", 1, creditNotePivots.Length);
				var creditNotePivot = creditNotePivots[0];
				AssertEquals("Pivot Status", EInvoicingPivotState.Queued, creditNotePivot.AIP_Status);
				AssertEquals("Pivot Action Type", EInvoicingPivotActionType.Cancel, creditNotePivot.AIP_ActionType);
				Factory.Save();

				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				CombineAssertions(logger.ToString(), () =>
				{
					var newFactory = new BusinessObjectFactory();
					creditNotePivots = newFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, creditNote.PK));
					AssertEquals("Credit Note Pivot exists", 1, creditNotePivots.Length);
					creditNotePivot = creditNotePivots[0];
					AssertEquals("Credit Note Pivot discarded, as original eInvoice has not been sent to HU govt.", EInvoicingPivotState.Discarded, creditNotePivot.AIP_Status);
					AssertEquals("Credit Note Pivot Action Type", EInvoicingPivotActionType.Cancel, creditNotePivot.AIP_ActionType);

					arInvoicePivots = newFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
					AssertEquals("Invoice Pivot exists", 1, arInvoicePivots.Length);
					arInvoicePivot = arInvoicePivots[0];
					AssertEquals("Invoice Pivot discarded, as it is reversed before sending to HU govt.", EInvoicingPivotState.Discarded, arInvoicePivot.AIP_Status);
					AssertEquals("Invoice Pivot Action Type", EInvoicingPivotActionType.Submit, arInvoicePivot.AIP_ActionType);

					var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company1.PK, EInvoicingBatchState.Discarded);
					AssertEDIInterchanges(new ZQuery(), 0, (interchangePK) => AssertEDIMessages(interchangePK, company1.FirstActiveBranch.PK, GlbDepartment.CurrentDepartment.PK, GetLinkedObjectIDs(batches)));
				});
			}
		}

		protected override void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany)
		{
			var credential = GlbCompanyExternalPasswordHUI.LoadForCompanyOrNew(Factory, branchWithCompany.Company);
			credential.SignatureKey = "meow";
			credential.ReplacementKey = "woof";
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing eInvoice)
		{
			// Approximate XML expected:
			//<GlobalElectronicInvoicing xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing">
			//  <Header>
			//    <ElectronicInvoiceBatchRequest>
			//      <MessagingSystem>Hungary NAV Online Invoicing System</MessagingSystem>
			//      <MessageType>GEN</MessageType>
			//      <BatchNumber>1</BatchNumber>
			//      <CompanyCode>HU1</CompanyCode>
			//      <BranchCode>B01</BranchCode>
			//      <SignKey>Base64 encrypted credential</SignKey>
			//      <ReplacementKey>Base64 encrypted credential</ReplacementKey>
			//      <IsProductionSystem>false</ReplacementKey>
			//    </ElectronicInvoiceBatchRequest>
			//  </Header>
			//  <Payload>Base64 encoded payload</Payload>
			//</GlobalElectronicInvoicing>

			var message = eInvoice.Header.ElectronicInvoiceBatchRequest;
			AssertEquals("MessagingSystem", "Hungary NAV Online Invoicing System", message.MessagingSystem);
			AssertEquals("MessageType", "GEN", message.MessageType);
			AssertEquals("BatchNumber", "1", message.BatchNumber);
			AssertEquals("CompanyCode", "HU1", message.CompanyCode);
			AssertEquals("BranchCode", "B01", message.BranchCode);
			AssertEquals("IsProductionSystem", false, message.IsProductionSystem);

			AssertEquals("meow", EInvoicingTestHelper.DecodePassword(message.SignKey));
			AssertEquals("woof", EInvoicingTestHelper.DecodePassword(message.ReplacementKey));

			var version = new EnterpriseInformationRetriever().VersionNumber;
			var expectedPayload = $"""
<?xml version="1.0" encoding="utf-8"?>
<ManageInvoiceRequest
	xmlns:common="http://schemas.nav.gov.hu/NTCA/1.0/common"
	xmlns="http://schemas.nav.gov.hu/OSA/3.0/api">
	<common:header>
		<common:requestId>$$requestIdReplaceMe$$</common:requestId>
		<common:timestamp>$$timestampReplaceMe$$</common:timestamp>
		<common:requestVersion>3.0</common:requestVersion>
		<common:headerVersion>1.0</common:headerVersion>
	</common:header>
	<common:user>
		<common:login></common:login>
		<common:passwordHash cryptoType="SHA-512"></common:passwordHash>
		<common:taxNumber>32323329</common:taxNumber>
		<common:requestSignature cryptoType="SHA3-512">$$requestSignatureReplaceMe$$</common:requestSignature>
	</common:user>
	<software>
		<softwareId>AU658947CARGOWISE1</softwareId>
		<softwareName>CargoWise</softwareName>
		<softwareOperation>ONLINE_SERVICE</softwareOperation>
		<softwareMainVersion>{version}</softwareMainVersion>
		<softwareDevName>Wisetech Global Limited</softwareDevName>
		<softwareDevContact>eInvoice.Hungary@WisetechGlobal.com</softwareDevContact>
		<softwareDevCountryCode>AU</softwareDevCountryCode>
		<softwareDevTaxNumber>41065894724</softwareDevTaxNumber>
	</software>
	<exchangeToken>$$exchangeTokenReplaceMe$$</exchangeToken>
	<invoiceOperations>
		<compressedContent>false</compressedContent>
		<invoiceOperation>
			<index>1</index>
			<invoiceOperation>CREATE</invoiceOperation>
			<invoiceData>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48SW52b2ljZURhdGEgeG1sbnM9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9kYXRhIiB4bWxuczpuczI9Imh0dHA6Ly9zY2hlbWFzLm5hdi5nb3YuaHUvT1NBLzMuMC9iYXNlIj48aW52b2ljZU51bWJlcj4wMDAwMTAwMDwvaW52b2ljZU51bWJlcj48aW52b2ljZUlzc3VlRGF0ZT4yMDEwLTAxLTAxPC9pbnZvaWNlSXNzdWVEYXRlPjxjb21wbGV0ZW5lc3NJbmRpY2F0b3I+ZmFsc2U8L2NvbXBsZXRlbmVzc0luZGljYXRvcj48aW52b2ljZU1haW4+PGludm9pY2U+PGludm9pY2VIZWFkPjxzdXBwbGllckluZm8+PHN1cHBsaWVyVGF4TnVtYmVyPjxuczI6dGF4cGF5ZXJJZD4zMjMyMzMyOTwvbnMyOnRheHBheWVySWQ+PC9zdXBwbGllclRheE51bWJlcj48c3VwcGxpZXJOYW1lPlRlc3QgQ29tcGFueSBOYW1lPC9zdXBwbGllck5hbWU+PHN1cHBsaWVyQWRkcmVzcz48bnMyOnNpbXBsZUFkZHJlc3M+PG5zMjpjb3VudHJ5Q29kZSAvPjxuczI6cG9zdGFsQ29kZT4wMDAwPC9uczI6cG9zdGFsQ29kZT48bnMyOmNpdHk+QWxleGFuZHJpYTwvbnMyOmNpdHk+PG5zMjphZGRpdGlvbmFsQWRkcmVzc0RldGFpbD4xODQgQm91cmtlIFJvYWQ8L25zMjphZGRpdGlvbmFsQWRkcmVzc0RldGFpbD48L25zMjpzaW1wbGVBZGRyZXNzPjwvc3VwcGxpZXJBZGRyZXNzPjwvc3VwcGxpZXJJbmZvPjxjdXN0b21lckluZm8+PGN1c3RvbWVyVmF0U3RhdHVzPk9USEVSPC9jdXN0b21lclZhdFN0YXR1cz48Y3VzdG9tZXJOYW1lPlRlc3QgQ29tcGFueSBOYW1lPC9jdXN0b21lck5hbWU+PGN1c3RvbWVyQWRkcmVzcz48bnMyOnNpbXBsZUFkZHJlc3M+PG5zMjpjb3VudHJ5Q29kZT5BVTwvbnMyOmNvdW50cnlDb2RlPjxuczI6cG9zdGFsQ29kZT4wMDAwPC9uczI6cG9zdGFsQ29kZT48bnMyOmNpdHk+QWxleGFuZHJpYTwvbnMyOmNpdHk+PG5zMjphZGRpdGlvbmFsQWRkcmVzc0RldGFpbD4xODQgQm91cmtlIFJvYWQ8L25zMjphZGRpdGlvbmFsQWRkcmVzc0RldGFpbD48L25zMjpzaW1wbGVBZGRyZXNzPjwvY3VzdG9tZXJBZGRyZXNzPjwvY3VzdG9tZXJJbmZvPjxpbnZvaWNlRGV0YWlsPjxpbnZvaWNlQ2F0ZWdvcnk+Tk9STUFMPC9pbnZvaWNlQ2F0ZWdvcnk+PGludm9pY2VEZWxpdmVyeURhdGU+MjAxMC0wMS0wMTwvaW52b2ljZURlbGl2ZXJ5RGF0ZT48Y3VycmVuY3lDb2RlPkVVUjwvY3VycmVuY3lDb2RlPjxleGNoYW5nZVJhdGU+MS4wMDAwMDA8L2V4Y2hhbmdlUmF0ZT48cGF5bWVudERhdGU+MjAxMC0wMS0wMTwvcGF5bWVudERhdGU+PGludm9pY2VBcHBlYXJhbmNlPkVMRUNUUk9OSUM8L2ludm9pY2VBcHBlYXJhbmNlPjwvaW52b2ljZURldGFpbD48L2ludm9pY2VIZWFkPjxpbnZvaWNlTGluZXM+PG1lcmdlZEl0ZW1JbmRpY2F0b3I+ZmFsc2U8L21lcmdlZEl0ZW1JbmRpY2F0b3I+PGxpbmU+PGxpbmVOdW1iZXI+MTwvbGluZU51bWJlcj48bGluZUV4cHJlc3Npb25JbmRpY2F0b3I+ZmFsc2U8L2xpbmVFeHByZXNzaW9uSW5kaWNhdG9yPjxsaW5lTmF0dXJlSW5kaWNhdG9yPlNFUlZJQ0U8L2xpbmVOYXR1cmVJbmRpY2F0b3I+PGxpbmVEZXNjcmlwdGlvbj5DaGFyZ2UgQ29kZSAxPC9saW5lRGVzY3JpcHRpb24+PGxpbmVBbW91bnRzTm9ybWFsPjxsaW5lTmV0QW1vdW50RGF0YT48bGluZU5ldEFtb3VudD4xMC4wMDwvbGluZU5ldEFtb3VudD48bGluZU5ldEFtb3VudEhVRj4xMC4wMDwvbGluZU5ldEFtb3VudEhVRj48L2xpbmVOZXRBbW91bnREYXRhPjxsaW5lVmF0UmF0ZSAvPjwvbGluZUFtb3VudHNOb3JtYWw+PC9saW5lPjwvaW52b2ljZUxpbmVzPjxpbnZvaWNlU3VtbWFyeT48c3VtbWFyeU5vcm1hbD48aW52b2ljZU5ldEFtb3VudD4xMC4wMDwvaW52b2ljZU5ldEFtb3VudD48aW52b2ljZU5ldEFtb3VudEhVRj4xMC4wMDwvaW52b2ljZU5ldEFtb3VudEhVRj48aW52b2ljZVZhdEFtb3VudD4wLjUzPC9pbnZvaWNlVmF0QW1vdW50PjxpbnZvaWNlVmF0QW1vdW50SFVGPjAuNTM8L2ludm9pY2VWYXRBbW91bnRIVUY+PC9zdW1tYXJ5Tm9ybWFsPjxzdW1tYXJ5R3Jvc3NEYXRhPjxpbnZvaWNlR3Jvc3NBbW91bnQ+MTAuNTM8L2ludm9pY2VHcm9zc0Ftb3VudD48aW52b2ljZUdyb3NzQW1vdW50SFVGPjEwLjUzPC9pbnZvaWNlR3Jvc3NBbW91bnRIVUY+PC9zdW1tYXJ5R3Jvc3NEYXRhPjwvaW52b2ljZVN1bW1hcnk+PC9pbnZvaWNlPjwvaW52b2ljZU1haW4+PC9JbnZvaWNlRGF0YT4=</invoiceData>
			<electronicInvoiceHash cryptoType="SHA3-512">$$invoiceHashReplaceMe$$</electronicInvoiceHash>
		</invoiceOperation>
	</invoiceOperations>
</ManageInvoiceRequest>
""";
			var actualPayload = Encoding.UTF8.GetString(Convert.FromBase64String(eInvoice.Payload)).Trim();
			this.AssertXMLEqualsIgnoreChildOrder("Expected payload (without invoice data)", expectedPayload.Trim(), actualPayload);
		}

		protected override ZString CountryCode => CountryCodes.Hungary;

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;

		protected override string ExpectedMessageTypeForGenerateCancellationRequest => "GEN";

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => "GEN";

		protected override string ExpectedMessagingSystem => "Hungary NAV Online Invoicing System";

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1 };
	}
}
