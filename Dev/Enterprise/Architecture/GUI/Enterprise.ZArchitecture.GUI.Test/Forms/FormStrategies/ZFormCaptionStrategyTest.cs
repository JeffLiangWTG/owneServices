using System.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.FormStrategies
{
	public class ZFormCaptionStrategyTest : TransactionedTestCase
	{
		public void TestConstructorOfKFormWhenGetGlobalFormTopCaptionThrowsException()
		{
			try
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);

				var currentUser = Env.CurrentUser;

				var environmentMock = new Mock<IEnvironment>();
				environmentMock.Setup(x => x.GlobalFormTopCaption).Returns(() => { throw SqlExceptionBuilder.CreateSqlException(0, 0, 64, Db.ServerName, "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)", null, 0, new Win32Exception(0)); });
				environmentMock.Setup(x => x.CurrentUser).Returns(currentUser);

				var envMock = new Mock<IEnv>();
				envMock.Setup(x => x.Instance).Returns(environmentMock.Object);

				using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
				using (new ZFormCaptionStrategy())
				{
					var ex = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
							SqlExceptionBuilder.CreateSqlError(2, 0, 11, Db.ServerName, "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)", "", 0)));
					var handler = new TopLevelExceptionHandler();
					Assert(handler.HandleSqlException(ex, ex));
				}
			}
			finally
			{
				Globals.SetIsUnitTestingProductionFunctionality(false);
			}
		}

		public void TestFormText()
		{
			using (new ZFormCaptionStrategy())
			{
				EnvProxy.Instance.Registry.GlobalFormTopCaption = "GlobalFormTopCaption";
				EnvProxy.Instance.Registry.ShowDatabaseName = true;
				EnvProxy.Instance.Registry.ShowBranchName = true;
				EnvProxy.Instance.Registry.ShowCompanyName = true;
				EnvProxy.Instance.Registry.ShowDepartmentName = true;
				EnvProxy.Instance.Registry.ShowUserName = true;

				using (var testForm = new KForm())
				{
					AssertEquals("FormText", "", testForm.Text);
					AssertEquals("FullCaption", "", testForm.TextIncludingSuffix);
					AssertEquals(
						"CurrentExtraCaption",
						"GlobalFormTopCaption - DB: " + Db.DatabaseName + " - Branch: " + EnvProxy.Instance.CurrentBranch.Name +
						" - Company: " + EnvProxy.Instance.CurrentCompany.Name + " - Department: " + EnvProxy.Instance.CurrentDepartment.Description +
						" - User: " + EnvProxy.Instance.CurrentUser.FullName,
						ZFormCaptionStrategy.GetGlobalFormTopCaption_ForTest(testForm));

					testForm.Text = "FormText";

					AssertEquals("Text", "FormText", testForm.Text);
					AssertEquals(
						"FullCaption",
						"FormText - GlobalFormTopCaption - DB: " + Db.DatabaseName + " - Branch: " + EnvProxy.Instance.CurrentBranch.Name +
						" - Company: " + EnvProxy.Instance.CurrentCompany.Name + " - Department: " + EnvProxy.Instance.CurrentDepartment.Description +
						" - User: " + EnvProxy.Instance.CurrentUser.FullName,
						testForm.TextIncludingSuffix);
					AssertEquals(
						"CurrentExtraCaption",
						"GlobalFormTopCaption - DB: " + Db.DatabaseName + " - Branch: " + EnvProxy.Instance.CurrentBranch.Name +
						" - Company: " + EnvProxy.Instance.CurrentCompany.Name + " - Department: " + EnvProxy.Instance.CurrentDepartment.Description +
						" - User: " + EnvProxy.Instance.CurrentUser.FullName,
						ZFormCaptionStrategy.GetGlobalFormTopCaption_ForTest(testForm));

					EnvProxy.Instance.Registry.GlobalFormTopCaption = "";
					EnvProxy.Instance.Registry.ShowDatabaseName = false;
					EnvProxy.Instance.Registry.ShowBranchName = false;
					EnvProxy.Instance.Registry.ShowCompanyName = false;
					EnvProxy.Instance.Registry.ShowDepartmentName = false;
					EnvProxy.Instance.Registry.ShowUserName = false;

					AssertEquals("Text", "FormText", testForm.Text);
					AssertEquals(
						"FullCaption",
						"FormText - GlobalFormTopCaption - DB: " + Db.DatabaseName + " - Branch: " + EnvProxy.Instance.CurrentBranch.Name +
						" - Company: " + EnvProxy.Instance.CurrentCompany.Name + " - Department: " + EnvProxy.Instance.CurrentDepartment.Description +
						" - User: " + EnvProxy.Instance.CurrentUser.FullName,
						testForm.TextIncludingSuffix);
					AssertEquals("FullCaption", "FormText", testForm.Text);
					AssertEquals("CurrentExtraCaption", "", ZFormCaptionStrategy.GetGlobalFormTopCaption_ForTest(testForm));
				}
			}
		}
	}
}
