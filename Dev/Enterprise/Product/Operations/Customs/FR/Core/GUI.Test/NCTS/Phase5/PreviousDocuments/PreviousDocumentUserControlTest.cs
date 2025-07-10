using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.GUI.NCTS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Testing
{
	sealed class PreviousDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestReferenceNumberCodeFindBox()
		{
			var referenceNumberCodeFindBox = control.ReferenceNumberCodeFindBox;
			AssertType<ZCodeFindBox>(referenceNumberCodeFindBox);
			AssertEquals("BindTo", nameof(PreviousDocument.CSI_ReferenceNumber), referenceNumberCodeFindBox.BindTo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new PreviousDocumentUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		PreviousDocumentUserControl control;
	}
}
