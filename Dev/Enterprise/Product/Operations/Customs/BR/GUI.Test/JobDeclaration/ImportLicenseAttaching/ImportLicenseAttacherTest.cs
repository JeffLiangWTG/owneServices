using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportLicenseAttacherTest : TestCaseForAttachGUI
	{
		public void TestAttach()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var importLicense = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			Factory.Save();

			using (var form = new ZForm(declaration))
			{
				var attacher = new ImportLicenseAttacher(declaration.AttachedImportLicenseEntries, declaration.PossibleImportLicenseDeclarationForAttachment_List, ModuleIDs.Customs.BR.License, declaration);
				attacher.Show(form);
				AssertEquals("Should have called through to base Show", 1, attacher.BaseShowCoreCallCountForTesting);
				attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { importLicense });
				attacher.LastShownAttachPopupForTesting.Dispose();

				AssertType<ImportLicenseAttachingForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
