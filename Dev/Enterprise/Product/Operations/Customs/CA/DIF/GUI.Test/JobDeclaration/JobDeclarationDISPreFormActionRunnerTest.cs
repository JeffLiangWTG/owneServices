using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.CA.DIF.GUI.Testing
{
	sealed class JobDeclarationDISPreFormActionRunnerTest : TestCaseWithFactory
	{
		public void TestExecute()
		{
			var disHostMock = new Mock<IDISHost>();
			disHostMock.Setup(m => m.NeedToDoPreFormAction()).Returns(false);
			var runner = new JobDeclarationDISPreFormActionRunner(disHostMock.Object);
			AssertEquals(true, runner.Execute());
			disHostMock.VerifyAll();

			disHostMock.Setup(m => m.NeedToDoPreFormAction()).Returns(true);
			disHostMock.Setup(m => m.DoPreFormAction()).Returns(true);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertEquals(true, runner.Execute());
			disHostMock.VerifyAll();

			disHostMock.Setup(m => m.NeedToDoPreFormAction()).Returns(true);
			disHostMock.Setup(m => m.DoPreFormAction()).Returns(false);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertEquals(false, runner.Execute());
			disHostMock.VerifyAll();

			disHostMock.Setup(m => m.NeedToDoPreFormAction()).Returns(true);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			AssertEquals(true, runner.Execute());
			disHostMock.VerifyAll();

			disHostMock.Setup(m => m.NeedToDoPreFormAction()).Returns(true);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AssertEquals(false, runner.Execute());
			disHostMock.VerifyAll();
		}
	}
}
