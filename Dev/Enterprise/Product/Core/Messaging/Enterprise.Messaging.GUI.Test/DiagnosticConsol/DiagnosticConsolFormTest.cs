using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Testing
{
	[TestedType(typeof(DiagnosticConsolForm))]
	sealed class DiagnosticConsolFormTest : ZFormBasherTest
	{
		public void TestStartButton_Click_DoRegistryAndCertificateChecksReturnsEmptyList()
		{
			var diagnosticConsolMock = new Mock<DiagnosticConsol>(Factory);
			diagnosticConsolMock
				.Setup<List<RegistryAndCertificateCheckResult>>(x => x.RegistryAndCertificateChecks())
				.Returns(new List<RegistryAndCertificateCheckResult>());
			using (var form = new DiagnosticConsolForm(diagnosticConsolMock.Object))
			{
				form.Show();
				try
				{
					form.StartButton.PerformClick();
				}
				catch
				{
				}

				diagnosticConsolMock
					.Protected()
					.Verify<ZGuid>("CreateTestMessage", Times.Once(), ItExpr.IsAny<BusinessObjectFactory>(), ItExpr.IsAny<ZString>());
				Assert(true);
			}
		}

		public void TestStartButton_Click_DoRegistryAndCertificateChecksReturnsCriticalLevelList()
		{
			var diagnosticConsolMock = new Mock<DiagnosticConsol>(Factory);
			diagnosticConsolMock
				.Setup(x => x.RegistryAndCertificateChecks())
				.Returns(new List<RegistryAndCertificateCheckResult>() { new("1", StabilityResultLevel.Critical), new("2", StabilityResultLevel.Warning) });
			using (var form = new DiagnosticConsolForm(diagnosticConsolMock.Object))
			{
				form.Show();
				try
				{
					form.StartButton.PerformClick();
				}
				catch
				{
				}

				diagnosticConsolMock
					.Protected()
					.Verify<ZGuid>("CreateTestMessage", Times.Never(), ItExpr.IsAny<BusinessObjectFactory>(), ItExpr.IsAny<ZString>());
				Assert(true);
			}
		}

		public void TestStartButton_Click_DoRegistryAndCertificateChecksReturnsHealthOrWarningList()
		{
			var diagnosticConsolMock = new Mock<DiagnosticConsol>(Factory);
			diagnosticConsolMock
				.Setup(x => x.RegistryAndCertificateChecks())
				.Returns(new List<RegistryAndCertificateCheckResult>() { new ("1", StabilityResultLevel.Warning) });
			using (var form = new DiagnosticConsolForm(diagnosticConsolMock.Object))
			{
				form.Show();
				try
				{
					form.StartButton.PerformClick();
				}
				catch
				{
				}

				diagnosticConsolMock
					.Protected()
					.Verify<ZGuid>("CreateTestMessage", Times.Once(), ItExpr.IsAny<BusinessObjectFactory>(), ItExpr.IsAny<ZString>());
				Assert(true);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new DiagnosticConsolForm(new Enterprise.Messaging.Business.Testing.DiagnosticConsolHelper(Factory));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "DiagnosticMessagesListBox")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}
	}
}
