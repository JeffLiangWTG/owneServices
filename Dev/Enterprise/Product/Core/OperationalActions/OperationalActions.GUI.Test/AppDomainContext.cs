using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	[Serializable]
	sealed class AppDomainContext
	{
		[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public static AppDomainContext ExtractFromCurrentAppDomain()
		{
			return new AppDomainContext();
		}

		[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		AppDomainContext()
		{
			baseSourcePath = TestCase.BaseSourcePath;

			databaseName = Db.DatabaseName;
			serverName = Db.ServerName;

			isUserInteractive = Globals.IsUserInteractive;

			loginName = Env.CurrentUser.LoginName;
			currentBranch = Env.CurrentBranch.PK;
			currentDepartment = Env.CurrentDepartment.PK;

			isRunningOnDAT = TestingState.IsRunningOnDAT;
			testMethod = TestingState.CurrentTestMethod;
		}

		[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public void ApplyToAppDomain()
		{
			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
			Initialiser.InitialiseWinForms();

			if (baseSourcePath != null)
			{
				TestCase.BaseSourcePath = baseSourcePath;
			}

			Db.InitializeDatabaseDetails(serverName, databaseName);
			Db.Connection.EnsureIsOpen();

			Globals.IsUserInteractive = isUserInteractive;

			var user = (IUser)new BusinessObjectFactory().LoadFromNaturalKey(ObjectFactory.GetType(nameof(IGlbStaff)), GlbStaffSchema.GS_LoginName, loginName);
			Env.SetUserContext(new UserContext(user, currentBranch, currentDepartment));

			TestingState.IsRunningOnDAT = isRunningOnDAT;
			TestingState.CurrentTestMethod = testMethod;
		}

		readonly string baseSourcePath;

		readonly string databaseName;
		readonly string serverName;

		readonly bool isUserInteractive;

		readonly string loginName;
		readonly Guid currentBranch;
		readonly Guid currentDepartment;

		readonly bool isRunningOnDAT;
		readonly MethodInfo testMethod;
	}
}
