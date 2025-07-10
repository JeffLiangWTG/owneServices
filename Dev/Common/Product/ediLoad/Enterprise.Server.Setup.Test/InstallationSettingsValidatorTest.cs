using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Data;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Enterprise.Client.Common;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	public class InstallationSettingsValidatorTest : TestCase
	{
		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			Application.ConfigureApplicationServices();
		}

		public void TestCheckRegisterInstance()
		{
			var mocker = new MockRepository(MockBehavior.Loose);
			var services = new MoqMockServiceContainer(mocker)
			{
				MessageBox = mocker.Create<IMessageBoxProxy>(MockBehavior.Strict).Object,
				CargoWiseOneInstanceClass = new Mock<ICargoWiseOneInstanceClass>(MockBehavior.Strict).Object
			};

			var configuration = new SetupConfiguration();
			configuration.InstallationSettings.SelectedDatabase = SelectCurrentUsingDatabase(configuration.InstallationSettings.DatabaseList);
			configuration.Services = services;
			configuration.InstallationSettings.LicenseCode = "XXXZZZ";
			configuration.InstallationSettings.LogPath = @"X:\logs";

			Mock.Get(services.MessageBox).Setup(m => m.Show(null, "The specified Instance Name contains non alphanumeric characters.", "Invalid Instance Name", MessageBoxButtons.OK, MessageBoxIcon.Error)).Returns(DialogResult.OK);
			var args = new CancelEventArgs();
			new InstallationSettingsValidator(configuration, null).Validate(args);
			mocker.VerifyAll();
			Assert(args.Cancel);

			configuration.InstallationSettings.InstanceName = "X/Y/Z";
			Mock.Get(services.MessageBox).Setup(m => m.Show(null, "The specified Instance Name contains non alphanumeric characters.", "Invalid Instance Name", MessageBoxButtons.OK, MessageBoxIcon.Error)).Returns(DialogResult.OK);
			args = new CancelEventArgs();
			new InstallationSettingsValidator(configuration, null).Validate(args);
			mocker.VerifyAll();
			Assert(args.Cancel);

			configuration.InstallationSettings.InstanceName = "VALID";
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
			Mock.Get(services.MessageBox).Setup(m => m.Show(null, CargoWiseOneInstanceManager.SchemaModificationWarning + "\r\n\r\n" + "Would you like to continue?", "Warning: Active Directory Schema Modification", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)).Returns(DialogResult.No);
			args = new CancelEventArgs();
			new InstallationSettingsValidator(configuration, null).Validate(args);
			mocker.VerifyAll();
			Assert(args.Cancel);

			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
			Mock.Get(services.MessageBox).Setup(m => m.Show(null, CargoWiseOneInstanceManager.SchemaModificationWarning + "\r\n\r\n" + "Would you like to continue?", "Warning: Active Directory Schema Modification", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)).Returns(DialogResult.Yes);
			args = new CancelEventArgs();
			new InstallationSettingsValidator(configuration, null).Validate(args);
			mocker.VerifyAll();
			Assert(!args.Cancel);

			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(true);
			args = new CancelEventArgs();
			new InstallationSettingsValidator(configuration, null).Validate(args);
			Assert(!args.Cancel);
			mocker.VerifyAll();
		}

		DatabaseChoice SelectCurrentUsingDatabase(IEnumerable<DatabaseChoice> databaseList)
		{
			var usingDb = (string)Db.Connection.ExecuteScalar(@"SELECT @@SERVICENAME");

			foreach (var database in databaseList)
			{
				if (database.ToString().Equals($"SQL Server instance '{usingDb}'", StringComparison.OrdinalIgnoreCase))
				{
					return database;
				}
			}

			var errMsg = "The current using database must exist in the database list in the Windows registry." + Environment.NewLine;
			errMsg += "Current using database: " + usingDb + Environment.NewLine;
			errMsg += "Database list in the Windows registry:" + Environment.NewLine;

			foreach (var database in databaseList)
			{
				errMsg += database.InstanceName + Environment.NewLine;
			}

			Assert(errMsg, false);

			return null;
		}

		public void TestRegistryGetValue()
		{
			var value = (string)InstallationSettingsValidator.RegistryGetValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", "MSSQLSERVER", null);
			Assert("This local machine registry should not be empty", !string.IsNullOrEmpty(value));
		}
	}
}
