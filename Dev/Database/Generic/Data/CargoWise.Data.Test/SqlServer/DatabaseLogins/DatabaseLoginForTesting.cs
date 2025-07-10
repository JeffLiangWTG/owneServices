using System;
using System.Collections.Generic;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Moq;
namespace CargoWise.Data.Testing
{
	sealed class DatabaseLoginForTesting : DatabaseLogin<CustomCredentials>
	{
		readonly Guid activeSecretId;
		readonly CustomCredentials credentials;

		public Mock<IProtectedDataService> pdsMock;
		public Mock<IProtectedDataStateService> stateServiceMock;
		public Mock<IProtectedDataServiceFactory> pdsFactoryMock;
		public DatabaseLoginForTesting(AdminConnection connection)
				: this(connection, false)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Types need to be explicitly specified for the service container")]
		public DatabaseLoginForTesting(AdminConnection connection, bool isImpersonateEnterpriseDbUser)
			: base(connection)
		{
			pdsMock = new Mock<IProtectedDataService>();
			stateServiceMock = new Mock<IProtectedDataStateService>();
			pdsFactoryMock = new Mock<IProtectedDataServiceFactory>();

			var services = new ServiceCollection();
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			services.ConfigureProtectedDataAdministrationServices();
			services.ConfigureProtectedDataSqlServerAdministrationServices();

			services.AddSingleton<IProtectedDataServiceFactory>((sp) => pdsFactoryMock.Object);
			services.AddSingleton<IProtectedDataStateService>((sp) => stateServiceMock.Object);
			services.AddSingleton<ProtectedDataServiceCapabilities, ProtectedDataServiceCapabilitiesWithCustom>();
			services.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, CargowisePDSAdministrationSqlContextManager>();
			services.AddTransient<ProtectedDataManager<CustomCredentials>, ApplicationLoginManager<CustomCredentials>>();

			services.AddTransient<LoginRepairService, LoginRepairService>();

			base.pdsServiceProvider = services.BuildServiceProvider();

			this.isImpersonateEnterpriseDbUser = isImpersonateEnterpriseDbUser;

			activeSecretId = Guid.NewGuid();
			credentials = new CustomCredentials("DatabaseTestLogin", "123abc");
			var secret = new ProtectedData<CustomCredentials>(activeSecretId, "Custom", DateTimeOffset.Now, DateTimeOffset.Now, credentials);
			pdsMock.Setup(x => x.LoadSecret(It.Is<Guid>((value) => value == activeSecretId))).Returns(secret);
			stateServiceMock.Setup(x => x.GetActiveProtectedDataId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(activeSecretId);
			pdsFactoryMock.Setup(x => x.CreateSystemService(It.IsAny<string>(), It.IsAny<string>())).Returns(pdsMock.Object);
		}

		public override string LoginSuffix => "DatabaseTestLoginSuffix";

		public override string LoginName => credentials.UserName;

		public override IEnumerable<DbRole> DbLevelRoles => new[] { new DatabaseTestRole() };

		protected override IEnumerable<string> DbLevelPermissions => new[] { "EXECUTE" };

		public override bool IsImpersonateEnterpriseDbUser => isImpersonateEnterpriseDbUser;

		readonly bool isImpersonateEnterpriseDbUser;
	}
}
