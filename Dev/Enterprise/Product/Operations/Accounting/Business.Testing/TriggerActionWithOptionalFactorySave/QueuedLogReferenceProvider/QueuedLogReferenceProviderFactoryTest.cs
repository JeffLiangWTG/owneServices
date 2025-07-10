using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	class QueuedLogReferenceProviderFactoryTest : TestCase
	{
		public void TestGetReferenceProviders_DefaultProvider()
		{
			var type = typeof(WorkflowTriggerActionTypeConstants.Codes);

			var actionsWithDefaultProvider = type.GetFields(BindingFlags.Public | BindingFlags.Static)
									 .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
									 .Select(field => (string)field.GetRawConstantValue())
									 .Where(actionCode => actionCode != WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies)
									 .ToList();

			foreach (var action in actionsWithDefaultProvider)
			{
				var provider = QueuedLogReferenceProviderFactory.GetReferenceProvider(action);
				Assert($"When use ${action}, it should return default provider", provider.GetType() == typeof(QueuedLogReferenceProvider));
			}
		}

		public void TestGetReferenceProviders_ImportAPInvoicesFromOtherCompanies()
		{
			var provider = QueuedLogReferenceProviderFactory.GetReferenceProvider(WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies);
			Assert("When use ImportAPInvoicesFromOtherCompanies, it should return a provider with CompanyPKDecorator", provider.GetType() == typeof(CompanyPKDecorator));
			Assert("When use ImportAPInvoicesFromOtherCompanies, it should return a provider with CompanyPKDecorator", ((CompanyPKDecorator)provider).GetProvider_ForTestOnly().GetType() == typeof(QueuedLogReferenceProvider));
		}

		public void TestGetReferenceProviders_Logger()
		{
			var type = typeof(WorkflowTriggerActionTypeConstants.Codes);

			var allActions = type.GetFields(BindingFlags.Public | BindingFlags.Static)
									 .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
									 .Select(field => (string)field.GetRawConstantValue())
									 .ToList();

			foreach (var action in allActions)
			{
				var providerWithLogger = QueuedLogReferenceProviderFactory.GetReferenceProvider(action, new LoggerForTesting());
				var providerWithoutLogger = QueuedLogReferenceProviderFactory.GetReferenceProvider(action);

				if (providerWithLogger == null && providerWithoutLogger == null)
				{
					continue;
				}

				Assert("both providers should not be null", providerWithLogger != null && providerWithoutLogger != null);
				Assert("logger should not be null", GetBaseProvider(providerWithLogger).Logger_ForTestOnly != null);
				Assert("logger should be null", GetBaseProvider(providerWithoutLogger).Logger_ForTestOnly == null);
			}
		}

		public void TestKeepSyncWithLogSubscriber()
		{
			var allActionCode = typeof(WorkflowTriggerActionTypeConstants.Codes)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
				.Select(f => f.GetValue(null) as string);
			var baseDecoratorType = typeof(QueuedLogReferenceProviderDecorator);
			var providerType = typeof(QueuedLogReferenceProvider);
			var companyPKDecoratorType = typeof(CompanyPKDecorator);

			foreach (var actionCode in allActionCode)
			{
				var parameters = new Mock<QueuedLogParameters>();
				var workflowGetCount = 0;
				var companyPKGetCount = 0;
				parameters
					.SetupGet(m => m.WorkflowProvider)
					.Callback(() => workflowGetCount++);
				parameters
					.SetupGet(m => m.CompanyPK)
					.Callback(() => companyPKGetCount++);
				TriggerActionWithOptionalFactorySaveLogSubscriber.GetProcessorInstance_ForTestOnly(actionCode, parameters.Object);

				var provider = QueuedLogReferenceProviderFactory.GetReferenceProvider(actionCode);
				var providerChain = new List<Type>();
				do
				{
					providerChain.Add(provider.GetType());
					provider = (provider as QueuedLogReferenceProviderDecorator)?.GetProvider_ForTestOnly();
				} while (provider != null);

				if (workflowGetCount > 0)
				{
					Assert($"When action is {actionCode}, provider chain should contain QueuedLogReferenceProvider", providerChain.Contains(providerType));
				}

				if (companyPKGetCount > 0)
				{
					Assert($"When action is {actionCode}, provider chain should contain CompanyPKDecorator", providerChain.Contains(companyPKDecoratorType));
				}
			}
		}

		QueuedLogReferenceProvider GetBaseProvider(IQueuedLogReferenceProvider provider)
		{
			return provider as QueuedLogReferenceProvider ??
				GetBaseProvider((provider as CompanyPKDecorator).GetProvider_ForTestOnly());
		}
	}
}
