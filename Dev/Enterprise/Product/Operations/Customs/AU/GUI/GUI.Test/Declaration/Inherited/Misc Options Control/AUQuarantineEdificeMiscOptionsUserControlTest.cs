using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUQuarantineEdificeMiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestChangeControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			using (var control = new AUQuarantineEdificeMiscOptionsUserControlForTest())
			{
				control.JobDeclaration = declaration;
				Assert(control.CertificateRequestCheckBox.Visible);
				Assert(control.JE_UseOwnerRefAsQuarantineRefCheckBox.Visible);
				Assert(control.EXDOCOptionsGroupBox.Visible);
				var invoice = declaration.Invoices.AddNew();
				Assert(control.CertificateRequestCheckBox.Visible);
				Assert(control.JE_UseOwnerRefAsQuarantineRefCheckBox.Visible);
				Assert(control.EXDOCOptionsGroupBox.Visible);
				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				Assert(!control.CertificateRequestCheckBox.Visible);
				Assert(control.JE_UseOwnerRefAsQuarantineRefCheckBox.Visible);
				Assert(control.EXDOCOptionsGroupBox.Visible);
				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
				Assert(control.CertificateRequestCheckBox.Visible);
				Assert(control.JE_UseOwnerRefAsQuarantineRefCheckBox.Visible);
				Assert(control.EXDOCOptionsGroupBox.Visible);
				declaration.Invoices.DeleteAll();
				Assert(control.CertificateRequestCheckBox.Visible);
				Assert(control.JE_UseOwnerRefAsQuarantineRefCheckBox.Visible);
				Assert(control.EXDOCOptionsGroupBox.Visible);
			}
		}

		sealed class AUQuarantineEdificeMiscOptionsUserControlForTest : AUQuarantineEdificeMiscOptionsUserControl
		{
			internal ZCheckBox CertificateRequestCheckBox => certificateRequestCheckBox;
			internal ZCheckBox JE_UseOwnerRefAsQuarantineRefCheckBox => jE_UseOwnerRefAsQuarantineRefCheckBox;
			internal ZGroupBox EXDOCOptionsGroupBox => eXDOCOptionsGroupBox;
		}
	}
}
