using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class ExitSummaryMainPanelFieldsUserControlTest : TestCase
	{
		public void TestBrokerCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.BrokerCodeFindBox);
		}

		public void TestCertificateDropEdit()
		{
			AssertType<ZDropEdit>(control.CertificateDropEdit);
		}

		public void TestDeclEmailAddrTextBox()
		{
			AssertType<ZTextBox>(control.DeclEmailAddrTextBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ExitSummaryMainPanelFieldsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ExitSummaryMainPanelFieldsUserControl control;
	}
}
