using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	public class GlobalEDIInterchangeCreatorTest : GEIEDIInterchangeCreatorTest
	{
		public override void TestIsBillingSupported()
		{
			var processor = new InterchangeProcessorWithIsBillingSupportedExposed();
			Assert("Billing transactions are created for E-invoicing.", processor.GetIsBillingSupported());
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			Assert("Each batch contains only one invoice, so partial failure of a batch is not possible for a Mexico, Argentina or Uruguay company", true);
		}

		#region Implementation

		protected override string CountryCode => CountryCodes.Zimbabwe;

		protected override string ExpectedServicePoint => "XHUB_ZZZ_EINVOICING";

		[ExpectNoExceptions]
		public void TestGetEInvoicingServicePointParameters()
		{
			var suffix = "SUFFIX";
			var mockGlobalInterchange = new MockGlobalEDIInterchangeCreatorEInvoicingBatch(GlbCompany.CurrentCompany, CountryFactoryMock.Object, () => new MockAccEInvoiceBatchToGEIConverter(), suffix: suffix);

			var servicePoint = mockGlobalInterchange.EInvoicingServicePoint;
			CountryFactoryMock.Verify(x => x.GetEInvoicingServicePoint(suffix), Times.Once);
		}

		public void TestGetDefaultCommunicationsMode_ReturnsEHub_WhenObjectFactoryConfiguredForEHub()
		{
			CountryFactoryMock.Setup(x => x.CommunicationTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);
			var globalInterchange = GetInterchangeProcessor(GlbCompany.CurrentCompany);
			AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, globalInterchange.CommunicationsMode.EK_CommunicationsTransport);
		}

		public void TestGetDefaultCommunicationsMode_ReturnsDirectXT_WhenObjectFactoryConfiguredForDirectXT()
		{
			CountryFactoryMock.Setup(x => x.CommunicationTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface);
			var globalInterchange = GetInterchangeProcessor(GlbCompany.CurrentCompany);
			AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, globalInterchange.CommunicationsMode.EK_CommunicationsTransport);
		}

		public void TestGetDefaultCommunicationsMode_Throws_ForUnsupportedTransport()
		{
			CountryFactoryMock.Setup(x => x.CommunicationTransport).Returns("BAD");
			var globalInterchange = GetInterchangeProcessor(GlbCompany.CurrentCompany);
			var ex = AssertExceptionThrown<NotSupportedException>(() => _ = globalInterchange.CommunicationsMode.EK_CommunicationsTransport);
			AssertEquals(ex.Message, "CountryFactory CommunicationTransport 'BAD' is not supported. Only 'HUB' and 'XTT' are supported.");
		}

		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company)
		{
			return new MockGlobalEDIInterchangeCreatorEInvoicingBatch(GlbCompany.CurrentCompany, CountryFactoryMock.Object, () => new MockAccEInvoiceBatchToGEIConverter());
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null, AccEInvoicingBatch batchWithErrors = null)
		{
			var interchangeCreator = new MockGlobalEDIInterchangeCreatorEInvoicingBatch(GlbCompany.CurrentCompany, CountryFactoryMock.Object, converter, forceThisErrorWhileCreatingEDIInterchange);
			interchangeCreator.BatchForWhichErrorWillBeForced = batchWithErrors;
			return interchangeCreator;
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

		class InterchangeProcessorWithIsBillingSupportedExposed : GlobalEDIInterchangeCreator
		{
			public InterchangeProcessorWithIsBillingSupportedExposed()
				: base(GlbCompany.CurrentCompany, null, ZString.Empty)
			{ }

			public bool GetIsBillingSupported() => IsBillingSupported;
		}
		#endregion

		public class MockGlobalEDIInterchangeCreatorEInvoicingBatch : GlobalEDIInterchangeCreator, IEDIIntechangeCreatorForTest
		{
			public MockGlobalEDIInterchangeCreatorEInvoicingBatch(GlbCompany company, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory, Func<IAccEInvoiceBatchToGEIConverter> converter, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null, string suffix = "")
				: base(company, countryEInvoicingObjectFactory, suffix)
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

		Mock<ICountryEInvoicingObjectFactory> CountryFactoryMock
		{
			get
			{
				if (countryFactoryMock == null)
				{
					countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
					countryFactoryMock.Setup(x => x.CountryCode).Returns("Netlandia");
					countryFactoryMock.Setup(x => x.GetEInvoicingServicePoint(It.IsAny<ZString>())).Returns(ExpectedServicePoint);
					countryFactoryMock.Setup(x => x.CommunicationTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);

					var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
					globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns(countryFactoryMock.Object);

					ObjectFactory.Substitute(globalFactoryMock.Object);
				}

				return countryFactoryMock;
			}
		}
		Mock<ICountryEInvoicingObjectFactory> countryFactoryMock;
	}
}
