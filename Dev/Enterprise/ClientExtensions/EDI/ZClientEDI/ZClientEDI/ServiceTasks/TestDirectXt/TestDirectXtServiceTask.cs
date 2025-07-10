using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.xTMessaging.Business.ConfigurationProvider;
using Enterprise.xTMessaging.ServiceTasks;
using Enterprise.xTMessaging.Shared;
using Xware.Xt.Grpc.Config;

namespace Enterprise.Client.EDI.ServiceTasks.TestDirectXt
{
	public abstract class TestDirectXtServiceTask : DirectxTServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			Configuration = GetConfiguration();
			var branch = GlbBranch.GetFirstActiveBranch();

			using (Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				GetMessageProcessor().Process(Configuration, token);
			}
		}

		protected abstract override IInterchangeProcessor GetMessageProcessor();

		protected virtual Configuration GetConfiguration()
		{
			var result = new Configuration();
			var registrationKey = ObjectFactory.Get<IProductRegistration>()?.Key;
			if (registrationKey != null)
			{
				var passwordHash = CargoWise.eHub.Common.SHA512Encryptor.Encrypt(registrationKey.DatabaseNumber + registrationKey.Password);

				var refDatabase = new ReferenceDatabaseUtils(Db.Connection);
				result.Connect = refDatabase.RefSysConfigLoader.Load(XTServerServerTestKey, ZDateTime.Today)?.ZRC_StringValue ?? "";
				result.CA = refDatabase.RefSysConfigLoader.Load(XTServerCertificateTestKey, ZDateTime.Today)?.ZRC_StringValue ?? "";

				result.Application = new Application
				{
					URI = registrationKey.EnterpriseCode + registrationKey.ServerCode,
					Password = passwordHash
				};
			}

			return result.IsValid() ? result : null;
		}

		protected override bool IsProduction()
		{
			return false;
		}

		const string XTServerCertificateTestKey = "XTCATEST";
		const string XTServerServerTestKey = "XTSVRTEST";
	}
}
