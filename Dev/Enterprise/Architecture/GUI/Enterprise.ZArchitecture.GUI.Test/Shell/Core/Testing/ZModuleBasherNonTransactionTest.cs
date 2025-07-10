using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.ZArchitecture.Modules.Testing
{
#if !WINZOR
	sealed class ZModuleBasherNonTransactionTest : DbSecurityNonTransactionedTest
	{
		[UseSnapshotProtection]
		public void TestRunningOnMTAThread()
		{
			var unregisteredHandlers = SetupTestRunningOnMTAThread();
			var processedModules = new HashSet<string>();

			foreach (var clientSpecificAssemblyFileName in ClientHookLoader.Instance.GetAllClientSpecificAssemblyFileNames())
			{
				var clientSpecificAssembly = ClientHookLoader.Instance.GetAssemblyFromFileName(clientSpecificAssemblyFileName);
				using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(clientSpecificAssembly))
				{
					foreach (var moduleId in ModuleIDs.AllIncludingClientModules.Except(exceptionModuleIds).Where(m => !ModuleIdHasNoModule(m)))
					{
						var countryCode = ZModuleFactory.Instance.GetCountryOverridesRegisteredForModule(moduleId).FirstOrDefault();
						if (processedModules.Add(moduleId.ToString()))
						{
							TestRunningOnMTAThreadForModule(moduleId, countryCode);
						}
					}
				}
			}

			TearDownTestRunningOnMTAThread(unregisteredHandlers);

			ErrorReporter.Clear(); // to clear error 'Controller xxx does not support viewing hence does not allow creating viewing url'
		}

		static bool ModuleIdHasNoModule(ModuleIdentifier moduleId)
		{
			return moduleId == ModuleIDs.MENTSeries;
		}

		static void TestRunningOnMTAThreadForModule(ModuleIdentifier moduleId, string countryCode)
		{
			ShowFormUrlHandler handler = null;
			var message = string.Empty;

			try
			{
				Task.Factory.StartNew(() =>
				{
					AssertEquals($"GIVEN module {moduleId.ToString()} and thread is MTA", ApartmentState.MTA, System.Threading.Thread.CurrentThread.GetApartmentState());

					using (Db.DisposableActionForDbConnection())
					using (var testConnection = Db.NewAdminConnection())
					{
						var factory = new BusinessObjectFactory(testConnection);
						var mockedHandler = new Mock<ShowFormUrlHandler>() { CallBase = true };

						if (!countryCode.IsNullOrEmpty() && countryCode != StaticCurrentFetcher.Instance.CurrentCompany.GC_RN_NKCountryCode)
						{
							StaticCurrentFetcher.Instance.CurrentCompany.GC_RN_NKCountryCode = countryCode;
						}

						using (var mockModuleResult = ZModuleFactory.Instance.Create(moduleId))
						{
							AssertNotNull($"GIVEN module {moduleId.ToString()} is not null", mockModuleResult);
						}

						handler = mockedHandler.Object;
						var dummyNotInDatabase = factory.New<DummyWithDependentsBusinessObject>();

						EnterpriseUrlHandlerService.RegisterUrlHandler(handler);

						try
						{
							var url = handler.CreateWithoutApplicationContext(DummyControllerIDs.Dummy, dummyNotInDatabase.PK);
							message = "completed";
						}
						catch (Exception e)
						{
							message = e.Message;
						}
						finally
						{
							EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
						}
					}
				}).Wait(TimeSpan.FromMinutes(1));
			}
			finally
			{
				EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
			}

			AssertEquals($"GIVEN module {moduleId.ToString()}, WHEN creating url THEN should not throw exception i.e. 'The calling thread must be STA, because many UI components require this' ", "completed", message);
		}

		List<UrlHandler> SetupTestRunningOnMTAThread()
		{
			var unregisteredHandlers = new List<UrlHandler>();
			var handlers = EnterpriseUrlHandlerService.UrlHandlers;

			foreach (var handler in handlers)
			{
				unregisteredHandlers.Add(handler);
				EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
			}

			return unregisteredHandlers;
		}

		void TearDownTestRunningOnMTAThread(List<UrlHandler> unregisteredHandlers)
		{
			foreach (var handler in unregisteredHandlers)
			{
				EnterpriseUrlHandlerService.RegisterUrlHandler(handler);
			}
		}

		readonly ModuleIdentifier[] exceptionModuleIds = {
			ModuleIDs.NotAssigned, // throw exception ModuleNotAssignedIDException

			// Not registered in ModuleRegistration.Add(new ModuleInfo ...
			DummyModuleIDs.DummyWithExtendedDescription,
			ModuleIDs.DripMarketingFilterRuleEDI,
			ModuleIDs.DripMarketingFilterRuleHR,
			ModuleIDs.DripMarketingFilterRule,
			ModuleIDs.JobSailing,
			ModuleIDs.WhsStocktakeLine,
			ModuleIDs.HRJobApplication,
			ModuleIDs.TariffBulkChange,
			ModuleIDs.Customs.NestedDummy,
			ModuleIDs.Customs.InvoiceLine,
			ModuleIDs.Customs.CA.CusSCAOceanBill,
		};
	}
#endif
}
