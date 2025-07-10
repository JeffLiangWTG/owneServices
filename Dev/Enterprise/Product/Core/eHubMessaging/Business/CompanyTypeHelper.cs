using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eHubMessaging.Business
{
	public static class CompanyTypeHelper
	{
		public static bool IsClientEnterpriseCode()
		{
			return !ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem();
		}

		public static bool IsWiseTechGlobalInternalSystem()
		{
			return ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem();
		}

		public static bool IsProductionDatabase()
		{
			return ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production
				&& !Globals.IsTest && !Globals.IsDebugMode;
		}

		public static bool IsProductionSystem()
		{
			try
			{
				return CompanyTypeHelper.IsClientEnterpriseCode() && CompanyTypeHelper.IsProductionDatabase();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return false;
			}
		}
	}
}
