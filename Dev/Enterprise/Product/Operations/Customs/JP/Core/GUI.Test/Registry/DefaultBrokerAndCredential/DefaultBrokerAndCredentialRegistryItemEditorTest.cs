using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(DefaultBrokerAndCredentialRegistryItemEditor))]
	sealed class DefaultBrokerAndCredentialRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new DefaultBrokerAndCredentialRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((DefaultBrokerAndCredentialRegistryItemUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(DefaultBrokerAndCredentialRegistryItemUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new DefaultBrokerAndCredentialRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override object[] GetValidRegistryValues()
		{
			var defaultBrokerAndCredential = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
			defaultBrokerAndCredential.DefaultBrokerCode = "AN";
			defaultBrokerAndCredential.DefaultCredentialAIR = "Test2002";
			defaultBrokerAndCredential.DefaultCredentialSEA = "Test1001";
			Factory.Save();
			return new object[] { defaultBrokerAndCredential };
		}

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();

			var currentBranch = GlbBranch.CurrentBranch;
			currentBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			currentBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var currentCompanyPk = GlbCompany.CurrentCompany.PK;

			var broker = factory.NewWithValidTestData<GlbStaff>();
			GlbStaff.CurrentUser.GS_GB_HomeBranch = currentBranch.PK;
			broker.GS_GB_HomeBranch = currentBranch.PK;
			broker.GS_LoginName = "Ayachi";
			broker.GS_FullName = "Ayachi Ne";
			broker.GS_Code = "AN";

			var glbExternalPassword1 = factory.New<GlbExternalPasswordCUS>();
			glbExternalPassword1.GP_GC = currentCompanyPk;
			glbExternalPassword1.GP_GS = broker.PK;
			glbExternalPassword1.GP_PasswordType = JPPasswordType.Codes.CUS;
			glbExternalPassword1.GP_Transport = UserCodeSpecificTransportModeList.Codes.SEA;
			glbExternalPassword1.GP_MailBoxID = "Test1";
			glbExternalPassword1.GP_UserID = "001";
			glbExternalPassword1.CurrentDecryptedPassword = "12345678";

			var glbExternalPassword2 = factory.New<GlbExternalPasswordCUS>();
			glbExternalPassword2.GP_GC = currentCompanyPk;
			glbExternalPassword2.GP_GS = broker.PK;
			glbExternalPassword2.GP_PasswordType = JPPasswordType.Codes.CUS;
			glbExternalPassword2.GP_Transport = UserCodeSpecificTransportModeList.Codes.AIR;
			glbExternalPassword2.GP_MailBoxID = "Test2";
			glbExternalPassword2.GP_UserID = "002";

			var glbExternalPassword3 = factory.New<GlbExternalPasswordCUS>();
			glbExternalPassword3.GP_GC = currentCompanyPk;
			glbExternalPassword3.GP_GS = broker.PK;
			glbExternalPassword3.GP_PasswordType = JPPasswordType.Codes.CUS;
			glbExternalPassword3.GP_Transport = UserCodeSpecificTransportModeList.Codes.BTH;
			glbExternalPassword3.GP_MailBoxID = "Test3";
			glbExternalPassword3.GP_UserID = "003";

			factory.Save();
		}
	}
}
