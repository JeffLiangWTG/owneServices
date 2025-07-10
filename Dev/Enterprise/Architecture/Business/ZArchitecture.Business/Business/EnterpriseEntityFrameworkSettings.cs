using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public class EnterpriseEntityFrameworkSettings : IEntityFrameworkSettings, IZSqlSaverConfiguration
	{
		public bool ApplyIsNotNullToJoinOnFK
		{
			get { return EnvProxy.Instance.Registry.ApplyIsNotNullToJoinOnFK; }
		}

		public bool DefaultToForceSeek
		{
			get { return EnvProxy.Instance.Registry.DefaultToForceSeek; }
		}

		public string MultiSearchSeparator
		{
			get { return EnvProxy.Instance.Registry.MultiSearchSeparator; }
		}

		public int MaximumParametersPerFetchHint
		{
			get { return EnvProxy.Instance.Registry.MaximumParametersPerFetchHint; }
		}

		public bool ConcatenateMultipleFetchHintTypes
		{
			get { return EnvProxy.Instance.Registry.ConcatenateMultipleFetchHintTypes; }
		}

		public string DebugBusinessObjectType
		{
			get { return EnvProxy.Instance.Registry.DebugBusinessObjectType; }
		}

		public bool LightValidationEnabled
		{
			get { return EnvProxy.Instance.Registry.LightValidationEnabled; }
		}

		public bool SuppressDbFilesHealthCheckNotificationsForHostedSystems
		{
			get { return EnvProxy.Instance.Registry.SuppressDbFilesHealthCheckNotificationsForHostedSystems; }
		}

		public bool ApplyOptionRecompile
		{
			get { return EnvProxy.Instance.Registry.ApplyOptionRecompile; }
		}

		public bool ParameterizeInsertAndUpdateStatements
		{
			get { return EnvProxy.Instance.Registry.ParameterizeInsertAndUpdateStatements; }
		}

		public bool ReportConcurrencyErrors
		{
			get { return ObjectFactory.Get<ISystemDataRegistry>().ReportConcurrencyErrors; }
		}

		public bool RunSelectTopNAsRowNumberQuery
		{
			get { return EnvProxy.Instance.Registry.RunSelectTopNAsRowNumberQuery; }
		}

		public bool ReportCrossThreadFactoryAccess
		{
			get
			{
				try
				{
					return EnvProxy.Instance.Registry.ReportCrossThreadFactoryAccess;
				}
				catch (SqlException)
				{
					return true;
				}
			}
		}

		public string FieldsToLiteralize
		{
			get { return EnvProxy.Instance.Registry.FieldsToLiteralize; }
		}

		public TVPRule TVPRule
		{
			get { return EnvProxy.Instance.Registry.TVPRule; }
		}

		public bool IsWeb
		{
			get { return Globals.IsWeb; }
		}

		public bool IsWebService
		{
			get { return Globals.IsWebService; }
		}

		public string CachedTables
		{
			get { return ObjectFactory.Get<ISystemDataRegistry>().CachedTables; }
		}

		static int depth;

		public int RegistryRefreshFrequencyInSeconds
		{
			get
			{
				if (depth > 0)
				{
					return int.MaxValue;
				}
				else
				{
					Interlocked.Increment(ref depth);
					var result = ObjectFactory.Get<ISystemDataRegistry>().RegistryRefreshFrequencyInSeconds;
					Interlocked.Decrement(ref depth);
					return result;
				}
			}
		}

		public int RowsToPostPerSqlStatement
		{
			get { return EnvProxy.Instance.Registry.ZSqlSaverRowsToPostPerSqlStatement; }
		}

		public int UberFactoryTimeoutPeriod
		{
			get
			{
				return EnvProxy.Instance.Registry.UberFactoryTimeoutPeriod;
			}
		}
	}
}
