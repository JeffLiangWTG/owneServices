using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Billing.Integration;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Customs.Universal;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts
{
	[TestsSubclassesOf(typeof(BaseStlScript))]
	abstract class BaseStlScriptTest : TestCaseWithFactory
	{
		public void TestTransactionRun_DisableSendingNonBilledItemsAsUsage()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			{
				SetupRefSysConfig(false);
				ExecuteTransactionRun(GetExpectedTransactionType(false));
			}
		}

		public void TestTransactionRun_EnableSendingNonBilledItemsAsUsage()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			{
				SetupRefSysConfig(true);
				ExecuteTransactionRun(GetExpectedTransactionType(true));
			}
		}

		public void TestTransactionRun_SendNonBilledItemsAsUsageOnInternalSystems()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: true))
			{
				ExecuteTransactionRun(GetExpectedTransactionType(true));
			}
		}

		void SetupRefSysConfig(bool enableSendingNonbilledItemsAsUsage)
		{
			var configType = Factory.New<Enterprise.Integration.Customs.Shared.IRefSysConfigType>();
			configType.ZRT_ConfigCode = RefStlScriptKeys.CollectUsageTransactionKey;
			configType.ZRT_Description = "Sending nonbilled items as usage transaction";
			configType.ZRT_LongDescription = "Sending nonbilled items as usage transaction";
			var config = Factory.New<Enterprise.Integration.Customs.Shared.IRefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = RefStlScriptKeys.CollectUsageTransactionKey;
			config.ZRC_StringValue = enableSendingNonbilledItemsAsUsage ? string.Empty : "UsageTransaction";
			config.ZRC_BitValue = enableSendingNonbilledItemsAsUsage;
			config.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			Factory.Save();
		}

		void ExecuteTransactionRun(Type expectedType)
		{
			if (string.IsNullOrEmpty(ScriptToTest.MaxCW1Version))
			{
				PrepareTestData();
				var transactions = Run();
				AssertEquals("Wrong transaction", expectedType, transactions.First().GetType());
				AssertTransactions(transactions);
			}
			else
			{
				Assert("We cannot run the old versions of the collectors on the new database, as they are not compatible", true);
			}
		}

		public void TestIsMandatoryForMilestones()
		{
			var item = ScriptToTest;
			AssertEquals("Incorrect value for property IsMandatoryForMilstones", IsMandatoryForMilestones, item.IsMandatoryForMilestones);
		}

		public abstract void TestCollectorType();
		public abstract void TestScriptTextContainsComment();
		public abstract void TestFieldMaxSizes();

		protected virtual bool IsMandatoryForMilestones => true;

		IEnumerable<IStlTransaction> Run()
		{
			if (ScriptToTest.StlGrain == StlDataGrain.Daily)
			{
				var allTransactions = new List<IStlTransaction>();
				var rangeStartInclusive = TestDateTimeRange.StartDateTimeInclusive;
				while (rangeStartInclusive < TestDateTimeRange.EndDateTimeExclusive)
				{
					var rangeEndExclusive = BaseDateTimeRange.GetMinDateTimeValue(rangeStartInclusive.AddDays(1), TestDateTimeRange.EndDateTimeExclusive);
					allTransactions.AddRange(ScriptToTest.Run(new RecurringRange(rangeStartInclusive, rangeEndExclusive)));
					rangeStartInclusive = rangeEndExclusive;
				}

				return allTransactions;
			}

			return ScriptToTest.Run(TestDateTimeRange);
		}

		protected abstract void AssertTransactions(IEnumerable<IStlTransaction> transactions);
		protected abstract void PrepareTestData();
		protected abstract IDateTimeRange TestDateTimeRange { get; }

		virtual protected IStlScript ScriptToTest
		{
			get
			{
				Type[] types = Array.Empty<Type>();
				var constructorInfo = TestedTypeHelper.GetTestedType(GetType()).GetConstructor(types);
				if (scriptToTest_DoNotUseDirectly == null && constructorInfo != null)
				{
					scriptToTest_DoNotUseDirectly = (BaseStlScript)constructorInfo.Invoke(Array.Empty<object>());
				}
				return scriptToTest_DoNotUseDirectly;
			}
		}

		BaseStlScript scriptToTest_DoNotUseDirectly;

		protected bool EnableSendingNonBilledItemsAsUsage => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem() || (bool)new RefSysConfig.Loader(Factory).GetBoolValue(RefStlScriptKeys.CollectUsageTransactionKey);

		protected bool CollectedAsUsageTransaction => EnableSendingNonBilledItemsAsUsage && !IsMandatoryForMilestones;

		protected virtual Type GetExpectedTransactionType(bool enableSendingNonbilledItemsAsUsage)
		{
			if (!enableSendingNonbilledItemsAsUsage)
			{
				return typeof(BillingTransaction);
			}

			return !ScriptToTest.IsMandatoryForMilestones ? typeof(UsageTransaction) : typeof(BillingTransaction);
		}
	}
}
