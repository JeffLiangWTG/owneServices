using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Integration.Licensing;

namespace Enterprise.ResourceStrings.Business
{
	public static class TranslationFeedbackConfiguration
	{
		public static bool IsMasterDatabase
		{
			get
			{
#if DEBUG
				if (isMasterDatabaseForTest)
				{
					return true;
				}
#endif
				return Db.Connection.ServerName == TranslationFeedbackMasterInfo.DatabaseServer && Db.Connection.CurrentDatabase == TranslationFeedbackMasterInfo.DatabaseName;
			}
		}

#if DEBUG
		static bool isMasterDatabaseForTest;

		public static IDisposable EnableIsMasterDatabaseForTest()
		{
			if (isMasterDatabaseForTest)
			{
				throw new InvalidOperationException("isMasterDatabaseForTest already enabled");
			}
			isMasterDatabaseForTest = true;
			var rego = ObjectFactory.Get<IProductRegistration>();
			var savedEnterpriseCode = rego.KeyForTest.EnterpriseCodeForTest;
			var savedServerCode = rego.KeyForTest.ServerCodeForTest;
			rego.KeyForTest.EnterpriseCodeForTest = TranslationFeedbackMasterInfo.EnterpriseCode;
			rego.KeyForTest.ServerCodeForTest = TranslationFeedbackMasterInfo.DatabaseCode;
			return new DisposableAction(delegate
				{
					isMasterDatabaseForTest = false;
					rego.KeyForTest.EnterpriseCodeForTest = savedEnterpriseCode;
					rego.KeyForTest.ServerCodeForTest = savedServerCode;
				});
		}
#endif
	}
}
