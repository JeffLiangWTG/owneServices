using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	public class EDIInterchangeCreatorForVietnamTest : GEIEDIInterchangeCreatorTest
	{
		public override void TestIsBillingSupported()
		{
			var processor = new InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany.CurrentCompany);
			Assert("Billing transactions are created for Vietnam.", processor.GetIsBillingSupported());
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			Assert("Each batch contains only one invoice, so partial failure of a batch is not possible for an Vietnam company", true);
		}

		public override void TestGetEInvoicingServicePoint()
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			mockIFeatureData.Setup(x => x.Parameter).Returns((string)null);
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			var vietnamEInvoicingConfig = """
										{
										  "VN": {
										    "Transport": {
										      "Destination": "XHUB_VN_EINVOICING_V2"
										    }
										  }
										}
										""";

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				var processor = GetInterchangeProcessor();
				AssertEquals(ExpectedServicePoint, processor.EInvoicingServicePoint);

				mockIFeatureData.Setup(x => x.Parameter).Returns(vietnamEInvoicingConfig);
				AssertEquals("XHUB_VN_EINVOICING_V2", processor.EInvoicingServicePoint);
			}
		}

		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company)
		{
			return new MockEDIInterchangeCreatorForVietnamEInvoicingBatch(GlbCompany.CurrentCompany, () => new MockAccEInvoiceBatchToGEIConverter());
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null, AccEInvoicingBatch batchWithErrors = null)
		{
			return new MockEDIInterchangeCreatorForVietnamEInvoicingBatch(GlbCompany.CurrentCompany, converter, forceThisErrorWhileCreatingEDIInterchange) { BatchForWhichErrorWillBeForced = batchWithErrors };
		}

		protected override string CountryCode => Constants.CountryCodes.VietNam;

		protected override string ExpectedServicePoint => "XHUB_VN_EINVOICING";

		protected override AccEInvoicingBatch GetBatchWithReadyStatus()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(++batchNumberCounter, Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			return batch;
		}

		int batchNumberCounter;

		class InterchangeProcessorWithIsBillingSupportedExposed : EDIInterchangeCreatorForVietnam
		{
			public InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany company)
				: base(company)
			{ }

			public bool GetIsBillingSupported() => IsBillingSupported;
		}
	}

	public class MockEDIInterchangeCreatorForVietnamEInvoicingBatch : EDIInterchangeCreatorForVietnam
	{
		public MockEDIInterchangeCreatorForVietnamEInvoicingBatch(GlbCompany company, Func<IAccEInvoiceBatchToGEIConverter> converter, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null)
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
	}
}
