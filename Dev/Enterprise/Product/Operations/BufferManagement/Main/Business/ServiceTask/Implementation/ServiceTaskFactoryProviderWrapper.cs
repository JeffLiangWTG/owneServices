using System;
using System.Runtime.CompilerServices;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class ServiceTaskFactoryProviderWrapper
	{
		public ServiceTaskFactoryProviderWrapper(ILogger logger, string serviceTaskCode = null, bool mustCatchZSaveConcurrencyException = true)
		{
			Logger = logger;
			this.serviceTaskCode = serviceTaskCode;
			this.mustCatchZSaveConcurrencyException = mustCatchZSaveConcurrencyException;
		}

		public void Save(bool createNew, bool swallowSaveConcurrencyException = true, [CallerMemberName] string callerMemberName = "")
		{
			SaveCore(createNew, swallowSaveConcurrencyException, callerMemberName);
		}

		protected virtual void SaveCore(bool createNew, bool swallowSaveConcurrencyException, string callerMemberName)
		{
			if (factoryProvider != null)
			{
				try
				{
					if (!factoryProvider.Current.IsValidationSuspended)
					{
						factoryProvider.Current.SuspendValidation();
					}

					if (createNew)
					{
						factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
						SetFactoryNameForDebugging(callerMemberName);
						factoryProvider.Current.SuspendValidation();
					}
					else
					{
						factoryProvider.SaveCurrentAndUpdateRecordCounts();
					}
				}
				catch (ZSaveConcurrencyException ex) when (mustCatchZSaveConcurrencyException)
				{
					LogZSaveConcurrencyError(ex.ToString());

					if (createNew)
					{
						Recreate();
					}

					if (!swallowSaveConcurrencyException)
					{
						throw;
					}
				}
			}
		}

		public void CreateNewWithoutSave([CallerMemberName] string callerMemberName = "")
		{
			FactoryProvider.CreateNewAndReclaimMemoryWithoutSave();
			SetFactoryNameForDebugging(callerMemberName);
			FactoryProvider.Current.SuspendValidation();
		}

		public void Recreate()
		{
			factoryProvider?.CreateNewAndReclaimMemoryWithoutSave();
		}

		public void LogZSaveConcurrencyError(string exceptionMessage)
		{
			Logger.Log(LogType.Warning, "Concurrency error: " + exceptionMessage); // Service Task Logging
		}

		#region Data Access Members Added Temporarily

		// This member is added to the interface just because some of the BM service tasks access the current factory directly.
		// Later if we decide to refactor all the BM service tasks and make them abstract from the database access layer, this member will be able to be removed.

		public BusinessObjectFactory Current => FactoryProvider.Current;

		// This member is added to the interface just because TransferRuleRunnerBase and some other classes accesses the factory provider directly.
		// Later if we decide to refactor that classes and when all data access logic from TransferRuleRunnerBase is extracted to another class, this member will be able to be removed.

		public BusinessObjectFactoryProvider InnerProvider => FactoryProvider;

		#endregion

		#region Implementation

		readonly string serviceTaskCode;

		#region FactoryProvider

		BusinessObjectFactoryProvider FactoryProvider => factoryProvider ?? (factoryProvider = CreateFactoryProvider(CreateStartingFactory()));

		BusinessObjectFactoryProvider factoryProvider;

		protected BusinessObjectFactoryProvider CreateFactoryProvider(BusinessObjectFactory startingFactory)
		{
			startingFactory.SuspendValidation();

			if (!string.IsNullOrEmpty(serviceTaskCode))
			{
				startingFactory.ServiceContainer.AddService(new ServiceTaskCodeService(serviceTaskCode));
			}

			return new ServiceTaskBusinessObjectFactoryProvider(startingFactory);
		}

		protected virtual BusinessObjectFactory CreateStartingFactory()
		{
			return new BusinessObjectFactory { NameForDebugging = GetType().Name };
		}

		#endregion

		#region Secondary Server Factory Provider

		public BusinessObjectFactoryProvider GetSecondaryServerFactoryProvider(DbConnection secondaryServerConnection)
		{
			var startingFactory = new ReadOnlyBusinessObjectFactory(secondaryServerConnection);
			return CreateFactoryProvider(startingFactory);
		}

		#endregion

		#region Creating New Factories

		void SetFactoryNameForDebugging(string callerMemberName)
		{
			factoryProvider.Current.NameForDebugging = FormattableString.Invariant($"{GetType().Name}.{callerMemberName}");
		}

		#endregion

		#region Handling Save Concurrency Errors

		readonly bool mustCatchZSaveConcurrencyException;

		public ILogger Logger { get; }

		#endregion

		#endregion
	}
}
