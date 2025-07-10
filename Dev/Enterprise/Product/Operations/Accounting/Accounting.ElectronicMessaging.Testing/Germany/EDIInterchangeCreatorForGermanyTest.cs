using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Germany.Testing
{
	public class EDIInterchangeCreatorForGermanyTest : GEIEDIInterchangeCreatorTest
	{
		protected override string CountryCode => CountryCodes.Germany;

		protected override string ExpectedServicePoint => "XHUB_DE_EINVOICING";

		EInvoicingTestHelperForGermany eInvoicingTestHelperForGermany;

		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company)
		{
			return new MockEDIInterchangeCreatorForGermanyEInvoicingBatch(GlbCompany.CurrentCompany, () => new MockAccEInvoiceBatchToGEIConverter());
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null, AccEInvoicingBatch batchWithErrors = null)
		{
			var interchangeCreator = new MockEDIInterchangeCreatorForGermanyEInvoicingBatch(GlbCompany.CurrentCompany, converter, forceThisErrorWhileCreatingEDIInterchange);
			interchangeCreator.BatchForWhichErrorWillBeForced = batchWithErrors;
			return interchangeCreator;
		}

		public override void TestIsBillingSupported()
		{
			var processor = new InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany.CurrentCompany);
			Assert("Billing transactions are created for Germany.", processor.GetIsBillingSupported());
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			Assert("Each batch contains only one invoice, so partial failure of a batch is not possible for a Germany company", true);
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatus()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(Business.TestObjectCreator.GetRandomInt(1, 1000), Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			return batch;
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatusForSending(out string number)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arInvoice.AH_ConsolidatedInvoiceRef = "STEST00001";
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
			Factory.Save();

			number = arInvoice.AH_TransactionNum;
			return batch;
		}

		public void TestRoutingToDirectXtForB2B()
		{
			var featureControlManager = eInvoicingTestHelperForGermany.GetFeatureControlManagerMock(isB2bEnabled: true, isB2GinXTEnabled: false);
			using (ObjectFactory.Substitute(featureControlManager))
			{
				var logger = new TestServiceLogger();
				AccEInvoicingBatch batch;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
				{
					batch = GetBatchWithReadyStatus();
					var converter = () => new TransactionBatchToGEIConverterForGermanyForTest(createLID: false)
					{
						AdditionalTransactionInfoForGermany_ForTest = CreateAdditionalTransactionInfoForGermany(
							transactionCategory: OrgConstants.Category.Business)
					};
					var processor = GetInterchangeProcessor(converter);
					processor.Process(logger);
				}

				AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);
				var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
				AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
				var result = ediInterchanges[0];

				AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, result.EI_TransportType);
				AssertEquals(EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, result.EI_InterchangeType);
				AssertEquals("QUE", result.EI_Status);
				AssertEquals(ExpectedServicePoint, result.EI_To);
			}
		}

		public void TestRoutingToDirectXtForB2G()
		{
			var featureControlManager = eInvoicingTestHelperForGermany.GetFeatureControlManagerMock(isB2bEnabled: true, isB2GinXTEnabled: true);
			using (ObjectFactory.Substitute(featureControlManager))
			{
				var logger = new TestServiceLogger();
				AccEInvoicingBatch batch;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
				{
					batch = GetBatchWithReadyStatus();
					var converter = () => new TransactionBatchToGEIConverterForGermanyForTest(createLID: true)
					{
						AdditionalTransactionInfoForGermany_ForTest = CreateAdditionalTransactionInfoForGermany(
							transactionCategory: OrgConstants.Category.Government)
					};
					var processor = GetInterchangeProcessor(converter);
					processor.Process(logger);
				}

				AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);
				var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
				AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
				var result = ediInterchanges[0];

				AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, result.EI_TransportType);
				AssertEquals(EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, result.EI_InterchangeType);
				AssertEquals("QUE", result.EI_Status);
				AssertEquals(ExpectedServicePoint, result.EI_To);
			}
		}

		public void TestRoutingToEHubForB2G()
		{
			var featureControlManager = eInvoicingTestHelperForGermany.GetFeatureControlManagerMock(isB2bEnabled: true, isB2GinXTEnabled: false);
			using (ObjectFactory.Substitute(featureControlManager))
			{
				var logger = new TestServiceLogger();
				AccEInvoicingBatch batch;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
				{
					batch = GetBatchWithReadyStatus();
					var converter = () => new TransactionBatchToGEIConverterForGermanyForTest(createLID: true)
					{
						AdditionalTransactionInfoForGermany_ForTest = CreateAdditionalTransactionInfoForGermany(
							transactionCategory: OrgConstants.Category.Government)
					};
					var processor = GetInterchangeProcessor(converter);
					processor.Process(logger);
				}

				AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);
				var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
				AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
				var result = ediInterchanges[0];

				AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, result.EI_TransportType);
				AssertEquals(EInvoiceAPICommandList.Codes.Request, result.EI_InterchangeType);
				AssertEquals("HQU", result.EI_Status);
				AssertEquals(ExpectedServicePoint, result.EI_To);
			}
		}

		AdditionalTransactionInfoForGermanyEInvoice CreateAdditionalTransactionInfoForGermany(string transactionCategory = "GOV")
		{
			return new AdditionalTransactionInfoForGermanyEInvoice()
			{
				OriginalTransactionPK = Guid.NewGuid(),
				OriginalTransactionNumber = "1",
				VATRegistrationNum = "DE999999999",
				BankName = "Example Bank",
				AccountNumber = "1234567890",
				SwiftNumber = "ABCDEFG",
				IBANNumber = "GB94BARC10201530093459",
				TransactionCategory = transactionCategory ?? "GOV",
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			eInvoicingTestHelperForGermany = new EInvoicingTestHelperForGermany();
		}

		#region Inner Classes

		class InterchangeProcessorWithIsBillingSupportedExposed : EDIInterchangeCreatorForGermany
		{
			public InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany company)
				: base(company)
			{ }

			public bool GetIsBillingSupported() => IsBillingSupported;
		}

		class TransactionBatchToGEIConverterForGermanyForTest : TransactionBatchToGEIConverterForGermany
		{
			readonly bool createLID;

			public AdditionalTransactionInfoForGermanyEInvoice AdditionalTransactionInfoForGermany_ForTest { get; set; }

			public TransactionBatchToGEIConverterForGermanyForTest(bool createLID)
			{
				this.createLID = createLID;
			}

			protected override void PopulateAdditionalTransactionInfo(AccEInvoicingBatch batch)
			{
				if (AdditionalTransactionInfoForGermany_ForTest != null)
				{
					AdditionalTransactionInfoForGermany = AdditionalTransactionInfoForGermany_ForTest;
					return;
				}

				base.PopulateAdditionalTransactionInfo(batch);
			}

			protected override void PerformBeforeConvert(AccEInvoicingBatch batch)
			{
				base.PerformBeforeConvert(batch);

				// Ensure the LID = LeitwegID = Routing ID for German government institutions is present
				if (createLID)
				{
					var allowAllWrites = new AllowAllStrategy();
					var transaction = UniversalBatch.TransactionCollection[0];
					transaction.OrganizationAddress ??= new UniversalDataBuss.DataObjects.Universal.OrganizationAddress(allowAllWrites);

					var list = new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>()
					{
						new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
						{
							CountryOfIssue = new UniversalDataBuss.DataObjects.Universal.Country() { Code = CountryCodes.Germany },
							Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType() { Code = GermanyOrgCusCodeInfo.OrgCusCodes.LID },
							Value = "12345"
						}
					};
					transaction.OrganizationAddress.SetRegistrationNumberCollection(() => list);
				}
			}
		}

		class AllowAllStrategy : IDataObjectWriterStrategy
		{
			public bool IsAllowSet(string fieldName)
			{
				return true;
			}
		}

		#endregion
	}

	public class MockEDIInterchangeCreatorForGermanyEInvoicingBatch : EDIInterchangeCreatorForGermany, IEDIIntechangeCreatorForTest
	{
		public MockEDIInterchangeCreatorForGermanyEInvoicingBatch(GlbCompany company, Func<IAccEInvoiceBatchToGEIConverter> converter, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null)
			: base(company)
		{
			Converter = converter;
			ForceThisError = forceThisErrorWhileCreatingEDIInterchange;
		}

		Func<IAccEInvoiceBatchToGEIConverter> Converter { get; }

		ExceptionTypes? ForceThisError { get; }

		public AccEInvoicingBatch BatchForWhichErrorWillBeForced { get; set; }

		protected override void CreateEDIMessageAndInterchangeCore(TransactionBatchProcessContext batchProcessContext, INotifications notifications)
		{
			base.CreateEDIMessageAndInterchangeCore(batchProcessContext, notifications);
			EInvoicingTestHelper.ForceErrorWhileCreatingEDIInterchange(batchProcessContext, notifications, ForceThisError, BatchForWhichErrorWillBeForced);
		}

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => Converter();

		bool IEDIIntechangeCreatorForTest.AddErrorToEDIMessageNotesIfAny => AddErrorToEDIMessageNotesIfAny;
	}
}
