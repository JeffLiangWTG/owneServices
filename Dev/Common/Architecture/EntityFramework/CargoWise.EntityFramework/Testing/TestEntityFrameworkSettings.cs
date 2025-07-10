#if DEBUG

using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using Moq;

namespace CargoWise.EntityFramework.Testing
{
	public sealed class TestEntityFrameworkSettings : IEntityFrameworkSettings, IZSqlSaverConfiguration, IDisposable
	{
		TestEntityFrameworkSettings()
		{
			var oldSettings = ObjectFactory.Get<IEntityFrameworkSettings>();
			substitution = ObjectFactory.Substitute<IEntityFrameworkSettings>(this);

			CachedTables = oldSettings.CachedTables;
			DefaultToForceSeek = false;
			MultiSearchSeparator = ",";
			ParameterizeInsertAndUpdateStatements = true;
			ConcatenateMultipleFetchHintTypes = false;
			LightValidationEnabled = true;
			RunSelectTopNAsRowNumberQuery = false;
			ReportCrossThreadFactoryAccess = true;
			SuppressDbFilesHealthCheckNotificationsForHostedSystems = true;
			RunSelectStatementsWithLocking = true;
			DebugBusinessObjectType = "";
			ApplyOptionRecompile = true;
			FieldsToLiteralize = string.Empty;
			TVPRule = new TVPRule("5,10,20,50,100");
			RowsToPostPerSqlStatement = 500;
			MaximumParametersPerFetchHint = 64;
			ApplyIsNotNullToJoinOnFK = true;

			// this allows effects to be immediate
			ParameterSettingsCache.RefreshCache();
		}

		public bool ApplyIsNotNullToJoinOnFK { get; set; }
		public bool DefaultToForceSeek { get; set; }

		public string MultiSearchSeparator { get; set; }
		public int MaximumParametersPerFetchHint { get; set; }
		public bool ConcatenateMultipleFetchHintTypes { get; set; }
		public bool IsWeb { get; set; }
		public bool IsWebService { get; set; }
		public bool LightValidationEnabled { get; set; }
		public bool ReportConcurrencyErrors { get; set; }
		public bool RunSelectStatementsWithLocking { get; set; }
		public bool RunSelectTopNAsRowNumberQuery { get; set; }
		public bool SuppressDbFilesHealthCheckNotificationsForHostedSystems { get; set; }

		public bool ApplyOptionRecompile { get; set; }
		public bool ParameterizeInsertAndUpdateStatements { get; set; }
		public bool ReportCrossThreadFactoryAccess { get; set; }
		public string CachedTables { get; set; }
		public int RegistryRefreshFrequencyInSeconds { get; set; }
		public string DebugBusinessObjectType { get; set; }
		public string FieldsToLiteralize { get; set; }
		public TVPRule TVPRule { get; set; }
		public int RowsToPostPerSqlStatement { get; set; }
		public int UberFactoryTimeoutPeriod { get; set; }

		public static TestEntityFrameworkSettings Get()
		{
			return ObjectFactory.Get<IEntityFrameworkSettings>() as TestEntityFrameworkSettings ?? new TestEntityFrameworkSettings();
		}

		public static Mock<IServiceProvider> GetMockServiceProvider()
		{
			var serviceProvider = GlobalServiceProvider.Instance;
			var mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(x => serviceProvider.GetService(x));
			mockServiceProvider.Setup(x => x.GetService(typeof(IZSqlSaverConfiguration))).Returns(Get());
			return mockServiceProvider;
		}

		public void Dispose()
		{
			substitution.Dispose();
			ParameterSettingsCache.RefreshCache();
		}

		readonly IDisposable substitution;
	}
}

#endif
