using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthElectronicMessagingProcessingQueryResultServiceTask))]
	public class KoreaSouthElectronicMessagingProcessingQueryResultServiceTaskTest : KoreaSouthElectronicMessagingProcessingServiceTaskTest<KoreaSouthElectronicMessagingProcessingQueryResultServiceTask>
	{
		public override void TestHostedServiceTimeProperties()
		{
			var attributes = typeof(KoreaSouthElectronicMessagingProcessingQueryResultServiceTask).Assembly.GetCustomAttributes<HostedServiceAttribute>();
			var attribute = attributes.First(x => x.Code == "EKQ");
			AssertEquals("1hour", attribute.DefaultScheduleRunEvery);
			AssertEquals("5minutes", attribute.MinimumPeriod);
			AssertEquals("1hour", attribute.MaximumPeriod);
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AddAdditionalInformationForCompany(company);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.Debtor1);
			AddCountrySpecificCredentialsForCompanyOrBranch(company.FirstActiveBranch);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", CurrencyOfTestTransaction, 10m, TestObjectCreator.Creditor1, CurrencyOfTestTransaction, 10m, TestObjectCreator.Debtor1);
				charge.JR_AT_SellGSTRate = TestObjectCreator.ServiceTax.PK;
				Factory.Save();

				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", company.LocalCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
				SaveARInvoice(arInvoice, EInvoicingPivotActionType.StatusCheck);

				AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 1);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var serviceTask = GetCountrySpecificServiceTask();
					var logger = InitialiseAndRunTaskSchedule(serviceTask);

					var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_GB, company.FirstActiveBranch.PK));
					AssertEquals("One interchange should be created", 1, interchanges.Length);

					var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
					AssertEquals("One message should be created", 1, messages.Length);
					var message = messages[0];

					var xmlSerializer = new XmlSerializer(typeof(GlobalElectronicInvoicing));
					using (var reader = new StringReader(message.EM_MessageText))
					{
						var eInvoice = (GlobalElectronicInvoicing)xmlSerializer.Deserialize(reader);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem), $"{CountryCode} Electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType), ExpectedMessageTypeForGenerateInvoiceRequest, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber), "2", eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode), company.FirstActiveBranch.GB_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode), company.GC_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified), true, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified);

						AssertCountrySpecificGEIMessageContent(eInvoice);
					}
				}
			}
		}

		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			Assert("Not applicable to Status Check Service Task.", true);
		}

		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			Assert("Not applicable to Status Check Service Task.", true);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override KoreaSouthElectronicMessagingProcessingQueryResultServiceTask GetCountrySpecificServiceTask() => new KoreaSouthElectronicMessagingProcessingQueryResultServiceTask();

		protected override void AssertProcessMessageSubType(params ARInvoice[] invoiceList)
		{
			AssertEReportingStatus("GEN E-Reporting status will still be Queued", EInvoicingPivotState.Queued, invoiceList[0], EInvoicingPivotActionType.Submit);
			AssertEReportingStatus("GEQ E-Reporting status will be Sent", EInvoicingPivotState.Sent, invoiceList[1], EInvoicingPivotActionType.StatusCheck);
		}

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest;

		protected override string ExpectedMessageTypeForGenerateCancellationRequest => KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest;

		protected override string ExpectedPivotActionType => EInvoicingPivotActionType.StatusCheck;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => Array.Empty<int>();

		protected override void PrepareTestDataBeforeRunServiceTask(InvoicingBase invoicingBase)
		{
			PrepareTestDataForGEQRequest(invoicingBase);
		}
	}
}
