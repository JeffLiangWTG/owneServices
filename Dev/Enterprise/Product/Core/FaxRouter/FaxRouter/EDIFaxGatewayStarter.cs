using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.FaxRouter.EventLogging;
using Enterprise.FaxRouter.Manager;
using Enterprise.Initialisation;

namespace Enterprise.FaxRouter
{
	public static class EDIFaxGatewayStarter
	{
		[STAThread]
		static void Main()
		{
			bool isFirstInstance;
			Mutex singleInstanceMutex = new Mutex(true, @"Global\EDIFaxGatewayInstance", out isFirstInstance);

			try
			{
				if (isFirstInstance)
				{
					Start();
				}
			}
			finally
			{
				singleInstanceMutex.Close();
			}
		}

		static void Start()
		{
			bool work = true;
			while (work)
			{
				try
				{
					Initialiser.InitialiseWinForms();

					Db.InitializeDatabaseDetails(Constants.ENTERPRISE_SERVER_NAME, Constants.ENTERPRISE_DATABASE_NAME);

					object dummy = Db.Connection;
					Login();
					Application.Run(new EDIFaxGatewayForm());
					work = false;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (!FaxDataModule.ExceptionCausedByNetworkProblems(ex))
					{
						EventLog.AddErrorEntry("EDIFaxGateway", ex);
						work = false;
					}
					else
					{
						Thread.Sleep(10000);
					}
				}
			}
		}

		static void Login()
		{
			Guid branchPK = (Guid)Db.Connection.ExecuteScalar("SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'SYD'");
			Guid departmentPK = (Guid)Db.Connection.ExecuteScalar("SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_Code = 'BRN'");

			//Use existing login from FaxEntEb. The service controller account cannot use for login anymore.
			Env.LoginController.LoginLocation(Env.LoginController.LoginUser("FaxAdmin", "FaxAdmin"), branchPK, departmentPK);
		}
	}
}
