#if DEBUG

using System;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Accounting.Business.Testing
{
	public static class TestMockObjectCreator
	{
		public static Mock<IGlobalAccountingCountryFactory> CreateAndRegisterIGlobalAccountingCountryFactory(IAccountingCountryFactory countryFactory = null)
		{
			return Enterprise.MasterFiles.Business.Testing.TestMockObjectCreator.CreateAndRegisterIGlobalAccountingCountryFactory(countryFactory);
		}

		public static Mock<IInstanceProvider<TInterface>> CreateIAccountingCountryFactoryProvidingInterface<TInterface>(TInterface interfaceImplementation)
		{
			var countryFactoryMock = new Mock<IAccountingCountryFactory>().As<IInstanceProvider<TInterface>>();
			countryFactoryMock.Setup(x => x.Get()).Returns(interfaceImplementation);

			return countryFactoryMock;
		}

		public static Mock<TInterface> CreateIAccountingCountryFactoryImplementingInterface<TInterface>() where TInterface : class
		{
			return Enterprise.MasterFiles.Business.Testing.TestMockObjectCreator.CreateIAccountingCountryFactoryImplementingInterface<TInterface>();
		}

		public static void CreateAndRegisterIComplianceInfoElectronicInvoicing(string authRecordType = "", string governmentNumberColumnName = "")
		{
			var complianceInfoEInvoicingMock = Enterprise.MasterFiles.Business.Testing.TestMockObjectCreator.CreateAndRegisterIComplianceInfoElectronicInvoicing();
			complianceInfoEInvoicingMock.Setup(x => x.GetAccTransactionHeaderAuthorisationRecordType()).Returns(authRecordType);
			complianceInfoEInvoicingMock.Setup(x => x.GetGovernmentAllocatedNumberColumnName()).Returns(governmentNumberColumnName);
		}

		public static Mock<IAccountingCountryComplianceGlobalFactory> CreateAndRegisterIAccountingCountryComplianceGlobalFactory()
		{
			return MasterFiles.Business.Testing.TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory();
		}

		public static Mock<TInterface> SetupFeatureInterface<TInterface>(this Mock<IAccountingCountryComplianceGlobalFactory> mockFactory) where TInterface : class
		{
			return MasterFiles.Business.Testing.TestMockObjectCreator.SetupFeatureInterface<TInterface>(mockFactory);
		}

		public static Mock<ITaxFrameworkConfigurationHelper> CreateAndRegisterITaxFrameworkConfigurationHelper()
		{
			return MasterFiles.Business.Testing.TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
		}

		#region IEInvoicingEligibilityDecider

		/// <summary>
		/// Adds IEInvoicingEligibilityDecider to the country factory.
		/// </summary>
		public static Mock<IAccountingCountryFactory> WithEInvoicingEligibilityDecider(this Mock<IAccountingCountryFactory> mockFactory, bool isEligible)
			=> WithEInvoicingEligibilityDecider(mockFactory, isEligible, out _);

		/// <summary>
		/// Adds IEInvoicingEligibilityDecider to the country factory, and captures the mock for .Verify() calls.
		/// </summary>
		public static Mock<IAccountingCountryFactory> WithEInvoicingEligibilityDecider(this Mock<IAccountingCountryFactory> mockFactory, bool isEligible, out Mock<IEInvoicingEligibilityDecider> eligibilityDecider)
		{
			eligibilityDecider = new Mock<IEInvoicingEligibilityDecider>();
			eligibilityDecider.Setup(x => x.IsTransactionEligible(It.IsAny<IEInvoicingEligibilityLiteTransaction>())).Returns(isEligible);

			mockFactory.As<IInstanceProvider<IEInvoicingEligibilityDecider>>().Setup(x => x.Get()).Returns(eligibilityDecider.Object);
			return mockFactory;
		}

		#endregion

		#region IEReportingComplianceDateReachedProvider

		/// <summary>
		/// Adds IEInvoicingPreEligibilityProvider to the country factory.
		/// </summary>
		public static Mock<IAccountingCountryFactory> WithEInvoicingPreEligibilityProvider(
			this Mock<IAccountingCountryFactory> mockFactory,
			bool? canEvaluateByComplianceDate = null,
			bool? canEvaluateByTransaction = null) => WithEInvoicingPreEligibilityProvider(mockFactory: mockFactory, preEligibilityProvider: out _, canEvaluateByComplianceDate: canEvaluateByComplianceDate, canEvaluateByTransaction: canEvaluateByTransaction);

		/// <summary>
		/// Adds IEInvoicingPreEligibilityProvider to the country factory, and captures the mock for .Verify() calls.
		/// </summary>
		public static Mock<IAccountingCountryFactory> WithEInvoicingPreEligibilityProvider(
			this Mock<IAccountingCountryFactory> mockFactory,
			out Mock<IEInvoicingPreEligibilityProvider> preEligibilityProvider,
			bool? canEvaluateByComplianceDate = null,
			bool? canEvaluateByTransaction = null)
		{
			preEligibilityProvider = new Mock<IEInvoicingPreEligibilityProvider>();
			if (canEvaluateByTransaction.HasValue)
			{
				preEligibilityProvider
					.Setup(x => x.CanEvaluateByTransaction(It.IsAny<AccTransactionHeader>()))
					.Returns(canEvaluateByTransaction.Value);
			}

			if (canEvaluateByComplianceDate.HasValue)
			{
				preEligibilityProvider
					.Setup(x => x.CanEvaluateByComplianceDate(
						It.IsAny<AccTransactionHeader>(),
						It.IsAny<DateTime>()))
					.Returns(canEvaluateByComplianceDate.Value);
			}
			else
			{
				preEligibilityProvider
					.Setup(x => x.CanEvaluateByComplianceDate(
						It.IsAny<AccTransactionHeader>(),
						It.IsAny<DateTime>()))
					.Returns((AccTransactionHeader transaction, DateTime complianceDate) =>
					{
						var provider = new EInvoicingPreEligibilityProvider();
						return provider.CanEvaluateByComplianceDate(transaction: transaction, complianceDate: complianceDate);
					});
			}

			mockFactory.As<IInstanceProvider<IEInvoicingPreEligibilityProvider>>().Setup(x => x.Get()).Returns(preEligibilityProvider.Object);
			return mockFactory;
		}

		#endregion

		#region IEInvoicingActionProvider

		/// <summary>
		/// Adds no-op IEInvoicingActionProvider.OnEvaluateEligibilityAndQueue() to the country factory, and captures the mock for .Verify() calls.
		/// </summary>
		public static Mock<IAccountingCountryFactory> WithEInvoicingActionProvider_OnEvaluateEligibilityAndQueue(this Mock<IAccountingCountryFactory> mockFactory, out Mock<IEInvoicingActionProvider> actionProvider)
		{
			actionProvider = new Mock<IEInvoicingActionProvider>();
			mockFactory.As<IInstanceProvider<IEInvoicingActionProvider>>().Setup(x => x.Get()).Returns(actionProvider.Object);
			return mockFactory;
		}

		/// <summary>
		/// Adds IEInvoicingActionProvider.OnEvaluateEligibilityAndQueue() to the country factory.
		/// </summary>
		public static Mock<IAccountingCountryFactory> WithEInvoicingActionProvider_OnEvaluateEligibilityAndQueue(this Mock<IAccountingCountryFactory> mockFactory, Action<AccTransactionHeader> action)
		{
			var actionProvider = new Mock<IEInvoicingActionProvider>();
			actionProvider.Setup(x => x.OnEvaluateEligibilityAndQueue(It.IsAny<AccTransactionHeader>())).Callback(action);

			mockFactory.As<IInstanceProvider<IEInvoicingActionProvider>>().Setup(x => x.Get()).Returns(actionProvider.Object);
			return mockFactory;
		}

		#endregion

		#region IEInvoicingPivotStatusProvider

		/// <summary>
		/// Adds IEInvoicingPivotStatusProvider.OnEvaluateEligibilityAndQueue() to the country factory.
		/// </summary>
		public static Mock<IAccountingCountryFactory> WithEInvoicingPivotStatusProvider_OnEvaluateEligibilityAndQueue(this Mock<IAccountingCountryFactory> mockFactory, out Mock<IEInvoicingPivotStatusProvider> statusProvider)
		{
			statusProvider = new Mock<IEInvoicingPivotStatusProvider>();
			mockFactory.As<IInstanceProvider<IEInvoicingPivotStatusProvider>>().Setup(x => x.Get()).Returns(statusProvider.Object);
			return mockFactory;
		}

		#endregion
	}
}

#endif
