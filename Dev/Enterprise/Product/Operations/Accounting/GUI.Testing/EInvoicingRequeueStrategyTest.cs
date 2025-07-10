using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	public class EInvoicingRequeueStrategyTest : TestCaseWithFactory
	{
		public void TestIComplianceInfoEInvoicingGUIActionQueueReversedTransaction_IsNotImplemented()
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>())).Returns(mockICountryComplianceInfoBase.Object);

			var transaction = Factory.NewWithValidTestData<ARInvoice>();

			ObjectFactory.Substitute(mockICountryComplianceFactory.Object);
			var requeueStrategy = ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingRequeueStrategy();

			var isTransactionEnableToRequeue = requeueStrategy.IsTransactionEnableToRequeue(transaction);

			AssertEquals("There is no additional condition to check, transaction must be enable to requeue", RequeueDecision.Requeue, isTransactionEnableToRequeue);
		}

		[ExpectNoExceptions]
		public void TestGetICountryComplianceInfoBaseParameters()
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			ObjectFactory.Substitute(mockICountryComplianceFactory.Object);
			var requeueStrategy = ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingRequeueStrategy();

			mockICountryComplianceFactory.Verify(x => x.GetICountryComplianceInfoBase("AU"), Times.Once);

			var expectedCountry = Core.Constants.CountryCodes.Taiwan;
			GlbCompany.CurrentCompany.SetCountry(expectedCountry);

			requeueStrategy = ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingRequeueStrategy();

			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>())).Returns(mockICountryComplianceInfoBase.Object);
			mockICountryComplianceFactory.Verify(x => x.GetICountryComplianceInfoBase(expectedCountry), Times.Once);
		}

		public void TestGetReasonsToExcludeTransaction()
		{
			var requeueStrategy = ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingRequeueStrategy();

			var messages = requeueStrategy.GetReasonsToExcludeTransaction(new HashSet<RequeueDecision>() { RequeueDecision.RejectCancelled });

			AssertEquals(1, messages.Count);
			AssertEquals("Reversed transactions cannot be re-queued.", messages.FirstOrDefault());

			messages = requeueStrategy.GetReasonsToExcludeTransaction(new HashSet<RequeueDecision>() { RequeueDecision.RejectCancelled, RequeueDecision.RejectCancelled });
			AssertEquals(1, messages.Count);

			messages = requeueStrategy.GetReasonsToExcludeTransaction(new HashSet<RequeueDecision>() { RequeueDecision.Requeue });
			AssertEquals(0, messages.Count);
		}

		public void TestIsTransactionEnableToRequeue_ArgumentNotNull()
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
				.Returns(mockICountryComplianceInfoBase.Object);

			ObjectFactory.Substitute(mockICountryComplianceFactory.Object);
			var requeueStrategy = ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingRequeueStrategy();

			AssertExceptionThrown<ArgumentNullException>(() => requeueStrategy.IsTransactionEnableToRequeue(null));
		}

		public void TestIsTransactionEnableToRequeue_ReturnsRejectCancelled()
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceInfoBase.As<IComplianceInfoEInvoicingGUIActionQueueReversedTransaction>()
				.Setup(x => x.RejectReQueueForReversedTransaction(It.IsAny<string>())).Returns(true);

			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
				.Returns(mockICountryComplianceInfoBase.Object);

			ObjectFactory.Substitute(mockICountryComplianceFactory.Object);
			var requeueStrategy = ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingRequeueStrategy();

			var transaction = Factory.NewWithValidTestData<ARInvoice>();
			transaction.AH_IsCancelled = true;

			var isTransactionEnableToRequeue = requeueStrategy.IsTransactionEnableToRequeue(transaction);

			AssertEquals(RequeueDecision.RejectCancelled, isTransactionEnableToRequeue);

			mockICountryComplianceInfoBase.As<IComplianceInfoEInvoicingGUIActionQueueReversedTransaction>().Verify(x => x.RejectReQueueForReversedTransaction(transaction.AH_ComplianceSubType), Times.Once);
		}
	}
}
